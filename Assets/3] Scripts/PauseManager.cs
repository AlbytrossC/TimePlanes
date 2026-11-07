using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Toggles pause with Escape (Keyboard) or Start/Options (Gamepad).
/// Pausing freezes gameplay (Time.timeScale = 0), shows the pause menu canvas
/// and puts the GameManager into Paused state. Resume works via the same input
/// or by clicking/selecting the Resume button.
/// </summary>
public class PauseManager : MonoBehaviour
{
    [Header("Pause Menu UI")]
    [Tooltip("The pause menu canvas. Toggled active while paused.")]
    [SerializeField] private GameObject pauseCanvas;
    [Tooltip("Resume button - unpauses the game.")]
    [SerializeField] private Button resumeButton;
    [Tooltip("Options button - placeholder for now.")]
    [SerializeField] private Button optionsButton;
    [Tooltip("Return to Title button - loads the title scene.")]
    [SerializeField] private Button returnToTitleButton;
    [Tooltip("Exit button - placeholder for now.")]
    [SerializeField] private Button exitButton;

    [Header("Scene")]
    [Tooltip("Scene loaded by Return to Title.")]
    [SerializeField] private string titleSceneName = "Title_Screen";

    private InputAction _pauseAction;
    private bool _paused;

    private void Awake()
    {
        // --- Pause: Escape (Keyboard) + Start/Options (Gamepad) ---
        _pauseAction = new InputAction("Pause", InputActionType.Button, "<Keyboard>/escape");
        _pauseAction.AddBinding("<Gamepad>/start");
    }

    private void Start()
    {
        // Coming from the title screen the GameManager already persists; if the
        // level was started directly, put the state machine into Playing.
        if (GameManager.Instance != null && GameManager.Instance.CurrentGameState == GameManager.GameState.MainMenu)
            GameManager.Instance.SetGameState(GameManager.GameState.Playing);
    }

    private void OnEnable()
    {
        _pauseAction.Enable();

        if (resumeButton != null) resumeButton.onClick.AddListener(Resume);
        if (optionsButton != null) optionsButton.onClick.AddListener(OnOptionsClicked);
        if (returnToTitleButton != null) returnToTitleButton.onClick.AddListener(ReturnToTitle);
        if (exitButton != null) exitButton.onClick.AddListener(OnExitClicked);
    }

    private void OnDisable()
    {
        _pauseAction.Disable();

        if (resumeButton != null) resumeButton.onClick.RemoveListener(Resume);
        if (optionsButton != null) optionsButton.onClick.RemoveListener(OnOptionsClicked);
        if (returnToTitleButton != null) returnToTitleButton.onClick.RemoveListener(ReturnToTitle);
        if (exitButton != null) exitButton.onClick.RemoveListener(OnExitClicked);
    }

    private void Update()
    {
        if (_pauseAction.WasPressedThisFrame())
            TogglePause();
    }

    public void TogglePause()
    {
        if (_paused)
            Resume();
        else
            Pause();
    }

    public void Pause()
    {
        _paused = true;
        Time.timeScale = 0f; // Freezes gameplay, scrolling and physics.

        if (pauseCanvas != null)
            pauseCanvas.SetActive(true);

        if (GameManager.Instance != null)
            GameManager.Instance.SetGameState(GameManager.GameState.Paused);

        // Show the cursor for menu clicking.
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Pre-select Resume so controllers can navigate straight to it.
        if (UnityEngine.EventSystems.EventSystem.current != null && resumeButton != null)
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);
    }

    public void Resume()
    {
        _paused = false;
        Time.timeScale = 1f;

        if (pauseCanvas != null)
            pauseCanvas.SetActive(false);

        if (GameManager.Instance != null)
            GameManager.Instance.SetGameState(GameManager.GameState.Playing);

        if (UnityEngine.EventSystems.EventSystem.current != null)
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
    }

    public void ReturnToTitle()
    {
        // Undo the pause first so timeScale and state carry over cleanly.
        _paused = false;
        Time.timeScale = 1f;

        if (pauseCanvas != null)
            pauseCanvas.SetActive(false);

        if (GameManager.Instance != null)
            GameManager.Instance.SetGameState(GameManager.GameState.MainMenu);

        SceneManager.LoadScene(titleSceneName);
    }

    private void OnOptionsClicked()
    {
        Debug.Log("[PauseManager] Options - not implemented yet.");
    }

    private void OnExitClicked()
    {
        Debug.Log("[PauseManager] Exit to Desktop - not implemented yet.");
    }
}