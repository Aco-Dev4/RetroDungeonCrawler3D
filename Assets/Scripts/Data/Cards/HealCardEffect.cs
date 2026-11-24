using UnityEngine;

[CreateAssetMenu(fileName = "HealCardEffect", menuName = "Cards/Effects/Heal on Acquisition")]
public class HealCardEffect : CardAcquisitionEffect
{
    public override void Apply(PlayerController player, float gainedValue)
    {
        if (player.TryGetComponent(out Health health))
            health.HealPercent(gainedValue);
    }
}
