using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] public bool player;
    [SerializeField] private Gradient gradient;
    [SerializeField] private Image fill;
    [SerializeField] private TMP_Text healthText;

    private static readonly Color BurnPreviewColor = new(1f, 0.59f, 0.12f, 1f);
    private Image _burnPreview;
    private int _incomingBurnDamage;

    public void SetMaxHealth(int health)
    {
        slider.maxValue = health;
        slider.value = health;

        UpdateHealthText();

        if (player)
            fill.color = gradient.Evaluate(1f);
    }

    public void SetHealth(int health)
    {
        slider.value = health;

        UpdateHealthText();
        UpdateBurnPreview();

        if (player)
            fill.color = gradient.Evaluate(slider.normalizedValue);
    }

    public void SetBurnDamage(int incomingDamage)
    {
        if (player) return;

        _incomingBurnDamage = incomingDamage;
        UpdateBurnPreview();
    }

    private void UpdateBurnPreview()
    {
        if (_incomingBurnDamage <= 0 || slider.value <= 0f)
        {
            if (_burnPreview != null)
                _burnPreview.enabled = false;

            return;
        }

        if (_burnPreview == null)
        {
            RectTransform healthFill = slider.fillRect;
            if (healthFill == null) return;

            var previewObject = new GameObject("Incoming Burn Damage", typeof(RectTransform), typeof(Image));
            previewObject.layer = healthFill.gameObject.layer;
            _burnPreview = previewObject.GetComponent<Image>();
            _burnPreview.rectTransform.SetParent(healthFill, false);
            _burnPreview.color = BurnPreviewColor;
            _burnPreview.raycastTarget = false;
        }

        // The fill already shrinks with HP. Mark only the trailing part of that fill.
        float start = Mathf.Clamp01(1f - _incomingBurnDamage / slider.value);
        RectTransform preview = _burnPreview.rectTransform;
        preview.anchorMin = new Vector2(start, 0f);
        preview.anchorMax = Vector2.one;
        preview.offsetMin = Vector2.zero;
        preview.offsetMax = Vector2.zero;
        _burnPreview.enabled = true;
    }

    private void UpdateHealthText()
    {
        if (healthText == null) return;
        healthText.text = $"{Mathf.RoundToInt(slider.value)} / {Mathf.RoundToInt(slider.maxValue)}";
    }
}
