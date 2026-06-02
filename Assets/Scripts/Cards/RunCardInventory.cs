using System.Collections.Generic;
using UnityEngine;

public class RunCardInventory : MonoBehaviour
{
    #region Runtime
    [SerializeField] private List<OwnedCard> ownedCards = new();
    #endregion

    #region Public Getters
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
            OwnedCard ownedCard = ownedCards[i];

            if (ownedCard != null && ownedCard.cardData == cardData)
                return ownedCard;
        }

        return null;
    }
    #endregion

    #region Card Changes
    public void AddCard(CardData cardData)
    {
        if (cardData == null) return;
        if (HasCard(cardData)) return;

        ownedCards.Add(new OwnedCard(cardData));
    }

    public void UpgradeCard(CardData cardData, int silverInvestment = 0)
    {
        OwnedCard ownedCard = GetOwnedCard(cardData);
        if (ownedCard == null) return;

        ownedCard.Upgrade(silverInvestment);
    }

    public void ClearCards()
    {
        ownedCards.Clear();
    }
    #endregion
}