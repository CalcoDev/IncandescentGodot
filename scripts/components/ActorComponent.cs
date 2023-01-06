using System;
using System.Collections.Generic;
using Godot;
using Incandescent.Managers;

namespace Incandescent.Components;

public partial class ActorComponent : Node2D
{
    [ExportCategory("Actor")]
    [Export] public AxisAlignedBoundingBoxComponent BoundingBox { get; private set; }
    [Export] public bool AutoStopCollision { get; set; } = true;

    public Vector2 Velocity { get; protected set; }

    protected Vector2 Remainder;

    public override void _Ready()
    {
        PhysicsManager.Instance.AddActor(this);
    }

    public override void _ExitTree()
    {
        PhysicsManager.Instance.RemoveActor(this);
    }

    public void MoveX(float amount, Action<AxisAlignedBoundingBoxComponent> onCollision = null)
    {
        Remainder.x += amount;
        int move = Mathf.FloorToInt(Remainder.x);
        
        if (move == 0)
            return;
        
        Remainder.x -= move;
        MoveXExact(move, onCollision);
    }

    public void MoveY(float amount, Action<AxisAlignedBoundingBoxComponent> onCollision = null)
    {
        Remainder.y += amount;
        int move = Mathf.FloorToInt(Remainder.y);
        
        if (move == 0)
            return;
        
        Remainder.y -= move;
        MoveYExact(move, onCollision);
    }
    
    public void MoveXExact(int amount, Action<AxisAlignedBoundingBoxComponent> onCollision = null)
    {
        int step = Mathf.Sign(amount);
        
        while (amount != 0)
        {
            List<AxisAlignedBoundingBoxComponent> aabbs =
                PhysicsManager.Instance.GetCollisionsWithSolidsAt(BoundingBox, Vector2i.Right * step);
            
            if (aabbs.Count == 0)
            {
                GlobalPosition += new Vector2(step, 0);
                amount -= step;
            }
            else
            {
                if (AutoStopCollision)
                    Velocity = new Vector2(0f, Velocity.y);
                
                foreach (var aabb in aabbs)
                    onCollision?.Invoke(aabb);
                break;
            }
        }
    }

    public void MoveYExact(int amount, Action<AxisAlignedBoundingBoxComponent> onCollision = null)
    {
        int step = Mathf.Sign(amount);
        
        while (amount != 0)
        {
            List<AxisAlignedBoundingBoxComponent> aabbs =
                PhysicsManager.Instance.GetCollisionsWithSolidsAt(BoundingBox, Vector2i.Down * step);
            
            if (aabbs.Count == 0)
            {
                GlobalPosition += new Vector2(0, step);
                amount -= step;
            }
            else
            {
                if (AutoStopCollision)
                    Velocity = new Vector2(Velocity.x, 0f);
                
                foreach (var aabb in aabbs)
                    onCollision?.Invoke(aabb);
                break;
            }
        }
    }

    public bool IsRiding(SolidComponent solid, Vector2i offset)
    {
        return !BoundingBox.IntersectsRel(solid.BoundingBox, Vector2i.Zero) &&
               BoundingBox.IntersectsRel(solid.BoundingBox, offset);
    }
}