using Godot;
using Incandescent.Components.Abilities.Definitions;
using Incandescent.Components.Logic;

namespace Incandescent.Components.Abilities;

public partial class SwordSlashAbilityComponent : AbilityComponent
{
    [Export] private CustomTimerComponent _timer;

    private SwordSlashAbilityDefinition _definition = new SwordSlashAbilityDefinition();

    public override AbilityDefinition GetAbilityDefinition()
    {
        return _definition;
    }

    public override void Activate()
    {
        GD.Print("Swing!");

        CooldownTimer.Start(GetAbilityDefinition().GetCooldown());
    }

    public override void Enter()
    {
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
        CooldownTimer.Start(_definition.GetCooldown());
    }
}