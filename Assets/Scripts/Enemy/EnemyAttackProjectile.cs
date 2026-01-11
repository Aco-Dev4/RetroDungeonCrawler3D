using UnityEngine;

public class EnemyAttackProjectile : MonoBehaviour, IEnemyAttack
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 5f;

    [Header("Collision")]
    [SerializeField] private LayerMask ignoreDestroyOnLayers;
    [SerializeField] private bool destroyOnAnyCollision = false;

    private int _damage;
    private GameObject _owner;

    public void Init(int damage, GameObject owner)
    {
        _damage = damage;
        _owner = owner;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == _owner) return;

        Health health = other.GetComponent<Health>();
        if (health != null)
        {
            health.TakeDamage(_damage, _owner);
            if (destroyOnAnyCollision) Destroy(gameObject);
            return;
        }

        if (destroyOnAnyCollision && ((ignoreDestroyOnLayers.value & (1 << other.gameObject.layer)) == 0))
        {
            Destroy(gameObject);
        }
    }
}
