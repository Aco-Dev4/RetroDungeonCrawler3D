using UnityEngine;

public class PlayerSavedColorLoader : MonoBehaviour
{
    [SerializeField] private ShopColorDatabase colorDatabase;
    [SerializeField] private PlayerColorApplier playerColorApplier;

    private void Start()
    {
        ApplySavedColor();
    }

    public void ApplySavedColor()
    {
        if (GameDataManager.Instance == null) return;
        if (colorDatabase == null) return;
        if (playerColorApplier == null) return;

        string selectedColorId = GameDataManager.Instance.GetSelectedColor();
        ShopColorData colorData = colorDatabase.GetColor(selectedColorId);

        if (colorData != null)
            playerColorApplier.ApplyShopColor(colorData);
    }
}