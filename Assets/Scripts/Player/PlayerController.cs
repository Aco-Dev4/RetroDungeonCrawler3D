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
    [SerializeField] private Animator _animator;
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
    #endregion
    
    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _mainCamera = Camera.main; // Needs the TAG MainCamera in unity
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
        
        if (IsGrounded() && _velocity < 0.0f) // If he is standing still he has a base velocity of -1
        {
            _velocity = -1.0f;
        }
        else // If not his velocity will be calculated by our gravity
        {
            _velocity += _gravity * gravityMultiplier *  Time.deltaTime;
        }
        
        _direction.y = _velocity; // Apply velocity to position Y
        _animator.SetBool("isGrounded", IsGrounded());
        _animator.SetFloat("VerticalVel", _velocity);
    }

    private void ApplyRotation()
    {
        if (_input.sqrMagnitude == 0) return;
        
        _direction = Quaternion.Euler(0.0f, _mainCamera.transform.eulerAngles.y, 0.0f) * new Vector3(_input.x, 0.0f, _input.y); // The direction he is pointing in will be the same as the main cameras with added input from movement
        var targetRotation = Quaternion.LookRotation(_direction, Vector3.up);
        
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime); // Making him smoothly look in the direction he is walking in
    }

    private void ApplyMovement()
    {
        _characterController.Move(_direction * (speed * Time.deltaTime)); // Movement in his direction with his speed
        float moveAmount = new Vector3(_direction.x, 0f, _direction.z).magnitude;
        _animator.SetFloat("Speed", moveAmount);
    }

    private void ApplySlide()
    {
        if (Physics.Raycast(transform.position + new Vector3(0f, 0.1f, 0f), Vector3.down, out RaycastHit hitInfo, 2f)) // Raycast straight down to detect slope under feet
        {
            float slopeAngle = Vector3.Angle(hitInfo.normal, Vector3.up); // Getting the slopes angle

            if (slopeAngle > _characterController.slopeLimit)
            {
                _isSliding = true;
                
                Vector3 slopeDirection = Vector3.ProjectOnPlane(Vector3.down, hitInfo.normal).normalized; // Get direction down the slope
                
                _characterController.Move(slopeDirection * (speed * Time.deltaTime)); // Apply movement along slope

                return;
            }
        }

        _isSliding = false;
    }

    
    public void Move(InputAction.CallbackContext context)
    {
        _input = context.ReadValue<Vector2>();
        _direction = new Vector3(_input.x, 0.0f, _input.y); // Seting the X and Z to be the same the player is pressing
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (!context.started) return; // If he didn't just jumped...
        if (!IsGrounded() && _numberOfJumps >= maxNumberOfJumps) return; // And if he is on the ground or he has jumps left...
        if (_isSliding) return; // And if he is not sliding...
        if (_numberOfJumps == 0) StartCoroutine(WaitForLanding()); // After the first jump wait for landing
        
        _numberOfJumps++; // Add jump
        _velocity = jumpPower; // Add velocity to character
    }

    private IEnumerator WaitForLanding()
    {
        yield return new WaitUntil(() => !IsGrounded()); // Checking if he is really off the ground
        yield return new WaitUntil(IsGrounded); // Checking when he touches the ground
        
        _numberOfJumps = 0; // Reset jumps
    }
    
    private bool IsGrounded() => _characterController.isGrounded; // Check if he is grounded
}
