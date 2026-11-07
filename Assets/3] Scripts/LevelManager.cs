using System;
using UnityEngine;

/// <summary>
/// Per-level state and coordination. Tracks the active timeline and announces
/// timeline changes to everyone listening (player, enemies, spawners, VFX...).
/// </summary>
public class LevelManager : MonoBehaviour
{
    #region Singleton Setup
    private static LevelManager instance;
    public static LevelManager Instance
    {
        get
        {
            // Lazy find so other components' OnEnable() can reach us even if script
            // execution order hasn't run this object's Awake() yet.
            if (instance == null)
                instance = UnityEngine.Object.FindFirstObjectByType<LevelManager>();
            return instance;
        }
    }
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject); // Prevent duplicates
            return;
        }
        instance = this;
        CurrentTimeline = startingTimeline;
    }
    #endregion

    #region Timeline Management

    [Header("Timeline")]
    [Tooltip("Which timeline the level starts in.")]
    [SerializeField] private Timeline startingTimeline = Timeline.Future;

    private bool _timelineInitialized;
    private Timeline _currentTimeline;

    /// <summary>
    /// The timeline the level is currently in. Self-initializes from the
    /// serialized starting timeline on first read, so subscribers that run
    /// before this object's Awake() still get the correct timeline.
    /// </summary>
    public Timeline CurrentTimeline
    {
        get
        {
            if (!_timelineInitialized)
            {
                _timelineInitialized = true;
                _currentTimeline = startingTimeline;
            }
            return _currentTimeline;
        }
        private set { _timelineInitialized = true; _currentTimeline = value; }
    }

    /// <summary>
    /// Announced after the timeline changes.
    /// Parameters: (newTimeline, previousTimeline).
    /// </summary>
    public event Action<Timeline, Timeline> OnTimelineChanged;

    /// <summary>
    /// Toggles the active timeline and announces the change.
    /// Called by the player's Time-Swap input.
    /// </summary>
    public void SwapTimeline()
    {
        Timeline previous = CurrentTimeline;
        CurrentTimeline = previous == Timeline.Past ? Timeline.Future : Timeline.Past;

        Debug.Log("[LevelManager] Timeline changed: " + previous + " -> " + CurrentTimeline);
        OnTimelineChanged?.Invoke(CurrentTimeline, previous);
    }

    #endregion
}