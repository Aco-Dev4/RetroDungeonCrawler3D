using UnityEngine;

public class PlayerRewardEffectHandler : MonoBehaviour
{
    #region Paid Reroll Settings
    [Header("Paid Reroll Cost")]
    [SerializeField] private int basePaidRerollCost = 10;
    [SerializeField] private int paidRerollCostIncrease = 5;
    #endregion

    #region Runtime
    private int _rewardRerolls;
    private int _totalRerollsGranted;
    private int _paidRerollsBought;
    #endregion

    #region Public Getters
    public int RewardRerolls => _rewardRerolls;

    public bool HasRerolls()
    {
        return _rewardRerolls > 0;
    }

    public int GetPaidRerollCost()
    {
        return basePaidRerollCost + paidRerollCostIncrease * _paidRerollsBought;
    }
    #endregion

    #region Free Rerolls
    public void SetRerolls(int totalAmountFromCards)
    {
        totalAmountFromCards = Mathf.Max(0, totalAmountFromCards);

        int newlyGainedRerolls = totalAmountFromCards - _totalRerollsGranted;

        if (newlyGainedRerolls > 0)
            _rewardRerolls += newlyGainedRerolls;

        _totalRerollsGranted = Mathf.Max(_totalRerollsGranted, totalAmountFromCards);
    }

    public bool TrySpendReroll()
    {
        if (_rewardRerolls <= 0)
            return false;

        _rewardRerolls--;
        return true;
    }
    #endregion

    #region Paid Rerolls
    public bool TryBuyPaidReroll()
    {
        if (CurrencyManager.Instance == null)
            return false;

        int cost = GetPaidRerollCost();

        if (CurrencyManager.Instance.GetSilver() < cost)
            return false;

        CurrencyManager.Instance.SetSilver(CurrencyManager.Instance.GetSilver() - cost);
        _paidRerollsBought++;

        return true;
    }
    #endregion
}