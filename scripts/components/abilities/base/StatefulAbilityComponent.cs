using System.Collections;
using Incandescent.Components.Logic;

namespace Incandescent.Components.Abilities;

public abstract partial class StatefulAbilityComponent : AbilityComponent
{
    public int SelfState { get; private set; }
    public int FallbackState { get; private set; }

    public void SetStates(int selfState, int fallbackState)
    {
        SelfState = selfState;
        FallbackState = fallbackState;
    }

    public virtual void Enter() { }
    public virtual int Update() { return SelfState; }
    public virtual void Exit() { }
    public virtual IEnumerator Coroutine() { yield break; }

    public void AddSelfToStateMachine(StateMachineComponent stateMachine, int selfState = -1, int fallbackState = -1)
    {
        if (selfState != -1 || fallbackState != -1)
            SetStates(selfState, fallbackState);

        stateMachine.SetCallbacks(SelfState, Update, Enter, Exit, Coroutine);
    }
}