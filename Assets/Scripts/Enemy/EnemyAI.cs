using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [SerializeField] private EnemyData enemyData;
    private EnemyStats _stats;
    private WaveInstance _waveInstance;

    #region Variables: NavMesh Travel
    private NavMeshAgent _agent;
    #endregion

    #region Variables: Target Follow
    [SerializeField] private Animator _animator;
    private GameObject _player;
    private Health _playerHealth;
    private float _distance;
    private Vector3 _lastValidDestination;
    #endregion

    #region Variables: Attack
    [SerializeField] private GameObject attackPrefab;
    [SerializeField] private Transform attackOrigin;
    // [SerializeField] private float attackRange = 2.0f;
    // [SerializeField] private int attackDamage = 10;
    // [SerializeField] private float attackSpeed = 1f;
    private bool _canAttack = true;
    private bool _isAttacking;
    #endregion

    private Collider _collider;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _collider = GetComponent<Collider>();

        GameObject foundPlayer = GameObject.FindWithTag("Player");
        if (foundPlayer != null)
        {
            _player = foundPlayer;
            _playerHealth = _player.GetComponent<Health>();
        }
        else
        {
            Debug.LogError("EnemyAI: No GameObject with tag 'Player' found!");
        }

        #region Debug
        if (enemyData == null)
        {
            Debug.LogError($"{name} has no EnemyData assigned!");
            return;
        }

        if (EnemyManager.Instance == null)
        {
            Debug.LogError("EnemyManager not found in scene!");
            return;
        }
        #endregion
        _stats = EnemyManager.Instance.GetStats(enemyData);

        _agent.speed = _stats.moveSpeed;
        Health health = GetComponent<Health>();
        if (health != null)
        {
            health.Init(_stats.maxHealth);
        }
    }

    public void InitWave(WaveInstance waveInstance)
    {
        _waveInstance = waveInstance;
    }

    public WaveInstance GetWaveInstance()
    {
        return _waveInstance;
    }

    void Update()
    {
        DistanceCheck();
        _agent.updateRotation = !_isAttacking;
        HandleRotation();
    }

    private void DistanceCheck()
    {
        if (_player == null) return;
        if (_isAttacking) return;

        _distance = Vector3.Distance(_agent.transform.position, _player.transform.position);

        // Check attack range OR if player is touching enemy collider
        if (_distance <= _stats.attackRange || IsPlayerTouching())
        {
            _agent.isStopped = true;
            if (_animator != null)
                _animator.SetBool("IsMoving", false);
            TryAttack();
        }
        else
        {
            _agent.isStopped = false;
            if (_animator != null)
                _animator.SetBool("IsMoving", true);
            MoveToPlayer();
        }
    }

    private bool IsPlayerTouching()
    {
        if (_player == null) return false;

        // Check if player collider intersects enemy collider bounds
        Collider playerCollider = _player.GetComponent<Collider>();
        if (playerCollider == null) return false;

        return _collider.bounds.Intersects(playerCollider.bounds);
    }

    private void TryAttack()
    {
        if (!_canAttack) return;

        _canAttack = false;
        _isAttacking = true;

        _agent.isStopped = true;
        _agent.velocity = Vector3.zero;

        if (_animator != null)
        {
            _animator.SetFloat("AttackSpeed", _stats.attackSpeed);
            _animator.SetTrigger("Attack");
        }

        StartCoroutine(AttackCooldown());
    }

    public void ApplyAttackDamage()
    {
        GameObject attackObj = Instantiate(attackPrefab, attackOrigin.position, attackOrigin.rotation);

        IEnemyAttack attack = attackObj.GetComponent<IEnemyAttack>();
        if (attack != null)
        {
            attack.Init(_stats.damage, gameObject);
        }
        else
        {
            Debug.LogWarning($"{attackObj.name} has no IEnemyAttack component.");
        }
    }

    public void OnAttackFinished()
    {
        _isAttacking = false;
    }

    private IEnumerator AttackCooldown() 
    { 
        if (_animator == null)
        {
            ApplyAttackDamage();
        }
        yield return new WaitForSeconds(1f / _stats.attackSpeed);
        _canAttack = true;
        _isAttacking = false;
    }


    private void MoveToPlayer()
    {
        if (_player == null) return;

        NavMeshPath path = new NavMeshPath();
        _agent.CalculatePath(_player.transform.position, path);

        if (path.status == NavMeshPathStatus.PathComplete)
        {
            _lastValidDestination = _player.transform.position;
            _agent.SetDestination(_player.transform.position);
        }
        else if (path.status == NavMeshPathStatus.PathPartial)
        {
            _lastValidDestination = path.corners[path.corners.Length - 1];
            _agent.SetDestination(_lastValidDestination);
        }
        else
        {
            if (_lastValidDestination != Vector3.zero)
            {
                _agent.SetDestination(_lastValidDestination);
            }
        }
    }

    private void HandleRotation()
    {
        if (_player == null) return;

        Vector3 direction = Vector3.zero;

        if (_agent.isOnOffMeshLink)
        {
            direction = (_agent.currentOffMeshLinkData.endPos - transform.position).normalized;
        }
        else if (!_isAttacking && _agent.velocity.sqrMagnitude > 0.1f)
        {
            direction = _agent.velocity.normalized;
        }
        else
        {
            direction = (_player.transform.position - transform.position).normalized;
        }

        direction.y = 0;

        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }
}

