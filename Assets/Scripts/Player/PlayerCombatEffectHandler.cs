using UnityEngine;

public class PlayerCombatEffectHandler : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private ParticleSystem criticalHitEffectPrefab;

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

    public void PlayCriticalHitEffect(Vector3 position)
    {
        if (criticalHitEffectPrefab != null)
            Instantiate(criticalHitEffectPrefab, position, Quaternion.identity);
    }

    internal void PlayDeathExplosion(Health defeatedEnemy, BurnSettings settings, float radius,
        float tickRateMultiplier, LayerMask enemyLayer)
    {
        Vector3 center = defeatedEnemy.TryGetComponent(out Collider body) && body.enabled
            ? body.bounds.center
            : defeatedEnemy.transform.position;

        if (settings.deathExplosionParticles != null)
        {
            ParticleSystem burst = Instantiate(settings.deathExplosionParticles, center, Quaternion.identity);
            burst.transform.localScale *= radius;
            burst.Play(true);
        }

        Collider[] hits = Physics.OverlapSphere(center, radius, enemyLayer, QueryTriggerInteraction.Ignore);
        foreach (Collider hit in hits)
        {
            Health target = hit.GetComponentInParent<Health>();
            if (target == null || target == defeatedEnemy || !target.TryGetComponent<EnemyAI>(out _)) continue;

            target.ApplyBurn(settings, settings.deathExplosionBurnDamage, tickRateMultiplier, playerController.gameObject);
        }
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