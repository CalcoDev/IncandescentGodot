using System.Collections;
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

    public abstract bool TryActivate();

    public int SelfState { get; set; }
    public int FallbackState { get; set; }

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