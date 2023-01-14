using System.Collections.Generic;
using Incandescent.Components;
using Godot;
using Incandescent.Components.Physics;

namespace Incandescent.Managers;

public partial class LevelManager : Node
{
    public static LevelManager Instance { get; private set; }

    public List<ActorComponent> Actors { get; } = new List<ActorComponent>();
    public List<SolidComponent> Solids { get; } = new List<SolidComponent>();

    public override void _EnterTree()
    {
        Instance = this;
    }

    #region Collisions

    public static IEnumerable<PhysicsBodyComponent> GetCollidingBodies(AABBComponent aabb,
        IEnumerable<PhysicsBodyComponent> bodies, Vector2i positionOffset)
    {
        foreach (PhysicsBodyComponent body in bodies)
        {
            if (body.IsCollidable && body.AABB != aabb && aabb.IntersectsRel(body.AABB, positionOffset))
                yield return body;
        }
    }

    public static bool CollideAt(AABBComponent aabb, IEnumerable<PhysicsBodyComponent> bodies, Vector2i positionOffset)
    {
        foreach (PhysicsBodyComponent body in bodies)
        {
            if (body.IsCollidable && body.AABB != aabb && aabb.IntersectsRel(body.AABB, positionOffset))
                return true;
        }

        return false;
    }

    #endregion

    #region Level Object Helpers

    // These are separate methods as maybe we want to do something else in the future,
    // and refactoring would become a pain.
    public void AddSolid(SolidComponent solid)
    {
        Solids.Add(solid);
    }

    public void RemoveSolid(SolidComponent solid)
    {
        Solids.Remove(solid);
    }

    public void AddActor(ActorComponent actor)
    {
        Actors.Add(actor);
    }

    public void RemoveActor(ActorComponent actor)
    {
        Actors.Remove(actor);
    }

    #endregion
}