using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class EnemyAttackProjectile : MonoBehaviour, IEnemyAttack
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 5f;

    [Header("Impact")]
    [SerializeField] private LayerMask groundLayers;
    [SerializeField] private LayerMask playerLayers;
    [SerializeField] private ParticleSystem impactEffectPrefab;
    [SerializeField] private string impactSoundName;

    private BoxCollider _hitbox;
    private int _damage;
    private Vector3 _direction;
    private GameObject _owner;
    private bool _spent;

    private void Awake()
    {
        _hitbox = GetComponent<BoxCollider>();
    }

    public void Init(int damage, GameObject owner, Vector3 targetPosition)
    {
        _damage = damage;
        _owner = owner;
        _direction = (targetPosition - transform.position).normalized;
        if (_direction != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(_direction);

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        Advance(speed * Time.deltaTime);
    }

    private void Advance(float distance)
    {
        if (_spent || distance <= 0f || _direction == Vector3.zero) return;

        Vector3 scale = transform.lossyScale;
        scale = new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
        Vector3 center = transform.TransformPoint(_hitbox.center);
        Vector3 halfSize = Vector3.Scale(_hitbox.size * 0.5f, scale);

        // Sweep the full projectile shape so it cannot skip past a surface between frames.
        if (Physics.BoxCast(center, halfSize, _direction, out RaycastHit hit, transform.rotation,
                distance, groundLayers | playerLayers, QueryTriggerInteraction.Ignore))
        {
            Vector3 contact = hit.distance > 0f ? hit.point : hit.collider.ClosestPoint(center);
            if (TryImpact(hit.collider, contact)) return;
        }

        transform.position += _direction * distance;
    }

    private void OnTriggerEnter(Collider other)
    {
        TryImpact(other, other.ClosestPoint(transform.position));
    }

    private bool TryImpact(Collider other, Vector3 contact)
    {
        if (_spent || other.gameObject == _owner) return false;

        int layer = 1 << other.gameObject.layer;
        Health playerHealth = null;
        if ((playerLayers.value & layer) != 0)
        {
            playerHealth = other.GetComponentInParent<Health>();
            if (playerHealth == null || !playerHealth.CompareTag("Player")) return false;
        }
        else if ((groundLayers.value & layer) == 0 || other.isTrigger)
        {
            return false;
        }

        _spent = true;
        if (!string.IsNullOrEmpty(impactSoundName))
            AudioManager.Instance?.PlaySFX(impactSoundName);
        if (impactEffectPrefab != null)
            Instantiate(impactEffectPrefab, contact - _direction * 0.08f, Quaternion.identity);

        gameObject.SetActive(false);
        Destroy(gameObject);
        if (playerHealth != null)
            playerHealth.TakeDamage(_damage, _owner);

        return true;
    }
}

