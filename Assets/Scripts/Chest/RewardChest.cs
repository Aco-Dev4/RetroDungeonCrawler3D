using UnityEngine;
using System.Collections;

public class RewardChest : MonoBehaviour, IInteractable
{
    public int Priority => 100; // chest beats everything

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject interactionCanvas;
    [SerializeField] private GameObject outlineObject;

    [Header("Set")]
    [SerializeField] private float timeToDespawn = 2f;

    private bool _opened;
    private PlayerInteract _playerInteract;
    private static RewardChest _activeChest;

    private void Awake()
    {
        interactionCanvas.SetActive(false);
        if (outlineObject != null)
            outlineObject.SetActive(false);
    }

    // ================= INTERFACE =================

    public void Interact()
    {
        if (_opened) return;
        if (_activeChest != null) return;

        _opened = true;
        _activeChest = this;
        interactionCanvas.SetActive(false);
        if (outlineObject != null)
            outlineObject.SetActive(false);

        animator.SetTrigger("Opened");
    }

    public void OnFocus()
    {
        if (_opened) return;
        interactionCanvas.SetActive(true);
        if (outlineObject != null)
            outlineObject.SetActive(true);
    }

    public void OnUnfocus()
    {
        interactionCanvas.SetActive(false);
        if (outlineObject != null)
            outlineObject.SetActive(false);
    }

    // ============== ANIMATION EVENT ==============

    public void OnChestOpened() // animation event
    {
        if (ChestRewardUI.Instance != null)
            ChestRewardUI.Instance.Open(this);
    }

    public void OnRewardUIClosed()
    {
        if (_activeChest == this)
            _activeChest = null;
        StartCoroutine(Despawn());
    }

    private IEnumerator Despawn()
    {
        _playerInteract?.Unregister(this);
        yield return new WaitForSeconds(timeToDespawn);
        Destroy(gameObject);
    }

    // ================= TRIGGERS ==================

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



