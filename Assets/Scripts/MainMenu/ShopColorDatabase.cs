using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShopColorDatabase", menuName = "Shop/Color Database")]
public class ShopColorDatabase : ScriptableObject
{
    public List<ShopColorData> colors = new();

    public ShopColorData GetColor(string colorId)
    {
        for (int i = 0; i < colors.Count; i++)
        {
            if (colors[i].colorId == colorId)
                return colors[i];
        }

        return null;
    }
}