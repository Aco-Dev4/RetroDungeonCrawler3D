using UnityEngine;
using UnityEngine.InputSystem;

public class EnemySpawnerTest : MonoBehaviour
{
    [SerializeField] private EnemySpawner spawner;
    [SerializeField] private EnemyData enemyData;

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (spawner != null && enemyData != null)
            {
                spawner.TrySpawn(enemyData);
            }
        }
    }
}
