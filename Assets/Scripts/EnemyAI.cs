using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    #region Variables: NavMesh Travel
    private NavMeshAgent _agent;
    #endregion

    #region Variables: Target Follow
    [SerializeField] private GameObject player;
    [SerializeField] private float rotationSpeed = 10f;
    private Health _playerHealth;
    private float _distance;
    private Vector3 _lastValidDestination;
    #endregion
    
    #region Variables: Attack
    [SerializeField] private float attackDistance = 2.0f;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float attackCooldown = 1.5f;
    private bool _canAttack = true;
    #endregion

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _playerHealth = player.GetComponent<Health>();
    }

    void Update()
    {
        DistanceCheck();

        HandleRotation();
    }

    private void DistanceCheck()
    {
        if (player == null) return;

        _distance = Vector3.Distance(_agent.transform.position, player.transform.position);

        // Check attack range
        if (_distance < attackDistance)
        {
            _agent.isStopped = true;
            TryAttack();
            // TODO: Play attack animation
        }
        else
        {
            _agent.isStopped = false;
            MoveToPlayer();
        }
    }

    private void TryAttack()
    {
        if (!_canAttack) return;

        StartCoroutine(AttackCooldown());
        PerformAttack();
    }

    private void PerformAttack()
    {
        if (player == null) return;
        
        if (_playerHealth != null)
        {
            _playerHealth.TakeDamage(attackDamage, gameObject);
        }
        
        #region Debuging: Attack Visual
        
        Debug.DrawLine(transform.position, player.transform.position, Color.red, 0.2f);
        Debug.DrawRay(transform.position, transform.forward * 2, Color.red, 0.2f);
        DebugExtension.DebugWireSphere(transform.position + transform.forward * (2), Color.red, 2, 0.2f);
        
        // Debug Red Sphere
        GameObject debugSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        GameObject.Destroy(debugSphere.GetComponent<Collider>());
        debugSphere.transform.position = transform.position + transform.forward * 2;
        debugSphere.transform.localScale = Vector3.one * 2;
        var sphereRenderer = debugSphere.GetComponent<Renderer>();
        sphereRenderer.material = new Material(Shader.Find("Standard"));
        sphereRenderer.material.color = new Color(1f, 0f, 0f, 0.4f);
        sphereRenderer.material.SetFloat("_Mode", 3); // force transparency
        sphereRenderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        sphereRenderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        sphereRenderer.material.SetInt("_ZWrite", 0);
        sphereRenderer.material.DisableKeyword("_ALPHATEST_ON");
        sphereRenderer.material.EnableKeyword("_ALPHABLEND_ON");
        sphereRenderer.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        sphereRenderer.material.renderQueue = 3000;
        GameObject.Destroy(debugSphere, 0.2f);
        
        #endregion
    }

    private IEnumerator AttackCooldown()
    {
        _canAttack = false;
        yield return new WaitForSeconds(attackCooldown);
        _canAttack = true;
    }

    private void MoveToPlayer()
    {
        NavMeshPath path = new NavMeshPath();
        _agent.CalculatePath(player.transform.position, path);

        if (path.status == NavMeshPathStatus.PathComplete)
        {
            _lastValidDestination = player.transform.position;
            _agent.SetDestination(player.transform.position);
        }
        else if (path.status == NavMeshPathStatus.PathPartial)
        {
            _lastValidDestination = path.corners[path.corners.Length - 1];
            _agent.SetDestination(_lastValidDestination);
        }
        else
        {
            // If no valid path (like player is in air), just go to last known reachable point
            if (_lastValidDestination != Vector3.zero)
            {
                _agent.SetDestination(_lastValidDestination);
            }
            // If we still have none (like at start), stay idle or patrol
        }
    }

    private void HandleRotation()
    {
        // While jumping (on NavMeshLink)
        if (_agent.isOnOffMeshLink)
        {
            // Look toward where the link leads
            Vector3 target = _agent.currentOffMeshLinkData.endPos;
            Vector3 direction = (target - transform.position).normalized;
            direction.y = 0;

            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
            }
        }
        else
        {
            // Regular rotation following velocity
            if (_agent.velocity.sqrMagnitude > 0.1f)
            {
                Quaternion lookRotation = Quaternion.LookRotation(_agent.velocity.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }
}
