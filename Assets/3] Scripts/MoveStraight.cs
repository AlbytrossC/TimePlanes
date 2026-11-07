using UnityEngine;

/// <summary>
/// Moves the object in a straight line at a constant speed.
/// Default: straight down-screen (the classic solo enemy drift).
/// If speed is left at 0 and an EnemyScript with a definition is present,
/// the definition's moveSpeed is used instead.
/// </summary>
public class MoveStraight : MonoBehaviour, ISpeedModifiable
{
    [Tooltip("Direction of travel (normalized on use).")]
    [SerializeField] private Vector2 direction = Vector2.down;
    [Tooltip("World units per second. 0 = use the EnemyDefinition's moveSpeed (fallback 2).")]
    [SerializeField] private float speed = 0f;

    private float _speed;
    private float _speedMultiplier = 1f;

    public void SetSpeedMultiplier(float multiplier) { _speedMultiplier = multiplier; }
    public float CurrentSpeed => _speed;

    // Resolved in Start (not Awake) so a FormationSpeedVariance on a parent
    // formation root can apply its multiplier first.
    private void Start()
    {
        _speed = speed;
        if (_speed <= 0f)
        {
            EnemyScript enemy = GetComponent<EnemyScript>();
            _speed = (enemy != null && enemy.Definition != null) ? enemy.Definition.moveSpeed : 2f;
        }
        _speed *= _speedMultiplier;
    }

    private void Update()
    {
        transform.position += (Vector3)(direction.normalized * (_speed * Time.deltaTime));
    }
}
