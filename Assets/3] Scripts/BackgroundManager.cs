using UnityEngine;

/// <summary>
/// Swaps a background tile's SpriteRenderer sprite to match the active timeline.
/// Auto-finds child GameObjects named "Past" and "Future" (if not assigned in the
/// Inspector), caches their Sprite references once in Awake(), and subscribes to
/// LevelManager.OnTimelineChanged so active and newly-spawned tiles swap with
/// zero per-swap GetComponent calls.
/// </summary>
public class BackgroundManager : MonoBehaviour
{
    public GameObject spritePast;
    public GameObject spriteFuture;

    private SpriteRenderer _spriteRenderer;
    private Sprite _pastSprite;
    private Sprite _futureSprite;
    private bool _subscribed;

    private void OnValidate()
    {
        if (spritePast == null) spritePast = transform.Find("Past")?.gameObject;
        if (spriteFuture == null) spriteFuture = transform.Find("Future")?.gameObject;
    }

    private void Awake()
    {
        if (spritePast == null) spritePast = transform.Find("Past")?.gameObject;
        if (spriteFuture == null) spriteFuture = transform.Find("Future")?.gameObject;

        // Cache the tile's own renderer and both timeline sprites once so
        // runtime timeline swaps are a single field assignment (no GetComponent in the hot path).
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer == null)
            Debug.LogWarning("[BackgroundManager] No SpriteRenderer found on '" + name + "'.");

        if (spritePast != null)
        {
            SpriteRenderer pastRenderer = spritePast.GetComponent<SpriteRenderer>();
            if (pastRenderer != null) _pastSprite = pastRenderer.sprite;
            spritePast.SetActive(false);
        }

        if (spriteFuture != null)
        {
            SpriteRenderer futureRenderer = spriteFuture.GetComponent<SpriteRenderer>();
            if (futureRenderer != null) _futureSprite = futureRenderer.sprite;
            spriteFuture.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (LevelManager.Instance != null)
            Subscribe();
    }

    private void Start()
    {
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
        SwapSprites(LevelManager.Instance.CurrentTimeline);
    }

    private void HandleTimelineChanged(Timeline newTimeline, Timeline previousTimeline)
    {
        SwapSprites(newTimeline);
    }

    // swap sprites based on the timeline the same way enemy planes do
    private void SwapSprites(Timeline timeline)
    {
        if (_spriteRenderer == null) return;

        Sprite target;
        switch (timeline)
        {
            case Timeline.Past:
                target = _pastSprite;
                break;
            case Timeline.Future:
            default:
                target = _futureSprite;
                break;
        }

        if (target != null)
            _spriteRenderer.sprite = target;
    }
}
