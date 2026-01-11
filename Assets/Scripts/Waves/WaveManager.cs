using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    [Header("Waves")]
    [SerializeField] private List<WaveData> waves = new();

    [Header("Spawners")]
    [SerializeField] private List<EnemySpawner> spawners = new();

    private List<WaveInstance> _activeWaves = new();
    private int _nextWaveIndex = 0;

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.nKey.wasPressedThisFrame)
        {
            StartNextWave();
        }
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

        Debug.Log($"Wave {waveNumber} started.");

        StartCoroutine(SpawnLoop(instance));
    }

    private IEnumerator SpawnLoop(WaveInstance wave)
    {
        while (!wave.IsCompleted)
        {
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
                            if (ai != null)
                                ai.InitWave(wave);

                            wave.remainingToSpawn--;
                            wave.aliveEnemies++;
                        }
                    }
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
        foreach (var kvp in wave.spawnQueue)
        {
            if (kvp.Value > 0)
            {
                wave.spawnQueue[kvp.Key]--;
                return kvp.Key;
            }
        }

        return null;
    }

    public void OnEnemyKilled(EnemyAI enemy, Vector3 position)
    {
        WaveInstance wave = enemy.GetWaveInstance();
        if (wave == null) return;

        wave.aliveEnemies--;
        wave.lastDeathPosition = position;
    }

    private void EndWave(WaveInstance wave)
    {
        Debug.Log($"Wave {wave.waveNumber} completed.");

        if (wave.data.rewardPrefab != null)
        {
            Instantiate(wave.data.rewardPrefab, wave.lastDeathPosition, Quaternion.identity);
        }

        _activeWaves.Remove(wave);
    }
}


