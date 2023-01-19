using System.Collections;
using Godot;
using Incandescent.Components.Logic;

namespace Incandescent.Components.Abilities;

// TODO(calco): Refactor this to be a Node2D to allow for cool sprites and things.
// TODO(calco): Maybe split this into a StatefulAbilityComponent and a StatelessAbilityComponent.
/// <summary>
/// More complex node handling the in game representation of an ability. The counterpart of <see cref="AbilityDefinition"/>.
/// </summary>
public abstract partial class AbilityComponent : Node
{
    [Export] protected CustomTimerComponent CooldownTimer;

    public abstract AbilityDefinition GetAbilityDefinition();

    protected AbilityActivationData LatestActivationData { get; private set; }
    public virtual bool TryActivate(AbilityActivationData activationData)
    {
        if (!CooldownTimer.IsRunning())
        {
            LatestActivationData = activationData;
            return true;
        }

        return false;
    }

    // TODO(calco): This is probably quite bad, and should be handled locally, but it's easier.
    public StatefulAbilityComponent AsStateful()
    {
        if (GetAbilityDefinition().IsStateful())
            return this as StatefulAbilityComponent;

        return null;
    }

    public StatelessAbilityComponent AsStateless()
    {
        if (!GetAbilityDefinition().IsStateful())
            return this as StatelessAbilityComponent;

        return null;
    }
}