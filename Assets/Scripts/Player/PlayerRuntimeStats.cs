using UnityEngine;

[System.Serializable]
public class PlayerRuntimeStats
{
    public int maxHealth;
    public float moveSpeed;
    public float rotationSpeed;
    public float gravityMultiplier;
    public float jumpPower;
    public int maxJumps;
    public float attackRange;
    public int attackDamage;
    public float attackSpeed;
    public int luck;
    public float knockbackStrength;
    public float silverGainMultiplier;
    public float critChance;
    public float critDamageMultiplier;
    public float waveHealPercent;
    public float lifestealPercent;
    public float berserkerMaxBonus;
    public float critKnockbackStrength;
    public int rewardRerolls;
    public int guardianAngelRevives;
    public int guaranteedCritEveryXHits;
    public int earthquakeJumpLevel;

    public void LoadBaseStats(PlayerData playerData, bool ignorePermanentUpgrades, int tutorialBaseDamage)
    {
        maxHealth = playerData.startingHealth;

        moveSpeed = playerData.moveSpeed;
        rotationSpeed = playerData.rotationSpeed;
        jumpPower = playerData.jumpPower;
        maxJumps = playerData.maxJumps;
        gravityMultiplier = playerData.gravityMultiplier;

        attackRange = playerData.attackRange;
        attackDamage = ignorePermanentUpgrades ? tutorialBaseDamage : playerData.attackDamage;
        attackSpeed = playerData.attackSpeed * 1.5f;

        luck = ignorePermanentUpgrades || GameDataManager.Instance == null ? 0 : GameDataManager.Instance.GetExtraLuck();
        silverGainMultiplier = ignorePermanentUpgrades || GameDataManager.Instance == null ? 1f : 1f + GameDataManager.Instance.GetExtraSilverMultiplier();

        knockbackStrength = 0f;

        critChance = ignorePermanentUpgrades || GameDataManager.Instance == null || !GameDataManager.Instance.HasUnlockedCriticalHits() ? 0f : 0.01f;
        critDamageMultiplier = 1.5f;

        waveHealPercent = 0f;
        lifestealPercent = 0f;
        berserkerMaxBonus = 0f;
        critKnockbackStrength = 0f;
        rewardRerolls = 0;
        guardianAngelRevives = 0;
        guaranteedCritEveryXHits = 0;
        earthquakeJumpLevel = 0;
    }
}