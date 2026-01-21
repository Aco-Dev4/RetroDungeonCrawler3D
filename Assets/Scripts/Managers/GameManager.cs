using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [SerializeField] private PlayerInput playerInput;

    public GameState State { get; private set; } = GameState.Playing;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void SetState(GameState newState)
    {
        State = newState;
    }

    public void PauseGame()
    {
        if (State != GameState.Playing) return;

        State = GameState.Paused;
        Time.timeScale = 0f;
        playerInput.SwitchCurrentActionMap("UI");
        CursorManager.Instance?.UnlockCursor();
    }

    public void ResumeGame()
    {
        if (State != GameState.Paused) return;

        State = GameState.Playing;
        Time.timeScale = 1f;
        playerInput.SwitchCurrentActionMap("Gameplay");
        CursorManager.Instance?.LockCursor();
    }

    public void GameOver()
    {
        State = GameState.GameOver;
        Time.timeScale = 1f;
        playerInput.SwitchCurrentActionMap("UI");
        CursorManager.Instance?.UnlockCursor();
    }
}

