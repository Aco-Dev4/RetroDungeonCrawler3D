using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerVignetteController : MonoBehaviour
{
    [SerializeField] private Volume volume;

    private Vignette _vignette;

    private void Awake()
    {
        if (volume.profile.TryGet(out _vignette))
            _vignette.intensity.value = 0f;
    }

    public void SetHealthNormalized(float health01)
    {
        if (_vignette == null) return;

        _vignette.intensity.value = GetIntensityFromHealth(health01);
    }

    private float GetIntensityFromHealth(float health01)
    {
        if (health01 > 0.75f) return 0.0f;
        else if (health01 > 0.50f) return 0.08f;
        else if (health01 > 0.25f) return 0.18f;
        else return 0.30f;
    }
}



