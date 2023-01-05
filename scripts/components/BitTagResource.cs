using Godot;

namespace Incandescent.Components;

public partial class BitTagResource : Resource
{
    [Export] public string Tag { get; set; }

    public BitTagResource() : this("")
    {
    }

    public BitTagResource(string tag)
    {
        Tag = tag;
    }
}