using UnityEngine;

public class Health : MonoBehaviour
{
    #region Variables: Health
    [SerializeField] private int maxHealth = 100;
    public float currentHealth;
    #endregion

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount, GameObject damageSource)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0); // Making sure health cannot be less than 0
        
        #region Debuging: Damage amount and source
        string attackerName = damageSource.name;
        Debug.Log($"{gameObject.name} took {amount} damage from {attackerName}. Current health: {currentHealth}");
        #endregion
        
        if (currentHealth <= 0)
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
