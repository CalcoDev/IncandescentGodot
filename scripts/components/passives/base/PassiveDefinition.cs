using Godot;

namespace Incandescent.Components.Passives;

public abstract partial class PassiveDefinition : RefCounted
{
    // TODO(calco): Consider making these properties instead of methods.
    public abstract string GetName();
    public abstract string GetDescription();
}