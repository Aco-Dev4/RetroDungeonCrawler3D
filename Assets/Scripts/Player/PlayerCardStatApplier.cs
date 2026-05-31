using UnityEngine;

public static class PlayerCardStatApplier
{
    public static void ApplyCard(PlayerRuntimeStats stats, OwnedCard ownedCard)
    {
        if (stats == null || ownedCard == null || ownedCard.cardData == null)
            return;

        CardData cardData = ownedCard.cardData;
        float value = ownedCard.GetCurrentValue();

        switch (cardData.statType)
        {
            case CardStatType.MaxHealth:
                stats.maxHealth += cardData.usePercent ? UnityEngine.Mathf.RoundToInt(stats.maxHealth * value) : UnityEngine.Mathf.RoundToInt(value);
                break;

            case CardStatType.Heal:
                break;

            case CardStatType.MoveSpeed:
                stats.moveSpeed += cardData.usePercent ? stats.moveSpeed * value : value;
                break;

            case CardStatType.JumpPower:
                stats.jumpPower += cardData.usePercent ? stats.jumpPower * value : value;
                break;

            case CardStatType.AttackDamage:
                stats.attackDamage += cardData.usePercent ? UnityEngine.Mathf.RoundToInt(stats.attackDamage * value) : UnityEngine.Mathf.RoundToInt(value);
                break;

            case CardStatType.AttackSpeed:
                stats.attackSpeed += cardData.usePercent ? stats.attackSpeed * value : value;
                break;

            case CardStatType.AttackRange:
                stats.attackRange += cardData.usePercent ? stats.attackRange * value : value;
                break;

            case CardStatType.JumpCount:
                stats.maxJumps += cardData.usePercent ? UnityEngine.Mathf.RoundToInt(stats.maxJumps * value) : UnityEngine.Mathf.RoundToInt(value);
                break;

            case CardStatType.Luck:
                stats.luck += cardData.usePercent ? UnityEngine.Mathf.RoundToInt(stats.luck * value) : UnityEngine.Mathf.RoundToInt(value);
                break;

            case CardStatType.Knockback:
                stats.knockbackStrength += cardData.usePercent ? stats.knockbackStrength * value : value;
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
                stats.guaranteedCritEveryXHits = Mathf.RoundToInt(value);
                break;
        }
    }
}