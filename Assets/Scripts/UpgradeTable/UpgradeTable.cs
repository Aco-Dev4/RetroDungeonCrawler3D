using UnityEngine;

public class UpgradeTable : MonoBehaviour, IInteractable
{
    public int Priority => 50;

    [SerializeField] private GameObject interactionCanvas;
    [SerializeField] private GameObject outlineObject;

    private PlayerInteract _playerInteract;

    private void Awake()
    {
        if (interactionCanvas != null) interactionCanvas.SetActive(false);
        if (outlineObject != null) outlineObject.SetActive(false);
    }

    public void Interact()
    {
        if (CardUpgradeUI.Instance != null)
            CardUpgradeUI.Instance.Open();
    }

    public void OnFocus()
    {
        if (interactionCanvas != null) interactionCanvas.SetActive(true);
        if (outlineObject != null) outlineObject.SetActive(true);
    }

    public void OnUnfocus()
    {
        if (interactionCanvas != null) interactionCanvas.SetActive(false);
        if (outlineObject != null) outlineObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        _playerInteract = other.GetComponent<PlayerInteract>();
        _playerInteract?.Register(this);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        _playerInteract?.Unregister(this);
        _playerInteract = null;
    }
}