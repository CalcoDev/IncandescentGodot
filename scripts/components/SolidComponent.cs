using System;
using System.Collections.Generic;
using Godot;
using Incandescent.Managers;

namespace Incandescent.Components;

public partial class SolidComponent : Node2D
{
    [ExportCategory("Solid")]
    [Export] public AxisAlignedBoundingBoxComponent BoundingBox { get; private set; }

    protected Vector2 Remainder;

    public override void _Ready()
    {
        BoundingBox.Size *= (Vector2i) Scale;
        PhysicsManager.Instance.AddSolid(this);
    }

    public override void _ExitTree()
    {
        PhysicsManager.Instance.RemoveSolid(this);
    }

    public void MoveX(float amount)
    {
        Remainder.x += amount;
        int move = Mathf.FloorToInt(Remainder.x);
        
        if (move == 0)
            return;

        List<ActorComponent> riders = PhysicsManager.Instance.GetRidingActors(this); 
        Remainder.x -= move;

        // TODO(calco): Move step by step. Otherwise, if moving too fast the actor will pass through the solid.
        GlobalPosition += new Vector2(move, 0);
        
        List<ActorComponent> actors = PhysicsManager.Instance.Actors;
        foreach (ActorComponent actor in actors)
        {
            if (BoundingBox.IntersectsRel(actor.BoundingBox, Vector2i.Zero))
            {
                if (amount < 0) 
                    actor.MoveXExact(BoundingBox.Left - actor.BoundingBox.Right);
                else
                    actor.MoveXExact(BoundingBox.Right - actor.BoundingBox.Left);
            }
            else if (riders.Contains(actor))
                actor.MoveXExact(move);
        }
    }

    public void MoveY(float amount)
    {
        Remainder.y += amount;
        int move = Mathf.FloorToInt(Remainder.y);
        
        if (move == 0)
            return;

        List<ActorComponent> riders = PhysicsManager.Instance.GetRidingActors(this); 
        Remainder.y -= move;
        
        // TODO(calco): Move step by step. Otherwise, if moving too fast the actor will pass through the solid.
        GlobalPosition += new Vector2(0, move);
        
        List<ActorComponent> actors = PhysicsManager.Instance.Actors;
        foreach (ActorComponent actor in actors)
        {
            if (BoundingBox.IntersectsRel(actor.BoundingBox, Vector2i.Zero))
            {
                if (amount < 0) 
                    actor.MoveYExact(BoundingBox.Top - actor.BoundingBox.Bottom);
                else
                    actor.MoveYExact(BoundingBox.Bottom - actor.BoundingBox.Top);
            }
            else if (riders.Contains(actor))
                actor.MoveYExact(move);
        }
    }
}