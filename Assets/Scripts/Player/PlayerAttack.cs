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

        #region Debuging: Attack Visual

        // Temporary visual
        Debug.DrawLine(transform.position, transform.position + transform.forward * attackRange, Color.red, 0.2f);
        Debug.DrawRay(transform.position, transform.forward * attackRange, Color.red, 0.2f);
        DebugExtension.DebugWireSphere(transform.position + transform.forward * (attackRange * 0.5f), Color.red, attackRange * 0.5f, 0.2f);
        
        // Debug Red Sphere
        GameObject debugSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        GameObject.Destroy(debugSphere.GetComponent<Collider>());
        debugSphere.transform.position = transform.position + transform.forward * (attackRange * 0.5f);
        debugSphere.transform.localScale = Vector3.one * attackRange;
        var sphereRenderer = debugSphere.GetComponent<Renderer>();
        sphereRenderer.material = new Material(Shader.Find("Standard"));
        sphereRenderer.material.color = new Color(1f, 0f, 0f, 0.4f);
        sphereRenderer.material.SetFloat("_Mode", 3); // force transparency
        sphereRenderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        sphereRenderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        sphereRenderer.material.SetInt("_ZWrite", 0);
        sphereRenderer.material.DisableKeyword("_ALPHATEST_ON");
        sphereRenderer.material.EnableKeyword("_ALPHABLEND_ON");
        sphereRenderer.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        sphereRenderer.material.renderQueue = 3000;
        GameObject.Destroy(debugSphere, 0.2f);
        
        #endregion
    }

    private IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        _canAttack = true;
    }
}
