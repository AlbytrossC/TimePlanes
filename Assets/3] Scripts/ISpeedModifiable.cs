/// <summary>
/// Implemented by movement behaviours whose speed can be scaled externally.
/// FormationSpeedVariance rolls one multiplier per formation at spawn and
/// applies it through this interface, so formations vary in pace from each
/// other while all planes within a formation stay coherent.
/// </summary>
public interface ISpeedModifiable
{
    /// <summary>Scales the resolved speed. Applied at spawn, before Start().</summary>
    void SetSpeedMultiplier(float multiplier);
    /// <summary>The resolved speed in world units per second (valid after Start()).</summary>
    float CurrentSpeed { get; }
}
