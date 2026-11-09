using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Keeps the HUD side panels (and optionally a full-height strip panel such as
/// the pause menu) exactly flush with the visible level area at any resolution
/// or aspect ratio, and swaps the side panel background colors to match the
/// active timeline (dark green in the Past, dark blue in the Future).
///
/// The level viewport (tile-strip width x camera height, in world units) is
/// identical on every display; the side panels simply fill whatever screen
/// space remains, so in-game UI can later be built outward from the level area
/// toward the screen edges.
/// </summary>
public class LevelFrameUI : MonoBehaviour
{
    [Header("Panels to pin to the level strip")]
    [SerializeField] private RectTransform leftPanel;
    [SerializeField] private RectTransform rightPanel;
    [Tooltip("Optional full-height panel that should exactly cover the strip (e.g. the pause menu).")]
    [SerializeField] private RectTransform stripPanel;

    [Header("Side Panel Colors (per timeline)")]
    [SerializeField] private Color pastColor = new Color(0.06f, 0.16f, 0.08f, 1f);
    [SerializeField] private Color futureColor = new Color(0.05f, 0.09f, 0.20f, 1f);

    [Header("References")]
    [Tooltip("Camera defining the view. Auto-found if left empty.")]
    [SerializeField] private Camera viewCamera;

    private Image _leftImage;
    private Image _rightImage;
    private bool _subscribed;

    private void Awake()
    {
        if (leftPanel == null) leftPanel = transform.Find("Left Black Box") as RectTransform;
        if (rightPanel == null) rightPanel = transform.Find("Right Black Box") as RectTransform;

        // Cache Image components once so timeline swaps are direct color writes (no GetComponent calls).
        if (leftPanel != null) _leftImage = leftPanel.GetComponent<Image>();
        if (rightPanel != null) _rightImage = rightPanel.GetComponent<Image>();
    }

    private void OnEnable()
    {
        if (LevelManager.Instance != null)
            Subscribe();
    }

    private void Start()
    {
        if (viewCamera == null) viewCamera = Camera.main;
        if (!_subscribed && LevelManager.Instance != null)
            Subscribe();
    }

    private void OnDisable()
    {
        if (_subscribed && LevelManager.Instance != null)
        {
            LevelManager.Instance.OnTimelineChanged -= HandleTimelineChanged;
            _subscribed = false;
        }
    }

    private void Subscribe()
    {
        LevelManager.Instance.OnTimelineChanged += HandleTimelineChanged;
        _subscribed = true;
        ApplyTimelineColor(LevelManager.Instance.CurrentTimeline);
    }

    private void HandleTimelineChanged(Timeline newTimeline, Timeline previousTimeline)
    {
        ApplyTimelineColor(newTimeline);
    }

    private void ApplyTimelineColor(Timeline timeline)
    {
        Color targetColor = timeline == Timeline.Past ? pastColor : futureColor;
        if (_leftImage != null) _leftImage.color = targetColor;
        if (_rightImage != null) _rightImage.color = targetColor;
    }

    private void LateUpdate()
    {
        if (viewCamera == null || LevelScroller.Instance == null) return;

        Rect area = LevelScroller.Instance.GetPlayArea();
        float camY = viewCamera.transform.position.y;

        float left = Mathf.Clamp01(viewCamera.WorldToViewportPoint(new Vector3(area.xMin, camY, 0f)).x);
        float right = Mathf.Clamp01(viewCamera.WorldToViewportPoint(new Vector3(area.xMax, camY, 0f)).x);

        Pin(leftPanel, 0f, left);
        Pin(rightPanel, right, 1f);
        Pin(stripPanel, left, right);
    }

    private static void Pin(RectTransform panel, float anchorXMin, float anchorXMax)
    {
        if (panel == null) return;
        panel.anchorMin = new Vector2(anchorXMin, 0f);
        panel.anchorMax = new Vector2(anchorXMax, 1f);
        // Horizontal offsets would push the panel off the strip edges; keep them zeroed.
        panel.offsetMin = new Vector2(0f, panel.offsetMin.y);
        panel.offsetMax = new Vector2(0f, panel.offsetMax.y);
    }
}
