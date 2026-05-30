using UnityEngine;

public class PlayerColorApplier : MonoBehaviour
{
    [Header("Renderers")]
    [SerializeField] private Renderer[] colorRenderers;

    [Header("Material Slot")]
    [SerializeField] private int colorMaterialIndex = 4;

    [Header("Shader Property Names")]
    [SerializeField] private string baseColorProperty = "_BaseColor";
    [SerializeField] private string rimColorProperty = "_RimLightColor";

    private Material[] _originalMaterials;

    private void Awake()
    {
        _originalMaterials = new Material[colorRenderers.Length];

        for (int i = 0; i < colorRenderers.Length; i++)
        {
            if (colorRenderers[i] == null) continue;

            Material[] materials = colorRenderers[i].materials;
            if (colorMaterialIndex < 0 || colorMaterialIndex >= materials.Length) continue;

            _originalMaterials[i] = materials[colorMaterialIndex];
        }
    }

    public void ApplyColor(Color color)
    {
        for (int i = 0; i < colorRenderers.Length; i++)
        {
            if (colorRenderers[i] == null) continue;

            Material[] materials = colorRenderers[i].materials;
            if (colorMaterialIndex < 0 || colorMaterialIndex >= materials.Length) continue;

            if (_originalMaterials != null && i < _originalMaterials.Length && _originalMaterials[i] != null)
                materials[colorMaterialIndex] = _originalMaterials[i];

            Material mat = materials[colorMaterialIndex];

            if (mat.HasProperty(baseColorProperty))
                mat.SetColor(baseColorProperty, color);

            if (mat.HasProperty(rimColorProperty))
                mat.SetColor(rimColorProperty, color);

            colorRenderers[i].materials = materials;
        }
    }

    public void ApplyMaterial(Material material)
    {
        if (material == null) return;

        for (int i = 0; i < colorRenderers.Length; i++)
        {
            if (colorRenderers[i] == null) continue;

            Material[] materials = colorRenderers[i].materials;
            if (colorMaterialIndex < 0 || colorMaterialIndex >= materials.Length) continue;

            materials[colorMaterialIndex] = material;
            colorRenderers[i].materials = materials;
        }
    }

    public void ApplyShopColor(ShopColorData colorData)
    {
        if (colorData == null) return;

        if (colorData.materialOverride != null)
            ApplyMaterial(colorData.materialOverride);
        else
            ApplyColor(colorData.color);
    }
}