using UnityEngine;

public class EnemyAttackHitbox : MonoBehaviour, IEnemyAttack
{
    [SerializeField] private ParticleSystem swingEffectPrefab;

    private int _damage;
    private GameObject _owner;

    public void Init(int damage, GameObject owner, Vector3 targetPosition)
    {
        _damage = damage;
        _owner = owner;
        PlaySwingEffect();
        Destroy(gameObject, 0.15f);
    }

    private void PlaySwingEffect()
    {
        if (swingEffectPrefab == null || _owner == null) return;

        Transform enemy = _owner.transform;
        ParticleSystem swing = Instantiate(swingEffectPrefab, enemy.position, enemy.rotation);
        swing.Play();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_owner != null && other.gameObject == _owner) return;

        Health health = other.GetComponent<Health>();
        if (health != null)
        {
            health.TakeDamage(_damage, _owner);
        }
    }
}

