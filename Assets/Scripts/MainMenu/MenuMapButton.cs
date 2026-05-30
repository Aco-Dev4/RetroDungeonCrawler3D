using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MenuMapButton : MonoBehaviour
{
    #region References
    [Header("References")]
    [SerializeField] private MenuManager menuManager;
    [SerializeField] private Button button;
    [SerializeField] private GameObject lockedVisual;
    [SerializeField] private TMP_Text stateText;
    #endregion

    #region Map Settings
    [Header("Map Settings")]
    [SerializeField] private string sceneName;
    [SerializeField] private string displayName = "Map";

    [SerializeField] private bool requiresFinishedTutorial;
    [SerializeField] private bool markTutorialFinishedForTesting;
    [SerializeField] private bool alwaysLocked;
    [SerializeField] private bool loadSceneOnClick = true;
    #endregion

    #region Text Colors
    [Header("Text Colors")]
    [SerializeField] private Color lockedColor = Color.red;
    [SerializeField] private Color unlockedColor = Color.green;
    #endregion

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();
    }

    private void Start()
    {
        RefreshState();
    }

    public void RefreshState()
    {
        if (GameDataManager.Instance == null)
            return;

        bool unlocked = !alwaysLocked &&
                        (!requiresFinishedTutorial || GameDataManager.Instance.HasFinishedTutorial());

        if (button != null)
            button.interactable = unlocked || markTutorialFinishedForTesting;

        SetLockedVisualVisible(!unlocked);

        if (stateText != null)
        {
            stateText.text = unlocked ? displayName : "LOCKED";
            stateText.color = unlocked ? unlockedColor : lockedColor;
        }
    }

    private void SetLockedVisualVisible(bool visible)
    {
        if (lockedVisual == null)
            return;

        lockedVisual.SetActive(visible);

        // Extra safety for weird UI setups
        Graphic[] graphics = lockedVisual.GetComponentsInChildren<Graphic>(true);
        for (int i = 0; i < graphics.Length; i++)
            graphics[i].enabled = visible;

        CanvasGroup canvasGroup = lockedVisual.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.blocksRaycasts = visible;
            canvasGroup.interactable = visible;
        }
    }

    public void OnPressed()
    {
        if (GameDataManager.Instance == null)
            return;

        bool unlocked = !alwaysLocked &&
                        (!requiresFinishedTutorial || GameDataManager.Instance.HasFinishedTutorial());

        if (!unlocked && !markTutorialFinishedForTesting)
            return;

        if (markTutorialFinishedForTesting)
        {
            GameDataManager.Instance.SetFinishedTutorial(true);

            if (menuManager != null)
                menuManager.RefreshMapButtons();

            Debug.Log("Tutorial marked as finished for testing.");
            return;
        }

        if (loadSceneOnClick && !string.IsNullOrWhiteSpace(sceneName))
            SceneManager.LoadScene(sceneName);
    }
}