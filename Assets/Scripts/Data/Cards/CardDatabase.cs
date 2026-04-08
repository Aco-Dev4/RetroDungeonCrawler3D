using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardDatabase", menuName = "Cards/Card Database")]
public class CardDatabase : ScriptableObject
{
    public List<CardData> cards = new();

    public List<CardData> GetCardsByRarity(CardRarity rarity)
    {
        List<CardData> result = new();

        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i] == null) continue;
            if (!cards[i].canAppearInChest) continue;
            if (cards[i].rarity != rarity) continue;

            result.Add(cards[i]);
        }

        return result;
    }
}