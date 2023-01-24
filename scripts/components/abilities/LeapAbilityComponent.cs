using Godot;
using Incandescent.Components.Abilities.Definitions;
using Incandescent.Components.Logic;

namespace Incandescent.Components.Abilities;

public partial class LeapAbilityComponent : StatefulAbilityComponent
{
    private LeapAbilityDefinition _definition = new LeapAbilityDefinition();

    private CustomTimerComponent _leapTimer;

    public override AbilityDefinition GetAbilityDefinition()
    {
        return _definition;
    }

    public override void Enter()
    {
        GD.Print("Started leap.");
        _leapTimer = CustomTimerComponent.Create(this, 0.5f, true);
    }

    public override int Update()
    {
        if (!_leapTimer.HasFinished())
            return SelfState;

        return FallbackState;
    }

    public override void Exit()
    {
        GD.Print("Finished leap.");
    }
}