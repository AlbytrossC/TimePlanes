using UnityEngine;

/// <summary>
/// Design-time data for one enemy type, shared by every instance of it.
/// Create via Assets > Create > TimePlanes > Enemy Definition.
/// Runtime state (current HP etc.) lives on EnemyScript - never write to this
/// asset at runtime, it is shared between all enemies of the type.
/// </summary>
[CreateAssetMenu(fileName = "EnemyDefinition", menuName = "TimePlanes/Enemy Definition")]
public class EnemyDefinition : ScriptableObject
{
    [Header("Stats")]
    [Min(1)] public int maxHP = 1;
    [Min(0f)] public float moveSpeed = 2f;
    [Min(0)] public int scoreValue = 100;

    [Header("Timeline")]
    [Tooltip("Sprite shown while the Past timeline is active.")]
    public Sprite pastSprite;
    [Tooltip("Sprite shown while the Future timeline is active.")]
    public Sprite futureSprite;
    [Header("Leader (cosmetic - formations mark one member as the leader)")]
    [Tooltip("Leader sprite while the Past timeline is active. Falls back to pastSprite if empty.")]
    public Sprite leaderPastSprite;
    [Tooltip("Leader sprite while the Future timeline is active. Falls back to futureSprite if empty.")]
    public Sprite leaderFutureSprite;

    [Tooltip("If false, the enemy is hidden and harmless while the Past timeline is active.")]
    public bool existsInPast = true;
    [Tooltip("If false, the enemy is hidden and harmless while the Future timeline is active.")]
    public bool existsInFuture = true;
}
