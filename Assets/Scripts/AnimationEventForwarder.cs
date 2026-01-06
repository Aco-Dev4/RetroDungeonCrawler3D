using UnityEngine;

public class AnimationEventForwarder : MonoBehaviour
{
    private PlayerAttack _playerAttack;

    private void Awake()
    {
        _playerAttack = GetComponentInParent<PlayerAttack>();
    }

    public void ApplyAttackDamage()
    {
        _playerAttack.ApplyAttackDamage();
    }
}
