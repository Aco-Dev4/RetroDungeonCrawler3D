using UnityEngine;

public class RunDifficultyManager : MonoBehaviour
{
    public static RunDifficultyManager Instance;

    #region Difficulty
    [Header("Difficulty")]
    [SerializeField] private RunDifficulty difficulty = RunDifficulty.Normal;

    [Header("Hard Mode Enemy Multipliers")]
    [SerializeField] private float hardHealthMultiplier = 1.35f;
    [SerializeField] private float hardDamageMultiplier = 1.2f;
    [SerializeField] private float hardMoveSpeedMultiplier = 1.1f;
    [SerializeField] private float hardAttackSpeedMultiplier = 1.1f;

    [Header("Hard Mode Scaling Per Wave")]
    [SerializeField] private float healthMultiplierPerWave = 0.05f;
    [SerializeField] private float damageMultiplierPerWave = 0.03f;
    [SerializeField] private float moveSpeedMultiplierPerWave = 0.01f;
    [SerializeField] private float attackSpeedMultiplierPerWave = 0.02f;

    [Header("Mini Boss")]
    [SerializeField] private float miniBossScaleMultiplier = 1.15f;
    [SerializeField] private float miniBossStatBoostMultiplier = 2f;
    [SerializeField] private float miniBossStatNerfMultiplier = 0.75f;

    [Header("Hard Mode First Clear")]
    [SerializeField] private int hardFirstClearGoldReward = 20;
    [SerializeField] private int hardRepeatClearGoldReward = 5;
    #endregion

    #region Unity
    private void Awake()
    {
        Instance = this;
        difficulty = SelectedRunSettings.Difficulty;
    }
    #endregion

    #region Public
    public RunDifficulty GetDifficulty()
    {
        return difficulty;
    }

    public bool IsHardMode()
    {
        return difficulty == RunDifficulty.Hard;
    }

    public float GetHealthMultiplier(int waveNumber)
    {
        if (!IsHardMode()) return 1f;

        int waveIndex = Mathf.Max(0, waveNumber - 1);
        return hardHealthMultiplier + healthMultiplierPerWave * waveIndex;
    }

    public float GetDamageMultiplier(int waveNumber)
    {
        if (!IsHardMode()) return 1f;

        int waveIndex = Mathf.Max(0, waveNumber - 1);
        return hardDamageMultiplier + damageMultiplierPerWave * waveIndex;
    }

    public float GetMoveSpeedMultiplier(int waveNumber)
    {
        if (!IsHardMode()) return 1f;

        int waveIndex = Mathf.Max(0, waveNumber - 1);
        return hardMoveSpeedMultiplier + moveSpeedMultiplierPerWave * waveIndex;
    }

    public float GetAttackSpeedMultiplier(int waveNumber)
    {
        if (!IsHardMode()) return 1f;

        int waveIndex = Mathf.Max(0, waveNumber - 1);
        return hardAttackSpeedMultiplier + attackSpeedMultiplierPerWave * waveIndex;
    }

    public int GetHardFirstClearGoldReward()
    {
        return hardFirstClearGoldReward;
    }

    public int GetHardRepeatClearGoldReward()
    {
        return hardRepeatClearGoldReward;
    }

    public float GetMiniBossScaleMultiplier()
    {
        return miniBossScaleMultiplier;
    }

    public float GetMiniBossStatBoostMultiplier()
    {
        return miniBossStatBoostMultiplier;
    }

    public float GetMiniBossStatNerfMultiplier()
    {
        return miniBossStatNerfMultiplier;
    }

    public void GetRandomMiniBossBoosts(out MiniBossStatType firstBoost, out MiniBossStatType secondBoost)
    {
        MiniBossStatType[] values =
        {
        MiniBossStatType.Health,
        MiniBossStatType.Damage,
        MiniBossStatType.MoveSpeed,
        MiniBossStatType.AttackSpeed
    };

        int firstIndex = Random.Range(0, values.Length);
        int secondIndex = Random.Range(0, values.Length);

        while (secondIndex == firstIndex)
            secondIndex = Random.Range(0, values.Length);

        firstBoost = values[firstIndex];
        secondBoost = values[secondIndex];
    }
    #endregion
}