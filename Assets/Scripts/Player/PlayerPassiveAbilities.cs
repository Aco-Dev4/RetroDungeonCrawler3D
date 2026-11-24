using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

internal sealed class PlayerPassiveAbilities
{
    private const float TargetRefreshTime = 0.25f;
    private const float FootNavMeshTolerance = 0.45f;

    private readonly CharacterController _player;
    private readonly LayerMask _enemyLayer;
    private readonly FireballSettings _settings;
    private readonly BurnSettings _burnSettings;
    private readonly NavMeshPath _path = new();
    private readonly HashSet<Health> _impactTargets = new();

    private Vector3? _lastGroundedFeet;
    private EnemyAI _target;
    private float _targetRefreshRemaining;

    private int _fireballLevel;
    private float _burnTickRate;
    private float _cooldownRemaining;
    private FireballPhase _phase;
    private float _phaseElapsed;
    private Vector3 _landingPoint;
    private FireballImpact _impact;
    private GameObject _marker;
    private ParticleSystem _fireball;

    private enum FireballPhase { Ready, Warning, Falling }

    public PlayerPassiveAbilities(CharacterController player, LayerMask enemyLayer,
        FireballSettings settings, BurnSettings burnSettings)
    {
        _player = player;
        _enemyLayer = enemyLayer;
        _settings = settings;
        _burnSettings = burnSettings;
    }

    public void SetFireballLevel(int level, float burnTickRate)
    {
        if (_fireballLevel == 0 && level > 0)
        {
            _cooldownRemaining = _settings.cooldown;
            _targetRefreshRemaining = 0f;
        }

        _fireballLevel = level;
        _burnTickRate = burnTickRate;
        if (level <= 0)
        {
            _target = null;
            CancelCast();
        }
    }

    public void Tick(float deltaTime, bool grounded)
    {
        if (GameManager.Instance?.State != GameState.Playing || deltaTime <= 0f)
            return;

        // Only grounded positions can replace the target position used while jumping.
        if (grounded)
        {
            Bounds body = _player.bounds;
            _lastGroundedFeet = new Vector3(body.center.x, body.min.y, body.center.z);
        }

        if (_fireballLevel <= 0) return;

        _targetRefreshRemaining -= deltaTime;
        if (_targetRefreshRemaining <= 0f)
            RefreshTarget();

        // Only reachable combat advances passive cooldowns.
        float cooldownTime = _target != null ? deltaTime : 0f;
        _cooldownRemaining = Mathf.Max(0f, _cooldownRemaining - cooldownTime);

        if (_phase != FireballPhase.Ready)
            AdvanceCast(deltaTime);
        else if (_cooldownRemaining <= 0f && _target != null)
            TryBeginCast();
    }

    private void RefreshTarget()
    {
        _targetRefreshRemaining = TargetRefreshTime;
        _target = null;
        if (WaveManager.Instance == null || !_lastGroundedFeet.HasValue ||
            !NavMesh.SamplePosition(_lastGroundedFeet.Value, out NavMeshHit playerGround, FootNavMeshTolerance, NavMesh.AllAreas))
            return;

        Vector3 playerPosition = _player.transform.position;
        float nearestDistance = float.PositiveInfinity;

        foreach (WaveInstance wave in WaveManager.Instance.ActiveWaves)
        {
            foreach (EnemyAI enemy in wave.AliveEnemies)
            {
                if (enemy == null || !enemy.TryGetComponent(out Health health) || health.currentHealth <= 0)
                    continue;

                float distance = (enemy.transform.position - playerPosition).sqrMagnitude;
                if (distance >= nearestDistance || !enemy.TryGetComponent(out NavMeshAgent agent) ||
                    !agent.enabled || !agent.isOnNavMesh || !agent.CalculatePath(playerGround.position, _path) ||
                    _path.status != NavMeshPathStatus.PathComplete)
                    continue;

                nearestDistance = distance;
                _target = enemy;
            }
        }
    }

    private void TryBeginCast()
    {
        RefreshTarget();
        if (_target == null || !Physics.Raycast(_target.transform.position + Vector3.up * 0.5f,
                Vector3.down, out RaycastHit ground, 10f, _settings.landingSurfaces, QueryTriggerInteraction.Ignore))
            return;

        _landingPoint = ground.point;
        _impact = _settings.GetImpact(_fireballLevel);
        _cooldownRemaining = _settings.cooldown;
        _phase = FireballPhase.Warning;
        _phaseElapsed = 0f;

        if (_settings.landingMarker != null)
        {
            _marker = Object.Instantiate(_settings.landingMarker, _landingPoint + Vector3.up * 0.055f, Quaternion.identity);
            _marker.transform.localScale *= _impact.radius;
            FitMarkerToGround();
        }
    }

    private void FitMarkerToGround()
    {
        foreach (LineRenderer line in _marker.GetComponentsInChildren<LineRenderer>())
        {
            for (int i = 0; i < line.positionCount; i++)
            {
                Vector3 vertex = line.transform.TransformPoint(line.GetPosition(i));
                if (Physics.Raycast(vertex + Vector3.up, Vector3.down, out RaycastHit hit,
                        2.5f, _settings.landingSurfaces, QueryTriggerInteraction.Ignore))
                    line.SetPosition(i, line.transform.InverseTransformPoint(hit.point + Vector3.up * 0.055f));
            }
        }
    }

    private void AdvanceCast(float deltaTime)
    {
        _phaseElapsed += deltaTime;
        if (_phase == FireballPhase.Warning)
        {
            if (_phaseElapsed < _settings.warningTime) return;

            _phaseElapsed = 0f;
            _phase = FireballPhase.Falling;
            if (_settings.fallingFireball != null)
                _fireball = Object.Instantiate(_settings.fallingFireball, FireballPosition(0f), Quaternion.identity);

            return;
        }

        float progress = Mathf.Clamp01(_phaseElapsed / Mathf.Max(0.1f, _settings.fallTime));
        if (_fireball != null)
            _fireball.transform.position = FireballPosition(progress);

        if (progress < 1f) return;

        CancelCast();
        LandFireball();
    }

    private Vector3 FireballPosition(float progress)
    {
        return _landingPoint + Vector3.up * (0.3f + _settings.fallHeight * (1f - progress * progress));
    }

    private void LandFireball()
    {
        if (_settings.impactBurst != null)
        {
            var burst = Object.Instantiate(_settings.impactBurst, _landingPoint + Vector3.up * 0.25f, Quaternion.identity);
            burst.transform.localScale *= _impact.radius;
            burst.Play(true);
        }

        if (!string.IsNullOrEmpty(_settings.impactSound))
            AudioManager.Instance?.PlaySFX(_settings.impactSound);

        Vector3 impactCenter = _landingPoint + Vector3.up * 0.75f;
        Collider[] hits = Physics.OverlapSphere(impactCenter, _impact.radius, _enemyLayer, QueryTriggerInteraction.Ignore);
        foreach (Collider hit in hits)
        {
            Health health = hit.GetComponentInParent<Health>();
            if (health == null || health.currentHealth <= 0 ||
                !health.TryGetComponent<EnemyAI>(out _) || !_impactTargets.Add(health))
                continue;

            int healthBefore = health.currentHealth;
            health.TakeDamage(_impact.damage, _player.gameObject);
            if (health.currentHealth < healthBefore)
                health.ApplyBurn(_burnSettings, _impact.burnDamage, _burnTickRate, _player.gameObject);
        }

        _impactTargets.Clear();
    }

    public void CancelCast()
    {
        if (_marker != null)
        {
            _marker.SetActive(false);
            Object.Destroy(_marker);
            _marker = null;
        }

        if (_fireball != null)
        {
            _fireball.gameObject.SetActive(false);
            Object.Destroy(_fireball.gameObject);
            _fireball = null;
        }

        _phase = FireballPhase.Ready;
    }
}

