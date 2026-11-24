using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChestDiscardConfirmation : MonoBehaviour
{
    [SerializeField] private Button keepChoosingButton;
    [SerializeField] private Button discardRewardButton;

    private UnityAction _onDiscard;
    private GameObject _previousSelection;

    private void Awake()
    {
        keepChoosingButton.onClick.AddListener(Hide);
        discardRewardButton.onClick.AddListener(ConfirmDiscard);
    }

    public void Show(UnityAction onDiscard)
    {
        _onDiscard = onDiscard;
        EventSystem events = EventSystem.current;
        _previousSelection = events != null ? events.currentSelectedGameObject : null;

        transform.SetAsLastSibling();
        gameObject.SetActive(true);
        if (events != null)
            events.SetSelectedGameObject(keepChoosingButton.gameObject);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        _onDiscard = null;

        EventSystem events = EventSystem.current;
        if (events != null)
            events.SetSelectedGameObject(_previousSelection);

        _previousSelection = null;
    }

    private void ConfirmDiscard()
    {
        _onDiscard?.Invoke();
    }
}
