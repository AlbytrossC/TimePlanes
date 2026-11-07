using UnityEngine;

/// <summary>
/// Per-plane path behaviour: fly diagonally down-screen, banking smoothly off
/// the left and right edges of the visible level area (the plane arcs through
/// the turn and hugs the edge, like the U-turn group's turn), until leaving
/// the bottom of the screen (DestroyWhenOffscreen handles cleanup).
///
/// Every plane runs its own copy of this logic - there is no leader. Planes
/// staggered along the same diagonal each bounce at the same world edges,
/// producing row-like path following without any plane depending on another.
/// Start the formation off-screen left/right (behind the HUD bars); planes are
/// safe there thanks to DestroyWhenOffscreen's requireOnScreenFirst.
/// </summary>
public class EnemyPathDiagonalBounce : MonoBehaviour, ISpeedModifiable
{
    public enum HorizontalDirection { Right, Left }
    private enum Phase { Flying, Turning }

    [Tooltip("Which way this plane flies horizontally at the start. Right = enters from the left side (L->R variant).")]
    [SerializeField] private HorizontalDirection horizontalDirection = HorizontalDirection.Right;
    [Tooltip("Descent angle in degrees below horizontal. 30 = mostly across, gently down.")]
    [Range(10f, 80f)] [SerializeField] private float angleDegrees = 30f;
    [Tooltip("World units per second. 0 = use the EnemyDefinition's moveSpeed.")]
    [SerializeField] private float speed = 0f;
    [Tooltip("Inset from the level-area side edges where the bounce happens (world units) - keeps the sprite fully inside.")]
    [SerializeField] private float bounceInset = 0.4f;
    [Tooltip("Radius of the banked turn when bouncing (world units). Smaller = tighter turn.")]
    [Min(0.1f)] [SerializeField] private float bounceTurnRadius = 0.6f;
    [Tooltip("Rotate the sprite to face the direction of travel (sprites are authored facing up).")]
    [SerializeField] private bool faceTravelDirection = true;

    private Phase _phase = Phase.Flying;
    private Vector2 _heading;
    private float _turnDegreesLeft;
    private float _turnSign = 1f;
    private float _edgeX;
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

        float rad = angleDegrees * Mathf.Deg2Rad;
        float sign = horizontalDirection == HorizontalDirection.Right ? 1f : -1f;
        _heading = new Vector2(sign * Mathf.Cos(rad), -Mathf.Sin(rad));
        ApplyRotation();
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        if (_phase == Phase.Turning)
        {
            // Banked turn: heading sweeps to its mirror image while the plane
            // holds its x at the edge (hugs the wall through the arc).
            float step = Mathf.Min((_speed / bounceTurnRadius) * Mathf.Rad2Deg * dt, _turnDegreesLeft);
            _heading = Quaternion.Euler(0f, 0f, _turnSign * step) * _heading;
            _turnDegreesLeft -= step;
            ApplyRotation();

            Vector3 tp = transform.position + (Vector3)(_heading * (_speed * dt));
            tp.x = _edgeX;
            transform.position = tp;

            if (_turnDegreesLeft <= 0f)
            {
                _heading.y = -Mathf.Abs(_heading.y); // guarantee still descending
                _phase = Phase.Flying;
            }
            return;
        }

        Vector3 pos = transform.position + (Vector3)(_heading * (_speed * dt));

        float minX, maxX;
        if (LevelScroller.Instance != null)
        {
            Rect area = LevelScroller.Instance.GetPlayArea();
            minX = area.xMin + bounceInset;
            maxX = area.xMax - bounceInset;
        }
        else if (_camera != null)
        {
            float halfW = _camera.orthographicSize * _camera.aspect;
            minX = _camera.transform.position.x - halfW + bounceInset;
            maxX = _camera.transform.position.x + halfW - bounceInset;
        }
        else { minX = float.MinValue; maxX = float.MaxValue; }

        if (_heading.x > 0f && pos.x >= maxX)
        {
            pos.x = maxX;
            BeginTurn(maxX);
        }
        else if (_heading.x < 0f && pos.x <= minX)
        {
            pos.x = minX;
            BeginTurn(minX);
        }

        transform.position = pos;
    }

    private void BeginTurn(float edgeX)
    {
        _phase = Phase.Turning;
        _edgeX = edgeX;
        // Bouncing flips the horizontal heading: from -angle to -(180 - angle).
        _turnDegreesLeft = 180f - 2f * angleDegrees;
        // Heading down-right rotates clockwise (negative), down-left counterclockwise.
        _turnSign = _heading.x > 0f ? -1f : 1f;
    }

    private void ApplyRotation()
    {
        if (!faceTravelDirection) return;
        transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(_heading.y, _heading.x) * Mathf.Rad2Deg - 90f);
    }
}
