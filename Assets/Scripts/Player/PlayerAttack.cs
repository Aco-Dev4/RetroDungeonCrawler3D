using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    #region Variables: Attack
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private int attackDamage = 25;
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private Animator _animator;
    [SerializeField] private AnimationClip attackClip;
    private bool _canAttack = true;
    #endregion

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (!context.started || !_canAttack) return; // If he can attack and didn't already...

        _canAttack = false;
        if (attackClip != null)
        {
            float speedMultiplier = attackClip.length / attackCooldown;
            _animator.SetFloat("AttackSpeed", speedMultiplier); // Animator should have float param "AttackSpeed"
        }
        _animator.SetTrigger("Attack");

        StartCoroutine(AttackCooldown());
    }

    public void ApplyAttackDamage()
    {
        Vector3 attackOrigin = transform.position + transform.forward * (attackRange * 0.5f);
        Collider[] hits = Physics.OverlapSphere(attackOrigin, attackRange * 0.5f, enemyLayer); // Lists each enemy we hit with our attack

        foreach (Collider hit in hits) // Deals damage to each enemy
        {
            Health target = hit.GetComponent<Health>();
            if (target != null)
            {
                target.TakeDamage(attackDamage, gameObject);
                break;
            }
        }
    }

    private IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        _canAttack = true;
    }
}
