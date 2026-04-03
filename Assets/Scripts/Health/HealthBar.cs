using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider slider;
    public bool player;
    public Gradient gradient;
    public Image fill;

    public void Show() { gameObject.SetActive(true); }
    public void Hide() { gameObject.SetActive(false); }

    public void SetMaxHealth(int health)
    {
        slider.maxValue = health;
        slider.value = health;

        if (player)
            fill.color = gradient.Evaluate(1f);
    }

    public void SetHealth(int health)
    {
        slider.value = health;

        if (player)
            fill.color = gradient.Evaluate(slider.normalizedValue);
    }
}
