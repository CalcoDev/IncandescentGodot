using System.Collections;
using Godot;
using Incandescent.Components.Logic;

namespace Incandescent.Components.Abilities;

// TODO(calco): Refactor this to be a Node2D to allow for cool sprites and things.
/// <summary>
/// More complex node handling the in game representation of an ability. The counterpart of <see cref="AbilityDefinition"/>.
/// </summary>
public abstract partial class AbilityComponent : Node2D
{
    [Export] protected CustomTimerComponent CooldownTimer;

    public abstract AbilityDefinition GetAbilityDefinition();

    protected AbilityActivationData LatestActivationData { get; private set; }
    public virtual bool CanActivate()
    {
        return !CooldownTimer.IsRunning();
    }

    /// <summary>
    /// Doesn't check if the ability can be activated.
    /// </summary>
    public int Activate(AbilityActivationData activationData)
    {
        LatestActivationData = activationData;

        var stateless = this.AsStateless();
        if (stateless != null)
        {
            stateless.Activate();
            return -1;
        }
        return this.AsStateful().SelfState;
    }

    // TODO(calco): This is probably quite bad, and should be handled locally, but it's easier this way.
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