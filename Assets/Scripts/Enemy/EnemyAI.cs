using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    #region Data
    [Header("Data")]
    [SerializeField] private EnemyData enemyData;

    private EnemyStats _stats;
    private WaveInstance _waveInstance;
    private bool _isMiniBoss;
    #endregion

    #region Components
    private NavMeshAgent _agent;
    private Animator _animator;
    private Collider _collider;

    [Header("Mini Boss")]
    [SerializeField] private GameObject miniBossIcon;
    #endregion

    #region Target
    private GameObject _player;
    private Health _playerHealth;
    private Collider _playerCollider;
    private Vector3 _lastValidDestination;
    private float _distanceToPlayer;
    #endregion

    #region Attack
    [Header("Attacks")]
    [SerializeField] private GameObject attackPrefab;
    [SerializeField] private Transform attackOrigin;
    [SerializeField] private LayerMask lineOfSightBlockers;
    [SerializeField] private float attackRangeForgiveness = 0.35f;
    [SerializeField] private float lineOfSightSphereRadius = 0.12f;

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
    [SerializeField] private float knockbackWallCheckHeight = 0.35f;
    [SerializeField] private float knockbackNavMeshSampleRadius = 0.75f;
    [SerializeField] private float knockbackMaxSampleOffset = 0.8f;

    private bool _isBeingKnockedBack;
    private Coroutine _knockbackRoutine;
    #endregion

    #region Unity
    private void Awake()
    {
        CacheComponents();
        ResolvePlayer();
        ResolveStats();
        ApplyStats();
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing)
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
        HandleAgentRotationMode();
        HandleRotation();
        UpdateMoveAnimation();
    }

    private void OnDisable()
    {
        StopKnockbackRoutine();
    }
    #endregion

    #region Initialization
    private void CacheComponents()
    {
        _agent = GetComponent<NavMeshAgent>();
        _collider = GetComponent<Collider>();
        _animator = GetComponentInChildren<Animator>();
    }

    private void ResolvePlayer()
    {
        _player = GameObject.FindWithTag("Player");

        if (_player == null)
        {
            Debug.LogError("EnemyAI: Player not found!");
            return;
        }

        _playerHealth = _player.GetComponent<Health>();
        _playerCollider = _player.GetComponent<Collider>();
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
            return;
        }

        _stats = CreateFallbackStats();

#if UNITY_EDITOR
        Debug.LogWarning($"{name} using fallback EnemyStats (EnemyManager not present).");
#endif
    }

    private EnemyStats CreateFallbackStats()
    {
        return new EnemyStats
        {
            maxHealth = enemyData.maxHealth,
            damage = enemyData.damage,
            attackSpeed = enemyData.attackSpeed,
            attackRange = enemyData.attackRange,
            moveSpeed = enemyData.moveSpeed,
            weight = enemyData.weight
        };
    }

    private void ApplyStats()
    {
        if (_agent != null)
            _agent.speed = _stats.moveSpeed;

        _attackInterval = 1f / Mathf.Max(0.01f, _stats.attackSpeed);

        SetMiniBossIcon(false);

        Health health = GetComponent<Health>();
        health?.Init(_stats.maxHealth);
    }

    public void InitMiniBoss(float scaleMultiplier, float statBoostMultiplier, float statNerfMultiplier, MiniBossStatType firstBoost, MiniBossStatType secondBoost)
    {
        _isMiniBoss = true;

        transform.localScale *= scaleMultiplier;

        ApplyMiniBossStatModifiers(statBoostMultiplier, statNerfMultiplier, firstBoost, secondBoost);

        ApplyStats();
        SetMiniBossIcon(true);
    }
    #endregion

    #region Wave
    public void InitWave(WaveInstance waveInstance)
    {
        _waveInstance = waveInstance;

        if (_waveInstance != null && EnemyManager.Instance != null)
        {
            _stats = EnemyManager.Instance.GetStats(enemyData, _waveInstance.waveNumber);
            ApplyStats();
        }
    }

    public WaveInstance GetWaveInstance()
    {
        return _waveInstance;
    }
    #endregion

    #region Hard Mode
    private void SetMiniBossIcon(bool active)
    {
        if (miniBossIcon != null)
            miniBossIcon.SetActive(active);
    }

    private void ApplyMiniBossStatModifiers(float boostMultiplier, float nerfMultiplier, MiniBossStatType firstBoost, MiniBossStatType secondBoost)
    {
        ApplyMiniBossStatModifier(MiniBossStatType.Health, IsBoostedMiniBossStat(MiniBossStatType.Health, firstBoost, secondBoost) ? boostMultiplier : nerfMultiplier);
        ApplyMiniBossStatModifier(MiniBossStatType.Damage, IsBoostedMiniBossStat(MiniBossStatType.Damage, firstBoost, secondBoost) ? boostMultiplier : nerfMultiplier);
        ApplyMiniBossStatModifier(MiniBossStatType.MoveSpeed, IsBoostedMiniBossStat(MiniBossStatType.MoveSpeed, firstBoost, secondBoost) ? boostMultiplier : nerfMultiplier);
        ApplyMiniBossStatModifier(MiniBossStatType.AttackSpeed, IsBoostedMiniBossStat(MiniBossStatType.AttackSpeed, firstBoost, secondBoost) ? boostMultiplier : nerfMultiplier);
    }

    private bool IsBoostedMiniBossStat(MiniBossStatType statType, MiniBossStatType firstBoost, MiniBossStatType secondBoost)
    {
        return statType == firstBoost || statType == secondBoost;
    }

    private void ApplyMiniBossStatModifier(MiniBossStatType statType, float multiplier)
    {
        switch (statType)
        {
            case MiniBossStatType.Health:
                _stats.maxHealth = Mathf.RoundToInt(_stats.maxHealth * multiplier);
                break;

            case MiniBossStatType.Damage:
                _stats.damage = Mathf.RoundToInt(_stats.damage * multiplier);
                break;

            case MiniBossStatType.MoveSpeed:
                _stats.moveSpeed *= multiplier;
                break;

            case MiniBossStatType.AttackSpeed:
                _stats.attackSpeed *= multiplier;
                break;
        }
    }
    #endregion

    #region Movement / Detection
    private void HandleDistanceCheck()
    {
        if (_player == null) return;
        if (_isAttacking) return;

        _distanceToPlayer = GetHorizontalDistanceToPlayer();

        if (_distanceToPlayer <= _stats.attackRange + attackRangeForgiveness || IsPlayerTouching())
            HandlePlayerInAttackRange();
        else
            ChasePlayer();
    }

    private float GetHorizontalDistanceToPlayer()
    {
        Vector3 enemyPosition = transform.position;
        Vector3 playerPosition = _player.transform.position;

        enemyPosition.y = 0f;
        playerPosition.y = 0f;

        return Vector3.Distance(enemyPosition, playerPosition);
    }

    private void HandlePlayerInAttackRange()
    {
        if (!HasLineOfSight())
        {
            ChasePlayer();
            return;
        }

        StopMovement();
        TryAttack();
    }

    private void ChasePlayer()
    {
        ResumeMovement();
        MoveToPlayer();
    }

    private bool HasLineOfSight()
    {
        if (_player == null || attackOrigin == null)
            return false;

        Vector3 origin = attackOrigin.position;
        Vector3 target = GetPlayerAimPosition();
        Vector3 direction = target - origin;

        if (direction.sqrMagnitude <= 0.01f)
            return true;

        float distance = direction.magnitude;

        return !Physics.SphereCast(
            origin,
            lineOfSightSphereRadius,
            direction.normalized,
            out _,
            distance,
            lineOfSightBlockers,
            QueryTriggerInteraction.Ignore
        );
    }

    private Vector3 GetPlayerAimPosition()
    {
        if (_playerCollider != null)
            return _playerCollider.bounds.center;

        return _player.transform.position + Vector3.up;
    }

    private bool IsPlayerTouching()
    {
        if (_player == null || _collider == null)
            return false;

        if (_playerCollider == null)
            _playerCollider = _player.GetComponent<Collider>();

        if (_playerCollider == null) return false;

        return _collider.bounds.Intersects(_playerCollider.bounds);
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

    #region Navigation
    private void MoveToPlayer()
    {
        if (_agent == null || !_agent.enabled || !_agent.isOnNavMesh) return;
        if (_player == null) return;

        NavMeshPath path = new NavMeshPath();
        _agent.CalculatePath(_player.transform.position, path);

        if (path.status == NavMeshPathStatus.PathComplete)
        {
            _lastValidDestination = _player.transform.position;
            _agent.SetDestination(_lastValidDestination);
        }
        else if (path.status == NavMeshPathStatus.PathPartial && path.corners.Length > 0)
        {
            _lastValidDestination = path.corners[^1];
            _agent.SetDestination(_lastValidDestination);
        }
        else if (_lastValidDestination != Vector3.zero)
        {
            _agent.SetDestination(_lastValidDestination);
        }
    }

    private void HandleAgentRotationMode()
    {
        if (_agent != null)
            _agent.updateRotation = !_isAttacking;
    }

    private void HandleRotation()
    {
        if (!_canRotate) return;
        if (_player == null || _agent == null) return;

        Vector3 direction = GetRotationDirection();
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(direction.normalized);
    }

    private Vector3 GetRotationDirection()
    {
        if (_agent.isOnOffMeshLink)
            return _agent.currentOffMeshLinkData.endPos - transform.position;

        if (!_isAttacking && _agent.velocity.sqrMagnitude > 0.1f)
            return _agent.velocity;

        return _player.transform.position - transform.position;
    }

    private void UpdateMoveAnimation()
    {
        if (_animator == null) return;
        if (_agent == null || !_agent.enabled) return;

        float normalizedSpeed = _agent.velocity.magnitude / Mathf.Max(0.01f, _agent.speed);
        _animator.SetFloat("MoveSpeed", normalizedSpeed);
    }
    #endregion

    #region Attack
    private void TryAttack()
    {
        if (!_canAttack) return;

        _canAttack = false;
        _isAttacking = true;

        PlayAttackAnimation();
        StartCoroutine(AttackCooldown());
    }

    private void PlayAttackAnimation()
    {
        if (_animator == null) return;

        _animator.SetFloat("AttackSpeed", GetAttackAnimationSpeed());
        _animator.SetTrigger("Attack");
    }

    private float GetAttackAnimationSpeed()
    {
        if (_animator == null || _animator.runtimeAnimatorController == null)
            return 1f;

        AnimationClip[] clips = _animator.runtimeAnimatorController.animationClips;

        if (clips == null || clips.Length == 0 || clips[0] == null)
            return 1f;

        return clips[0].length / _attackInterval;
    }

    public void ApplyAttackDamage()
    {
        PlayAttackSound();

        if (_player == null) return;
        if (attackPrefab == null || attackOrigin == null) return;

        GameObject attackObject = Instantiate(attackPrefab, attackOrigin.position, attackOrigin.rotation);

        IEnemyAttack attack = attackObject.GetComponent<IEnemyAttack>();
        attack?.Init(_stats.damage, gameObject, _player.transform.position);
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

    #region Knockback
    public void ApplyKnockback(Vector3 sourcePosition, float strength)
    {
        if (!CanApplyKnockback(strength)) return;

        Vector3 direction = GetKnockbackDirection(sourcePosition);
        float distance = strength / Mathf.Max(0.1f, _stats.weight);

        StopKnockbackRoutine();
        _knockbackRoutine = StartCoroutine(KnockbackRoutine(direction.normalized, distance));
    }

    private bool CanApplyKnockback(float strength)
    {
        if (!gameObject.activeInHierarchy) return false;
        if (_agent == null || !_agent.enabled || !_agent.isOnNavMesh) return false;
        if (strength <= 0f) return false;

        return true;
    }

    private Vector3 GetKnockbackDirection(Vector3 sourcePosition)
    {
        Vector3 direction = transform.position - sourcePosition;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            direction = -transform.forward;

        return direction.normalized;
    }

    private IEnumerator KnockbackRoutine(Vector3 direction, float distance)
    {
        if (_agent == null || !_agent.enabled || !_agent.isOnNavMesh)
            yield break;

        _isBeingKnockedBack = true;

        Quaternion lockedRotation = transform.rotation;
        Vector3 startGroundPosition = GetGroundPosition();
        Vector3 visualOffset = transform.position - startGroundPosition;
        Vector3 lastValidGroundPosition = startGroundPosition;

        BeginKnockbackMovement();

        float elapsed = 0f;

        while (elapsed < knockbackDuration)
        {
            if (_agent == null || !_agent.enabled || !gameObject.activeInHierarchy)
                break;

            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / knockbackDuration);
            Vector3 desiredGroundPosition = startGroundPosition + direction * distance * t;

            if (IsKnockbackBlocked(lastValidGroundPosition, desiredGroundPosition))
                break;

            if (!TryMoveKnockbackPosition(lastValidGroundPosition, desiredGroundPosition, visualOffset, out lastValidGroundPosition))
                break;

            transform.rotation = lockedRotation;

            yield return null;
        }

        EndKnockbackMovement(lastValidGroundPosition, visualOffset);

        _isBeingKnockedBack = false;
        _knockbackRoutine = null;
    }

    private void BeginKnockbackMovement()
    {
        if (_agent == null || !_agent.enabled || !_agent.isOnNavMesh) return;

        _agent.isStopped = true;
        _agent.updatePosition = false;

        if (_animator != null)
            _animator.SetBool("IsMoving", false);
    }

    private bool IsKnockbackBlocked(Vector3 lastValidGroundPosition, Vector3 desiredGroundPosition)
    {
        Vector3 rayStart = lastValidGroundPosition + Vector3.up * knockbackWallCheckHeight;
        Vector3 rayEnd = desiredGroundPosition + Vector3.up * knockbackWallCheckHeight;

        return Physics.Linecast(rayStart, rayEnd, knockbackBlockers, QueryTriggerInteraction.Ignore);
    }

    private bool TryMoveKnockbackPosition(Vector3 previousGroundPosition, Vector3 desiredGroundPosition, Vector3 visualOffset, out Vector3 lastValidGroundPosition)
    {
        lastValidGroundPosition = previousGroundPosition;

        if (!NavMesh.SamplePosition(desiredGroundPosition, out NavMeshHit hit, knockbackNavMeshSampleRadius, NavMesh.AllAreas))
            return false;

        if (Vector3.Distance(hit.position, desiredGroundPosition) > knockbackMaxSampleOffset)
            return false;

        if (IsKnockbackBlocked(previousGroundPosition, hit.position))
            return false;

        lastValidGroundPosition = hit.position;
        transform.position = hit.position + visualOffset;

        return true;
    }

    private void EndKnockbackMovement(Vector3 finalGroundPosition, Vector3 visualOffset)
    {
        if (_agent == null || !_agent.enabled)
            return;

        if (NavMesh.SamplePosition(finalGroundPosition, out NavMeshHit finalHit, 0.5f, NavMesh.AllAreas))
            transform.position = finalHit.position + visualOffset;

        if (_agent.isOnNavMesh)
        {
            _agent.Warp(transform.position);
            _agent.updatePosition = true;
        }

        if (_isAttacking)
            StopMovement();
        else
            ResumeMovement();
    }

    private void StopKnockbackRoutine()
    {
        if (_knockbackRoutine != null)
            StopCoroutine(_knockbackRoutine);

        _knockbackRoutine = null;
        _isBeingKnockedBack = false;

        if (_agent != null && _agent.enabled && _agent.isOnNavMesh)
        {
            _agent.updatePosition = true;
            _agent.Warp(transform.position);
        }
    }

    private Vector3 GetGroundPosition()
    {
        if (groundPoint != null)
            return groundPoint.position;

        if (_collider != null)
            return new Vector3(transform.position.x, _collider.bounds.min.y, transform.position.z);

        return transform.position;
    }
    #endregion

    #region Audio
    public void PlayAttackSound()
    {
        if (enemyData == null) return;
        if (string.IsNullOrWhiteSpace(enemyData.attackSoundName)) return;

        AudioManager.Instance?.PlaySFX(enemyData.attackSoundName);
    }

    public void PlayHitSound()
    {
        if (enemyData == null) return;
        if (string.IsNullOrWhiteSpace(enemyData.hitSoundName)) return;

        AudioManager.Instance?.PlaySFX(enemyData.hitSoundName);
    }

    private void PlayDeathSound()
    {
        if (enemyData == null) return;
        if (string.IsNullOrWhiteSpace(enemyData.deathSoundName)) return;

        AudioManager.Instance?.PlaySFX(enemyData.deathSoundName);
    }
    #endregion

    #region Death
    public void HandleDeath()
    {
        PlayDeathSound();

        UpdateWaveOnDeath();
        GiveSilverReward();

        RunStatsManager.Instance?.AddEnemyDefeated();
    }

    private void UpdateWaveOnDeath()
    {
        WaveManager.Instance?.OnEnemyKilled(this, transform.position);
    }

    private void GiveSilverReward()
    {
        if (enemyData == null) return;
        if (CurrencyManager.Instance == null) return;

        int silverAmount = Random.Range(enemyData.minSilverDrop, enemyData.maxSilverDrop + 1);

        if (_isMiniBoss)
            silverAmount *= 2;

        silverAmount = Mathf.RoundToInt(silverAmount * GetPlayerSilverMultiplier());

        CurrencyManager.Instance.AddSilver(silverAmount);
    }

    private float GetPlayerSilverMultiplier()
    {
        PlayerController playerController = FindFirstObjectByType<PlayerController>();
        return playerController != null ? playerController.GetSilverGainMultiplier() : 1f;
    }
    #endregion
}