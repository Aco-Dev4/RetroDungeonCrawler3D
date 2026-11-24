using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class HealthFeedback
{
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ToonColorId = Shader.PropertyToID("_Color");
    private static readonly int FirstShadeId = Shader.PropertyToID("_1st_ShadeColor");
    private static readonly int SecondShadeId = Shader.PropertyToID("_2nd_ShadeColor");
    private static readonly int RimColorId = Shader.PropertyToID("_RimLightColor");

    private readonly Health _owner;
    private readonly HealthFeedbackSettings _settings;
    private readonly List<Material> _materials = new();
    private readonly List<TintedSlot> _slots = new();

    private Renderer[] _renderers;
    private MaterialPropertyBlock _tint;
    private Coroutine _flash;
    private ParticleSystem _healEffect;
    private int _tintedCount;
    private float _nextHealTime;

    private sealed class TintedSlot
    {
        public Renderer renderer;
        public int materialIndex;
        public readonly MaterialPropertyBlock original = new();
    }

    public HealthFeedback(Health owner, HealthFeedbackSettings settings)
    {
        _owner = owner;
        _settings = settings;
    }

    public void PlayHeal()
    {
        if (_settings.healEffectPrefab == null || _healEffect != null || Time.unscaledTime < _nextHealTime) return;

        _nextHealTime = Time.unscaledTime + _settings.healCooldown;
        Camera camera = Camera.main;
        Quaternion rotation = camera != null ? camera.transform.rotation : Quaternion.identity;
        Vector3 position = _owner.transform.position + Vector3.up * _settings.healHeight;
        _healEffect = Object.Instantiate(_settings.healEffectPrefab, position, rotation);
    }

    public void PlayDamageFlash()
    {
        if (!_owner.isActiveAndEnabled || _settings.damageDuration <= 0f || _settings.damageStrength <= 0f)
            return;

        if (_flash != null)
            _owner.StopCoroutine(_flash);
        else
            ApplyTint();

        if (_tintedCount > 0)
            _flash = _owner.StartCoroutine(RestoreAfterFlash());
    }

    public void StopDamageFlash()
    {
        if (_flash != null)
            _owner.StopCoroutine(_flash);
        RestoreTint();
    }

    private void ApplyTint()
    {
        _tint ??= new MaterialPropertyBlock();
        _renderers ??= _owner.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer renderer in _renderers)
        {
            if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy ||
                renderer is not (SkinnedMeshRenderer or MeshRenderer)) continue;
            renderer.GetSharedMaterials(_materials);
            for (int i = 0; i < _materials.Count; i++)
            {
                Material material = _materials[i];
                if (material == null || !material.HasProperty(BaseColorId)) continue;

                if (_tintedCount == _slots.Count)
                    _slots.Add(new TintedSlot());

                TintedSlot slot = _slots[_tintedCount++];
                slot.renderer = renderer;
                slot.materialIndex = i;
                renderer.GetPropertyBlock(slot.original, i);

                if (slot.original.isEmpty)
                    renderer.GetPropertyBlock(_tint);
                else
                    renderer.GetPropertyBlock(_tint, i);

                TintColor(material, BaseColorId, _settings.damageTint);
                if (_settings.tintToonShading)
                {
                    TintColor(material, ToonColorId, _settings.damageTint);
                    TintColor(material, FirstShadeId, _settings.damageTint * 0.76f);
                    TintColor(material, SecondShadeId, _settings.damageTint * 0.52f);
                    TintColor(material, RimColorId, _settings.damageTint);
                }
                renderer.SetPropertyBlock(_tint, i);
            }
        }
        _materials.Clear();
    }

    private void TintColor(Material material, int property, Color color)
    {
        if (!material.HasProperty(property)) return;

        Color original = _tint.HasColor(property) ? _tint.GetColor(property) : material.GetColor(property);
        Color tint = Color.Lerp(original, color, _settings.damageStrength);
        tint.a = original.a;
        _tint.SetColor(property, tint);
    }

    private IEnumerator RestoreAfterFlash()
    {
        yield return new WaitForSecondsRealtime(_settings.damageDuration);
        RestoreTint();
    }

    private void RestoreTint()
    {
        for (int i = 0; i < _tintedCount; i++)
        {
            TintedSlot slot = _slots[i];
            if (slot.renderer != null)
                slot.renderer.SetPropertyBlock(slot.original.isEmpty ? null : slot.original, slot.materialIndex);
            slot.renderer = null;
        }

        _tintedCount = 0;
        _flash = null;
    }
}
