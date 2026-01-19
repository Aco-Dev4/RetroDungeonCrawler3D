using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;

    [SerializeField] private CinemachineCamera deathCamera;
    [SerializeField] private float orbitSpeed = 15f;
    [SerializeField] private Vector3 offset;

    private Transform orbitTarget;
    private bool _isOrbiting = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void StartDeathOrbit(Transform pivot)
    {
        Debug.Log("START DEATH ORBIT");
        orbitTarget = pivot;

        Vector3 pos = orbitTarget.transform.position;
        pos = orbitTarget.position + offset;
        deathCamera.transform.position = pos;

        deathCamera.Follow = orbitTarget;
        deathCamera.LookAt = orbitTarget;
        deathCamera.Priority = 100;

        _isOrbiting = true;
    }

    private void LateUpdate()
    {
        if (!_isOrbiting) return;

        deathCamera.transform.RotateAround(orbitTarget.position, Vector3.up, orbitSpeed * Time.deltaTime);
    }

}

