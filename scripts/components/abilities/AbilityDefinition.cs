using Godot;

namespace Incandescent.Components.Abilities;

/// <summary>
/// Just a data container for an ability. In theory I could make this a "Scriptable Object" but it's not worth it.
/// </summary>
public abstract partial class AbilityDefinition : RefCounted
{
    // public abstract string Name { get; }
    // public abstract string Description { get; }

    public abstract string GetName();
    public abstract string GetDescription();

    public abstract float GetCooldown();
    public abstract bool IsStateful();
}