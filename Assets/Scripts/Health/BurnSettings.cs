using UnityEngine;

[CreateAssetMenu(menuName = "Rogue Knight/Combat/Burn Settings")]
public sealed class BurnSettings : ScriptableObject
{
    [Min(1)] public int damagePerTick = 2;
    [Min(0.05f)] public float tickInterval = 0.8f;
    public ParticleSystem enemyFlames;
    public ParticleSystem swordFlames;

    [Header("Explosive Death")]
    [Min(1)] public int deathExplosionBurnDamage = 10;
    public ParticleSystem deathExplosionParticles;
}
