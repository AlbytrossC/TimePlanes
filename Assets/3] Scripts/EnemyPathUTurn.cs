using UnityEngine;

/// <summary>
/// Per-plane path behaviour: fly straight down-screen, sweep a 180-degree turn
/// when THIS plane reaches a given screen height, then fly back up off-screen.
///
/// Every plane runs its own copy of this logic - there is no leader. Planes
/// arranged in a vertical line each turn when they cross the same world line,
/// which produces train-like path following without any plane depending on
/// another: destroying any plane never affects the rest.
/// </summary>
public class EnemyPathUTurn : MonoBehaviour, ISpeedModifiable
{
    public enum TurnDirection { AutoInward, Left, Right }
    private enum Phase { Descending, Turning, Ascending }

    [Tooltip("World units per second. 0 = use the EnemyDefinition's moveSpeed.")]
    [SerializeField] private float speed = 0f;
    [Tooltip("Viewport height (0 = bottom, 1 = top) where THIS plane begins its turn. 0.33 = two-thirds down the screen.")]
    [Range(0f, 1f)] [SerializeField] private float turnTriggerViewportY = 0.33f;
    [Tooltip("Turn radius in world units. The sweep displaces the plane 2x radius sideways and radius further down.")]
    [Min(0.25f)] [SerializeField] private float turnRadius = 0.9f;
    [Tooltip("AutoInward sweeps toward the screen centre so the turn stays on-screen. Left/Right force a sweep direction.")]
    [SerializeField] private TurnDirection turnDirection = TurnDirection.AutoInward;
    [Tooltip("Rotate the sprite to face the direction of travel (sprites are authored facing up).")]
    [SerializeField] private bool faceTravelDirection = true;

    private Phase _phase = Phase.Descending;
    private Vector2 _heading = Vector2.down;
    private float _turnedDegrees;
    private float _turnSign = 1f; // +1 sweeps toward +x (counterclockwise from straight-down)
    private float _speed;
    private float _speedMultiplier = 1f;
    private Camera _camera;

    public void SetSpeedMultiplier(float multiplier) { _speedMultiplier = multiplier; }
    public float CurrentSpeed => _speed;

    private void Start()
    {
        _camera = Camera.main;
        _speed = speed;
        if (_speed <= 0f)
        {
            EnemyScript enemy = GetComponent<EnemyScript>();
            _speed = (enemy != null && enemy.Definition != null) ? enemy.Definition.moveSpeed : 2f;
        }
        _speed *= _speedMultiplier;
        ApplyRotation();
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        if (_phase == Phase.Turning)
        {
            float step = Mathf.Min((_speed / turnRadius) * Mathf.Rad2Deg * dt, 180f - _turnedDegrees);
            _heading = Quaternion.Euler(0f, 0f, _turnSign * step) * _heading;
            _turnedDegrees += step;
            ApplyRotation();
            if (_turnedDegrees >= 180f)
            {
                _phase = Phase.Ascending;
                _heading = Vector2.up;
                ApplyRotation();
            }
        }
        else if (_phase == Phase.Descending && _camera != null
                 && _camera.WorldToViewportPoint(transform.position).y <= turnTriggerViewportY)
        {
            _turnSign = turnDirection == TurnDirection.AutoInward
                ? (transform.position.x > 0f ? -1f : 1f)
                : (turnDirection == TurnDirection.Right ? 1f : -1f);
            _phase = Phase.Turning;
        }

        transform.position += (Vector3)(_heading * (_speed * dt));
    }

    private void ApplyRotation()
    {
        if (!faceTravelDirection) return;
        transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(_heading.y, _heading.x) * Mathf.Rad2Deg - 90f);
    }
}
