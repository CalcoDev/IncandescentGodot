using System;
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

    [Signal]
    public delegate void OnVelocityChangedEventHandler(Vector2 velocity);

    [Export]
    private float _interval = 0.2f;

    private Vector2 _targetPos;

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
    }

    public override void _PhysicsProcess(double delta)
    {
        _intervalTimer.Update((float)delta);
    }

    public void SetTargetInterval(Vector2 targetPos)
    {
        if (_intervalTimer.IsRunning() || Agent.TargetLocation == targetPos)
            return;

        GD.Print("Set target interval.");

        _intervalTimer.Start(_interval);
        Agent.TargetLocation = targetPos;
    }

    private void OnVelocityComputed(Vector2 velocity)
    {
        LastComputedVelocity = velocity;
        EmitSignal(SignalName.OnVelocityChanged, velocity);
    }
}