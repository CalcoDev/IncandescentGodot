using Godot;
using Incandescent.Components.Passives.Definitions;

namespace Incandescent.Components.Passives;

public partial class FlamePassiveComponent : PassiveComponent
{
    private FlamePassiveDefinition _definition = new FlamePassiveDefinition();

    public override PassiveDefinition GetPassiveDefinition()
    {
        return _definition;
    }

    public override void OnAddedToEntity()
    {
        GD.Print("Added Flame Passive to entity.");
    }

    public override void OnRemovedFromEntity()
    {
        GD.Print("Removed Flame Passive from entity.");
    }
}