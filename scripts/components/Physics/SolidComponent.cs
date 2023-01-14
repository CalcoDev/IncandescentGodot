using System.Collections.Generic;
using System.Linq;
using Godot;
using Incandescent.Managers;

namespace Incandescent.Components.Physics;

public partial class SolidComponent : PhysicsBodyComponent
{
    public override void _Ready()
    {
        Vector2 t = GlobalPosition;
        TopLevel = true;
        GlobalPosition = t;

        LevelManager.Instance.AddSolid(this);
    }

    public override void _ExitTree()
    {
        LevelManager.Instance.RemoveSolid(this);
    }

    public void MoveX(float amount)
    {
        _remainder.x += amount;
        int move = Mathf.FloorToInt(_remainder.x);

        if (move == 0)
            return;

        IsCollidable = false;

        _remainder.x -= move;
        GlobalPosition += new Vector2(move, 0f);

        List<ActorComponent> riders = GetRidingActors().ToList();
        foreach (ActorComponent actor in LevelManager.Instance.Actors)
        {
            if (AABB.IntersectsRel(actor.AABB, Vector2i.Zero))
            {
                // TODO(calco): If the solid and the actor fully overlap, this will push the actor, stopping the squash.
                // Push left
                if (amount < 0)
                    actor.MoveXExact(AABB.Left - actor.AABB.Right, actor.Squish);
                // Push right
                else
                    actor.MoveXExact(AABB.Right - actor.AABB.Left, actor.Squish);
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
        int move = Mathf.FloorToInt(_remainder.y);

        if (move == 0)
            return;

        IsCollidable = false;

        _remainder.y -= move;
        GlobalPosition += new Vector2(0f, move);

        List<ActorComponent> riders = GetRidingActors().ToList();
        foreach (ActorComponent actor in LevelManager.Instance.Actors)
        {
            if (AABB.IntersectsRel(actor.AABB, Vector2i.Zero))
            {
                // TODO(calco): If the solid and the actor fully overlap, this will push the actor, stopping the squash.
                // Push top
                if (amount < 0)
                    actor.MoveYExact(AABB.Top - actor.AABB.Bottom, actor.Squish);
                // Push right
                else
                    actor.MoveYExact(AABB.Bottom - actor.AABB.Top, actor.Squish);
            }
            // Carry
            else if (riders.Contains(actor))
            {
                actor.MoveYExact(move);
            }
        }

        IsCollidable = true;
    }

    public IEnumerable<ActorComponent> GetRidingActors()
    {
        foreach (ActorComponent actor in LevelManager.Instance.Actors)
        {
            if (AABB.IntersectsRel(actor.AABB, Vector2i.Up))
            {
                yield return actor;
            }
        }
    }
}