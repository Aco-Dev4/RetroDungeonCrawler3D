using UnityEngine;

public class PlayerRunEffectHandler : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;

    private void Awake()
    {
        if (playerController == null)
            playerController = GetComponent<PlayerController>();
    }

    public void OnWaveCompleted()
    {
        if (playerController == null) return;

        float waveHealPercent = playerController.GetWaveHealPercent();
        if (waveHealPercent <= 0f) return;

        Health health = playerController.GetComponent<Health>();
        if (health == null) return;

        health.HealPercent(waveHealPercent);

        //Debug.Log($"Wave Heal: healed {waveHealPercent * 100f:0}%");
    }
}