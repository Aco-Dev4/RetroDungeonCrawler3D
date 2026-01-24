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
        // If Init() was not called (e.g. player), fall back safely
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

    public void TakeDamage(int amount, GameObject damageSource)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0); // Making sure health cannot be less than 0
        if (healthBar != null)
            healthBar.SetHealth(currentHealth);
            
        _vignette?.SetHealthNormalized((float)currentHealth / maxHealth);
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void IncreaseMaxHealth(int amount)
    {
        maxHealth += amount;
        currentHealth += amount; // optional but feels good
        healthBar.SetMaxHealth(maxHealth);
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
