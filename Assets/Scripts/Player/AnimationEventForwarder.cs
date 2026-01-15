using UnityEngine;

public class AnimationEventForwarder : MonoBehaviour
{
    private PlayerController _playerController;

    private void Awake()
    {
        _playerController = GetComponentInParent<PlayerController>();
    }

    public void ApplyAttackDamage()
    {
        _playerController.ApplyAttackDamage();
    }
}
