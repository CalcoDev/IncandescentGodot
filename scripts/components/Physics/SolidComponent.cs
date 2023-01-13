using System.Collections.Generic;
using System.Linq;
using Eyes.Managers;
using Godot;

namespace Eyes.Components.Physics;

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
        int move = Mathf.FloorToInt(Remainder.x);

        if (move == 0)
            return;

        // List<ActorComponent> riders = PhysicsManager.Instance.GetRidingActors(this);
        IsCollidable = false;

        _remainder.x -= move;
        GlobalPosition += new Vector2(move, 0);

        List<ActorComponent> riders = GetRidingActors().ToList();
        foreach (ActorComponent actor in LevelManager.Instance.Actors)
        {
            if (AABB.IntersectsRel(actor.AABB, Vector2i.Zero))
            {
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