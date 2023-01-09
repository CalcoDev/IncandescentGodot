using Godot;

namespace Incandescent.Components;

public partial class TrailRendererComponent : Line2D
{
    [Export] public int Length { get; set; } = 100;
    [Export] public float Delay { get; set; } = 0.1f;
    [Export]
    public bool Emitting
    {
        get { return emitting; }
        set
        {
            emitting = value;
            if (!emitting)
                _addonPoints = 0;
        }
    }

    [Export] private CustomTimerComponent _timer;

    private Node2D _parent;
    private Vector2 _offset;

    private int _addonPoints;
    private bool emitting = true;

    public void ResetTimerToZero()
    {
        _timer.SetTime(0f);
    }

    public void StartClearingPoints()
    {
        _addonPoints = Length - GetPointCount();
    }

    public override void _Ready()
    {
        _timer.SetTime(Delay);
        _offset = Position;

        // FIXME(calco): Probably shouldn't reparent.
        CallDeferred(nameof(Reparent));
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
                GD.Print($"Added point at: {_parent.GlobalPosition + _offset}");
                AddPoint(_parent.GlobalPosition + _offset);
            }
            else
            {
                _addonPoints += 1;
            }

            if (GetPointCount() > Length || (!Emitting && GetPointCount() > 0 && GetPointCount() + _addonPoints > Length))
            {
                GD.Print($"Removing point at: {Points[0]}");
                RemovePoint(0);
            }
        }
    }

    private void Reparent()
    {
        _parent = GetParent<Node2D>();

        _parent.RemoveChild(this);
        _parent.GetTree().Root.AddChild(this);
    }
}