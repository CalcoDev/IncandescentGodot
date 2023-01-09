using System;
using System.Collections.Generic;
using Godot;
using Incandescent.Managers;

namespace Incandescent.Components;

public partial class ActorComponent : Node2D
{
    [ExportCategory("Actor")]
    [Export] public AxisAlignedBoundingBoxComponent BoundingBox { get; private set; }

    public Vector2 Remainder => _remainder;
    private Vector2 _remainder;

    public override void _Ready()
    {
        Vector2 t = GlobalPosition;
        TopLevel = true;
        GlobalPosition = t;

        PhysicsManager.Instance.AddActor(this);
    }

    public override void _ExitTree()
    {
        PhysicsManager.Instance.RemoveActor(this);
    }

    public virtual bool IsRiding(SolidComponent solid, Vector2i offset)
    {
        return !BoundingBox.IntersectsRel(solid.BoundingBox, Vector2i.Zero) &&
               BoundingBox.IntersectsRel(solid.BoundingBox, offset);
    }

    public virtual void Squish(AxisAlignedBoundingBoxComponent aabb)
    {
    }

    public void ClearRemainderX()
    {
        _remainder.x = 0f;
    }

    public void ClearRemainderY()
    {
        _remainder.y = 0f;
    }

    public void MoveX(float amount, Action<AxisAlignedBoundingBoxComponent> onCollision = null)
    {
        _remainder.x += amount;
        int move = Mathf.FloorToInt(_remainder.x);

        if (move == 0)
            return;

        _remainder.x -= move;
        MoveXExact(move, onCollision);
    }

    public void MoveY(float amount, Action<AxisAlignedBoundingBoxComponent> onCollision = null)
    {
        _remainder.y += amount;
        int move = Mathf.FloorToInt(_remainder.y);

        if (move == 0)
            return;

        _remainder.y -= move;
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
                Position += new Vector2(step, 0);
                amount -= step;
            }
            else
            {
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
                Position += new Vector2(0, step);
                amount -= step;
            }
            else
            {
                foreach (var aabb in aabbs)
                    onCollision?.Invoke(aabb);
                break;
            }
        }
    }
}