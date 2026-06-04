using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonSound : MonoBehaviour
{
    [SerializeField] private string clickSoundName = "UIClick";

    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(PlayClickSound);
    }

    private void OnDestroy()
    {
        if (_button != null)
            _button.onClick.RemoveListener(PlayClickSound);
    }

    private void PlayClickSound()
    {
        if (string.IsNullOrWhiteSpace(clickSoundName)) return;

        AudioManager.Instance?.PlaySFX(clickSoundName);
    }
}