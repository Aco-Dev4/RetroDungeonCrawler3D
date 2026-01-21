using UnityEngine;

public class ChestRewardUI : MonoBehaviour
{
    public static ChestRewardUI Instance;

    [SerializeField] private GameObject rootPanel;

    private RewardChest _currentChest;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        rootPanel.SetActive(false);
    }

    public void Open(RewardChest chest)
    {
        _currentChest = chest;
        rootPanel.SetActive(true);
        GameManager.Instance.PauseGame();
    }

    // Called by X button
    public void Close()
    {
        rootPanel.SetActive(false);
        GameManager.Instance.ResumeGame();

        if (_currentChest != null)
        {
            _currentChest.OnRewardUIClosed();
            _currentChest = null;
        }
    }
}
