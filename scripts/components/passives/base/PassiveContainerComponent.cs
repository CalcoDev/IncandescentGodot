using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace Incandescent.Components.Passives;

// TODO(calco): Make this a Node, but don't children elements
public partial class PassiveContainerComponent : Node
{
    private readonly List<PassiveComponent> _passives = new List<PassiveComponent>();

    public void AddPassive(PassiveComponent passive)
    {
        passive.OnAddedToEntity();
        _passives.Add(passive);
    }

    public void RemovePassive(PassiveComponent passive)
    {
        passive.OnRemovedFromEntity();
        _passives.Remove(passive);
    }
}