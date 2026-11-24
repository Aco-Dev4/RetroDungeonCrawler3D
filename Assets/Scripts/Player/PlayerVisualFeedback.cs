using UnityEngine;

public sealed class PlayerVisualFeedback
{
    private ParticleSystem _swordFlames;

    public void SetSwordFlames(Transform sword, ParticleSystem prefab, bool active)
    {
        if (active && _swordFlames == null && prefab != null)
            _swordFlames = Object.Instantiate(prefab, sword, false);

        if (_swordFlames != null && _swordFlames.gameObject.activeSelf != active)
            _swordFlames.gameObject.SetActive(active);
    }

    private const string PunchForearmPath = "Armature/body/arm1.R/arm2.R";

    private readonly CharacterController _controller;
    private readonly Transform _forearm;
    private readonly PlayerVisualFeedbackSettings _settings;

    public PlayerVisualFeedback(CharacterController controller, Animator animator, PlayerVisualFeedbackSettings settings)
    {
        _controller = controller;
        _forearm = animator != null ? animator.transform.Find(PunchForearmPath) : null;
        _settings = settings;
    }

    public void PlayJumpPuff()
    {
        if (_settings.jumpPuff == null) return;

        Vector3 position = FeetPosition + Vector3.up * _settings.jumpPuffHeight;
        Object.Instantiate(_settings.jumpPuff, position, Quaternion.identity);
    }

    public void PlayFistPuff()
    {
        if (_settings.fistPuff == null) return;

        Transform player = _controller.transform;
        Vector3 fist = _forearm != null
            ? _forearm.position + _forearm.up * (_settings.forearmLength * Mathf.Abs(player.lossyScale.x))
            : player.TransformPoint(new Vector3(0.16f, 0.38f, 1.04f));
        Vector3 position = fist + player.forward * _settings.fistPuffOffset;
        Object.Instantiate(_settings.fistPuff, position, player.rotation);
    }

    public void PlaySwordSlash(Vector3 attackCenter, float attackRadius, float bladeScale)
    {
        Transform player = _controller.transform;
        PlaySwipe(_settings.swordSlash, player.position, player.rotation, bladeScale);
        PlaySwipe(_settings.swordEcho, attackCenter, player.rotation, attackRadius);
    }

    private static void PlaySwipe(ParticleSystem prefab, Vector3 position, Quaternion rotation, float width)
    {
        if (prefab == null) return;

        ParticleSystem swipe = Object.Instantiate(prefab, position, rotation);
        var main = swipe.main;
        main.startSizeX = width;
        main.startSizeZ = width;
        swipe.Play();
    }

    public void PlayGuardianRevive()
    {
        if (_settings.guardianReviveGlow == null) return;

        Object.Instantiate(_settings.guardianReviveGlow, FeetPosition, Quaternion.identity, _controller.transform);
    }

    private Vector3 FeetPosition
    {
        get
        {
            Bounds bounds = _controller.bounds;
            return new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
        }
    }
}

