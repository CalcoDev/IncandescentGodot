using Godot;
using Incandescent.Components.Logic;

namespace Incandescent.Components.Abilities;

/// <summary>
/// More complex node handling the in game representation of an ability. The counterpart of <see cref="Abilities.AbilityDefinition"/>.
/// </summary>
public abstract partial class AbilityComponent : Node
{
    [Export] protected CustomTimerComponent _cooldownTimer;

    public abstract AbilityDefinition GetAbilityDefinition();

    public abstract bool Activate();
}