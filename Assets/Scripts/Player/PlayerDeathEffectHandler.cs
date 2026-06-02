using System.Collections;
using UnityEngine;

public class PlayerDeathEffectHandler : MonoBehaviour
{
    #region Settings
    [Header("Guardian Angel")]
    [SerializeField] private float reviveDelay = 3f;
    [SerializeField] private float immunityDuration = 3f;
    [SerializeField] private float reviveHealthPercent = 0.5f;
    [SerializeField] private PlayerInvulnerabilityVisual invulnerabilityVisual;
    #endregion

    #region Runtime
    private int _guardianAngelRevives;
    private int _totalRevivesGranted;
    private bool _isReviving;
    #endregion

    #region Public
    public void SetRevives(int totalAmountFromCards)
    {
        totalAmountFromCards = Mathf.Max(0, totalAmountFromCards);

        int newlyGainedRevives = totalAmountFromCards - _totalRevivesGranted;

        if (newlyGainedRevives > 0)
            _guardianAngelRevives += newlyGainedRevives;

        _totalRevivesGranted = Mathf.Max(_totalRevivesGranted, totalAmountFromCards);
    }

    public bool TryPreventDeath()
    {
        if (_isReviving) return true;
        if (_guardianAngelRevives <= 0) return false;

        Debug.Log("Guardian Angel activated!");
        _guardianAngelRevives--;
        StartCoroutine(ReviveRoutine());

        return true;
    }
    #endregion

    #region Revive
    private IEnumerator ReviveRoutine()
    {
        _isReviving = true;

        Health health = GetComponent<Health>();
        if (health == null)
        {
            _isReviving = false;
            yield break;
        }

        health.SetInvulnerable(true);
        invulnerabilityVisual?.StartInvulnerabilityEffect(reviveDelay + immunityDuration);
        health.Revive(1);

        yield return new WaitForSeconds(reviveDelay);

        int reviveHealth = Mathf.RoundToInt(health.GetMaxHealth() * reviveHealthPercent);
        health.Revive(reviveHealth);

        yield return new WaitForSeconds(immunityDuration);

        health.SetInvulnerable(false);
        invulnerabilityVisual?.StopInvulnerabilityEffect();
        
        _isReviving = false;
    }
    #endregion
}