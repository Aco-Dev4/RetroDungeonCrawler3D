using System.Collections.Generic;
using UnityEngine;

public class WaveInstance
{
    public WaveData data;

    public int remainingToSpawn;
    public int aliveEnemies;
    public int waveNumber;

    public bool IsCompleted => remainingToSpawn <= 0 && aliveEnemies <= 0;

    public Vector3 lastDeathPosition;

    public Queue<EnemyData> spawnQueue = new();

    public WaveInstance(WaveData waveData, int number)
    {
        this.data = waveData;
        this.waveNumber = number;

        remainingToSpawn = 0;
        aliveEnemies = 0;

        foreach (var entry in waveData.enemies)
        {
            for (int i = 0; i < entry.count; i++)
                spawnQueue.Enqueue(entry.enemy);

            remainingToSpawn += entry.count;
        }
    }
}
