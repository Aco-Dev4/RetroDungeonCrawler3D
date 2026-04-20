using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class PersistentPlayerData
{
    public bool finishedTutorial = false;
    public int gold = 0;

    public float sensitivity = 1f;
    public int qualityLevel = 2;

    public List<string> boughtUpgradeIds = new();
}

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance;

    #region Data
    [Header("Debug")]
    [SerializeField] private bool createFreshDataOnAwake = false;

    private PersistentPlayerData _data;
    private string SavePath => Path.Combine(Application.persistentDataPath, "playerdata.json");
    #endregion

    #region Unity
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (createFreshDataOnAwake)
        {
            _data = new PersistentPlayerData();
            Save();
        }
        else
        {
            Load();
        }

        ApplySavedSettings();
    }
    #endregion

    #region Save Load
    public void Save()
    {
        string json = JsonUtility.ToJson(_data, true);
        File.WriteAllText(SavePath, json);
    }

    public void Load()
    {
        if (File.Exists(SavePath))
        {
            string json = File.ReadAllText(SavePath);
            _data = JsonUtility.FromJson<PersistentPlayerData>(json);
        }
        else
        {
            _data = new PersistentPlayerData();
            Save();
        }

        if (_data == null)
            _data = new PersistentPlayerData();
    }

    [ContextMenu("Reset Saved Data")]
    public void ResetSavedData()
    {
        _data = new PersistentPlayerData();
        Save();
        ApplySavedSettings();
        Debug.Log("Saved player data has been reset.");
    }
    #endregion

    #region Settings
    public void SetSensitivity(float value)
    {
        _data.sensitivity = value;
        Save();
    }

    public float GetSensitivity()
    {
        return _data.sensitivity;
    }

    public void SetQualityLevel(int value)
    {
        _data.qualityLevel = value;
        QualitySettings.SetQualityLevel(value);
        Save();
    }

    public int GetQualityLevel()
    {
        return _data.qualityLevel;
    }

    private void ApplySavedSettings()
    {
        QualitySettings.SetQualityLevel(_data.qualityLevel);
    }
    #endregion

    #region Gold
    public int GetGold()
    {
        return _data.gold;
    }

    public void SetGold(int value)
    {
        _data.gold = Mathf.Max(0, value);
        Save();
    }

    public void AddGold(int amount)
    {
        _data.gold = Mathf.Max(0, _data.gold + amount);
        Save();
    }

    public bool SpendGold(int amount)
    {
        if (_data.gold < amount)
            return false;

        _data.gold -= amount;
        Save();
        return true;
    }
    #endregion

    #region Tutorial
    public bool HasFinishedTutorial()
    {
        return _data.finishedTutorial;
    }

    public void SetFinishedTutorial(bool value)
    {
        _data.finishedTutorial = value;
        Save();
    }
    #endregion

    #region Shop Upgrades
    public bool HasBoughtUpgrade(string upgradeId)
    {
        return _data.boughtUpgradeIds.Contains(upgradeId);
    }

    public bool BuyUpgrade(string upgradeId, int cost)
    {
        if (HasBoughtUpgrade(upgradeId))
            return false;

        if (!SpendGold(cost))
            return false;

        _data.boughtUpgradeIds.Add(upgradeId);
        Save();
        return true;
    }
    #endregion
}