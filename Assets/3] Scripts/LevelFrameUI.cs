using UnityEngine;

/// <summary>
/// Keeps the HUD side panels (and optionally a full-height strip panel such as
/// the pause menu) exactly flush with the visible level area at any resolution
/// or aspect ratio. The level viewport (tile-strip width x camera height, in
/// world units) is identical on every display; the side panels simply fill
/// whatever screen space remains, so in-game UI can later be built outward from
/// the level area toward the screen edges.
/// Runs in play mode only; in edit mode the panels keep their designed layout.
/// </summary>
public class LevelFrameUI : MonoBehaviour
{
    [Header("Panels to pin to the level strip")]
    [SerializeField] private RectTransform leftPanel;
    [SerializeField] private RectTransform rightPanel;
    [Tooltip("Optional full-height panel that should exactly cover the strip (e.g. the pause menu).")]
    [SerializeField] private RectTransform stripPanel;

    [Header("References")]
    [Tooltip("Camera defining the view. Auto-found if left empty.")]
    [SerializeField] private Camera viewCamera;

    private void Start()
    {
        if (viewCamera == null) viewCamera = Camera.main;
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
