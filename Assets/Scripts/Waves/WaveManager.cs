using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;

public class WaveManager : MonoBehaviour
{
    [Header("Waves")]
    [SerializeField] private List<WaveData> waves = new();

    [Header("Spawners")]
    [SerializeField] private List<EnemySpawner> spawners = new();

    [Header("UI")]
    [SerializeField] private WavePopupUI wavePopupUI;
    [SerializeField] private WaveCounterUI waveCounterUI;

    private List<WaveInstance> _activeWaves = new();
    private int _nextWaveIndex = 0;

    void Awake()
    {
        if (waveCounterUI != null)
            waveCounterUI.SetWave(0, waves.Count);
    }

    public void OnNextWave(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        StartNextWave();
    }

    private void StartNextWave()
    {
        if (_nextWaveIndex >= waves.Count)
        {
            Debug.Log("All waves have already been started.");
            return;
        }

        WaveData waveData = waves[_nextWaveIndex];
        int waveNumber = _nextWaveIndex + 1;
        _nextWaveIndex++;

        WaveInstance instance = new WaveInstance(waveData, waveNumber);
        _activeWaves.Add(instance);

        if (waveCounterUI != null)
            waveCounterUI.SetWave(waveNumber, waves.Count);

        Debug.Log($"Wave {waveNumber} started.");
        if (wavePopupUI != null)
            wavePopupUI.Show($"Wave {waveNumber} Started", WavePopupType.WaveStarted);

        StartCoroutine(SpawnLoop(instance));
    }

    private IEnumerator SpawnLoop(WaveInstance wave)
    {
        while (!wave.IsCompleted)
        {
            if (GameManager.Instance.State == GameState.GameOver)
                yield break;

            if (wave.remainingToSpawn > 0 && wave.aliveEnemies < wave.data.maxAliveEnemies)
            {
                EnemySpawner spawner = GetRandomReadySpawner();
                if (spawner != null)
                {
                    EnemyData enemy = GetNextEnemy(wave);
                    if (enemy != null)
                    {
                        GameObject spawned = spawner.TrySpawn(enemy);
                        if (spawned != null)
                        {
                            EnemyAI ai = spawned.GetComponent<EnemyAI>();
                            if (ai != null) ai.InitWave(wave);

                            wave.remainingToSpawn--;
                            wave.aliveEnemies++;
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Missing Enemy!");
                    }
                }
                else
                {
                    Debug.LogWarning("Missing Spawner!");
                }
            }

            yield return null;
        }

        EndWave(wave);
    }

    private EnemySpawner GetRandomReadySpawner()
    {
        List<EnemySpawner> ready = new();

        foreach (var spawner in spawners)
        {
            if (spawner.IsReady)
                ready.Add(spawner);
        }

        if (ready.Count == 0)
            return null;

        return ready[Random.Range(0, ready.Count)];
    }

    private EnemyData GetNextEnemy(WaveInstance wave)
    {
        if (wave.spawnQueue.Count == 0) return null;
        return wave.spawnQueue.Dequeue();
    }

    public void OnEnemyKilled(EnemyAI enemy, Vector3 position)
    {
        WaveInstance wave = enemy.GetWaveInstance();
        if (wave == null)
        {
            Debug.LogWarning($"Enemy {enemy.name} died but had NO wave assigned!");
            return;
        }

        wave.aliveEnemies--;
        wave.lastDeathPosition = position;
    }

    [SerializeField] private LayerMask groundLayer;

    private void EndWave(WaveInstance wave)
    {
        Debug.Log($"Wave {wave.waveNumber} completed.");

        if (wavePopupUI != null)
            wavePopupUI.Show($"Wave {wave.waveNumber} Completed", WavePopupType.WaveCompleted);

        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.AddGold(wave.data.goldReward);

        if (wave.data.rewardPrefab != null)
        {
            Vector3 origin = wave.lastDeathPosition + Vector3.up * 2f;

            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 10f, groundLayer))
            {
                Vector3 forwardOnGround = Vector3.ProjectOnPlane(wave.lastDeathRotation * Vector3.forward, hit.normal).normalized;
                Quaternion rot = Quaternion.LookRotation(forwardOnGround, hit.normal);
                Instantiate(wave.data.rewardPrefab, hit.point + hit.normal * 0.5f, rot * Quaternion.Euler(0f, 180f, 0f));
            }
        }

        _activeWaves.Remove(wave);
    }

}


