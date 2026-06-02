using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    #region Components
    private PlayerRuntimeStats _stats = new();
    private CharacterController _characterController;
    private Camera _mainCamera;
    private Health _health;

    [Header("Core References")]
    [SerializeField] private PlayerData playerData;
    [SerializeField] private Animator _animator;
    [SerializeField] private Transform orbitPivot;

    [Header("Card / Effect References")]
    [SerializeField] private RunCardInventory runCardInventory;
    [SerializeField] private PlayerCombatEffectHandler combatEffectHandler;
    [SerializeField] private PlayerRunEffectHandler runEffectHandler;
    [SerializeField] private PlayerDeathEffectHandler deathEffectHandler;
    [SerializeField] private PlayerRewardEffectHandler rewardEffectHandler;

    [Header("Tutorial Override")]
    [SerializeField] private bool ignorePermanentUpgrades;
    [SerializeField] private int tutorialBaseDamage = 15;
    #endregion

    #region Movement Runtime
    private Vector2 _input;
    private Vector3 _direction;

    private float _gravity = -9.81f;
    private float _velocity;
    private int _numberOfJumps;
    private bool _isSliding;
    #endregion

    #region Attack Settings
    [SerializeField] private LayerMask enemyLayer;

    [Header("Punch")]
    [SerializeField] private AnimationClip punchAttackClip;

    [Header("Sword")]
    [SerializeField] private GameObject swordObject;
    [SerializeField] private AnimationClip swordAttackClip;
    [SerializeField] private float swordRangeBonus = 1f;
    [SerializeField] private int swordDamageBonus = 5;
    [SerializeField] private Vector3 baseSwordScale = Vector3.one;
    [SerializeField] private float swordScalePerBonusRange = 0.15f;
    #endregion

    #region Earthquake Jump
    [Header("Earthquake Jump")]
    [SerializeField] private LayerMask earthquakeEnemyLayer;
    [SerializeField] private float earthquakeMinFallDistance = 2f;
    [SerializeField] private float earthquakeBaseRadius = 2f;
    [SerializeField] private float earthquakeRadiusPerLevel = 0.4f;
    [SerializeField] private float earthquakeDamagePerFallUnit = 5f;
    [SerializeField] private float earthquakeDamagePerLevel = 3f;
    [SerializeField] private float earthquakeBaseKnockback = 5f;
    [SerializeField] private float earthquakeKnockbackPerLevel = 1.5f;
    [SerializeField] private int earthquakeMaxEnemiesHit = 8;
    [SerializeField] private float earthquakeCooldown = 0.25f;

    private bool _wasGroundedLastFrame = true;
    private bool _isTrackingFall;
    private float _fallStartY;
    private float _lastEarthquakeTime = -999f;
    private readonly Collider[] _earthquakeHits = new Collider[32];
    #endregion

    #region Attack Runtime
    private bool _canAttack = true;
    private bool _hasSword;

    private float AttackInterval => 1f / _attackSpeed;
    #endregion

    #region Runtime Stats
    private int _maxHealth;
    private float _moveSpeed;
    private float _rotationSpeed;
    private float _gravityMultiplier;
    private float _jumpPower;
    private int _maxJumps;

    private float _attackRange;
    private int _attackDamage;
    private float _attackSpeed;

    private int _luck;
    private float _knockbackStrength;
    private float _silverGainMultiplier;
    private float _critChance;
    private float _critDamageMultiplier;
    private int _successfulHitsSinceGuaranteedCrit;
    #endregion

    #region Unity
    private void Awake()
    {
        CacheComponents();
        ApplyBaseStats();
        SyncEffectHandlers();
        SetupSword();
        InitHealth();
    }

    private void Start()
    {
        CursorManager.Instance?.LockCursor();
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing)
            return;

        HandleRotation();
        HandleGravity();
        HandleSlide();
        HandleMovement();
        HandleEarthquakeLanding();
    }
    #endregion

    #region Setup
    private void CacheComponents()
    {
        _characterController = GetComponent<CharacterController>();
        _mainCamera = Camera.main;
        _health = GetComponent<Health>();

        if (runCardInventory == null)
            runCardInventory = GetComponent<RunCardInventory>();

        if (combatEffectHandler == null)
            combatEffectHandler = GetComponent<PlayerCombatEffectHandler>();

        if (runEffectHandler == null)
            runEffectHandler = GetComponent<PlayerRunEffectHandler>();

        if (deathEffectHandler == null)
            deathEffectHandler = GetComponent<PlayerDeathEffectHandler>();

        if (rewardEffectHandler == null)
            rewardEffectHandler = GetComponent<PlayerRewardEffectHandler>();
    }

    private void SetupSword()
    {
        _hasSword = !ignorePermanentUpgrades &&
                    GameDataManager.Instance != null &&
                    GameDataManager.Instance.HasSword();

        if (swordObject != null)
            swordObject.SetActive(_hasSword);

        if (swordObject != null)
            baseSwordScale = swordObject.transform.localScale;
    }

    private void UpdateSwordScale()
    {
        if (swordObject == null) return;
        if (!_hasSword) return;

        float bonusRange = Mathf.Max(0f, _attackRange - playerData.attackRange);
        float scaleMultiplier = 1f + bonusRange * swordScalePerBonusRange;

        swordObject.transform.localScale = baseSwordScale * scaleMultiplier;
    }

    private void InitHealth()
    {
        if (_health != null)
            _health.Init(_maxHealth);
    }

    private void SyncEffectHandlers()
    {
        rewardEffectHandler?.SetRerolls(_stats.rewardRerolls);
        deathEffectHandler?.SetRevives(_stats.guardianAngelRevives);
    }

    private bool ShouldForceGuaranteedCrit()
    {
        if (_stats.guaranteedCritEveryXHits <= 0)
            return false;

        return _successfulHitsSinceGuaranteedCrit + 1 >= _stats.guaranteedCritEveryXHits;
    }

    private void RegisterSuccessfulHit(PlayerAttackResult attackResult)
    {
        if (_stats.guaranteedCritEveryXHits <= 0)
            return;

        if (attackResult.isGuaranteedCrit)
        {
            Debug.Log("GUARANTEED CRIT USED - counter reset");
            _successfulHitsSinceGuaranteedCrit = 0;
            return;
        }

        _successfulHitsSinceGuaranteedCrit++;
    }
    #endregion

    #region Input
    public void Move(InputAction.CallbackContext context)
    {
        _input = context.ReadValue<Vector2>();
        _direction = new Vector3(_input.x, 0f, _input.y);
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        if (!CanJump()) return;

        if (_numberOfJumps == 0)
            StartCoroutine(WaitForLanding());

        _numberOfJumps++;
        _velocity = _jumpPower;
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        if (!_canAttack) return;

        StartAttack();
    }
    #endregion

    #region Movement
    private void HandleRotation()
    {
        if (_input.sqrMagnitude == 0f) return;
        if (_mainCamera == null) return;

        _direction = Quaternion.Euler(0f, _mainCamera.transform.eulerAngles.y, 0f) * new Vector3(_input.x, 0f, _input.y);

        Quaternion targetRotation = Quaternion.LookRotation(_direction, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
    }

    private void HandleGravity()
    {
        if (_isSliding) return;

        if (IsGrounded() && _velocity < 0f)
            _velocity = -1f;
        else
            _velocity += _gravity * _gravityMultiplier * Time.deltaTime;

        _direction.y = _velocity;

        if (_animator != null)
        {
            _animator.SetBool("isGrounded", IsGrounded());
            _animator.SetFloat("VerticalVel", _velocity);
        }
    }

    private void HandleSlide()
    {
        if (_characterController == null) return;

        if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out RaycastHit hitInfo, 2f))
        {
            float slopeAngle = Vector3.Angle(hitInfo.normal, Vector3.up);

            if (slopeAngle > _characterController.slopeLimit)
            {
                _isSliding = true;

                Vector3 slopeDirection = Vector3.ProjectOnPlane(Vector3.down, hitInfo.normal).normalized;
                _characterController.Move(slopeDirection * (_moveSpeed * Time.deltaTime));

                return;
            }
        }

        _isSliding = false;
    }

    private void HandleMovement()
    {
        if (_characterController == null) return;

        _characterController.Move(_direction * (_moveSpeed * Time.deltaTime));

        if (_animator != null)
        {
            float moveAmount = new Vector3(_direction.x, 0f, _direction.z).magnitude;
            _animator.SetFloat("Speed", moveAmount);
        }
    }

    private bool CanJump()
    {
        if (_isSliding) return false;
        if (!IsGrounded() && _numberOfJumps >= _maxJumps) return false;

        return true;
    }

    private IEnumerator WaitForLanding()
    {
        yield return new WaitUntil(() => !IsGrounded());
        yield return new WaitUntil(IsGrounded);

        _numberOfJumps = 0;
    }

    private bool IsGrounded()
    {
        return _characterController != null && _characterController.isGrounded;
    }
    #endregion

    #region Earthquake Jump
    private void HandleEarthquakeLanding()
    {
        if (_stats.earthquakeJumpLevel <= 0)
            return;

        bool isGroundedNow = IsGrounded();

        if (!isGroundedNow && _wasGroundedLastFrame)
            StartFallTracking();

        if (isGroundedNow && !_wasGroundedLastFrame)
            TryQueueEarthquakeJump();

        if (_isTrackingFall)
            _fallStartY = Mathf.Max(_fallStartY, transform.position.y);

        _wasGroundedLastFrame = isGroundedNow;
    }

    private void StartFallTracking()
    {
        _isTrackingFall = true;
        _fallStartY = transform.position.y;
    }

    private void TryQueueEarthquakeJump()
    {
        if (!_isTrackingFall)
            return;

        _isTrackingFall = false;

        if (Time.time < _lastEarthquakeTime + earthquakeCooldown)
            return;

        float fallDistance = _fallStartY - transform.position.y;

        if (fallDistance < earthquakeMinFallDistance)
            return;

        _lastEarthquakeTime = Time.time;
        StartCoroutine(TriggerEarthquakeNextFrame(fallDistance));
    }

    private IEnumerator TriggerEarthquakeNextFrame(float fallDistance)
    {
        yield return null;

        TriggerEarthquake(fallDistance);
    }

    private void TriggerEarthquake(float fallDistance)
    {
        int level = _stats.earthquakeJumpLevel;

        float radius = earthquakeBaseRadius + earthquakeRadiusPerLevel * (level - 1);
        int damage = Mathf.RoundToInt(fallDistance * earthquakeDamagePerFallUnit + earthquakeDamagePerLevel * level);
        float knockback = earthquakeBaseKnockback + earthquakeKnockbackPerLevel * (level - 1);

        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, radius, _earthquakeHits, earthquakeEnemyLayer);
        int enemiesHit = 0;

        for (int i = 0; i < hitCount; i++)
        {
            if (enemiesHit >= earthquakeMaxEnemiesHit)
                break;

            if (!TryGetEnemyHitData(_earthquakeHits[i], out Health enemyHealth, out EnemyAI enemy))
                continue;

            enemyHealth.TakeDamage(damage, gameObject);

            if (enemy != null)
                enemy.ApplyKnockback(transform.position, knockback);

            enemiesHit++;
        }

        Debug.Log($"EARTHQUAKE! Hit: {enemiesHit}, Radius: {radius}, Damage: {damage}, Knockback: {knockback}");
    }
    #endregion

    #region Attack
    private void StartAttack()
    {
        _canAttack = false;

        if (_animator != null)
        {
            _animator.SetFloat("AttackSpeed", _attackSpeed);
            _animator.SetTrigger(_hasSword ? "Swing" : "Attack");
        }

        StartCoroutine(AttackCooldown());
    }

    public void ApplyAttackDamage()
    {
        float finalRange = PlayerDamageCalculator.GetAttackRange(_stats, _hasSword, swordRangeBonus);
        bool forceCrit = ShouldForceGuaranteedCrit();
        PlayerAttackResult attackResult = PlayerDamageCalculator.GetAttackResult(_stats, _hasSword, swordDamageBonus, _health, forceCrit);

        if (attackResult.isCrit)
            Debug.Log($"CRIT! {attackResult.damage} damage");

        Vector3 attackOrigin = transform.position + transform.forward * (finalRange * 0.5f);
        Collider[] hits = Physics.OverlapSphere(attackOrigin, finalRange * 0.5f, enemyLayer);

        HitEnemies(hits, attackResult);
    }

    private void HitEnemies(Collider[] hits, PlayerAttackResult attackResult)
    {
        for (int i = 0; i < hits.Length; i++)
        {
            if (!TryGetEnemyHitData(hits[i], out Health targetHealth, out EnemyAI enemy))
                continue;

            targetHealth.TakeDamage(attackResult.damage, gameObject);

            RegisterSuccessfulHit(attackResult);

            combatEffectHandler?.OnDamageDealt(attackResult, targetHealth, enemy);
            ApplyNormalKnockback(enemy);

            if (!_hasSword)
                break;
        }
    }

    private bool TryGetEnemyHitData(Collider hit, out Health targetHealth, out EnemyAI enemy)
    {
        targetHealth = null;
        enemy = null;

        if (hit == null)
            return false;

        targetHealth = hit.GetComponent<Health>();
        if (targetHealth == null)
            targetHealth = hit.GetComponentInParent<Health>();

        if (targetHealth == null)
            return false;

        enemy = hit.GetComponent<EnemyAI>();
        if (enemy == null)
            enemy = hit.GetComponentInParent<EnemyAI>();

        return true;
    }

    private void ApplyNormalKnockback(EnemyAI enemy)
    {
        if (enemy == null) return;
        if (_knockbackStrength <= 0f) return;

        enemy.ApplyKnockback(transform.position, _knockbackStrength);
    }

    private IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(AttackInterval);
        _canAttack = true;
    }
    #endregion

    #region Death
    public bool TryPreventDeath()
    {
        return deathEffectHandler != null && deathEffectHandler.TryPreventDeath();
    }

    public void HandleDeath()
    {
        enabled = false;
        _velocity = 0f;

        CursorManager.Instance?.UnlockCursor();

        Debug.Log("PLAYER HANDLE DEATH CALLED");

        if (orbitPivot != null)
            orbitPivot.SetParent(null, true);

        CameraManager.Instance?.StartOrbit(orbitPivot);

        Debug.Log(orbitPivot != null ? "DeathPivot OK" : "DeathPivot IS NULL");
        Debug.Log(CameraManager.Instance != null ? "CameraManager OK" : "CameraManager IS NULL");
    }

    public Transform GetOrbitPivot()
    {
        return orbitPivot;
    }
    #endregion

    #region Cards / Stats
    public void RecalculateStats()
    {
        _stats.LoadBaseStats(playerData, ignorePermanentUpgrades, tutorialBaseDamage);

        ApplyOwnedCardsToStats();
        CopyStatsToFields();
        SyncEffectHandlers();
        ApplyHealthStatChanges();
    }

    private void ApplyBaseStats()
    {
        _stats.LoadBaseStats(playerData, ignorePermanentUpgrades, tutorialBaseDamage);
        CopyStatsToFields();
    }

    private void ApplyOwnedCardsToStats()
    {
        if (runCardInventory == null)
            return;

        foreach (OwnedCard ownedCard in runCardInventory.OwnedCards)
            PlayerCardStatApplier.ApplyCard(_stats, ownedCard);
    }

    private void CopyStatsToFields()
    {
        _maxHealth = _stats.maxHealth;
        _moveSpeed = _stats.moveSpeed;
        _rotationSpeed = _stats.rotationSpeed;
        _jumpPower = _stats.jumpPower;
        _maxJumps = _stats.maxJumps;
        _gravityMultiplier = _stats.gravityMultiplier;

        _attackRange = _stats.attackRange;
        _attackDamage = _stats.attackDamage;
        _attackSpeed = _stats.attackSpeed;

        _luck = _stats.luck;
        _knockbackStrength = _stats.knockbackStrength;
        _silverGainMultiplier = _stats.silverGainMultiplier;

        _critChance = _stats.critChance;
        _critDamageMultiplier = _stats.critDamageMultiplier;

        UpdateSwordScale();
    }

    private void ApplyHealthStatChanges()
    {
        if (_health != null)
            _health.SetMaxHealth(_maxHealth, true);
    }

    public void OnWaveCompleted()
    {
        runEffectHandler?.OnWaveCompleted();
    }

    public PlayerRewardEffectHandler GetRewardEffectHandler()
    {
        return rewardEffectHandler;
    }
    #endregion

    #region Stat Getters
    public int GetMaxHealth() { return _maxHealth; }

    public float GetMoveSpeed() { return _moveSpeed; }
    public float GetJumpPower() { return _jumpPower; }
    public int GetMaxJumps() { return _maxJumps; }

    public int GetAttackDamage() { return _attackDamage; }
    public float GetAttackSpeed() { return _attackSpeed; }
    public float GetAttackRange() { return _attackRange; }

    public int GetLuck() { return _luck; }
    public float GetKnockbackStrength() { return _knockbackStrength; }
    public float GetSilverGainMultiplier() { return _silverGainMultiplier; }

    public float GetCritChance() { return _stats.critChance; }
    public float GetCritDamageMultiplier() { return _stats.critDamageMultiplier; }
    public float GetCritKnockbackStrength() { return _stats.critKnockbackStrength; }

    public float GetWaveHealPercent() { return _stats.waveHealPercent; }
    public float GetLifestealPercent() { return _stats.lifestealPercent; }

    public int GetRewardRerolls()
    {
        return rewardEffectHandler != null ? rewardEffectHandler.RewardRerolls : 0;
    }

    public int GetOwnedCardCount()
    {
        return runCardInventory != null ? runCardInventory.OwnedCards.Count : 0;
    }
    #endregion
}