using UnityEngine;

public class PlayerRewardEffectHandler : MonoBehaviour
{
    private int _rewardRerolls;

    public int RewardRerolls => _rewardRerolls;

    public bool HasRerolls()
    {
        return _rewardRerolls > 0;
    }

    public void SetRerolls(int amount)
    {
        _rewardRerolls = Mathf.Max(0, amount);
    }

    public void AddRerolls(int amount)
    {
        if (amount <= 0) return;
        _rewardRerolls += amount;
    }

    public bool TrySpendReroll()
    {
        if (_rewardRerolls <= 0)
            return false;

        _rewardRerolls--;
        return true;
    }
}