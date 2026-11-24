using UnityEngine;

public class Health : MonoBehaviour
{
    #region Health
    private int maxHealth;
    public int currentHealth;
    public HealthBar healthBar;
    [SerializeField] private HealthFeedbackSettings feedbackSettings;
    private bool _isInvulnerable;
    #endregion

    #region Runtime
    private PlayerVignetteController _vignette;
    private HealthFeedback _feedback;
    private BurnEffect _burn;
    private bool _hasTakenDamage;
    #endregion

    #region Unity
    private void Awake()
    {
        _vignette = GetComponent<PlayerVignetteController>();
        if (feedbackSettings != null)
            _feedback = new HealthFeedback(this, feedbackSettings);

        if (maxHealth <= 0)
            InitFromCurrentHealth();
    }
    private void OnDisable()
    {
        _feedback?.StopDamageFlash();
        _burn?.Stop();
    }
    #endregion

    #region Setup
    public void Init(int maxHealthValue)
    {
        _burn?.Stop();
        _hasTakenDamage = false;
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

        int healthBefore = currentHealth;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);

        RefreshHealthVisuals();
        if (currentHealth > healthBefore)
            _feedback?.PlayHeal();
    }

    public void HealPercent(float percent)
    {
        if (percent <= 0f) return;

        Heal(Mathf.RoundToInt(maxHealth * percent));
    }

    public void TakeDamage(int amount, GameObject damageSource)
    {
        ApplyDamage(amount, damageSource, playHitSound: true);
    }

    public void ApplyBurn(BurnSettings settings, int totalDamage, float tickRateMultiplier = 1f, GameObject damageSource = null)
    {
        if (!isActiveAndEnabled || _isInvulnerable || currentHealth <= 0) return;
        if (settings == null || totalDamage <= 0) return;

        _burn ??= new BurnEffect(this);
        _burn.Apply(settings, totalDamage, tickRateMultiplier, damageSource);
    }

    internal void TakeBurnDamage(int amount, GameObject damageSource)
    {
        ApplyDamage(amount, damageSource, playHitSound: false);
    }

    private void ApplyDamage(int amount, GameObject damageSource, bool playHitSound)
    {
        if (_isInvulnerable) return;
        if (amount <= 0) return;
        if (currentHealth <= 0) return;

        currentHealth = Mathf.Max(currentHealth - amount, 0);

        if (currentHealth > 0)
            _feedback?.PlayDamageFlash();

        if (playHitSound && currentHealth > 0)
        {
            if (TryGetComponent<PlayerController>(out _))
                AudioManager.Instance?.PlaySFX("PlayerHit");
            else if (TryGetComponent(out EnemyAI enemy))
                enemy.PlayHitSound();
        }

        ShowEnemyHealthBarAfterFirstHit();
        RefreshHealthVisuals();

        if (currentHealth <= 0)
            Die(damageSource);
    }
    #endregion

    #region Visuals
    internal void SetBurnPreview(int incomingDamage)
    {
        if (incomingDamage > 0)
            ShowEnemyHealthBarAfterFirstHit();

        healthBar?.SetBurnDamage(incomingDamage);
    }

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
    private void Die(GameObject damageSource)
    {
        PlayerController player = GetComponent<PlayerController>();

        if (player != null && player.TryPreventDeath())
            return;

        if (TryGetComponent(out EnemyAI enemy))
        {
            enemy.HandleDeath();
            if (damageSource != null && damageSource.TryGetComponent(out PlayerController attacker))
                attacker.OnEnemyKilled(this);
        }
        else if (player != null)
            HandlePlayerDeath(player);

        Destroy(gameObject);
    }

    private void HandlePlayerDeath(PlayerController player)
    {
        player.HandleDeath();

        AudioManager.Instance?.StopMusic();
        AudioManager.Instance?.PlaySFX("PlayerDeath");
        AudioManager.Instance?.PlaySFX("GameOver");

        GameManager.Instance?.GameOver();
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
