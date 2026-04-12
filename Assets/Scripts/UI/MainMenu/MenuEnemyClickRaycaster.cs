using UnityEngine;
using UnityEngine.InputSystem;

public class MenuEnemyClickRaycaster : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private LayerMask clickableLayers = ~0;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    private void Update()
    {
        if (Mouse.current == null) return;
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;
        if (targetCamera == null) return;

        Ray ray = targetCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, clickableLayers))
        {
            MenuEnemyClickTarget target = hit.collider.GetComponent<MenuEnemyClickTarget>();
            if (target == null)
                target = hit.collider.GetComponentInParent<MenuEnemyClickTarget>();

            if (target != null)
                target.TakeClickDamage();
        }
    }
}
