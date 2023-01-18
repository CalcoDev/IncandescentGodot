using Incandescent.Components.Abilities.Definitions;

namespace Incandescent.Components.Abilities;

public partial class LeapAbilityComponent : AbilityComponent
{
    private LeapAbilityDefinition _definition = new LeapAbilityDefinition();

    public override AbilityDefinition GetAbilityDefinition()
    {
        return _definition;
    }
}