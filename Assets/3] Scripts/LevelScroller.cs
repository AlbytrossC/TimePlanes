using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Scrolls level tiles downward past a fixed camera, giving the illusion of
/// flying up the level. Tiles are templates (scene objects and/or prefabs)
/// listed from the level start (bottom) upward. Scene-object templates are
/// hidden at runtime; every spawned tile is a clone.
///
/// Loop mode: the FIRST tile repeats endlessly (the rest of the list is ignored).
/// Non-loop mode: tiles advance in list order, and scrolling stops when the
/// top of the last tile reaches the top of the screen (the level's end).
/// </summary>
public class LevelScroller : MonoBehaviour
{
    [Header("Scrolling")]
    [Tooltip("World units per second the level travels down the screen.")]
    [SerializeField] private float scrollSpeed = 2f;
    [Tooltip("Loop mode repeats the FIRST tile endlessly instead of advancing to the next tile in the list.")]
    [SerializeField] private bool loopMode = false;

    [Header("Tiles (bottom to top)")]
    [Tooltip("Tile templates from the level start (bottom) upward. Scene objects are hidden at runtime and cloned.")]
    [SerializeField] private GameObject[] tiles = new GameObject[0];

    [Header("References")]
    [Tooltip("Camera defining the visible area. Auto-found if left empty.")]
    [SerializeField] private Camera viewCamera;

    #region Singleton Access
    private static LevelScroller instance;
    public static LevelScroller Instance
    {
        get
        {
            // Lazy find so other components' OnEnable() can reach us even if script
            // execution order hasn't run this object's Awake() yet.
            if (instance == null)
                instance = UnityEngine.Object.FindFirstObjectByType<LevelScroller>();
            return instance;
        }
    }
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this); // Prevent duplicate components, keep the GameObject
            return;
        }
        instance = this;
    }
    #endregion

    #region Runtime State
    private struct ActiveTile
    {
        public Transform Instance;
        public float BottomY; // world Y of the tile content's bottom edge
        public float TopY;    // world Y of the tile content's top edge
    }

    private readonly List<ActiveTile> _activeTiles = new List<ActiveTile>();
    private float _stackTopY;            // content top edge of the highest active tile
    private float _maxContentHalfWidth = -1f;
    private int _nextTileIndex;          // next list entry to spawn (unused in loop mode)
    private bool _scrolling = true;

    // Coverage margins so tiles are always spawned/culled slightly off-screen.
    private const float SpawnPadding = 0.5f;
    private const float CullPadding = 0.5f;
    #endregion

    /// <summary>Total world units the level has scrolled this play session. LevelSpawner keys encounters off this.</summary>
    public float ScrolledDistance { get; private set; }

    private float ScreenBottomY => viewCamera.transform.position.y - viewCamera.orthographicSize;
    private float ScreenTopY => viewCamera.transform.position.y + viewCamera.orthographicSize;

    private void Start()
    {
        if (viewCamera == null) viewCamera = Camera.main;
        if (viewCamera == null)
        {
            Debug.LogWarning("[LevelScroller] No camera found - disabling.");
            enabled = false;
            return;
        }
        if (tiles == null || tiles.Length == 0)
        {
            Debug.LogWarning("[LevelScroller] No tiles assigned - disabling.");
            enabled = false;
            return;
        }

        HideSceneTemplates();

        // Lay the first tiles from the bottom of the screen upward.
        _stackTopY = ScreenBottomY;
        FillCoverage();
    }

    private void Update()
    {
        if (!_scrolling)
        {
            // Loop mode can always keep going; toggling it on resumes a stopped scroll.
            if (loopMode)
                _scrolling = true;
            else
                return;
        }

        float move = scrollSpeed * Time.deltaTime;

        // Non-loop: clamp the movement so the top of the level stops exactly at
        // the top of the screen (it must never scroll lower than that).
        if (!loopMode && !HasNextTile())
        {
            float remaining = _stackTopY - ScreenTopY;
            if (remaining <= 0f)
            {
                _scrolling = false;
                return;
            }
            move = Mathf.Min(move, remaining);
        }

        Vector3 down = Vector3.down * move;
        for (int i = 0; i < _activeTiles.Count; i++)
        {
            ActiveTile t = _activeTiles[i];
            t.Instance.position += down;
            t.TopY -= move;
            t.BottomY -= move;
            _activeTiles[i] = t;
        }
        _stackTopY -= move;
        ScrolledDistance += move;

        // Despawn tiles that have scrolled fully below the screen.
        float cullY = ScreenBottomY - CullPadding;
        while (_activeTiles.Count > 0 && _activeTiles[0].TopY < cullY)
        {
            Destroy(_activeTiles[0].Instance.gameObject);
            _activeTiles.RemoveAt(0);
        }

        // Keep spawning tiles above the stack as the top edge falls off-screen.
        FillCoverage();
    }

    /// <summary>
    /// The world-space rectangle of the visible level area: the tile strip's
    /// width by the camera's height. The player clamps itself inside this.
    /// </summary>
    public Rect GetPlayArea()
    {
        float camX = viewCamera.transform.position.x;
        float camY = viewCamera.transform.position.y;
        float halfH = viewCamera.orthographicSize;
        float halfW = _maxContentHalfWidth > 0f ? _maxContentHalfWidth : halfH * viewCamera.aspect;
        return new Rect(camX - halfW, camY - halfH, halfW * 2f, halfH * 2f);
    }

    private void FillCoverage()
    {
        float spawnLimit = ScreenTopY + SpawnPadding;
        while (_stackTopY < spawnLimit && HasNextTile())
        {
            if (!SpawnTileAt(_stackTopY))
            {
                // A tile slot was empty/invalid; stop to avoid spinning forever.
                _scrolling = false;
                return;
            }
        }
    }

    private bool HasNextTile()
    {
        return loopMode || _nextTileIndex < tiles.Length;
    }

    private bool SpawnTileAt(float bottomY)
    {
        int index = loopMode ? 0 : _nextTileIndex;
        GameObject template = tiles[index];
        if (!loopMode) _nextTileIndex++;

        if (template == null)
        {
            Debug.LogWarning("[LevelScroller] Tile slot " + index + " is empty - cannot spawn.");
            return false;
        }

        GameObject go = Instantiate(template, transform);
        go.SetActive(true);

        Bounds content = ComputeContentBounds(go);
        if (content.size.y <= 0f)
        {
            Debug.LogWarning("[LevelScroller] Tile '" + template.name + "' has no visible content - cannot spawn.");
            Destroy(go);
            return false;
        }

        Vector3 offset = content.center - go.transform.position;
        float height = content.size.y;

        // Center horizontally on the camera and place the content's bottom edge at bottomY.
        Vector3 camPos = viewCamera.transform.position;
        go.transform.position = new Vector3(
            camPos.x - offset.x,
            bottomY - offset.y + height * 0.5f,
            go.transform.position.z);

        if (content.extents.x > _maxContentHalfWidth)
            _maxContentHalfWidth = content.extents.x;

        _activeTiles.Add(new ActiveTile { Instance = go.transform, BottomY = bottomY, TopY = bottomY + height });
        _stackTopY = bottomY + height;
        return true;
    }

    private static Bounds ComputeContentBounds(GameObject go)
    {
        Bounds bounds = new Bounds(go.transform.position, Vector3.zero);
        bool initialized = false;
        foreach (Renderer r in go.GetComponentsInChildren<Renderer>())
        {
            if (!initialized)
            {
                bounds = r.bounds;
                initialized = true;
            }
            else
            {
                bounds.Encapsulate(r.bounds);
            }
        }
        return bounds;
    }

    private void HideSceneTemplates()
    {
        foreach (GameObject tile in tiles)
        {
            // Prefab assets aren't in a scene; only hide scene-object templates.
            if (tile != null && tile.scene.IsValid())
                tile.SetActive(false);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (viewCamera == null) viewCamera = Camera.main;
        if (viewCamera == null) return;

        Gizmos.color = Color.cyan;
        Rect area = GetPlayArea();
        Gizmos.DrawWireCube(new Vector3(area.center.x, area.center.y, 0f), new Vector3(area.width, area.height, 0f));
    }
}