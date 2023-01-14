using Eyes.Managers;
using Godot;

namespace Eyes.Components.Physics;

[Tool]
public partial class AABBComponent : Node2D
{
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

    public Vector2i IntPosition => (Vector2i)GlobalPosition + PositionOffset;

    public int Left => IntPosition.x;
    public int Right => IntPosition.x + Size.x;
    public int Top => IntPosition.y;
    public int Bottom => IntPosition.y + Size.y;

    public int CentreX => IntPosition.x + (Size.x / 2);
    public int CentreY => IntPosition.y + (Size.y / 2);

    private Vector2i _size;
    private Vector2i _positionOffset;

    public override void _Draw()
    {
        if (Engine.IsEditorHint() || GameManager.DebugMode)
            DrawRect(new Rect2(_positionOffset, _size), new Color(0f, .65f, .75f, 0.5f));
    }

    public bool IntersectsRel(AABBComponent other, Vector2i positionOffset)
    {
        return Left + positionOffset.x < other.Right && Right + positionOffset.x > other.Left &&
               Top + positionOffset.y < other.Bottom && Bottom + positionOffset.y > other.Top;
    }
}