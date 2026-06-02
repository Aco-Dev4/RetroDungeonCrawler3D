using UnityEngine;

[System.Serializable]
public class OwnedCard
{
    #region Data
    public CardData cardData;
    public int level = 1;
    public int silverInvested;
    #endregion

    #region Constructor
    public OwnedCard(CardData cardData)
    {
        this.cardData = cardData;
        level = 1;
        silverInvested = 0;
    }
    #endregion

    #region Public
    public void Upgrade(int silverInvestment = 0)
    {
        level++;
        silverInvested += Mathf.Max(0, silverInvestment);
    }

    public float GetCurrentValue()
    {
        if (cardData == null) return 0f;

        return cardData.baseValue + cardData.valuePerUpgrade * (level - 1);
    }
    #endregion
}