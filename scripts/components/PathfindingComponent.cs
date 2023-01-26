using Godot;
using GodotUtilities;

namespace Incandescent.Components.Logic;

public partial class PathfindingComponent : Node2D
{
    [Node("NavigationAgent2D")]
    public NavigationAgent2D Agent { get; private set; }
    [Node("IntervalTimer")]
    private CustomTimerComponent _intervalTimer;

    public Vector2 LastComputedVelocity { get; private set; }

    [Export]
    private float _interval = 0.1f;

    public override void _Notification(long what)
    {
        if (what == NotificationEnterTree)
        {
            GD.Print("Wired nodes.");
            this.WireNodes();
        }
    }

    public override void _Ready()
    {
        Agent.VelocityComputed += OnVelocityComputed;

        _intervalTimer.OnTimeout += () =>
        {
            _intervalTimer.SetTime(_interval);
        };
    }

    public override void _PhysicsProcess(double delta)
    {
        _intervalTimer.Update((float)delta);
    }

    public void SetTargetPosition(Vector2 targetPos)
    {
        Agent.TargetLocation = targetPos;

        if (!_intervalTimer.IsRunning())
            _intervalTimer.SetTime(_interval);
    }

    public void SetCurrentVelocity(Vector2 velocity)
    {
        Agent.SetVelocity(velocity);
    }

    private void OnVelocityComputed(Vector2 velocity)
    {
        LastComputedVelocity = velocity;
    }
}