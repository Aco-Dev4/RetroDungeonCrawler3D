using UnityEngine;

[CreateAssetMenu(fileName = "PlayerVisualFeedbackSettings", menuName = "Game/Player Visual Feedback")]
public sealed class PlayerVisualFeedbackSettings : ScriptableObject
{
    [Header("Movement")]
    public ParticleSystem jumpPuff;
    [Min(0f)] public float jumpPuffHeight = 0.07f;

    [Header("Attacks")]
    public ParticleSystem fistPuff;
    [Min(0f)] public float forearmLength = 0.43f;
    [Min(0f)] public float fistPuffOffset = 0.15f;
    public ParticleSystem swordSlash;
    public ParticleSystem swordEcho;

    [Header("Guardian Angel")]
    public ParticleSystem guardianReviveGlow;
}

