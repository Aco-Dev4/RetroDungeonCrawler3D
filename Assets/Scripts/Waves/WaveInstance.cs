using System.Collections.Generic;
using UnityEngine;

public class WaveInstance
{
    public WaveData data;

    public int remainingToSpawn;
    public int aliveEnemies;
    public int waveNumber;

    public Vector3 lastDeathPosition;
    public Quaternion lastDeathRotation;

    public Queue<EnemySpawnData> spawnQueue = new();

    private readonly List<EnemyAI> _aliveEnemies = new();

    public bool IsCompleted => remainingToSpawn <= 0 && GetAliveEnemyCount() <= 0;

    public WaveInstance(WaveData waveData, int number)
    {
        this.data = waveData;
        this.waveNumber = number;

        remainingToSpawn = 0;
        aliveEnemies = 0;

        foreach (var entry in waveData.enemies)
        {
            if (entry == null || entry.enemy == null) continue;

            for (int i = 0; i < entry.count; i++)
                spawnQueue.Enqueue(new EnemySpawnData(entry.enemy, false));

            remainingToSpawn += entry.count;
        }

        TryAddHardModeMiniBoss();
    }

    public void RegisterEnemy(EnemyAI enemy)
    {
        if (enemy == null) return;
        if (_aliveEnemies.Contains(enemy)) return;

        _aliveEnemies.Add(enemy);
        aliveEnemies = GetAliveEnemyCount();
    }

    public void UnregisterEnemy(EnemyAI enemy, Vector3 deathPosition, Quaternion deathRotation)
    {
        if (enemy != null)
            _aliveEnemies.Remove(enemy);

        lastDeathPosition = deathPosition;
        lastDeathRotation = deathRotation;

        aliveEnemies = GetAliveEnemyCount();
    }

    public int GetAliveEnemyCount()
    {
        for (int i = _aliveEnemies.Count - 1; i >= 0; i--)
        {
            if (_aliveEnemies[i] == null)
                _aliveEnemies.RemoveAt(i);
        }

        return _aliveEnemies.Count;
    }

    public void ForceNoRemainingEnemiesToSpawn()
    {
        remainingToSpawn = 0;
    }

    private void TryAddHardModeMiniBoss()
    {
        if (RunDifficultyManager.Instance == null) return;
        if (!RunDifficultyManager.Instance.IsHardMode()) return;
        if (data == null || data.miniBossEnemy == null) return;

        spawnQueue.Enqueue(new EnemySpawnData(data.miniBossEnemy, true));
        remainingToSpawn++;
    }
}