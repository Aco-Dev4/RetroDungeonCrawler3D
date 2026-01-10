using UnityEngine;

public class EnemyAnimationEventForwarder : MonoBehaviour
{
    private EnemyAI _enemyAI;

    private void Awake()
    {
        _enemyAI = GetComponentInParent<EnemyAI>();
    }

    public void ApplyAttackDamage()
    {
        _enemyAI.ApplyAttackDamage();
    }

    public void OnAttackFinished()
    {
        _enemyAI.OnAttackFinished();
    }
}

