using System.Collections;
using UnityEngine;

internal sealed class BurnEffect
{
    private readonly Health _target;
    private Coroutine _routine;
    private ParticleSystem _flames;
    private GameObject _damageSource;

    private int _remainingDamage;
    private int _damagePerTick;
    private float _timeUntilTick;
    private float _tickInterval;

    public BurnEffect(Health target)
    {
        _target = target;
    }

    public void Apply(BurnSettings settings, int totalDamage, float tickRateMultiplier, GameObject damageSource)
    {
        // Refresh the burn without stacking damage or postponing the next tick.
        if (totalDamage >= _remainingDamage)
        {
            _remainingDamage = totalDamage;
            _damageSource = damageSource;
        }

        _damagePerTick = Mathf.Max(1, settings.damagePerTick);
        float tickInterval = Mathf.Max(0.05f, settings.tickInterval / Mathf.Max(1f, tickRateMultiplier));

        if (_routine == null)
        {
            _tickInterval = tickInterval;
            _timeUntilTick = tickInterval;

            ShowFlames(settings.enemyFlames);
            _routine = _target.StartCoroutine(Run());
        }
        else if (!Mathf.Approximately(tickInterval, _tickInterval))
        {
            // Preserve progress toward the next tick when a card changes the cadence.
            _timeUntilTick *= tickInterval / _tickInterval;
            _tickInterval = tickInterval;
        }

        _target.SetBurnPreview(_remainingDamage);
    }

    public void Stop()
    {
        if (_routine != null)
            _target.StopCoroutine(_routine);

        Clear();
    }

    private IEnumerator Run()
    {
        while (_remainingDamage > 0 && _target.currentHealth > 0)
        {
            yield return null;
            if (GameManager.Instance?.State == GameState.GameOver)
                break;

            Advance(Time.deltaTime);
        }

        Clear();
    }

    private void Advance(float deltaTime)
    {
        if (deltaTime <= 0f) return;

        _timeUntilTick -= deltaTime;
        if (_timeUntilTick > 0f) return;

        // Extra burn damage adds ticks. The final tick may use a smaller remainder.
        while (_timeUntilTick <= 0f && _remainingDamage > 0 && _target.currentHealth > 0)
        {
            int damage = Mathf.Min(_remainingDamage, _damagePerTick);
            _remainingDamage -= damage;
            _timeUntilTick += _tickInterval;
            _target.TakeBurnDamage(damage, _damageSource);
        }

        _target.SetBurnPreview(_remainingDamage);
    }

    private void ShowFlames(ParticleSystem prefab)
    {
        if (prefab == null) return;

        if (_flames == null)
        {
            _flames = Object.Instantiate(prefab, _target.transform, false);
            FitFlamesToTarget();
        }

        _flames.Play(true);
    }

    private void FitFlamesToTarget()
    {
        if (!_target.TryGetComponent(out CapsuleCollider body))
        {
            _flames.transform.localPosition = Vector3.up * 0.6f;
            return;
        }

        float diameter = body.radius * 2f;
        Vector3 bodySize = Vector3.one * diameter;
        bodySize[body.direction] = Mathf.Max(body.height, diameter);
        _flames.transform.localPosition = body.center;

        var shape = _flames.shape;
        shape.scale = Vector3.Scale(shape.scale, bodySize);

        // Wider bodies need more particles and a modest increase in particle size.
        const float standardEnemyDiameter = 1.2f;
        float widthScale = Mathf.Max(0.5f, diameter / standardEnemyDiameter);
        var main = _flames.main;
        main.startSizeMultiplier *= Mathf.Sqrt(widthScale);
        var emission = _flames.emission;
        emission.rateOverTimeMultiplier *= widthScale;
    }

    private void Clear()
    {
        _routine = null;
        _damageSource = null;
        _remainingDamage = 0;
        _target.SetBurnPreview(0);

        if (_flames != null)
            _flames.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
}
