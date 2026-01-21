using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteract : MonoBehaviour
{
    private readonly List<IInteractable> _inRange = new();
    private IInteractable _current;

    // Called by Input System (E key)
    public void Interact(InputAction.CallbackContext context)
    {
        if (!context.started) return;

        _current?.Interact();
    }

    // Chooses the highest-priority interactable
    private void SelectBestTarget()
    {
        IInteractable best = null;

        foreach (var interactable in _inRange)
        {
            if (best == null || interactable.Priority > best.Priority)
                best = interactable;
        }

        if (_current == best) return;

        _current?.OnUnfocus();
        _current = best;
        _current?.OnFocus();
    }

    // Called by trigger enter
    public void Register(IInteractable interactable)
    {
        if (_inRange.Contains(interactable)) return;

        _inRange.Add(interactable);
        SelectBestTarget();
    }

    // Called by trigger exit
    public void Unregister(IInteractable interactable)
    {
        if (!_inRange.Remove(interactable)) return;

        if (_current == interactable)
        {
            _current.OnUnfocus();
            _current = null;
            SelectBestTarget();
        }
    }
}



