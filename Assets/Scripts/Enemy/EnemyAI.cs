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
    #endregion

    #region Attack
    [SerializeField] private GameObject attackPrefab;
    [SerializeField] private Transform attackOrigin;

    private bool _canAttack = true;
    private bool _isAttacking;
    #endregion

    #region State
    private float _distanceToPlayer;
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
        if (_player == null) return;

        HandleDistanceCheck();
        _agent.updateRotation = !_isAttacking;
        HandleRotation();
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

        if (EnemyManager.Instance == null)
        {
            Debug.LogError("EnemyManager not found in scene!");
            return;
        }

        _stats = EnemyManager.Instance.GetStats(enemyData);
    }

    private void ApplyStats()
    {
        _agent.speed = _stats.moveSpeed;

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

    private void HandleDistanceCheck()
    {
        if (_isAttacking) return;

        _distanceToPlayer = Vector3.Distance(transform.position, _player.transform.position);

        if (_distanceToPlayer <= _stats.attackRange || IsPlayerTouching())
        {
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
        _agent.isStopped = true;
        _agent.velocity = Vector3.zero;

        if (_animator != null)
            _animator.SetBool("IsMoving", false);
    }

    private void ResumeMovement()
    {
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
            _animator.SetFloat("AttackSpeed", _stats.attackSpeed);
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
        {
            attack.Init(_stats.damage, gameObject, _player.transform.position);
        }
        else
        {
            Debug.LogWarning($"{attackObj.name} has no IEnemyAttack component.");
        }
    }

    public void OnAttackFinished()
    {
        _isAttacking = false;
    }

    private IEnumerator AttackCooldown()
    {
        if (_animator == null)
        {
            ApplyAttackDamage();
        }

        float attackInterval = 1f / _stats.attackSpeed;
        yield return new WaitForSeconds(attackInterval);

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
        Vector3 direction;

        if (_agent.isOnOffMeshLink)
        {
            direction = _agent.currentOffMeshLinkData.endPos - transform.position;
        }
        else if (!_isAttacking && _agent.velocity.sqrMagnitude > 0.1f)
        {
            direction = _agent.velocity;
        }
        else
        {
            direction = _player.transform.position - transform.position;
        }

        direction.y = 0f;

        if (direction != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(direction.normalized);
    }

    #endregion
}


