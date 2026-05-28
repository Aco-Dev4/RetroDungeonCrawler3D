using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;

    [SerializeField] private CinemachineCamera orbitCamera;
    [SerializeField] private float orbitSpeed = 15f;
    [SerializeField] private Vector3 offset;

    private Transform orbitTarget;
    private bool _isOrbiting;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void StartOrbit(Transform pivot)
    {
        if (pivot == null) return;

        orbitTarget = pivot;

        orbitCamera.transform.position = orbitTarget.position + offset;
        orbitCamera.Follow = orbitTarget;
        orbitCamera.LookAt = orbitTarget;
        orbitCamera.Priority = 100;

        _isOrbiting = true;
    }

    public void StopOrbit()
    {
        _isOrbiting = false;

        if (orbitCamera != null)
            orbitCamera.Priority = 0;
    }

    private void LateUpdate()
    {
        if (!_isOrbiting || orbitTarget == null) return;

        orbitCamera.transform.RotateAround(orbitTarget.position, Vector3.up, orbitSpeed * Time.deltaTime);
    }
}