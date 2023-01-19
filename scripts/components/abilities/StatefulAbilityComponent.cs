using System.Collections;

namespace Incandescent.Components.Abilities;

public abstract partial class StatefulAbilityComponent : AbilityComponent
{
    protected int SelfState { get; private set; }
    protected int FallbackState { get; private set; }

    public void SetStates(int selfState, int fallbackState)
    {
        SelfState = selfState;
        FallbackState = fallbackState;
    }

    public virtual void Enter() { }
    public virtual int Update() { return SelfState; }
    public virtual void Exit() { }
    public virtual IEnumerator Coroutine() { yield break; }
}