using Godot;

namespace Incandescent.Components.Passives;

public abstract partial class PassiveComponent : Node2D
{
    public abstract PassiveDefinition GetPassiveDefinition();

    // Starting with only these two events. Will eventually like a custom Update method and things.
    public abstract void OnAddedToEntity();
    public abstract void OnRemovedFromEntity();
}