using System.Collections.Generic;
using UnityEngine;

public class RunCardInventory : MonoBehaviour
{
    [SerializeField] private List<OwnedCard> ownedCards = new();

    public List<OwnedCard> OwnedCards => ownedCards;

    public bool HasCard(CardData cardData)
    {
        return GetOwnedCard(cardData) != null;
    }

    public OwnedCard GetOwnedCard(CardData cardData)
    {
        if (cardData == null) return null;

        for (int i = 0; i < ownedCards.Count; i++)
        {
            if (ownedCards[i].cardData == cardData)
                return ownedCards[i];
        }

        return null;
    }

    public void AddCard(CardData cardData)
    {
        if (cardData == null) return;
        if (HasCard(cardData)) return;

        ownedCards.Add(new OwnedCard(cardData));
    }

    public void UpgradeCard(CardData cardData)
    {
        OwnedCard ownedCard = GetOwnedCard(cardData);
        if (ownedCard == null) return;

        ownedCard.level++;
    }

    public void ClearCards()
    {
        ownedCards.Clear();
    }
}