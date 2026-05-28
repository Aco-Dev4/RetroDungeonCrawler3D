using UnityEngine;

public class RunStatsManager : MonoBehaviour
{
    public static RunStatsManager Instance;

    public int GoldGained { get; private set; }
    public int SilverGained { get; private set; }
    public int EnemiesDefeated { get; private set; }
    public int WavesCompleted { get; private set; }
    public int CurrentWave { get; private set; }
    public int CardsUpgraded { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void AddGold(int amount)
    {
        GoldGained += amount;
    }

    public void AddSilver(int amount)
    {
        SilverGained += amount;
    }

    public void AddEnemyDefeated()
    {
        EnemiesDefeated++;
    }

    public void SetCurrentWave(int wave)
    {
        CurrentWave = wave;
    }

    public void AddWaveCompleted()
    {
        WavesCompleted++;
    }

    public void AddCardUpgraded()
    {
        CardsUpgraded++;
    }
}