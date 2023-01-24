namespace Incandescent.Components.Passives.Definitions;

public partial class FlamePassiveDefinition : PassiveDefinition
{
    public override string GetName()
    {
        return "Flame";
    }

    public override string GetDescription()
    {
        return "On hit, set the target on fire.";
    }
}