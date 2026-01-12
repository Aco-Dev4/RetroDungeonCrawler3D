using System.Collections.Generic;
using UnityEngine;

public class WaveInstance
{
    public WaveData data;

    public int remainingToSpawn;
    public int aliveEnemies;
    public int waveNumber;

    public Dictionary<EnemyData, int> spawnQueue = new();

    public bool IsCompleted => remainingToSpawn <= 0 && aliveEnemies <= 0;

    public Vector3 lastDeathPosition;

    public WaveInstance(WaveData waveData, int number)
    {
        this.data = waveData;
        this.waveNumber = number;

        remainingToSpawn = 0;
        aliveEnemies = 0;

        foreach (var entry in waveData.enemies)
        {
            spawnQueue[entry.enemy] = entry.count;
            remainingToSpawn += entry.count;
        }
    }
}
