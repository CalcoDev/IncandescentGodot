using System;
using Godot;
using Godot.Collections;
using Incandescent.Managers;

namespace Incandescent.Components;

public partial class CollisionCheckerComponent : Node2D
{
    [ExportGroup("Settings")]
    [Export] private AxisAlignedBoundingBoxComponent _boundingBox;
    [Export] private bool _selfUpdate = false;

    public AxisAlignedBoundingBoxComponent AABB => _boundingBox;

    [ExportGroup("Debug")]
    [Export] public bool IsColliding { get; private set; }
    [Export] public bool WasColliding { get; private set; }

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
        IsColliding = PhysicsManager.Instance.CheckWithSolidsCollisionAt(_boundingBox, Vector2i.Zero);

        if (WasColliding && !IsColliding)
            OnSeparate?.Invoke();
        else if (!WasColliding && IsColliding)
            OnCollide?.Invoke();

        WasColliding = IsColliding;
    }
}