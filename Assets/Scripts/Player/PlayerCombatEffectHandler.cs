using UnityEngine;

public class PlayerCombatEffectHandler : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;

    private void Awake()
    {
        if (playerController == null)
            playerController = GetComponent<PlayerController>();
    }

    public void OnDamageDealt(PlayerAttackResult attackResult, Health targetHealth, EnemyAI enemy)
    {
        ApplyLifesteal(attackResult);
        ApplyCriticalKnockback(attackResult, enemy);
    }

    private void ApplyLifesteal(PlayerAttackResult attackResult)
    {
        if (playerController == null) return;

        float lifestealPercent = playerController.GetLifestealPercent();
        if (lifestealPercent <= 0f) return;

        Health playerHealth = playerController.GetComponent<Health>();
        if (playerHealth == null) return;

        int healAmount = Mathf.Max(1, Mathf.RoundToInt(attackResult.damage * lifestealPercent));
        playerHealth.Heal(healAmount);

        //Debug.Log($"Lifesteal healed {healAmount} HP");
    }

    private void ApplyCriticalKnockback(PlayerAttackResult attackResult, EnemyAI enemy)
    {
        if (playerController == null) return;
        if (enemy == null) return;
        if (!attackResult.isCrit) return;

        float strength = playerController.GetCritKnockbackStrength();
        if (strength <= 0f) return;

        enemy.ApplyKnockback(playerController.transform.position, strength);

        //Debug.Log($"Critical Knockback: {strength}");
    }
}