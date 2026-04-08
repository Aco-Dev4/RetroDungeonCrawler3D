using UnityEngine;

[System.Serializable]
public class OwnedCard
{
    public CardData cardData;
    public int level = 1;

    public OwnedCard(CardData cardData)
    {
        this.cardData = cardData;
        level = 1;
    }

    public float GetCurrentValue()
    {
        if (cardData == null) return 0f;
        return cardData.baseValue + cardData.valuePerUpgrade * (level - 1);
    }
}