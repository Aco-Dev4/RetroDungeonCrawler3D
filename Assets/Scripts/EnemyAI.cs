using System;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    private NavMeshAgent _agent;
    private float _distance;
    private Vector3 _startingPoint;
    private Vector3 _lastValidDestination;

    [SerializeField] private GameObject player;
    [SerializeField] private float attackDistance = 2.0f;
    [SerializeField] private float rotationSpeed = 10f;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _startingPoint = transform.position;
    }

    void Update()
    {
        if (player == null) return;

        _distance = Vector3.Distance(_agent.transform.position, player.transform.position);

        // Check attack range
        if (_distance < attackDistance)
        {
            _agent.isStopped = true;
            // TODO: Play attack animation or deal damage
        }
        else
        {
            _agent.isStopped = false;
            MoveToPlayer();
        }

        HandleRotation();
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
