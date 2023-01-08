using Godot;

namespace Incandescent.Components;

public partial class TrailRendererComponent : Line2D
{
    [Export] public int Length { get; set; } = 100;
    [Export] public float Delay { get; set; } = 0.1f;
    [Export] public bool Emitting { get; set; } = true;

    [Export] private CustomTimerComponent _timer;

    private Node2D _parent;
    private Vector2 _offset;

    public override void _Ready()
    {
        _timer.SetTime(Delay);

        _parent = GetParent<Node2D>();
        _offset = Position;
    }

    public override void _Process(double delta)
    {
        _timer.Update((float)delta);

        GlobalPosition = Vector2.Zero;
        GlobalRotation = 0f;

        if (_timer.HasFinished())
        {
            _timer.SetTime(Delay);

            if (Emitting)
            {
                // Get a random number between 0 and 1
                // var random = GD.RandRange(-1, 1);

                AddPoint(_parent.GlobalPosition + _offset);
                // AddPoint(_parent.GlobalPosition + _offset + Vector2.Up * random);
            }

            if (GetPointCount() > Length)
            {
                RemovePoint(0);
                // RemovePoint(0);
            }
        }
    }
}