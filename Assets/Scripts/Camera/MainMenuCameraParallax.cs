using UnityEngine;
using UnityEngine.InputSystem;

public class MainMenuCameraParallax : MonoBehaviour
{
    [Header("Rotation Amount")]
    [SerializeField] private float maxPitch = 3f;
    [SerializeField] private float maxYaw = 4f;

    [Header("Smoothness")]
    [SerializeField] private float smoothTime = 6f;

    private Quaternion _startRotation;
    private Quaternion _targetRotation;

    private void Awake()
    {
        _startRotation = transform.rotation;
        _targetRotation = _startRotation;
    }

    private void Update()
    {
        if (Mouse.current == null) return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();

        float screenX = Screen.width * 0.5f;
        float screenY = Screen.height * 0.5f;

        float normalizedX = (mousePosition.x - screenX) / screenX;
        float normalizedY = (mousePosition.y - screenY) / screenY;

        normalizedX = Mathf.Clamp(normalizedX, -1f, 1f);
        normalizedY = Mathf.Clamp(normalizedY, -1f, 1f);

        float yaw = normalizedX * maxYaw;
        float pitch = -normalizedY * maxPitch;

        _targetRotation = _startRotation * Quaternion.Euler(pitch, yaw, 0f);

        transform.rotation = Quaternion.Slerp(transform.rotation, _targetRotation, smoothTime * Time.deltaTime);
    }
}