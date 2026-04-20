using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    #region References
    [Header("Panels")]
    [SerializeField] private GameObject mapSelectPanel;

    [Header("Map Buttons")]
    [SerializeField] private List<MenuMapButton> mapButtons = new();
    #endregion

    private void Start()
    {
        if (mapSelectPanel != null)
            mapSelectPanel.SetActive(false);

        RefreshMapButtons();
    }

    #region Buttons
    public void OnStartPressed()
    {
        if (mapSelectPanel != null)
            mapSelectPanel.SetActive(true);

        RefreshMapButtons();
    }

    public void OnCloseMapSelectPressed()
    {
        if (mapSelectPanel != null)
            mapSelectPanel.SetActive(false);
    }

    public void RefreshMapButtons()
    {
        for (int i = 0; i < mapButtons.Count; i++)
        {
            if (mapButtons[i] != null)
                mapButtons[i].RefreshState();
        }
    }
    #endregion
}
