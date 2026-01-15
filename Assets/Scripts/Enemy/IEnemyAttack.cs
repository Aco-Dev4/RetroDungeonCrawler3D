using UnityEngine;

public interface IEnemyAttack
{
    void Init(int damage, GameObject owner, Vector3 targetPosition);
}
