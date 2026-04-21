using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class TutorialFlow : MonoBehaviour
{
    #region References
    [Header("Managers")]
    [SerializeField] private WaveManager waveManager;

    [Header("Objects To Remove After Waves")]
    [SerializeField] private GameObject firstBlockedObject;
    [SerializeField] private GameObject secondBlockedObject;

    [Header("UI")]
    [SerializeField] private GameObject upgradeInstructionRoot;
    [SerializeField] private TMP_Text upgradeInstructionText;

    [SerializeField] private GameObject tutorialCompletePopupRoot;
    [SerializeField] private TMP_Text tutorialCompleteText;
    #endregion

    #region Debug
    [Header("Debug")]
    [SerializeField] private Key completeTutorialKey = Key.T;
    #endregion

    #region Runtime
    private bool _wavesCompleted;
    private bool _tutorialCompleted;
    #endregion

    private void Awake()
    {
        if (upgradeInstructionRoot != null)
            upgradeInstructionRoot.SetActive(false);

        if (tutorialCompletePopupRoot != null)
            tutorialCompletePopupRoot.SetActive(false);
    }

    private void Update()
    {
        if (!_wavesCompleted) return;
        if (_tutorialCompleted) return;
        if (Keyboard.current == null) return;
        if (!Keyboard.current[completeTutorialKey].wasPressedThisFrame) return;

        CompleteTutorialFromDebugKey();
    }

    private void OnEnable()
    {
        if (waveManager != null)
            waveManager.OnAllWavesCompleted += HandleAllWavesCompleted;
    }

    private void OnDisable()
    {
        if (waveManager != null)
            waveManager.OnAllWavesCompleted -= HandleAllWavesCompleted;
    }

    #region Wave Completion
    private void HandleAllWavesCompleted()
    {
        _wavesCompleted = true;

        if (firstBlockedObject != null)
            Destroy(firstBlockedObject);

        if (secondBlockedObject != null)
            Destroy(secondBlockedObject);

        if (upgradeInstructionRoot != null)
            upgradeInstructionRoot.SetActive(true);

        if (upgradeInstructionText != null)
            upgradeInstructionText.text = "Upgrade a Card of Your choice in the Card Upgrade Table!";
    }
    #endregion

    #region Temporary Debug Completion
    private void CompleteTutorialFromDebugKey()
    {
        _tutorialCompleted = true;

        if (tutorialCompletePopupRoot != null)
            tutorialCompletePopupRoot.SetActive(true);

        if (tutorialCompleteText != null)
            tutorialCompleteText.text = "Tutorial <color=#66FF66>Complete</color>!";

        GameManager.Instance?.PauseGame();

        if (GameDataManager.Instance != null)
            GameDataManager.Instance.SetFinishedTutorial(true);
    }
    #endregion
}