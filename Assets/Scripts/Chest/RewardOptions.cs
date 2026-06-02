using UnityEngine;

[System.Serializable]
public class RewardOption
{
    public CardData cardData;
    public bool isUpgrade;

    public RewardOption(CardData cardData, bool isUpgrade)
    {
        this.cardData = cardData;
        this.isUpgrade = isUpgrade;
    }

    public bool Matches(RewardOption other)
    {
        if (other == null) return false;

        return cardData == other.cardData && isUpgrade == other.isUpgrade;
    }
}