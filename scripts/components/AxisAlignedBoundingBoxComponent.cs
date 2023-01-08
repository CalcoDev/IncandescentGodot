using Godot;

namespace Incandescent.Components;

[Tool]
public partial class AxisAlignedBoundingBoxComponent : Node2D
{
    [ExportCategory("AABB")]
    [Export]
    public Vector2i Size
    {
        get => _size;
        set
        {
            _size = value;
            QueueRedraw();
        }
    }
    [Export]
    public Vector2i PositionOffset
    {
        get => _positionOffset;
        set
        {
            _positionOffset = value;
            QueueRedraw();
        }
    }
    [Export]
    public Color Color
    {
        get => _color;
        set
        {
            _color = value;
            QueueRedraw();
        }
    }

    public Vector2i IntPosition => (Vector2i)GlobalPosition + PositionOffset;

    public int Left => IntPosition.x;
    public int Right => IntPosition.x + Size.x;
    public int Top => IntPosition.y;
    public int Bottom => IntPosition.y + Size.y;

    private Vector2i _positionOffset = Vector2i.Zero;
    private Vector2i _size = Vector2i.One * 8;
    private Color _color = new(1f, 0.55f, 0f, 0.25f);

    public override void _Draw()
    {
        if (Engine.IsEditorHint())
            DrawRect(new Rect2(_positionOffset, _size), _color);
    }

    public bool IntersectsRel(AxisAlignedBoundingBoxComponent other, Vector2i positionOffset)
    {
        return Left + positionOffset.x < other.Right && Right + positionOffset.x > other.Left &&
               Top + positionOffset.y < other.Bottom && Bottom + positionOffset.y > other.Top;
    }
}