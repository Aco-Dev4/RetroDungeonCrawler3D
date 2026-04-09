using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private EnemyData enemyData;

    private EnemyStats _stats;
    private WaveInstance _waveInstance;

    #region Components
    private NavMeshAgent _agent;
    private Animator _animator;
    private Collider _collider;
    #endregion

    #region Target
    private GameObject _player;
    private Health _playerHealth;
    private Vector3 _lastValidDestination;
    private float _distanceToPlayer;
    #endregion

    #region Attack
    [Header("Attacks")]
    [SerializeField] private GameObject attackPrefab;
    [SerializeField] private Transform attackOrigin;

    [SerializeField] private LayerMask lineOfSightBlockers;

    private bool _canAttack = true;
    private bool _canRotate = true;
    private bool _isAttacking;

    private float _attackInterval;
    #endregion

    #region Knockback
    [Header("Knockback")]
    [SerializeField] private float knockbackDuration = 0.12f;
    [SerializeField] private Transform groundPoint;
    [SerializeField] private LayerMask knockbackBlockers;
    [SerializeField] private float wallStopPadding = 0.1f;

    private bool _isBeingKnockedBack;
    private Coroutine _knockbackRoutine;
    #endregion

    private void Awake()
    {
        CacheComponents();
        ResolvePlayer();
        ResolveStats();
        ApplyStats();
    }

    private void Update()
    {
        if (GameManager.Instance.State != GameState.Playing)
        {
            StopMovement();
            return;
        }

        if (_player == null) return;

        if (_isBeingKnockedBack)
        {
            StopMovement();
            return;
        }

        HandleDistanceCheck();
        _agent.updateRotation = !_isAttacking;
        HandleRotation();
        UpdateMoveAnimation();
    }

    #region Initialization
    private void CacheComponents()
    {
        _agent = GetComponent<NavMeshAgent>();
        _collider = GetComponent<Collider>();
        _animator = GetComponentInChildren<Animator>();
    }

    private void ResolvePlayer()
    {
        GameObject found = GameObject.FindWithTag("Player");
        if (found == null)
        {
            Debug.LogError("EnemyAI: Player not found!");
            return;
        }

        _player = found;
        _playerHealth = _player.GetComponent<Health>();
    }

    private void ResolveStats()
    {
        if (enemyData == null)
        {
            Debug.LogError($"{name} has no EnemyData assigned!");
            return;
        }

        if (EnemyManager.Instance != null)
        {
            _stats = EnemyManager.Instance.GetStats(enemyData);
        }
        else
        {
            _stats = new EnemyStats
            {
                maxHealth = enemyData.maxHealth,
                damage = enemyData.damage,
                attackSpeed = enemyData.attackSpeed,
                attackRange = enemyData.attackRange,
                moveSpeed = enemyData.moveSpeed,
                weight = enemyData.weight
            };

#if UNITY_EDITOR
            Debug.LogWarning($"{name} using fallback EnemyStats (EnemyManager not present).");
#endif
        }
    }

    private void ApplyStats()
    {
        _agent.speed = _stats.moveSpeed;
        _attackInterval = 1f / Mathf.Max(0.01f, _stats.attackSpeed);

        Health health = GetComponent<Health>();
        if (health != null)
            health.Init(_stats.maxHealth);
    }
    #endregion

    #region Wave
    public void InitWave(WaveInstance waveInstance)
    {
        _waveInstance = waveInstance;
    }

    public WaveInstance GetWaveInstance()
    {
        return _waveInstance;
    }
    #endregion

    #region Movement & Detection
    private bool HasLineOfSight()
    {
        Vector3 dir = _player.transform.position - attackOrigin.position;
        if (Physics.Raycast(attackOrigin.position, dir.normalized, out RaycastHit hit, dir.magnitude, lineOfSightBlockers))
            return false;
        return true;
    }

    private void HandleDistanceCheck()
    {
        if (_isBeingKnockedBack || _player == null) return;
        if (_isAttacking) return;

        _distanceToPlayer = Vector3.Distance(transform.position, _player.transform.position);

        if (_distanceToPlayer <= _stats.attackRange || IsPlayerTouching())
        {
            if (!HasLineOfSight())
            {
                ResumeMovement();
                MoveToPlayer();
                return;
            }

            StopMovement();
            TryAttack();
        }
        else
        {
            ResumeMovement();
            MoveToPlayer();
        }
    }

    private bool IsPlayerTouching()
    {
        Collider playerCollider = _player.GetComponent<Collider>();
        if (playerCollider == null) return false;

        return _collider.bounds.Intersects(playerCollider.bounds);
    }

    private void StopMovement()
    {
        if (_agent != null && _agent.enabled && _agent.isOnNavMesh)
        {
            _agent.isStopped = true;
            _agent.velocity = Vector3.zero;
        }

        if (_animator != null)
            _animator.SetBool("IsMoving", false);
    }

    private void ResumeMovement()
    {
        if (_agent != null && _agent.enabled && _agent.isOnNavMesh)
            _agent.isStopped = false;

        if (_animator != null)
            _animator.SetBool("IsMoving", true);
    }
    #endregion

    #region Attack Logic
    private void TryAttack()
    {
        if (!_canAttack) return;

        _canAttack = false;
        _isAttacking = true;

        if (_animator != null)
        {
            float animSpeed = 1f;
            AnimationClip clip = _animator.runtimeAnimatorController.animationClips[0];
            if (clip != null)
                animSpeed = clip.length / _attackInterval;

            _animator.SetFloat("AttackSpeed", animSpeed);
            _animator.SetTrigger("Attack");
        }

        StartCoroutine(AttackCooldown());
    }

    public void ApplyAttackDamage()
    {
        if (_player == null) return;

        GameObject attackObj = Instantiate(attackPrefab, attackOrigin.position, attackOrigin.rotation);

        IEnemyAttack attack = attackObj.GetComponent<IEnemyAttack>();
        if (attack != null)
            attack.Init(_stats.damage, gameObject, _player.transform.position);
    }

    public void DisableRotation()
    {
        _canRotate = false;
    }

    public void OnAttackFinished()
    {
        _isAttacking = false;
        _canRotate = true;
    }

    private IEnumerator AttackCooldown()
    {
        if (_animator == null)
            ApplyAttackDamage();

        yield return new WaitForSeconds(_attackInterval);

        _canAttack = true;

        if (_animator == null)
            _isAttacking = false;
    }
    #endregion

    #region Navigation
    private void MoveToPlayer()
    {
        NavMeshPath path = new NavMeshPath();
        _agent.CalculatePath(_player.transform.position, path);

        if (path.status == NavMeshPathStatus.PathComplete)
        {
            _lastValidDestination = _player.transform.position;
            _agent.SetDestination(_player.transform.position);
        }
        else if (path.status == NavMeshPathStatus.PathPartial)
        {
            _lastValidDestination = path.corners[^1];
            _agent.SetDestination(_lastValidDestination);
        }
        else if (_lastValidDestination != Vector3.zero)
        {
            _agent.SetDestination(_lastValidDestination);
        }
    }

    private void HandleRotation()
    {
        if (!_canRotate) return;

        Vector3 direction;

        if (_agent.isOnOffMeshLink)
            direction = _agent.currentOffMeshLinkData.endPos - transform.position;
        else if (!_isAttacking && _agent.velocity.sqrMagnitude > 0.1f)
            direction = _agent.velocity;
        else
            direction = _player.transform.position - transform.position;

        direction.y = 0f;

        if (direction != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(direction.normalized);
    }

    private void UpdateMoveAnimation()
    {
        if (_animator == null) return;
        if (_agent == null || !_agent.enabled) return;

        float normalizedSpeed = _agent.velocity.magnitude / Mathf.Max(0.01f, _agent.speed);
        _animator.SetFloat("MoveSpeed", normalizedSpeed);
    }
    #endregion

    #region Knockback
    public void ApplyKnockback(Vector3 sourcePosition, float strength)
    {
        if (!gameObject.activeInHierarchy) return;
        if (_agent == null || !_agent.enabled || !_agent.isOnNavMesh) return;
        if (strength <= 0f) return;

        Vector3 direction = transform.position - sourcePosition;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            direction = -transform.forward;

        float distance = strength / Mathf.Max(0.1f, _stats.weight);

        if (_knockbackRoutine != null)
            StopCoroutine(_knockbackRoutine);

        _knockbackRoutine = StartCoroutine(KnockbackRoutine(direction.normalized, distance));
    }

    private IEnumerator KnockbackRoutine(Vector3 direction, float distance)
    {
        if (_agent == null || !_agent.enabled || !_agent.isOnNavMesh)
            yield break;

        _isBeingKnockedBack = true;

        Quaternion lockedRotation = transform.rotation;
        Vector3 startGroundPosition = GetGroundPosition();
        Vector3 visualOffset = transform.position - startGroundPosition;

        _agent.isStopped = true;
        _agent.updatePosition = false;

        if (_animator != null)
            _animator.SetBool("IsMoving", false);

        float elapsed = 0f;
        Vector3 lastValidGroundPosition = startGroundPosition;

        while (elapsed < knockbackDuration)
        {
            if (_agent == null || !_agent.enabled || !gameObject.activeInHierarchy)
                yield break;

            elapsed += Time.deltaTime;
            float t = elapsed / knockbackDuration;

            Vector3 desiredGroundPosition = startGroundPosition + direction * distance * t;
            Vector3 rayStart = lastValidGroundPosition + Vector3.up * 0.2f;
            Vector3 rayEnd = desiredGroundPosition + Vector3.up * 0.2f;

            if (Physics.Linecast(rayStart, rayEnd, out RaycastHit wallHit, knockbackBlockers))
                break;

            if (NavMesh.SamplePosition(desiredGroundPosition, out NavMeshHit hit, 1.5f, NavMesh.AllAreas))
            {
                lastValidGroundPosition = hit.position;
                transform.position = hit.position + visualOffset;
            }
            else
            {
                break;
            }

            transform.rotation = lockedRotation;
            yield return null;
        }

        if (_agent != null && _agent.enabled && _agent.isOnNavMesh)
        {
            Vector3 finalGroundPosition = lastValidGroundPosition;

            if (NavMesh.SamplePosition(finalGroundPosition, out NavMeshHit finalHit, 1f, NavMesh.AllAreas))
                transform.position = finalHit.position + visualOffset;

            _agent.Warp(transform.position);
            _agent.updatePosition = true;

            if (_isAttacking)
                StopMovement();
            else
                ResumeMovement();
        }

        _isBeingKnockedBack = false;
        _knockbackRoutine = null;
    }

    private Vector3 GetGroundPosition()
    {
        if (groundPoint != null)
            return groundPoint.position;

        if (_collider != null)
            return new Vector3(transform.position.x, _collider.bounds.min.y, transform.position.z);

        return transform.position;
    }

    private void OnDisable()
    {
        if (_knockbackRoutine != null)
            StopCoroutine(_knockbackRoutine);

        _knockbackRoutine = null;
        _isBeingKnockedBack = false;
    }
    #endregion

    #region Death
    public void HandleDeath()
    {
        if (_waveInstance != null)
        {
            _waveInstance.aliveEnemies = Mathf.Max(0, _waveInstance.aliveEnemies - 1);
            _waveInstance.lastDeathPosition = transform.position;
            _waveInstance.lastDeathRotation = transform.rotation;
        }

        if (enemyData != null && CurrencyManager.Instance != null)
        {
            int silverAmount = Random.Range(enemyData.minSilverDrop, enemyData.maxSilverDrop + 1);
            CurrencyManager.Instance.AddSilver(silverAmount);
        }
    }
    #endregion
}