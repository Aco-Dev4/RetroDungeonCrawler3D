using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float cooldown = 2f;

    private bool _isReady = true;

    public bool IsReady => _isReady;

    public GameObject TrySpawn(EnemyData enemyData)
    {
        if (!_isReady || enemyData == null || enemyData.prefab == null)
            return null;

        GameObject obj = Instantiate(enemyData.prefab, spawnPoint.position, spawnPoint.rotation);
        StartCoroutine(Cooldown());
        return obj;
    }

    private IEnumerator Cooldown()
    {
        _isReady = false;
        yield return new WaitForSeconds(cooldown);
        _isReady = true;
    }
}
