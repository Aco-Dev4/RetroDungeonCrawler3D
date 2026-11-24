using UnityEngine;

[CreateAssetMenu(fileName = "HealthFeedbackSettings", menuName = "Game/Health Feedback")]
public class HealthFeedbackSettings : ScriptableObject
{
    [Header("Damage")]
    public Color damageTint = new(1f, 0.16f, 0.12f);
    [Range(0f, 1f)] public float damageStrength = 0.5f;
    [Min(0f)] public float damageDuration = 0.11f;
    [Tooltip("Also tints the Toon shader's shadow and rim colors.")]
    public bool tintToonShading;

    [Header("Healing")]
    public ParticleSystem healEffectPrefab;
    [Min(0f)] public float healCooldown = 0.45f;
    public float healHeight = 1.45f;
}
