using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Player control script for Time Planes (new Input System).
/// Movement: WASD (Keyboard) or Left Thumbstick (Gamepad).
/// Buttons: Primary/shoot, Secondary, and Time-Swap.
/// All input actions are defined in code so this script is self-contained.
/// </summary>
public class PlayerScript : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Movement speed in world units per second.")]
    [SerializeField] private float moveSpeed = 8f;

    [Header("Play Area Clamp")]
    [Tooltip("Padding (world units) kept between the player's sprite and the visible level area / UI frame.")]
    [SerializeField] private float playAreaPadding = 0.5f;
    [Header("References")]
    public GameObject sprite;
    public GameObject hitbox;

    private InputAction _moveAction;
    private InputAction _primaryAction;
    private InputAction _secondaryAction;
    private InputAction _timeSwapAction;
    private Camera _camera;
    private SpriteRenderer _playerRenderer;

    private void Awake()
    {
        // --- Move: WASD (Keyboard) + Left Thumbstick (Gamepad) ---
        _moveAction = new InputAction("Move", InputActionType.Value);
        if (sprite == null) sprite = transform.Find("Sprite")?.gameObject;
        if (hitbox == null) hitbox = transform.Find("Hitbox")?.gameObject;

        // --- Play area ---
        _camera = Camera.main;
        _playerRenderer = GetComponentInChildren<SpriteRenderer>();

        _moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");
        _moveAction.AddBinding("<Gamepad>/leftStick");

        // --- Primary: '<' key (Keyboard) + South Face (Gamepad) ---
        _primaryAction = new InputAction("Primary", InputActionType.Button, "<Keyboard>/comma");
        _primaryAction.AddBinding("<Gamepad>/buttonSouth");

        // --- Secondary: '>' key (Keyboard) + East Face (Gamepad) ---
        _secondaryAction = new InputAction("Secondary", InputActionType.Button, "<Keyboard>/period");
        _secondaryAction.AddBinding("<Gamepad>/buttonEast");

        // --- Time Swap: '?' key (Keyboard) + North Face (Gamepad) ---
        _timeSwapAction = new InputAction("TimeSwap", InputActionType.Button, "<Keyboard>/slash");
        _timeSwapAction.AddBinding("<Gamepad>/buttonNorth");
    }

    private void OnEnable()
    {
        _moveAction.Enable();
        _primaryAction.Enable();
        _secondaryAction.Enable();
        _timeSwapAction.Enable();
    }

    private void OnDisable()
    {
        _moveAction.Disable();
        _primaryAction.Disable();
        _secondaryAction.Disable();
        _timeSwapAction.Disable();
    }

    private void Update()
    {
        // Gameplay input is frozen while paused.
        if (GameManager.Instance != null && GameManager.Instance.CurrentGameState == GameManager.GameState.Paused)
            return;

        MovePlayer();
        ClampToPlayArea();
        HandleButtons();
    }

    private void MovePlayer()
    {
        Vector2 move = _moveAction.ReadValue<Vector2>();
        transform.position += (Vector3)(move * (moveSpeed * Time.deltaTime));
    }

    private void HandleButtons()
    {
        if (_primaryAction.WasPressedThisFrame())
        {
            Debug.Log("Primary button pressed (Shoot)");
        }

        if (_secondaryAction.WasPressedThisFrame())
        {
            Debug.Log("Secondary button pressed");
        }

        if (_timeSwapAction.WasPressedThisFrame())
        {
            Debug.Log("Time-Swap button pressed");
            if (LevelManager.Instance != null)
                LevelManager.Instance.SwapTimeline();
            else
                Debug.LogWarning("[PlayerScript] No LevelManager in the scene - cannot swap timelines.");
        }
    }

    /// <summary>Keeps the player inside the visible level area (plus padding).</summary>
    private void ClampToPlayArea()
    {
        Rect area = GetPlayArea();
        if (area.width <= 0f || area.height <= 0f) return;

        // Half the player's own sprite size, so the whole ship stays inside.
        float halfW = _playerRenderer != null ? _playerRenderer.bounds.extents.x : 0f;
        float halfH = _playerRenderer != null ? _playerRenderer.bounds.extents.y : 0f;

        float minX = area.xMin + halfW + playAreaPadding;
        float maxX = area.xMax - halfW - playAreaPadding;
        float minY = area.yMin + halfH + playAreaPadding;
        float maxY = area.yMax - halfH - playAreaPadding;

        Vector3 p = transform.position;
        p.x = maxX <= minX ? area.center.x : Mathf.Clamp(p.x, minX, maxX);
        p.y = maxY <= minY ? area.center.y : Mathf.Clamp(p.y, minY, maxY);
        transform.position = p;
    }

    /// <summary>The visible level area: tile strip width by camera height.</summary>
    private Rect GetPlayArea()
    {
        if (LevelScroller.Instance != null)
            return LevelScroller.Instance.GetPlayArea();

        // Fallback when no scroller exists: the full camera view.
        if (_camera == null) _camera = Camera.main;
        if (_camera == null) return new Rect(0f, 0f, 0f, 0f);

        float halfH = _camera.orthographicSize;
        float halfW = halfH * _camera.aspect;
        Vector3 c = _camera.transform.position;
        return new Rect(c.x - halfW, c.y - halfH, halfW * 2f, halfH * 2f);
    }

    private void OnDestroy()
    {
        _moveAction.Dispose();
        _primaryAction.Dispose();
        _secondaryAction.Dispose();
        _timeSwapAction.Dispose();
    }
}

