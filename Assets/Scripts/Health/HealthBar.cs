using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private bool player;
    [SerializeField] private Gradient gradient;
    [SerializeField] private Image fill;
    [SerializeField] private TMP_Text healthText;

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

        if (player)
            fill.color = gradient.Evaluate(slider.normalizedValue);
    }

    private void UpdateHealthText()
    {
        if (healthText == null) return;
        healthText.text = $"{Mathf.RoundToInt(slider.value)} / {Mathf.RoundToInt(slider.maxValue)}";
    }
}
