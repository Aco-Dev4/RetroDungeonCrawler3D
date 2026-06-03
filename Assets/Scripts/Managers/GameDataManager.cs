using System.Collections.Generic;
using System.IO;
using UnityEngine;

#region Persistent Player Data
[System.Serializable]
public class PersistentPlayerData
{
    public bool finishedTutorial = false;
    public bool tutorialGoldClaimed;
    public int gold = 0;

    public float sensitivity = 1f;
    public int qualityLevel = 2;

    public float masterVolume = 0.5f;
    public float sfxVolume = 1f;
    public float musicVolume = 1f;
    public int resolutionIndex = -1;

    public List<string> boughtUpgradeIds = new();
    public List<string> completedMapIds = new();
    public List<string> completedHardMapIds = new();

    public List<string> upgradeTierIds = new();
    public List<int> upgradeTierValues = new();

    public string selectedColorId = "Blue";
    public List<string> ownedColorIds = new() { "Blue" };
}
#endregion

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
        //Debug.Log("Saved player data has been reset.");
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
        AudioListener.volume = _data.masterVolume;
    }

    public void SetMasterVolume(float value)
    {
        _data.masterVolume = Mathf.Clamp01(value);
        AudioListener.volume = _data.masterVolume;
        Save();
    }

    public float GetMasterVolume()
    {
        return _data.masterVolume;
    }

    public void SetSfxVolume(float value)
    {
        _data.sfxVolume = Mathf.Clamp01(value);
        Save();
    }

    public float GetSfxVolume()
    {
        return _data.sfxVolume;
    }

    public void SetMusicVolume(float value)
    {
        _data.musicVolume = Mathf.Clamp01(value);
        Save();
    }

    public float GetMusicVolume()
    {
        return _data.musicVolume;
    }

    public void SetResolutionIndex(int value)
    {
        _data.resolutionIndex = Mathf.Max(0, value);
        Save();
    }

    public int GetResolutionIndex()
    {
        return _data.resolutionIndex;
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

    public bool HasClaimedTutorialGold()
    {
        return _data.tutorialGoldClaimed;
    }

    public void ClaimTutorialGold()
    {
        _data.tutorialGoldClaimed = true;
        Save();
    }
    #endregion

    #region Completed Maps
    public bool HasCompletedMap(string mapId)
    {
        return _data.completedMapIds.Contains(mapId);
    }

    public void SetMapCompleted(string mapId)
    {
        if (string.IsNullOrWhiteSpace(mapId)) return;
        if (_data.completedMapIds.Contains(mapId)) return;

        _data.completedMapIds.Add(mapId);
        Save();
    }

    public bool HasCompletedHardMap(string mapId)
    {
        return _data.completedHardMapIds.Contains(mapId);
    }

    public void SetHardMapCompleted(string mapId)
    {
        if (string.IsNullOrWhiteSpace(mapId)) return;
        if (_data.completedHardMapIds.Contains(mapId)) return;

        _data.completedHardMapIds.Add(mapId);
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

    public int GetUpgradeTier(string upgradeId)
    {
        int index = _data.upgradeTierIds.IndexOf(upgradeId);

        if (index < 0)
            return 0;

        return _data.upgradeTierValues[index];
    }

    public void SetUpgradeTier(string upgradeId, int tier)
    {
        int index = _data.upgradeTierIds.IndexOf(upgradeId);

        if (index < 0)
        {
            _data.upgradeTierIds.Add(upgradeId);
            _data.upgradeTierValues.Add(tier);
        }
        else
        {
            _data.upgradeTierValues[index] = tier;
        }

        Save();
    }

    public bool HasSword()
    {
        return GetUpgradeTier("Sword") > 0;
    }

    public void UnlockSword()
    {
        if (_data.boughtUpgradeIds.Contains("Sword")) return;

        _data.boughtUpgradeIds.Add("Sword");
        Save();
    }

    public int GetExtraLuck()
    {
        int tier = GetUpgradeTier("ExtraLuck");

        if (tier <= 0) return 0;
        if (tier == 1) return 5;
        if (tier == 2) return 8;
        if (tier == 3) return 12;
        if (tier == 4) return 15;
        return 20;
    }

    public float GetExtraSilverMultiplier()
    {
        int tier = GetUpgradeTier("ExtraSilver");

        if (tier <= 0) return 0f;
        if (tier == 1) return 0.10f;
        if (tier == 2) return 0.20f;
        return 0.35f;
    }

    public bool HasUnlockedEpicCards()
    {
        return GetUpgradeTier("NewRarity") >= 1;
    }

    public bool HasUnlockedLegendaryCards()
    {
        return GetUpgradeTier("NewRarity") >= 2;
    }

    public bool HasUnlockedCriticalHits()
    {
        return GetUpgradeTier("CriticalHits") >= 1;
    }
    #endregion

    #region Player Colors
    public bool HasColor(string colorId)
    {
        return _data.ownedColorIds.Contains(colorId);
    }

    public void BuyColor(string colorId)
    {
        if (string.IsNullOrWhiteSpace(colorId)) return;
        if (_data.ownedColorIds.Contains(colorId)) return;

        _data.ownedColorIds.Add(colorId);
        Save();
    }

    public string GetSelectedColor()
    {
        return _data.selectedColorId;
    }

    public void SetSelectedColor(string colorId)
    {
        if (string.IsNullOrWhiteSpace(colorId)) return;
        if (!_data.ownedColorIds.Contains(colorId)) return;

        _data.selectedColorId = colorId;
        Save();
    }
    #endregion
}