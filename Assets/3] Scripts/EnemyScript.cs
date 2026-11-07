using UnityEngine;

/// <summary>
/// Runtime state for one enemy instance. Design data comes from the shared
/// EnemyDefinition asset (read-only at runtime); per-instance state like
/// current HP lives here.
/// Also handles per-timeline existence: an enemy that doesn't exist in the
/// active timeline is hidden and its hitbox disabled (not destroyed, so it
/// comes back if the timeline swaps again).
/// Expects children named "Sprite" and "Hitbox" (auto-found in Awake).
/// </summary>
public class EnemyScript : MonoBehaviour
{
    [SerializeField] private EnemyDefinition definition;

    private int _currentHP;
    private GameObject _sprite;
    private GameObject _hitbox;
    private bool _subscribed;

    public EnemyDefinition Definition => definition;
    public int CurrentHP => _currentHP;

    private void Awake()
    {
        _currentHP = definition != null ? definition.maxHP : 1;
        Transform s = transform.Find("Sprite");
        Transform h = transform.Find("Hitbox");
        if (s != null) _sprite = s.gameObject;
        if (h != null) _hitbox = h.gameObject;
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
        ApplyTimeline(LevelManager.Instance.CurrentTimeline);
    }

    private void HandleTimelineChanged(Timeline newTimeline, Timeline previousTimeline)
    {
        ApplyTimeline(newTimeline);
    }

    private void ApplyTimeline(Timeline timeline)
    {
        if (definition == null) return;
        bool exists = timeline == Timeline.Past ? definition.existsInPast : definition.existsInFuture;
        if (_sprite != null) _sprite.SetActive(exists);
        if (_hitbox != null) _hitbox.SetActive(exists);
    }

    public void TakeDamage(int damage)
    {
        _currentHP -= damage;
        if (_currentHP <= 0)
            Die();
    }

    private void Die()
    {
        // Scoring, explosions and drops hook in here later.
        Destroy(gameObject);
    }
}
