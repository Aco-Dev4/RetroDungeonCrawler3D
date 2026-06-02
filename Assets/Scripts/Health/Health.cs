using UnityEngine;

public class Health : MonoBehaviour
{
    #region Health
    private int maxHealth;
    public int currentHealth;
    public HealthBar healthBar;
    private bool _isInvulnerable;
    #endregion

    #region Runtime
    private PlayerVignetteController _vignette;
    private bool _hasTakenDamage;
    #endregion

    #region Unity
    private void Awake()
    {
        _vignette = GetComponent<PlayerVignetteController>();

        if (maxHealth <= 0)
            InitFromCurrentHealth();
    }
    #endregion

    #region Setup
    public void Init(int maxHealthValue)
    {
        maxHealth = maxHealthValue;
        currentHealth = maxHealth;

        RefreshHealthVisuals();

        _vignette?.SetHealthNormalized(1f);

        if (healthBar != null && !healthBar.player)
            healthBar.gameObject.SetActive(false);
    }

    private void InitFromCurrentHealth()
    {
        maxHealth = currentHealth;

        if (healthBar != null)
            healthBar.SetMaxHealth(maxHealth);
    }
    #endregion

    #region Public Getters
    public int GetMaxHealth()
    {
        return maxHealth;
    }
    #endregion

    #region Health Changes
    public void SetMaxHealth(int newMaxHealth, bool healByDifference)
    {
        int difference = newMaxHealth - maxHealth;

        maxHealth = newMaxHealth;

        if (healByDifference && difference > 0)
            currentHealth = Mathf.Min(currentHealth + difference, maxHealth);
        else
            currentHealth = Mathf.Min(currentHealth, maxHealth);

        RefreshHealthVisuals();
    }

    public void IncreaseMaxHealth(int amount)
    {
        if (amount <= 0) return;

        maxHealth += amount;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);

        RefreshHealthVisuals();
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;
        if (currentHealth <= 0) return;

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);

        RefreshHealthVisuals();
    }

    public void HealPercent(float percent)
    {
        if (percent <= 0f) return;

        Heal(Mathf.RoundToInt(maxHealth * percent));
    }

    public void TakeDamage(int amount, GameObject damageSource)
    {
        if (_isInvulnerable) return;
        if (amount <= 0) return;
        if (currentHealth <= 0) return;

        currentHealth = Mathf.Max(currentHealth - amount, 0);

        ShowEnemyHealthBarAfterFirstHit();
        RefreshHealthVisuals();

        if (currentHealth <= 0)
            Die();
    }
    #endregion

    #region Visuals
    private void RefreshHealthVisuals()
    {
        if (healthBar != null)
        {
            healthBar.SetMaxHealth(maxHealth);
            healthBar.SetHealth(currentHealth);
        }

        if (maxHealth > 0)
            _vignette?.SetHealthNormalized((float)currentHealth / maxHealth);
    }

    private void ShowEnemyHealthBarAfterFirstHit()
    {
        if (_hasTakenDamage) return;

        _hasTakenDamage = true;

        if (healthBar != null && !healthBar.player)
            healthBar.gameObject.SetActive(true);
    }
    #endregion

    #region Death
    private void Die()
    {
        PlayerController player = GetComponent<PlayerController>();

        if (player != null && player.TryPreventDeath())
            return;

        if (TryGetComponent(out EnemyAI enemy))
            enemy.HandleDeath();
        else if (player != null)
            HandlePlayerDeath(player);

        Destroy(gameObject);
    }

    private void HandlePlayerDeath(PlayerController player)
    {
        player.HandleDeath();

        GameManager.Instance?.SetState(GameState.GameOver);
        UIManager.Instance?.ShowGameOver();
    }

    public void SetInvulnerable(bool isInvulnerable)
    {
        _isInvulnerable = isInvulnerable;
    }

    public void Revive(int reviveHealth)
    {
        currentHealth = Mathf.Clamp(reviveHealth, 1, maxHealth);
        RefreshHealthVisuals();
    }
    #endregion
}