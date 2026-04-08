using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Game/Wave Data")]
public class WaveData : ScriptableObject
{
    [Header("Wave Settings")]
    public int maxAliveEnemies = 3;

    [Header("Enemies in this Wave")]
    public List<WaveEnemyEntry> enemies = new();

    [Header("Rewards")]
    public GameObject rewardPrefab;
    public int goldReward = 5;
}
