namespace Incandescent.Components.Abilities.Definitions;

public partial class LeapAbilityDefinition : AbilityDefinition
{
    public override string GetName()
    {
        return "Leapy Boy";
    }

    public override string GetDescription()
    {
        return "GO FIAUUUU.";
    }

    public override float GetCooldown()
    {
        return 1.5f;
    }

    public override bool IsStateful()
    {
        return true;
    }
}