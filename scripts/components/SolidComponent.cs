using Godot;
using Incandescent.Managers;

namespace Incandescent.Components;

public partial class SolidComponent : Node2D
{
    [ExportCategory("Solid")]
    [Export] public AxisAlignedBoundingBoxComponent BoundingBox { get; private set; }

    public override void _Ready()
    {
        Node2D parent = GetParent<Node2D>();

        BoundingBox.Size *= (Vector2i) parent.Scale;
        
        SolidManager.Instance.AddSolid(this);
    }

    public override void _ExitTree()
    {
        SolidManager.Instance.RemoveSolid(this);
    }
}