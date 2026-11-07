using UnityEngine;

/// <summary>
/// Spawns encounters as the level scrolls. Entries are keyed off scroll
/// DISTANCE (world units), not time, so the layout is tied to the level's
/// geography and stays correct if the scroll speed changes (and it pauses
/// with the game). Entries must be listed in ascending distance order.
///
/// Spawn heights: y = 7 puts things just above the screen top (they fly in);
/// the diagonal row formations should keep their off-screen x (behind the HUD
/// bars) so they enter from the sides as designed.
/// </summary>
public class LevelSpawner : MonoBehaviour
{
    [System.Serializable]
    public class SpawnEntry
    {
        [Tooltip("Scroll distance (world units) at which to spawn this encounter.")]
        public float distance;
        [Tooltip("Enemy or formation prefab to spawn.")]
        public GameObject prefab;
        [Tooltip("World position to spawn at (usually y = 7, just above the screen).")]
        public Vector2 position;
    }

    [Tooltip("Encounters in ascending scroll-distance order.")]
    [SerializeField] private SpawnEntry[] spawns = new SpawnEntry[0];

    private int _nextIndex;

    private void Update()
    {
        if (LevelScroller.Instance == null) return;
        float scrolled = LevelScroller.Instance.ScrolledDistance;

        while (_nextIndex < spawns.Length && spawns[_nextIndex] != null && scrolled >= spawns[_nextIndex].distance)
        {
            SpawnEntry entry = spawns[_nextIndex];
            _nextIndex++;
            if (entry.prefab == null)
            {
                Debug.LogWarning("[LevelSpawner] Entry at distance " + entry.distance + " has no prefab - skipped.");
                continue;
            }
            Instantiate(entry.prefab, new Vector3(entry.position.x, entry.position.y, 0f), Quaternion.identity);
            Debug.Log("[LevelSpawner] Scrolled " + scrolled.ToString("F1") + ": spawned " + entry.prefab.name + " at " + entry.position);
        }
    }
}
