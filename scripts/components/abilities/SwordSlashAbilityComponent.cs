using Godot;
using Incandescent.Components.Abilities.Definitions;
using Incandescent.Components.Logic;

namespace Incandescent.Components.Abilities;

public partial class SwordSlashAbilityComponent : StatelessAbilityComponent
{
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
}