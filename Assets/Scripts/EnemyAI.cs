using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    
    private NavMeshAgent _agent;
    
    [SerializeField] private GameObject player;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }
    
    // Update is called once per frame
    void Update()
    {
        _agent.SetDestination(player.transform.position);
    }
}
