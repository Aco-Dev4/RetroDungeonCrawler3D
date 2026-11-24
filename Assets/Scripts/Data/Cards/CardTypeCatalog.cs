using System;
using UnityEngine;

[Flags]
public enum CardType
{
    None = 0,
    Fire = 1,
    CriticalHit = 2,
    Ability = 4,
    Sword = 8
}

[Serializable]
public struct CardTypeEntry
{
    public CardType type;
    public string label;
    public Sprite icon;
}

[CreateAssetMenu(menuName = "Cards/Card Type Catalog")]
public sealed class CardTypeCatalog : ScriptableObject
{
    public CardTypeEntry[] entries;
}

