using UnityEngine;

public static class PlayerCardStatApplier
{
    #region Public
    public static void ApplyCard(PlayerRuntimeStats stats, OwnedCard ownedCard)
    {
        if (stats == null || ownedCard == null || ownedCard.cardData == null)
            return;

        CardData cardData = ownedCard.cardData;
        float value = ownedCard.GetCurrentValue();

        ApplyStat(stats, cardData, value, ownedCard.level);
    }
    #endregion

    #region Stat Applying
    private static void ApplyStat(PlayerRuntimeStats stats, CardData cardData, float value, int level)
    {
        switch (cardData.statType)
        {
            case CardStatType.MaxHealth:
                stats.maxHealth += GetIntBonus(stats.maxHealth, value, cardData.usePercent);
                break;

            case CardStatType.Heal:
                break;

            case CardStatType.MoveSpeed:
                stats.moveSpeed += GetFloatBonus(stats.moveSpeed, value, cardData.usePercent);
                break;

            case CardStatType.JumpPower:
                stats.jumpPower += GetFloatBonus(stats.jumpPower, value, cardData.usePercent);
                break;

            case CardStatType.AttackDamage:
                stats.attackDamage += GetIntBonus(stats.attackDamage, value, cardData.usePercent);
                break;

            case CardStatType.AttackSpeed:
                stats.attackSpeed += GetFloatBonus(stats.attackSpeed, value, cardData.usePercent);
                break;

            case CardStatType.AttackRange:
                stats.attackRange += GetFloatBonus(stats.attackRange, value, cardData.usePercent);
                break;

            case CardStatType.JumpCount:
                stats.maxJumps += GetIntBonus(stats.maxJumps, value, cardData.usePercent);
                break;

            case CardStatType.Luck:
                stats.luck += GetIntBonus(stats.luck, value, cardData.usePercent);
                break;

            case CardStatType.Knockback:
                stats.knockbackStrength += GetFloatBonus(stats.knockbackStrength, value, cardData.usePercent);
                break;

            case CardStatType.SilverGain:
                stats.silverGainMultiplier += value;
                break;

            case CardStatType.CritChance:
                stats.critChance += value;
                break;

            case CardStatType.CritDamage:
                stats.critDamageMultiplier += value;
                break;

            case CardStatType.WaveHeal:
                stats.waveHealPercent += value;
                break;

            case CardStatType.Lifesteal:
                stats.lifestealPercent += value;
                break;

            case CardStatType.Berserker:
                stats.berserkerMaxBonus += value;
                break;

            case CardStatType.RewardReroll:
                stats.rewardRerolls += Mathf.RoundToInt(value);
                break;

            case CardStatType.CritKnockback:
                stats.critKnockbackStrength += value;
                break;

            case CardStatType.GuardianAngel:
                stats.guardianAngelRevives += Mathf.RoundToInt(value);
                break;

            case CardStatType.GuaranteedCrit:
                stats.guaranteedCritEveryXHits = Mathf.Max(1, Mathf.RoundToInt(value));
                break;

            case CardStatType.AllPowerful:
                ApplyAllPowerful(stats, level);
                break;

            case CardStatType.EarthquakeJump:
                stats.earthquakeJumpLevel += Mathf.RoundToInt(value);
                break;
        }
    }
    #endregion

    #region Helpers
    private static float GetFloatBonus(float baseValue, float value, bool usePercent)
    {
        return usePercent ? baseValue * value : value;
    }

    private static int GetIntBonus(int baseValue, float value, bool usePercent)
    {
        return usePercent ? Mathf.RoundToInt(baseValue * value) : Mathf.RoundToInt(value);
    }

    private static void ApplyAllPowerful(PlayerRuntimeStats stats, int level)
    {
        if (stats == null) return;

        stats.attackDamage += Mathf.RoundToInt(stats.attackDamage * (0.1f + 0.05f * (level - 1)));
        stats.attackSpeed += stats.attackSpeed * (0.1f + 0.05f * (level - 1));
        stats.maxHealth += 20 + 10 * (level - 1);
        stats.moveSpeed += 0.2f + 0.1f * (level - 1);
        stats.jumpPower += 0.2f + 0.1f * (level - 1);
    }
    #endregion
}