using System;
using Godot;
using Incandescent.Managers;

namespace Incandescent.Components.Physics;

public partial class CollisionCheckerComponent : Node2D
{
    [Export] private AABBComponent _aabb;
    [Export] private bool _selfUpdate = false;

    public AABBComponent AABB => _aabb;

    [ExportGroup("Debug")]
    [Export] public bool IsColliding { get; private set; }
    [Export] public bool WasColliding { get; private set; }

    [Signal]
    public delegate void OnCollideEventHandler();

    [Signal]
    public delegate void OnSeparateEventHandler();

    public override void _Process(double delta)
    {
        if (!_selfUpdate)
            return;

        Update();
    }

    public void Update()
    {
        IsColliding = LevelManager.CollideAt(_aabb, LevelManager.Instance.Solids, Vector2i.Zero);

        if (WasColliding && !IsColliding)
            EmitSignal(SignalName.OnSeparate);
        else if (!WasColliding && IsColliding)
            EmitSignal(SignalName.OnCollide);

        WasColliding = IsColliding;
    }
}