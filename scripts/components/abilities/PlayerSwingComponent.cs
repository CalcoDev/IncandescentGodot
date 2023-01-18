using Godot;
using Incandescent.Components.Abilities.Definitions;

namespace Incandescent.Components.Abilities;

public partial class PlayerSwingComponent : AbilityComponent
{
    private PlayerSwingAbilityDefinition _definition = new PlayerSwingAbilityDefinition();

    public override AbilityDefinition GetAbilityDefinition()
    {
        return _definition;
    }

    public override bool Activate()
    {
        if (_cooldownTimer.IsRunning())
            return false;

        _cooldownTimer.Start(_definition.GetCooldown());
        GD.Print("Swoosh swosh.");

        return true;
    }
}