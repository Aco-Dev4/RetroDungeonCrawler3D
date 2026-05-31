using UnityEngine;

public static class PlayerDamageCalculator
{
    public static PlayerAttackResult GetAttackResult(PlayerRuntimeStats stats, bool hasSword, int swordDamageBonus, Health playerHealth)
    {
        if (stats == null)
            return new PlayerAttackResult(0, false);

        int damage = hasSword ? stats.attackDamage + swordDamageBonus : stats.attackDamage;

        damage = ApplyBerserkerDamage(damage, stats, playerHealth);

        bool isCrit = stats.critChance > 0f && Random.value <= stats.critChance;

        if (isCrit)
            damage = Mathf.RoundToInt(damage * stats.critDamageMultiplier);

        return new PlayerAttackResult(damage, isCrit);
    }

    private static int ApplyBerserkerDamage(int damage, PlayerRuntimeStats stats, Health playerHealth)
    {
        if (stats.berserkerMaxBonus <= 0f) return damage;
        if (playerHealth == null) return damage;

        float maxHealth = playerHealth.GetMaxHealth();
        if (maxHealth <= 0f) return damage;

        float currentHealthPercent = playerHealth.currentHealth / maxHealth;
        float missingHealthPercent = 1f - currentHealthPercent;

        float multiplier = 1f + missingHealthPercent * stats.berserkerMaxBonus;

        return Mathf.RoundToInt(damage * multiplier);
    }

    public static float GetAttackRange(PlayerRuntimeStats stats, bool hasSword, float swordRangeBonus)
    {
        if (stats == null)
            return 0f;

        return hasSword ? stats.attackRange + swordRangeBonus : stats.attackRange;
    }
}