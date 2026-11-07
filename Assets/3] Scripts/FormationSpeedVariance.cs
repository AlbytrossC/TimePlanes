using UnityEngine;

/// <summary>
/// Put on a formation root: rolls one random speed multiplier at spawn and
/// applies it to every member's movement behaviour. Formations vary in pace
/// from each other; all planes within a formation share the multiplier and
/// stay coherent. Solo enemies (no formation root) always fly at base speed.
/// Keep the range above ~0.77x to stay faster than the level scroll (1.0)
/// when the base speed is 1.3; lower values make a formation drift backward
/// relative to the terrain, which reads as passive/retreating.
/// </summary>
public class FormationSpeedVariance : MonoBehaviour
{
    [Tooltip("Rolled multiplier range applied to every member's speed at spawn (shared by the whole formation).")]
    [SerializeField] private float minMultiplier = 0.85f;
    [SerializeField] private float maxMultiplier = 1.15f;

    private void Awake()
    {
        float multiplier = Random.Range(minMultiplier, maxMultiplier);
        foreach (ISpeedModifiable m in GetComponentsInChildren<ISpeedModifiable>())
            m.SetSpeedMultiplier(multiplier);
    }
}
