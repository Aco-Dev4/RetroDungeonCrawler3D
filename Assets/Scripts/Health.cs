using UnityEngine;

public class Health : MonoBehaviour
{
    #region Variables: Health
    [SerializeField] private int maxHealth = 100;
    private float _currentHealth;
    #endregion

    private void Awake()
    {
        _currentHealth = maxHealth;
    }

    public void TakeDamage(int amount, GameObject damageSource)
    {
        _currentHealth -= amount;
        _currentHealth = Mathf.Max(_currentHealth, 0); // Making sure health cannot be less than 0
        
        #region Debuging: Damage amount and source
        string attackerName = damageSource.name;
        Debug.Log($"{gameObject.name} took {amount} damage from {attackerName}. Current health: {_currentHealth}");
        #endregion
        
        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Later: play animation, drop loot, etc.
        Destroy(gameObject);
    }
}
