using UnityEngine;

public class UpgradeTableArrow : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform target;

    [Header("Settings")]
    [SerializeField] private float heightOffset = 3f;
    [SerializeField] private float rotationSpeed = 10f;

    private bool _isActive;

    private void Update()
    {
        if (!_isActive || player == null || target == null)
            return;

        transform.position = player.position + Vector3.up * heightOffset;

        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    public void ShowArrow()
    {
        _isActive = true;
        gameObject.SetActive(true);
    }

    public void HideArrow()
    {
        _isActive = false;
        gameObject.SetActive(false);
    }
}