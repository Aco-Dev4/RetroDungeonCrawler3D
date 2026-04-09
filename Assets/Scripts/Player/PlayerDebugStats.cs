using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDebugStats : MonoBehaviour
{
    #region References
    [SerializeField] private PlayerController playerController;
    [SerializeField] private CardManager cardManager;
    [SerializeField] private Health health;
    [SerializeField] private Key debugKey = Key.P;
    #endregion

    private void Awake()
    {
        if (playerController == null) playerController = GetComponent<PlayerController>();
        if (health == null) health = GetComponent<Health>();
        if (cardManager == null) cardManager = FindFirstObjectByType<CardManager>();
    }

    private void Update()
    {
        if (Keyboard.current == null) return;
        if (!Keyboard.current[debugKey].wasPressedThisFrame) return;

        PrintStats();
    }

    #region Debug
    private void PrintStats()
    {
        if (playerController == null)
        {
            Debug.LogWarning("PlayerDebugStats: PlayerController missing.");
            return;
        }

        string healthText = health != null ? $"{health.currentHealth}/{health.GetMaxHealth()}" : "No Health";

        Debug.Log(
            "===== PLAYER STATS =====\n" +
            $"Health: {healthText}\n" +
            $"Move Speed: {playerController.GetMoveSpeed()}\n" +
            $"Jump Power: {playerController.GetJumpPower()}\n" +
            $"Max Jumps: {playerController.GetMaxJumps()}\n" +
            $"Attack Damage: {playerController.GetAttackDamage()}\n" +
            $"Attack Speed: {playerController.GetAttackSpeed()}\n" +
            $"Attack Range: {playerController.GetAttackRange()}\n" +
            $"Knockback Strength: {playerController.GetKnockbackStrength()}\n" +
            $"Card Luck: {playerController.GetLuck()}\n" +
            $"Wave Luck: {(cardManager != null ? cardManager.GetCurrentWaveLuck() : 0)}\n" +
            $"Effective Luck: {(cardManager != null ? cardManager.GetCurrentEffectiveLuck() : playerController.GetLuck())}\n" +
            $"Owned Cards: {playerController.GetOwnedCardCount()}\n" +
            "========================"
        );
    }
    #endregion
}