using UnityEngine;

public abstract class CardAcquisitionEffect : ScriptableObject
{
    public abstract void Apply(PlayerController player, float gainedValue);
}
