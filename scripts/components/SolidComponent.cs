using System.Collections.Generic;
using Godot;
using Incandescent.Managers;

namespace Incandescent.Components;

public partial class SolidComponent : Node2D
{
    [ExportCategory("Solid")]
    [Export] public AxisAlignedBoundingBoxComponent BoundingBox { get; private set; }
    [Export] public bool IsCollidable { get; set; } = true;

    public Vector2 Remainder => _remainder;
    private Vector2 _remainder;

    public override void _Ready()
    {
        // TODO(calco): Create a base class for Solids and Actors, to avoid code duplication like this.
        Vector2 t = GlobalPosition;
        TopLevel = true;
        GlobalPosition = t;

        // TODO(calco): This is awful lmao. Please fix somehow.
        BoundingBox.Size *= (Vector2i)Scale * (Vector2i)GetParent<Node2D>().Scale;
        PhysicsManager.Instance.AddSolid(this);
    }

    public override void _ExitTree()
    {
        PhysicsManager.Instance.RemoveSolid(this);
    }

    public void MoveX(float amount)
    {
        _remainder.x += amount;
        int move = Mathf.FloorToInt(Remainder.x);

        if (move == 0)
            return;

        List<ActorComponent> riders = PhysicsManager.Instance.GetRidingActors(this);
        IsCollidable = false;

        _remainder.x -= move;
        GlobalPosition += new Vector2(move, 0);

        List<ActorComponent> actors = PhysicsManager.Instance.Actors;
        foreach (ActorComponent actor in actors)
        {
            if (BoundingBox.IntersectsRel(actor.BoundingBox, Vector2i.Zero))
            {
                // Push left
                if (amount < 0)
                    actor.MoveXExact(BoundingBox.Left - actor.BoundingBox.Right, actor.Squish);
                // Push right
                else
                    actor.MoveXExact(BoundingBox.Right - actor.BoundingBox.Left, actor.Squish);
            }
            // Carry
            else if (riders.Contains(actor))
            {
                actor.MoveXExact(move);
            }
        }

        IsCollidable = true;
    }

    public void MoveY(float amount)
    {
        _remainder.y += amount;
        int move = Mathf.FloorToInt(Remainder.y);

        if (move == 0)
            return;

        List<ActorComponent> riders = PhysicsManager.Instance.GetRidingActors(this);
        IsCollidable = false;

        _remainder.y -= move;
        GlobalPosition += new Vector2(0, move);

        List<ActorComponent> actors = PhysicsManager.Instance.Actors;
        foreach (ActorComponent actor in actors)
        {
            if (BoundingBox.IntersectsRel(actor.BoundingBox, Vector2i.Zero))
            {
                // Push up
                if (amount < 0)
                    actor.MoveYExact(BoundingBox.Top - actor.BoundingBox.Bottom, actor.Squish);
                // Push down
                else
                    actor.MoveYExact(BoundingBox.Bottom - actor.BoundingBox.Top, actor.Squish);
            }
            // Carry
            else if (riders.Contains(actor))
            {
                actor.MoveYExact(move);
            }
        }

        IsCollidable = true;
    }
}