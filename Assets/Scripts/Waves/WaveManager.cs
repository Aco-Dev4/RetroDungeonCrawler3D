using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance;

    #region References
    [Header("Waves")]
    [SerializeField] private List<WaveData> waves = new();

    [Header("Spawners")]
    [SerializeField] private List<EnemySpawner> spawners = new();

    [Header("UI")]
    [SerializeField] private TutorialFlow tutorialFlow;
    [SerializeField] private WavePopupUI wavePopupUI;
    [SerializeField] private WaveCounterUI waveCounterUI;
    [SerializeField] private NextWavePromptUI nextWavePromptUI;
    [SerializeField] private FinishMapPromptUI finishPromptUI;
    [SerializeField] private string mapName = "Forest";

    [Header("Upgrade Table Arrow")]
    [SerializeField] private UpgradeTableArrow upgradeTableArrow;
    [SerializeField] private bool isTutorial;
    [SerializeField] private int tutorialArrowUnlockWave = 2;

    [Header("Chest Spawn")]
    [SerializeField] private LayerMask groundLayer;
    #endregion

    #region Runtime
    private readonly List<WaveInstance> _activeWaves = new();

    private int _nextWaveIndex;
    private int _completedWaveCount;

    private bool _allWavesCompletedTriggered;
    private bool _tutorialReadyToFinish;

    public Action OnAllWavesCompleted;
    #endregion

    #region Unity
    private void Awake()
    {
        Instance = this;

        waveCounterUI?.SetWave(0, waves.Count);
        upgradeTableArrow?.HideArrow();

        UpdateNextWavePrompt();
    }
    #endregion

    #region Public
    public void OnNextWave(InputAction.CallbackContext context)
    {
        if (!context.started) return;

        if (TryHandleFinishPrompt())
            return;

        StartNextWave();
    }

    public int GetCompletedWaveCount()
    {
        return _completedWaveCount;
    }

    public void OnEnemyKilled(EnemyAI enemy, Vector3 position)
    {
        if (enemy == null) return;

        WaveInstance wave = enemy.GetWaveInstance();

        if (wave == null)
        {
            Debug.LogWarning($"Enemy {enemy.name} died but had NO wave assigned!");
            return;
        }

        wave.UnregisterEnemy(enemy, position, enemy.transform.rotation);
    }

    public void OnTutorialUpgradeCompleted()
    {
        if (!isTutorial) return;

        _tutorialReadyToFinish = true;
    }

    public void TryShowTutorialFinish()
    {
        if (!isTutorial) return;
        if (!_tutorialReadyToFinish) return;

        finishPromptUI?.Show(mapName);
    }
    #endregion

    #region Wave Flow
    private void StartNextWave()
    {
        if (_nextWaveIndex >= waves.Count)
        {
            //Debug.Log("All waves have already been started.");
            return;
        }

        WaveData waveData = waves[_nextWaveIndex];
        int waveNumber = _nextWaveIndex + 1;

        _nextWaveIndex++;

        WaveInstance wave = new WaveInstance(waveData, waveNumber);
        _activeWaves.Add(wave);

        RunStatsManager.Instance?.SetCurrentWave(waveNumber);
        waveCounterUI?.SetWave(waveNumber, waves.Count);
        wavePopupUI?.Show($"Wave {waveNumber} Started", WavePopupType.WaveStarted);
        AudioManager.Instance?.PlaySFX("WaveStart");
        upgradeTableArrow?.HideArrow();

        //Debug.Log($"Wave {waveNumber} started.");

        StartCoroutine(SpawnLoop(wave));
        UpdateNextWavePrompt();
    }

    private IEnumerator SpawnLoop(WaveInstance wave)
    {
        while (!wave.IsCompleted)
        {
            if (GameManager.Instance != null && GameManager.Instance.State == GameState.GameOver)
                yield break;

            TrySpawnEnemy(wave);

            wave.aliveEnemies = wave.GetAliveEnemyCount();

            yield return null;
        }

        EndWave(wave);
    }

    private void EndWave(WaveInstance wave)
    {
        if (wave == null) return;

        //Debug.Log($"Wave {wave.waveNumber} completed.");

        _completedWaveCount++;

        RunStatsManager.Instance?.AddWaveCompleted();

        ApplyPlayerWaveCompletedEffects();
        ShowWaveCompletedPopup(wave);
        AudioManager.Instance?.PlaySFX("WaveEnd");
        GiveWaveGoldReward(wave);
        SpawnRewardChest(wave);

        _activeWaves.Remove(wave);

        UpdateNextWavePrompt();
        TryCompleteAllWaves();
        UpdateUpgradeArrow();
    }
    #endregion

    #region Spawning
    private void TrySpawnEnemy(WaveInstance wave)
    {
        if (wave == null) return;
        if (wave.remainingToSpawn <= 0) return;
        if (wave.GetAliveEnemyCount() >= wave.data.maxAliveEnemies) return;

        EnemySpawner spawner = GetRandomReadySpawner();

        if (spawner == null)
            return;

        EnemySpawnData spawnData = GetNextEnemy(wave);

        if (spawnData == null || spawnData.enemyData == null)
        {
            Debug.LogWarning($"Wave {wave.waveNumber} had missing spawn data. Forcing remaining spawn count down.");
            wave.ForceNoRemainingEnemiesToSpawn();
            return;
        }

        GameObject spawnedEnemy = spawner.TrySpawn(spawnData.enemyData);
        if (spawnedEnemy == null) return;

        EnemyAI enemyAI = spawnedEnemy.GetComponent<EnemyAI>();

        if (enemyAI == null)
        {
            Debug.LogError($"{spawnedEnemy.name} spawned without EnemyAI!");
            Destroy(spawnedEnemy);
            return;
        }

        enemyAI.InitWave(wave);

        if (spawnData.isMiniBoss)
            InitMiniBoss(enemyAI);

        wave.remainingToSpawn--;
        wave.RegisterEnemy(enemyAI);
    }

    private void InitMiniBoss(EnemyAI enemyAI)
    {
        if (enemyAI == null) return;
        if (RunDifficultyManager.Instance == null) return;

        RunDifficultyManager.Instance.GetRandomMiniBossBoosts(out MiniBossStatType firstBoost, out MiniBossStatType secondBoost);

        enemyAI.InitMiniBoss(
            RunDifficultyManager.Instance.GetMiniBossScaleMultiplier(),
            RunDifficultyManager.Instance.GetMiniBossStatBoostMultiplier(),
            RunDifficultyManager.Instance.GetMiniBossStatNerfMultiplier(),
            firstBoost,
            secondBoost
        );
    }

    private EnemySpawner GetRandomReadySpawner()
    {
        List<EnemySpawner> readySpawners = new();

        foreach (EnemySpawner spawner in spawners)
        {
            if (spawner != null && spawner.IsReady)
                readySpawners.Add(spawner);
        }

        if (readySpawners.Count == 0)
            return null;

        int index = UnityEngine.Random.Range(0, readySpawners.Count);
        return readySpawners[index];
    }

    private EnemySpawnData GetNextEnemy(WaveInstance wave)
    {
        if (wave == null) return null;
        if (wave.spawnQueue.Count == 0) return null;

        return wave.spawnQueue.Dequeue();
    }
    #endregion

    #region Rewards
    private void GiveWaveGoldReward(WaveInstance wave)
    {
        if (CurrencyManager.Instance == null) return;

        if (isTutorial)
        {
            TryGiveTutorialGold();
            return;
        }

        CurrencyManager.Instance.AddGold(wave.data.goldReward);
    }

    private void TryGiveTutorialGold()
    {
        if (GameDataManager.Instance == null) return;
        if (GameDataManager.Instance.HasClaimedTutorialGold()) return;

        CurrencyManager.Instance.AddGold(1);
        GameDataManager.Instance.ClaimTutorialGold();
    }

    private void SpawnRewardChest(WaveInstance wave)
    {
        if (wave == null) return;
        if (wave.data.rewardPrefab == null) return;

        Vector3 rayOrigin = wave.lastDeathPosition + Vector3.up * 2f;

        if (!Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 10f, groundLayer))
            return;

        Vector3 forwardOnGround = Vector3.ProjectOnPlane(wave.lastDeathRotation * Vector3.forward, hit.normal).normalized;

        if (forwardOnGround.sqrMagnitude <= 0.01f)
            forwardOnGround = Vector3.ProjectOnPlane(transform.forward, hit.normal).normalized;

        Quaternion groundRotation = Quaternion.LookRotation(forwardOnGround, hit.normal);
        Vector3 spawnPosition = hit.point + hit.normal * 0.5f;
        Quaternion spawnRotation = groundRotation * Quaternion.Euler(0f, 180f, 0f);

        GameObject chestObject = Instantiate(wave.data.rewardPrefab, spawnPosition, spawnRotation);
        RewardChest chest = chestObject.GetComponent<RewardChest>();

        if (chest != null)
            chest.SetWaveNumber(wave.waveNumber);
    }

    private void GiveHardModeClearReward()
    {
        if (RunDifficultyManager.Instance == null) return;
        if (!RunDifficultyManager.Instance.IsHardMode()) return;
        if (GameDataManager.Instance == null) return;
        if (CurrencyManager.Instance == null) return;

        string hardMapId = $"{mapName}_Hard";
        bool alreadyCompletedHard = GameDataManager.Instance.HasCompletedHardMap(hardMapId);

        int reward = alreadyCompletedHard
            ? RunDifficultyManager.Instance.GetHardRepeatClearGoldReward()
            : RunDifficultyManager.Instance.GetHardFirstClearGoldReward();

        if (reward > 0)
            CurrencyManager.Instance.AddGold(reward);

        if (!alreadyCompletedHard)
            GameDataManager.Instance.SetHardMapCompleted(hardMapId);
    }
    #endregion

    #region UI / Completion
    private bool TryHandleFinishPrompt()
    {
        if (finishPromptUI == null || !finishPromptUI.IsVisible())
            return false;

        if (isTutorial)
        {
            tutorialFlow?.CompleteTutorial();
            return true;
        }

        finishPromptUI.Hide();
        GiveHardModeClearReward();
        VictoryUI.Instance?.Show(mapName);
        return true;
    }

    private void UpdateNextWavePrompt()
    {
        if (nextWavePromptUI == null) return;

        bool canStartMoreWaves = _nextWaveIndex < waves.Count;
        bool hasActiveWaves = _activeWaves.Count > 0;
        int nextWaveNumber = _nextWaveIndex + 1;

        if (isTutorial && nextWaveNumber <= 1)
            return;

        nextWavePromptUI.SetPrompt(nextWaveNumber, hasActiveWaves, canStartMoreWaves);
    }

    private void ShowWaveCompletedPopup(WaveInstance wave)
    {
        wavePopupUI?.Show($"Wave {wave.waveNumber} Completed", WavePopupType.WaveCompleted);
    }

    private void UpdateUpgradeArrow()
    {
        if (upgradeTableArrow == null) return;

        bool requiredWaveReached = _completedWaveCount >= (isTutorial ? tutorialArrowUnlockWave : 1);
        bool noActiveWaves = _activeWaves.Count == 0;

        if (requiredWaveReached && noActiveWaves)
            upgradeTableArrow.ShowArrow();
        else
            upgradeTableArrow.HideArrow();
    }

    private void TryCompleteAllWaves()
    {
        if (_allWavesCompletedTriggered) return;
        if (_nextWaveIndex < waves.Count) return;
        if (_activeWaves.Count > 0) return;

        _allWavesCompletedTriggered = true;

        //Debug.Log("All waves completed.");

        if (!isTutorial)
            finishPromptUI?.Show(mapName);

        OnAllWavesCompleted?.Invoke();
    }
    #endregion

    #region Player Effects
    private void ApplyPlayerWaveCompletedEffects()
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        player?.OnWaveCompleted();
    }
    #endregion
}