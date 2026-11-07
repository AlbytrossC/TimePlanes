using UnityEngine;

/// <summary>
/// Swaps a SpriteRenderer's sprite to match the level's active timeline.
/// Attach to any entity (player, enemies, props) that looks different per timeline.
/// The SpriteRenderer is found automatically on this object or its children.
/// Subscribing to the LevelManager means existing entities swap instantly,
/// and entities spawned later sync to the current timeline on enable.
/// </summary>
public class TimelineSprite : MonoBehaviour
{
    [Header("Sprites (per timeline)")]
    [SerializeField] private Sprite pastSprite;
    [SerializeField] private Sprite futureSprite;

    [Header("Optional definition (overrides the sprite fields above)")]
    [SerializeField] private EnemyDefinition definition;

    [Header("Leader (cosmetic)")]
    [Tooltip("Formation leaders use the definition's leader sprites instead of the standard ones.")]
    [SerializeField] private bool useLeaderSprites;

    private SpriteRenderer _spriteRenderer;
    private bool _subscribed;

    private void Awake()
    {
        // Cache once; swapping later is just a field write (no lookups in the hot path).
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (_spriteRenderer == null)
            Debug.LogWarning("[TimelineSprite] No SpriteRenderer found on '" + name + "' or its children.");
    }

    private void OnEnable()
    {
        // Covers runtime spawns and most scene-load orders. If the LevelManager
        // hasn't finished its Awake() yet, Start() picks up the slack.
        if (LevelManager.Instance != null)
            Subscribe();
    }

    private void Start()
    {
        // By Start() every Awake() in the scene has run, so a LevelManager
        // present in the scene is guaranteed to be found here.
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
        ApplyTimeline(LevelManager.Instance.CurrentTimeline);
    }

    private void HandleTimelineChanged(Timeline newTimeline, Timeline previousTimeline)
    {
        ApplyTimeline(newTimeline);
    }

    private void ApplyTimeline(Timeline timeline)
    {
        if (_spriteRenderer == null) return;

        Sprite target;
        if (definition != null)
        {
            Sprite past = useLeaderSprites && definition.leaderPastSprite != null ? definition.leaderPastSprite : definition.pastSprite;
            Sprite future = useLeaderSprites && definition.leaderFutureSprite != null ? definition.leaderFutureSprite : definition.futureSprite;
            target = timeline == Timeline.Past ? past : future;
        }
        else
            target = timeline == Timeline.Past ? pastSprite : futureSprite;
        if (target != null)
            _spriteRenderer.sprite = target;
    }
}