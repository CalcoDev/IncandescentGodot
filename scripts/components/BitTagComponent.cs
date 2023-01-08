using Godot;
using Godot.Collections;

namespace Incandescent.Components;

public partial class BitTagComponent : Node
{
    [Export] private Array<BitTagResource> _tags;

    public bool HasTag(BitTagResource bitTag)
    {
        return _tags.Contains(bitTag);
    }

    public bool HasTagString(string bitTag)
    {
        foreach (var bitTagResource in _tags)
        {
            if (bitTagResource.Tag == bitTag)
                return true;
        }

        return false;
    }

    public void AddTag(BitTagResource bitTag)
    {
        if (!HasTag(bitTag))
            _tags.Add(bitTag);
    }
}