using UnityEngine;

public class RewardChestAnimationEventForwarder : MonoBehaviour
{
    private RewardChest _chest;

    private void Awake()
    {
        _chest = GetComponentInParent<RewardChest>();
    }

    // Animation Event
    public void OnChestOpened()
    {
        _chest.OnChestOpened();
    }
}

