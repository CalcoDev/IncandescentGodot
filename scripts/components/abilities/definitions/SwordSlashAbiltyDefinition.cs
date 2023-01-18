using Godot;

namespace Incandescent.Components.Abilities.Definitions;

public partial class SwordSlashAbilityDefinition : AbilityDefinition
{
    public override string GetName()
    {
        return "Swinger";
    }

    public override string GetDescription()
    {
        return "Swing your sword in a cool fashion.";
    }

    public override float GetCooldown()
    {
        return 1f;
    }

    public override bool IsStateful()
    {
        return true;
    }
}