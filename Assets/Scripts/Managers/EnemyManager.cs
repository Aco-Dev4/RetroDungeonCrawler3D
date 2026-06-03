using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;

    [Header("Global Enemy Modifiers")]
    public float damageMultiplier = 1f;
    public float moveSpeedMultiplier = 1f;
    public float attackSpeedMultiplier = 1f;
    public float healthMultiplier = 1f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public EnemyStats GetStats(EnemyData data, int waveNumber = 1)
    {
        EnemyStats stats = new EnemyStats();

        float finalHealthMultiplier = healthMultiplier;
        float finalDamageMultiplier = damageMultiplier;
        float finalMoveSpeedMultiplier = moveSpeedMultiplier;
        float finalAttackSpeedMultiplier = attackSpeedMultiplier;

        if (RunDifficultyManager.Instance != null)
        {
            finalHealthMultiplier *= RunDifficultyManager.Instance.GetHealthMultiplier(waveNumber);
            finalDamageMultiplier *= RunDifficultyManager.Instance.GetDamageMultiplier(waveNumber);
            finalMoveSpeedMultiplier *= RunDifficultyManager.Instance.GetMoveSpeedMultiplier(waveNumber);
            finalAttackSpeedMultiplier *= RunDifficultyManager.Instance.GetAttackSpeedMultiplier(waveNumber);
        }

        stats.maxHealth = Mathf.RoundToInt(data.maxHealth * finalHealthMultiplier);
        stats.damage = Mathf.RoundToInt(data.damage * finalDamageMultiplier);
        stats.attackSpeed = data.attackSpeed * finalAttackSpeedMultiplier;
        stats.attackRange = data.attackRange;
        stats.moveSpeed = data.moveSpeed * finalMoveSpeedMultiplier;
        stats.weight = data.weight;

        return stats;
    }
}
