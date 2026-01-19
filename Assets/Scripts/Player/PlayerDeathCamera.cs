using UnityEngine;
using Unity.Cinemachine;

public class PlayerDeathCamera : MonoBehaviour
{
    public static PlayerDeathCamera Instance;

    [SerializeField] private CinemachineCamera deathCamera;
    [SerializeField] private float orbitSpeed = 25f;
    [SerializeField] private Vector3 cameraOffset = new Vector3(0f, 2.5f, -6f);

    private Transform pivot;
    private bool isOrbiting;

    private void Awake()
    {
        Instance = this;
        deathCamera.gameObject.SetActive(false);
    }

    public void StartOrbit(Vector3 deathPosition)
    {
        GameObject pivotObj = new GameObject("DeathCameraPivot");
        pivotObj.transform.position = deathPosition;

        pivot = pivotObj.transform;

        // Set initial offset
        pivot.position = deathPosition;
        deathCamera.Follow = pivot;
        deathCamera.LookAt = pivot;

        // IMPORTANT: set camera offset using local position
        deathCamera.transform.localPosition = cameraOffset;

        deathCamera.gameObject.SetActive(true);
        isOrbiting = true;
    }

    private void LateUpdate()
    {
        if (!isOrbiting || pivot == null) return;

        // Rotate the FOLLOW TARGET, not the camera
        pivot.Rotate(Vector3.up, orbitSpeed * Time.deltaTime);
    }
}



