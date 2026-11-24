using UnityEngine;

public sealed class RewardChestLight : MonoBehaviour
{
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int BaseMapId = Shader.PropertyToID("_BaseMap");

    [SerializeField] private MeshRenderer lightFan;
    [SerializeField] private MeshRenderer openingGlow;

    [Header("Closed")]
    [SerializeField] private Vector3 glowCenter = new(-0.067f, 0.52f, -0.62f);
    [SerializeField] private Vector2 glowSize = new(0.66f, 0.46f);
    [SerializeField] private Color glowColor = new(1f, 0.96f, 0.75f, 0.56f);

    [Header("Open")]
    [SerializeField] private Vector3 fanBase = new(-0.067f, 0.30f, -0.26f);
    [SerializeField] private Vector2 fanSize = new(3.8f, 2.8f);
    [SerializeField] private Texture2D fanTexture;
    [SerializeField] private Texture2D openingTexture;
    [SerializeField, Min(1f)] private float fullyOpenAngle = 66.8f;
    [SerializeField, Min(0.1f)] private float glowChangeSpeed = 6f;

    private MaterialPropertyBlock _properties;
    private Transform _camera;
    private Transform _lid;
    private Quaternion _closedLidRotation;
    private float _openness;

    private void Awake()
    {
        _properties = new MaterialPropertyBlock();
        openingGlow.enabled = true;

        Camera camera = Camera.main;
        if (camera != null)
            _camera = camera.transform;

        SetGlow(0f);
        FaceCamera();
    }

    public void FollowLid(Transform lid)
    {
        if (lid == null) return;

        _lid = lid;
        _closedLidRotation = lid.localRotation;
    }

    private void LateUpdate()
    {
        if (_lid != null)
        {
            float lidAngle = Quaternion.Angle(_closedLidRotation, _lid.localRotation);
            float target = Mathf.Clamp01(lidAngle / fullyOpenAngle);
            float next = Mathf.MoveTowards(_openness, target, glowChangeSpeed * Time.deltaTime);
            if (next != _openness)
            {
                _openness = next;
                SetGlow(_openness);
            }
        }

        FaceCamera();
    }

    private void SetGlow(float openness)
    {
        float amount = Mathf.SmoothStep(0f, 1f, openness);
        Vector3 openCenter = fanBase + Vector3.up * (fanSize.y * 0.5f);
        Vector3 center = Vector3.Lerp(glowCenter, openCenter, amount);
        Vector2 size = Vector2.Lerp(glowSize, fanSize, amount);
        Color tint = Color.Lerp(glowColor, Color.white, amount);
        SetFanAppearance(center, size, tint, fanTexture);

        Color insideTint = new Color(1f, 1f, 1f, Mathf.Lerp(0.035f, 1f, amount));
        SetAppearance(openingGlow, insideTint, openingTexture);
    }

    public void Hide()
    {
        lightFan.enabled = false;
        openingGlow.enabled = false;
        enabled = false;
    }

    private void SetFanAppearance(Vector3 center, Vector2 size, Color color, Texture texture)
    {
        lightFan.transform.localPosition = center;
        lightFan.transform.localScale = new Vector3(size.x, size.y, 1f);
        SetAppearance(lightFan, color, texture);
    }

    private void SetAppearance(MeshRenderer renderer, Color color, Texture texture)
    {
        _properties.Clear();
        _properties.SetColor(BaseColorId, color);
        if (texture != null)
            _properties.SetTexture(BaseMapId, texture);
        renderer.SetPropertyBlock(_properties);
    }

    private void FaceCamera()
    {
        if (_camera == null) return;

        Vector3 towardCamera = _camera.position - lightFan.transform.position;
        towardCamera.y = 0f;
        if (towardCamera.sqrMagnitude > 0.001f)
            lightFan.transform.rotation = Quaternion.LookRotation(towardCamera);
    }
}

