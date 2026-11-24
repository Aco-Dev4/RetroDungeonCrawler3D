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
    public void ApplyCard(CardData cardData, PlayerController player, int silverInvestment = 0)
    {
        if (cardData == null || player == null) return;

        OwnedCard ownedCard = GetOwnedCard(cardData);
        bool isUpgrade = ownedCard != null;
        if (isUpgrade)
        {
            ownedCard.Upgrade(silverInvestment);
            RunStatsManager.Instance?.AddCardUpgraded();
        }
        else
        {
            ownedCards.Add(new OwnedCard(cardData));
        }
        player.RecalculateStats();

        float gainedValue = isUpgrade ? cardData.valuePerUpgrade : cardData.baseValue;
        if (cardData.acquisitionEffect != null)
            cardData.acquisitionEffect.Apply(player, gainedValue);
        else if (cardData.statType == CardStatType.Heal && player.TryGetComponent(out Health health))
            health.HealPercent(gainedValue);
    }

    public void ClearCards()
    {
        ownedCards.Clear();
    }
    #endregion
}
