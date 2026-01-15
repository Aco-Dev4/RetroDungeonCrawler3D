using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    #region Components
    private CharacterController _characterController;
    private Camera _mainCamera;
    [SerializeField] private PlayerData playerData;
    [SerializeField] private Animator _animator;
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
    [SerializeField] private AnimationClip attackClip;
    private bool _canAttack = true;
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
    #endregion

    private float AttackInterval => 1f / _attackSpeed;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _mainCamera = Camera.main;

        ApplyBaseStats();

        // Health init
        Health health = GetComponent<Health>();
        if (health != null)
            health.Init(_maxHealth);
    }

    private void ApplyBaseStats()
    {
        _maxHealth = playerData.startingHealth;

        _moveSpeed = playerData.moveSpeed;
        _rotationSpeed = playerData.rotationSpeed;
        _jumpPower = playerData.jumpPower;
        _maxJumps = playerData.maxJumps;
        _gravityMultiplier = playerData.gravityMultiplier;

        _attackRange = playerData.attackRange;
        _attackDamage = playerData.attackDamage;
        _attackSpeed = playerData.attackSpeed;
    }

    private void Update()
    {
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

        if (attackClip != null)
        {
            _animator.SetFloat("AttackSpeed", _attackSpeed);
        }

        _animator.SetTrigger("Attack");
        StartCoroutine(AttackCooldown());
    }
    #endregion

    #region Attack Logic
    public void ApplyAttackDamage()
    {
        Vector3 attackOrigin = transform.position + transform.forward * (_attackRange * 0.5f);
        Collider[] hits = Physics.OverlapSphere(attackOrigin, _attackRange * 0.5f, enemyLayer);

        foreach (Collider hit in hits)
        {
            Health target = hit.GetComponent<Health>();
            if (target != null)
            {
                target.TakeDamage(_attackDamage, gameObject);
                break;
            }
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
}

