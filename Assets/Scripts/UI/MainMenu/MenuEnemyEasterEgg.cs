using UnityEngine;

public class MenuEnemyClickTarget : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int damagePerClick = 25;

    [Header("UI")]
    [SerializeField] private GameObject healthBarRoot;
    [SerializeField] private HealthBar healthBar;

    [Header("Optional")]
    [SerializeField] private GameObject destroyTarget;

    private int _currentHealth;
    private bool _wasHit;

    private void Awake()
    {
        _currentHealth = maxHealth;

        if (healthBarRoot != null)
            healthBarRoot.SetActive(false);

        if (healthBar != null)
        {
            healthBar.SetMaxHealth(maxHealth);
            healthBar.SetHealth(maxHealth);
        }

        if (destroyTarget == null)
            destroyTarget = gameObject;
    }

    public void TakeClickDamage()
    {
        if (_currentHealth <= 0) return;

        if (!_wasHit)
        {
            _wasHit = true;

            if (healthBarRoot != null)
                healthBarRoot.SetActive(true);
        }

        _currentHealth -= damagePerClick;
        _currentHealth = Mathf.Max(_currentHealth, 0);

        if (healthBar != null)
            healthBar.SetHealth(_currentHealth);

        if (_currentHealth <= 0)
            Destroy(destroyTarget);
    }
}
