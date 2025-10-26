using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    #region Variables: Componenets
    
    private CharacterController _characterController;
    private Camera _mainCamera;
    
    #endregion
    #region Variables: Movement
    
    private Vector2 _input;
    private Vector3 _direction;
    [SerializeField] private float speed;
    
    #endregion
    #region Variables: Rotation
    
    [SerializeField] private float rotationSpeed = 500f;
    
    #endregion
    #region Variables: Gravity

    private float _gravity = -9.81f;
    [SerializeField] private float gravityMultiplier = 3.0f;
    private float _velocity;
    
    #endregion
    #region Variables: Jump
    
    [SerializeField] private float jumpPower;
    private int _numberOfJumps;
    [SerializeField] private int maxNumberOfJumps = 2;
    
    #endregion
    #region Variables: Slide

    private bool _isSliding;
    private Vector3 _slopeSlideVelocity;
    
    #endregion
    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _mainCamera = Camera.main;
    }

    private void Update()
    {
        ApplyRotation();
        ApplyGravity();
        ApplySlide();
        ApplyMovement();
    }

    private void ApplyGravity()
    {
        if (_isSliding) return;
        
        if (IsGrounded() && _velocity < 0.0f)
        {
            _velocity = -1.0f;
        }
        else
        {
            _velocity += _gravity * gravityMultiplier *  Time.deltaTime;
        }
        
        _direction.y = _velocity;
    }

    private void ApplyRotation()
    {
        if (_input.sqrMagnitude == 0) return;
        
        _direction = Quaternion.Euler(0.0f, _mainCamera.transform.eulerAngles.y, 0.0f) * new Vector3(_input.x, 0.0f, _input.y);
        var targetRotation = Quaternion.LookRotation(_direction, Vector3.up);
        
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void ApplyMovement()
    {
        _characterController.Move(_direction * (speed * Time.deltaTime));
    }

    private void ApplySlide()
    {
        // Raycast straight down to detect slope under feet
        if (Physics.Raycast(transform.position + new Vector3(0f, 0.1f, 0f), Vector3.down, out RaycastHit hitInfo, 2f))
        {
            float slopeAngle = Vector3.Angle(hitInfo.normal, Vector3.up);

            if (slopeAngle > _characterController.slopeLimit)
            {
                _isSliding = true;

                // Get direction down the slope
                Vector3 slopeDirection = Vector3.ProjectOnPlane(Vector3.down, hitInfo.normal).normalized;

                // Apply movement along slope
                _characterController.Move(slopeDirection * (speed * Time.deltaTime));

                return;
            }
        }

        _isSliding = false;
    }

    
    public void Move(InputAction.CallbackContext context)
    {
        _input = context.ReadValue<Vector2>();
        _direction = new Vector3(_input.x, 0.0f, _input.y);
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        if (!IsGrounded() && _numberOfJumps >= maxNumberOfJumps) return;
        if (_isSliding) return;
        if (_numberOfJumps == 0) StartCoroutine(WaitForLanding());
        
        _numberOfJumps++;
        _velocity = jumpPower;
    }

    private IEnumerator WaitForLanding()
    {
        yield return new WaitUntil(() => !IsGrounded());
        yield return new WaitUntil(IsGrounded);
        
        _numberOfJumps = 0;
    }
    
    private bool IsGrounded() => _characterController.isGrounded;
}
