using UnityEngine;

[CreateAssetMenu(menuName = "Rogue Knight/Abilities/Fireball Settings")]
public sealed class FireballSettings : ScriptableObject
{
    [Header("Cast")]
    [Min(0.1f)] public float cooldown = 15f;
    [Min(0f)] public float warningTime = 0.65f;
    [Min(0.1f)] public float fallTime = 1.4f;
    [Min(1f)] public float fallHeight = 8f;
    public LayerMask landingSurfaces;

    [Header("Impact")]
    [Min(1)] public int baseImpactDamage = 50;
    [Min(0)] public int impactDamagePerUpgrade = 12;
    [Min(1)] public int baseBurnDamage = 24;
    [Min(0)] public int burnDamagePerUpgrade = 6;
    [Min(0.1f)] public float baseRadius = 3.5f;
    [Min(0f)] public float radiusPerUpgrade = 0.15f;

    [Header("Feedback")]
    public GameObject landingMarker;
    public ParticleSystem fallingFireball;
    public ParticleSystem impactBurst;
    public string impactSound = "WizardImpact";

    internal FireballImpact GetImpact(int level)
    {
        int upgrades = Mathf.Max(0, level - 1);
        return new FireballImpact(
            baseImpactDamage + impactDamagePerUpgrade * upgrades,
            baseBurnDamage + burnDamagePerUpgrade * upgrades,
            baseRadius + radiusPerUpgrade * upgrades);
    }
}

internal readonly struct FireballImpact
{
    public readonly int damage;
    public readonly int burnDamage;
    public readonly float radius;

    public FireballImpact(int damage, int burnDamage, float radius)
    {
        this.damage = damage;
        this.burnDamage = burnDamage;
        this.radius = radius;
    }
}

