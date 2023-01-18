using Godot;

namespace Incandescent.Components.Abilities.Definitions;

public partial class PlayerSwingAbilityDefinition : AbilityDefinition
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
}