using System;
using UnityEngine;

[Serializable]
public class ShopColorData
{
    public string colorId;
    public string displayName;
    public Color color = Color.white;
    public Material materialOverride;
    public Sprite icon;
    public int cost = 1;
}