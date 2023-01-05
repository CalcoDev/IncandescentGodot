using System;
using System.Collections.Generic;
using Godot;
using Incandescent.Managers;

namespace Incandescent.Components;

public partial class ActorComponent : Node2D
{
    [ExportCategory("Actor")]
    [Export] public AxisAlignedBoundingBoxComponent BoundingBox { get; private set; }

    public Vector2i IntPosition { get; private set; } = Vector2i.Zero;
    
    public Vector2 Velocity { get; set; }

    private Vector2 _remainder;

    public override void _EnterTree()
    {
        IntPosition = (Vector2i) GlobalPosition;
    }

    public void MoveX(float amount, Action<AxisAlignedBoundingBoxComponent> onCollision = null)
    {
        _remainder.x += amount;
        // int move = Mathf.RoundToInt(_remainder.x);
        int move = Mathf.FloorToInt(_remainder.x);
        
        if (move == 0)
            return;
        
        _remainder.x -= move;
        int step = Mathf.Sign(move);
        
        while (move != 0)
        {
            List<AxisAlignedBoundingBoxComponent> aabbs =
                SolidManager.Instance.GetCollisionsAt(BoundingBox, Vector2i.Right * step);
            
            if (aabbs.Count == 0)
            {
                IntPosition += new Vector2i(step, 0);
                move -= step;
            }
            else
            {
                Velocity = new Vector2(0f, Velocity.y);
                
                foreach (var aabb in aabbs)
                    onCollision?.Invoke(aabb);
                break;
            }
        }
    }

    public void MoveY(float amount, Action<AxisAlignedBoundingBoxComponent> onCollision = null)
    {
        _remainder.y += amount;
        // int move = Mathf.RoundToInt(_remainder.y);
        int move = Mathf.FloorToInt(_remainder.y);
        
        if (move == 0)
            return;
        
        _remainder.y -= move;
        int step = Mathf.Sign(move);
        
        while (move != 0)
        {
            List<AxisAlignedBoundingBoxComponent> aabbs =
                SolidManager.Instance.GetCollisionsAt(BoundingBox, Vector2i.Down * step);
            
            if (aabbs.Count == 0)
            {
                IntPosition += new Vector2i(0, step);
                move -= step;
            }
            else
            {
                Velocity = new Vector2(Velocity.x, 0f);
                
                // foreach (var aabb in aabbs)
                    // onCollision?.Invoke(aabb);
                break;
            }
        }
    }
}