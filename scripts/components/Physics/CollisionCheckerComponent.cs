using System;
using Eyes.Managers;
using Godot;

namespace Eyes.Components.Physics;

public partial class CollisionCheckerComponent : Node2D
{
    [Export] private AABBComponent _aabb;
    [Export] private bool _selfUpdate = false;

    public AABBComponent AABB => _aabb;

    [ExportGroup("Debug")]
    [Export] public bool IsColliding { get; private set; }
    [Export] public bool WasColliding { get; private set; }

    // TODO(calco): Make these Signals
    public Action OnCollide { get; set; }
    public Action OnSeparate { get; set; }

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
            OnSeparate?.Invoke();
        else if (!WasColliding && IsColliding)
            OnCollide?.Invoke();

        WasColliding = IsColliding;
    }
}