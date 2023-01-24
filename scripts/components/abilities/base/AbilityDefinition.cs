using Godot;

namespace Incandescent.Components.Abilities;

/// <summary>
/// Just a data container for an ability. In theory I could make this a "Scriptable Object" but it's not worth it.
/// </summary>
public abstract partial class AbilityDefinition : RefCounted
{
    // TODO(calco): Consider making these properties instead of methods.
    public abstract string GetName();
    public abstract string GetDescription();

    public abstract float GetCooldown();
    public abstract bool IsStateful();
}