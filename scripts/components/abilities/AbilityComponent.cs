using System.Collections;
using Godot;
using Incandescent.Components.Logic;

namespace Incandescent.Components.Abilities;

// TODO(calco): Refactor this to be a Node2D to allow for cool sprites and things.
/// <summary>
/// More complex node handling the in game representation of an ability. The counterpart of <see cref="AbilityDefinition"/>.
/// </summary>
public abstract partial class AbilityComponent : Node
{
    [Export] protected CustomTimerComponent CooldownTimer;

    public abstract AbilityDefinition GetAbilityDefinition();

    public virtual bool TryActivate(AbilityActivationData activationData)
    {
        if (!CooldownTimer.IsRunning())
        {
            LatestActivationData = activationData;
            return true;
        }

        return false;
    }

    protected int SelfState { get; private set; }
    protected int FallbackState { get; private set; }
    protected AbilityActivationData LatestActivationData { get; private set; }

    public void SetStates(int selfState, int fallbackState)
    {
        SelfState = selfState;
        FallbackState = fallbackState;
    }

    public virtual void Enter() { }
    public virtual int Update()
    {
        return SelfState;
    }
    public virtual void Exit() { }
    public virtual IEnumerator Coroutine()
    {
        yield break;
    }
}