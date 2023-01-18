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

    public override void Enter()
    {
        GD.Print("Started Player Swing.");
        _timer.Start(1f);

        LatestActivationData.SenderVelocity.Set(LatestActivationData.Direction * 10f);
    }

    public override int Update()
    {
        if (_timer.IsRunning())
        {
            LatestActivationData.Sender.MoveX(LatestActivationData.SenderVelocity.X * (float)GetProcessDeltaTime());
            LatestActivationData.Sender.MoveY(LatestActivationData.SenderVelocity.Y * (float)GetProcessDeltaTime());
            return SelfState;
        }

        return FallbackState;
    }

    public override void Exit()
    {
        GD.Print("Ended Player Swing.");

        CooldownTimer.Start(_definition.GetCooldown());
    }
}