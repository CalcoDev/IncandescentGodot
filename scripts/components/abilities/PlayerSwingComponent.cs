using Godot;
using Incandescent.Components.Abilities.Definitions;
using Incandescent.Components.Logic;

namespace Incandescent.Components.Abilities;

public partial class PlayerSwingComponent : AbilityComponent
{
    [Export] private CustomTimerComponent _timer;

    private PlayerSwingAbilityDefinition _definition = new PlayerSwingAbilityDefinition();

    public override AbilityDefinition GetAbilityDefinition()
    {
        return _definition;
    }

    // TODO(calco): Maybe make this the base class implementation?
    public override bool TryActivate()
    {
        if (_cooldownTimer.IsRunning())
            return false;

        _cooldownTimer.Start(_definition.GetCooldown());
        return true;
    }

    public override void Enter()
    {
        GD.Print("Started Player Swing.");
        _timer.Start(1f);
    }

    public override int Update()
    {
        if (_timer.IsRunning())
            return SelfState;

        return FallbackState;
    }

    public override void Exit()
    {
        GD.Print("Ended Player Swing.");
    }
}