using TMPro;
using UnityEngine;

public class FinishMapPromptUI : MonoBehaviour
{
    [SerializeField] private GameObject rootPanel;
    [SerializeField] private TMP_Text promptText;

    private bool _isVisible;

    private void Awake()
    {
        Hide();
    }

    public void Show(string mapName)
    {
        _isVisible = true;

        if (rootPanel != null)
            rootPanel.SetActive(true);

        if (promptText != null)
            promptText.text = $"PRESS 'TAB' TO FINISH {mapName.ToUpper()}";
    }

    public void Hide()
    {
        _isVisible = false;

        if (rootPanel != null)
            rootPanel.SetActive(false);
    }

    public bool IsVisible()
    {
        return _isVisible;
    }
}