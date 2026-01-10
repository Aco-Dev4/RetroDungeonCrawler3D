using UnityEngine;

public class EnemyAttackHitbox : MonoBehaviour
{
    private int _damage;
    private GameObject _owner;

    public void Init(int damage, GameObject owner)
    {
        _damage = damage;
        _owner = owner;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == _owner) return;

        Health health = other.GetComponent<Health>();
        if (health != null)
        {
            health.TakeDamage(_damage, _owner);
        }
    }
}

