using UnityEngine;

public class Health : MonoBehaviour
{
    #region Variables: Health
    private int maxHealth;
    public int currentHealth;
    public HealthBar healthBar;
    private PlayerVignetteController _vignette;
    #endregion

    private void Awake()
    {
        _vignette = GetComponent<PlayerVignetteController>();

        if (maxHealth <= 0)
        {
            maxHealth = currentHealth;
            if (healthBar != null)
                healthBar.SetMaxHealth(maxHealth);
        }
    }

    public void Init(int maxHealthValue)
    {
        maxHealth = maxHealthValue;
        currentHealth = maxHealth;

        if (healthBar != null)
            healthBar.SetMaxHealth(maxHealth);

        _vignette?.SetHealthNormalized(1f);
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }

    public void SetMaxHealth(int newMaxHealth, bool healByDifference)
    {
        int difference = newMaxHealth - maxHealth;
        maxHealth = newMaxHealth;

        if (healByDifference && difference > 0)
            currentHealth = Mathf.Min(currentHealth + difference, maxHealth);
        else
            currentHealth = Mathf.Min(currentHealth, maxHealth);

        if (healthBar != null)
        {
            healthBar.SetMaxHealth(maxHealth);
            healthBar.SetHealth(currentHealth);
        }

        _vignette?.SetHealthNormalized((float)currentHealth / maxHealth);
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;
        if (currentHealth <= 0) return;

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);

        if (healthBar != null)
            healthBar.SetHealth(currentHealth);

        _vignette?.SetHealthNormalized((float)currentHealth / maxHealth);
    }

    public void HealPercent(float percent)
    {
        if (percent <= 0f) return;
        Heal(Mathf.RoundToInt(maxHealth * percent));
    }

    public void TakeDamage(int amount, GameObject damageSource)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);

        if (healthBar != null)
            healthBar.SetHealth(currentHealth);

        _vignette?.SetHealthNormalized((float)currentHealth / maxHealth);

        if (currentHealth <= 0)
            Die();
    }

    public void IncreaseMaxHealth(int amount)
    {
        maxHealth += amount;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);

        if (healthBar != null)
        {
            healthBar.SetMaxHealth(maxHealth);
            healthBar.SetHealth(currentHealth);
        }

        _vignette?.SetHealthNormalized((float)currentHealth / maxHealth);
    }

    private void Die()
    {
        if (TryGetComponent(out EnemyAI enemy))
        {
            enemy.HandleDeath();
        }
        else if (TryGetComponent(out PlayerController player))
        {
            player.HandleDeath();
            GameManager.Instance?.SetState(GameState.GameOver);
            UIManager.Instance?.ShowGameOver();
        }

        Destroy(gameObject);
    }
}