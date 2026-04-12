using UnityEngine;

public class UITitlePulse : MonoBehaviour
{
    [Header("Scale Settings")]
    [SerializeField] private float minScale = 1f;
    [SerializeField] private float maxScale = 1.2f;

    [Header("Timing")]
    [SerializeField] private float duration = 2f; // full loop time

    private Vector3 _baseScale;

    private void Awake()
    {
        _baseScale = transform.localScale;
    }

    private void Update()
    {
        if (duration <= 0f) return;

        float time = Time.time / duration;

        // sin goes from -1 → 1, we convert it to 0 → 1
        float t = (Mathf.Sin(time * Mathf.PI * 2f) + 1f) * 0.5f;

        float scale = Mathf.Lerp(minScale, maxScale, t);

        transform.localScale = _baseScale * scale;
    }
}
