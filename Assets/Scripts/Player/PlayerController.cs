using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    #region Components
    private PlayerRuntimeStats _stats = new();
    private CharacterController _characterController;
    private Camera _mainCamera;
    [SerializeField] private PlayerData playerData;
    [SerializeField] private Animator _animator;
    [SerializeField] private RunCardInventory runCardInventory;
    [SerializeField] private PlayerCombatEffectHandler combatEffectHandler;
    [SerializeField] private PlayerRunEffectHandler runEffectHandler;
    [SerializeField] private PlayerDeathEffectHandler deathEffectHandler;
    [SerializeField] private PlayerRewardEffectHandler rewardEffectHandler;

    [Header("Tutorial Override")]
    [SerializeField] private bool ignorePermanentUpgrades;
    [SerializeField] private int tutorialBaseDamage = 15;
    #endregion

    #region Movement
    private Vector2 _input;
    private Vector3 _direction;
    #endregion

    #region Gravity
    private float _gravity = -9.81f;
    private float _velocity;
    #endregion

    #region Jump
    private int _numberOfJumps;
    #endregion

    #region Slide
    private bool _isSliding;
    #endregion

    #region Attack
    [SerializeField] private LayerMask enemyLayer;

    [Header("Punch")]
    [SerializeField] private AnimationClip punchAttackClip;

    [Header("Sword")]
    [SerializeField] private GameObject swordObject;
    [SerializeField] private AnimationClip swordAttackClip;
    [SerializeField] private float swordRangeBonus = 1f;
    [SerializeField] private int swordDamageBonus = 5;

    private bool _canAttack = true;
    private bool _hasSword;
    #endregion

    #region Runtime Stats (modifiable by cards)
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
    #endregion

    private float AttackInterval => 1f / _attackSpeed;
    [SerializeField] private Transform orbitPivot;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _mainCamera = Camera.main;
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

        ApplyBaseStats();

        rewardEffectHandler?.SetRerolls(_stats.rewardRerolls);
        deathEffectHandler?.SetRevives(_stats.guardianAngelRevives);

        _hasSword = !ignorePermanentUpgrades && GameDataManager.Instance != null && GameDataManager.Instance.HasSword();

        if (swordObject != null)
            swordObject.SetActive(_hasSword);

        // Health init
        Health health = GetComponent<Health>();
        if (health != null)
            health.Init(_maxHealth);
    }

    void Start()
    {
        CursorManager.Instance.LockCursor();
    }

    public void OnWaveCompleted()
    {
        runEffectHandler?.OnWaveCompleted();
    }

    public bool TryPreventDeath()
    {
        return deathEffectHandler != null && deathEffectHandler.TryPreventDeath();
    }

    public PlayerRewardEffectHandler GetRewardEffectHandler()
    {
        return rewardEffectHandler;
    }

    public int GetRewardRerolls()
    {
        return rewardEffectHandler != null ? rewardEffectHandler.RewardRerolls : 0;
    }

    private void ApplyBaseStats()
    {
        _stats.LoadBaseStats(playerData, ignorePermanentUpgrades, tutorialBaseDamage);
        CopyStatsToFields();
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
    }

    private void Update()
    {
        if (GameManager.Instance.State != GameState.Playing)
            return;
        ApplyRotation();
        ApplyGravity();
        ApplySlide();
        ApplyMovement();
    }

    #region Movement Logic
    private void ApplyGravity()
    {
        if (_isSliding) return;

        if (IsGrounded() && _velocity < 0f)
            _velocity = -1f;
        else
            _velocity += _gravity * _gravityMultiplier * Time.deltaTime;

        _direction.y = _velocity;
        _animator.SetBool("isGrounded", IsGrounded());
        _animator.SetFloat("VerticalVel", _velocity);
    }

    private void ApplyRotation()
    {
        if (_input.sqrMagnitude == 0) return;

        _direction = Quaternion.Euler(0f, _mainCamera.transform.eulerAngles.y, 0f)
                   * new Vector3(_input.x, 0f, _input.y);

        Quaternion targetRotation = Quaternion.LookRotation(_direction, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
    }

    private void ApplyMovement()
    {
        _characterController.Move(_direction * (_moveSpeed * Time.deltaTime));
        float moveAmount = new Vector3(_direction.x, 0f, _direction.z).magnitude;
        _animator.SetFloat("Speed", moveAmount);
    }

    private void ApplySlide()
    {
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
        if (!IsGrounded() && _numberOfJumps >= _maxJumps) return;
        if (_isSliding) return;

        if (_numberOfJumps == 0)
            StartCoroutine(WaitForLanding());

        _numberOfJumps++;
        _velocity = _jumpPower;
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (!context.started || !_canAttack) return;

        _canAttack = false;

        _animator.SetFloat("AttackSpeed", _attackSpeed);

        if (_hasSword)
            _animator.SetTrigger("Swing");
        else
            _animator.SetTrigger("Attack");

        StartCoroutine(AttackCooldown());
    }

    #endregion

    #region Attack Logic
    public void ApplyAttackDamage()
    {
        float finalRange = PlayerDamageCalculator.GetAttackRange(_stats, _hasSword, swordRangeBonus);
        Health playerHealth = GetComponent<Health>();
        PlayerAttackResult attackResult = PlayerDamageCalculator.GetAttackResult(_stats, _hasSword, swordDamageBonus, playerHealth);
        int finalDamage = attackResult.damage;

        if (attackResult.isCrit)
            Debug.Log($"CRIT! {finalDamage} damage");

        Vector3 attackOrigin = transform.position + transform.forward * (finalRange * 0.5f);
        Collider[] hits = Physics.OverlapSphere(attackOrigin, finalRange * 0.5f, enemyLayer);

        for (int i = 0; i < hits.Length; i++)
        {
            Collider hit = hits[i];

            Health target = hit.GetComponent<Health>();
            if (target == null)
                target = hit.GetComponentInParent<Health>();

            if (target == null)
                continue;

            EnemyAI enemy = hit.GetComponent<EnemyAI>();
            if (enemy == null)
                enemy = hit.GetComponentInParent<EnemyAI>();

            target.TakeDamage(finalDamage, gameObject);

            combatEffectHandler?.OnDamageDealt(attackResult, target, enemy);

            if (enemy != null && _knockbackStrength > 0f)
                enemy.ApplyKnockback(transform.position, _knockbackStrength);

            if (!_hasSword)
                break;
        }
    }

    private IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(AttackInterval);
        _canAttack = true;
    }
    #endregion

    private IEnumerator WaitForLanding()
    {
        yield return new WaitUntil(() => !IsGrounded());
        yield return new WaitUntil(IsGrounded);
        _numberOfJumps = 0;
    }

    private bool IsGrounded() => _characterController.isGrounded;

    #region Death
    public void HandleDeath()
    {
        // Disable player control
        enabled = false;

        // Stop movement
        _velocity = 0f;

        // Unlock cursor
        CursorManager.Instance.UnlockCursor();


        Debug.Log("PLAYER HANDLE DEATH CALLED");
        // Tell camera to enter death mode
        orbitPivot.SetParent(null, true);
        CameraManager.Instance.StartOrbit(orbitPivot);

        Debug.Log(orbitPivot != null ? "DeathPivot OK" : "DeathPivot IS NULL");
        Debug.Log(CameraManager.Instance != null ? "CameraManager OK" : "CameraManager IS NULL");
    }

    public Transform GetOrbitPivot()
    {
        return orbitPivot;
    }
    #endregion

    #region Cards
    public void RecalculateStats()
    {
        Health health = GetComponent<Health>();

        _stats.LoadBaseStats(playerData, ignorePermanentUpgrades, tutorialBaseDamage);

        if (runCardInventory != null)
        {
            foreach (OwnedCard ownedCard in runCardInventory.OwnedCards)
                PlayerCardStatApplier.ApplyCard(_stats, ownedCard);
        }

        CopyStatsToFields();

        rewardEffectHandler?.SetRerolls(_stats.rewardRerolls);
        deathEffectHandler?.SetRevives(_stats.guardianAngelRevives);

        if (health != null)
            health.SetMaxHealth(_maxHealth, true);
    }

    public float GetMoveSpeed() { return _moveSpeed; }
    public float GetJumpPower() { return _jumpPower; }
    public int GetMaxJumps() { return _maxJumps; }
    public int GetAttackDamage() { return _attackDamage; }
    public float GetAttackSpeed() { return _attackSpeed; }
    public float GetAttackRange() { return _attackRange; }
    public int GetLuck() { return _luck; }
    public float GetKnockbackStrength() { return _knockbackStrength; }
    public int GetOwnedCardCount() { return runCardInventory != null ? runCardInventory.OwnedCards.Count : 0; }
    public float GetSilverGainMultiplier() { return _silverGainMultiplier; }
    public float GetWaveHealPercent() { return _stats.waveHealPercent; }
    public float GetLifestealPercent() { return _stats.lifestealPercent; }
    public int GetMaxHealth() { return _maxHealth; }
    public float GetCritChance() { return _stats.critChance; }
    public float GetCritDamageMultiplier() { return _stats.critDamageMultiplier; }
    public float GetCritKnockbackStrength() { return _stats.critKnockbackStrength; }
    #endregion
}

