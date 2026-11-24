using UnityEngine;
using UnityEngine.Rendering;

public class OrcSlamEffects : MonoBehaviour
{
    [SerializeField] private Material warningMaterial;
    [SerializeField] private ParticleSystem impactPrefab;
    [SerializeField] private LayerMask groundLayer;

    private LineRenderer _warning;
    private Transform _attackOrigin;
    private Vector3 _scaledCenter;
    private Vector2 _halfExtents;

    public void ShowWarning(Transform attackOrigin, BoxCollider hitbox)
    {
        if (warningMaterial == null) return;

        _attackOrigin = attackOrigin;
        Vector3 scale = hitbox.transform.localScale;
        _scaledCenter = Vector3.Scale(hitbox.center, scale);
        _halfExtents = new Vector2(hitbox.size.x * Mathf.Abs(scale.x), hitbox.size.z * Mathf.Abs(scale.z)) * 0.5f;

        if (_warning == null)
        {
            var marker = new GameObject("Slam Warning");
            marker.transform.SetParent(transform, false);
            _warning = marker.AddComponent<LineRenderer>();
            _warning.sharedMaterial = warningMaterial;
            _warning.positionCount = 4;
            _warning.loop = true;
            _warning.widthMultiplier = 0.11f;
            _warning.numCornerVertices = 3;
            _warning.shadowCastingMode = ShadowCastingMode.Off;
            _warning.receiveShadows = false;
        }

        UpdateWarningPosition();
    }

    private void LateUpdate()
    {
        if (_attackOrigin != null)
            UpdateWarningPosition();
    }

    public void PlayImpact()
    {
        if (impactPrefab != null && TryGetGround(out RaycastHit ground))
        {
            Vector3 forward = Vector3.ProjectOnPlane(_attackOrigin.forward, ground.normal);
            Quaternion rotation = forward.sqrMagnitude > 0.01f
                ? Quaternion.LookRotation(forward, ground.normal)
                : Quaternion.FromToRotation(Vector3.up, ground.normal);
            ParticleSystem impact = Instantiate(impactPrefab, ground.point + ground.normal * 0.04f, rotation);
            var shape = impact.shape;
            // The emitter is rotated upward. Its local XY spans the hitbox's horizontal footprint.
            shape.scale = new Vector3(_halfExtents.x * 2f, _halfExtents.y * 2f, 0.08f);
            impact.Play();
        }

        HideWarning();
    }

    public void HideWarning()
    {
        _attackOrigin = null;
        if (_warning != null)
            _warning.enabled = false;
    }

    private void OnDisable()
    {
        HideWarning();
    }

    private void UpdateWarningPosition()
    {
        _warning.enabled = TryGetGround(out RaycastHit ground);
        if (!_warning.enabled) return;

        Vector3 forward = Vector3.ProjectOnPlane(_attackOrigin.forward, ground.normal).normalized;
        if (forward.sqrMagnitude < 0.01f)
        {
            _warning.enabled = false;
            return;
        }
        Vector3 front = forward * _halfExtents.y;
        Vector3 right = Vector3.Cross(ground.normal, forward) * _halfExtents.x;
        Vector3 center = ground.point + ground.normal * 0.05f;
        _warning.SetPosition(0, center + front + right);
        _warning.SetPosition(1, center + front - right);
        _warning.SetPosition(2, center - front - right);
        _warning.SetPosition(3, center - front + right);
    }

    private bool TryGetGround(out RaycastHit hit)
    {
        hit = default;
        if (_attackOrigin == null) return false;

        Vector3 hitboxCenter = _attackOrigin.position + _attackOrigin.rotation * _scaledCenter;
        return Physics.Raycast(hitboxCenter + Vector3.up * 2f, Vector3.down, out hit, 5f, groundLayer,
            QueryTriggerInteraction.Ignore);
    }
}
