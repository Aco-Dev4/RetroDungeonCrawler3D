using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MenuManager : MonoBehaviour
{
    #region References
    [Header("Panels")]
    [SerializeField] private GameObject mapSelectPanel;

    [Header("Map Buttons")]
    [SerializeField] private List<MenuMapButton> mapButtons = new();

    [Header("Shop")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private MenuShopCamera shopCamera;
    #endregion

    [Header("Debug")]
    [SerializeField] private Key giveGoldKey = Key.G;
    [SerializeField] private int debugGoldAmount = 10;

    private void Start()
    {
        if (mapSelectPanel != null)
            mapSelectPanel.SetActive(false);

        RefreshMapButtons();
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current[giveGoldKey].wasPressedThisFrame)
        {
            CurrencyManager.Instance?.AddGold(debugGoldAmount);
            Debug.Log($"+{debugGoldAmount} Gold");
        }
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

    public void OnShopPressed()
    {
        if (shopPanel != null)
            shopPanel.SetActive(true);

        if (mapSelectPanel != null)
            mapSelectPanel.SetActive(false);

        shopCamera?.MoveToShopView();
    }

    public void OnCloseShopPressed()
    {
        if (shopPanel != null)
            shopPanel.SetActive(false);

        shopCamera?.MoveToNormalView();
    }

    public void OnSwordBought()
    {
        GameDataManager.Instance?.UnlockSword();
        Debug.Log("Sword bought");
    }
    #endregion
}
