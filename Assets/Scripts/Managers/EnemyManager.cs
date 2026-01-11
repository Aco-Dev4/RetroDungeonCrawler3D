using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;

    [Header("Global Enemy Modifiers")]
    public float damageMultiplier = 1f;
    public float moveSpeedMultiplier = 1f;
    public float attackSpeedMultiplier = 1f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public EnemyStats GetStats(EnemyData data)
    {
        EnemyStats stats = new EnemyStats();

        stats.maxHealth = data.maxHealth;
        stats.damage = Mathf.RoundToInt(data.damage * damageMultiplier);
        stats.attackSpeed = data.attackSpeed * attackSpeedMultiplier;
        stats.attackRange = data.attackRange;
        stats.moveSpeed = data.moveSpeed * moveSpeedMultiplier;

        return stats;
    }
}
