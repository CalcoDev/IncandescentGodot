using System.Collections.Generic;
using Godot;
using Incandescent.Components;
using Incandescent.GameObjects;

namespace Incandescent.Managers;

public partial class PhysicsManager : Node
{
    public static PhysicsManager Instance { get; private set; } = null;

    public List<SolidComponent> Solids { get; } = new();
    public List<ActorComponent> Actors { get; } = new();

    public override void _EnterTree()
    {
        if (Instance != null)
        {
            GD.PrintErr("Multiple instances of SolidManager detected!");
            // TODO: delete
            return;
        }

        Instance = this;
    }

    public void AddActor(ActorComponent actor)
    {
        Actors.Add(actor);
    }

    public void RemoveActor(ActorComponent actor)
    {
        Actors.Remove(actor);
    }

    public void AddSolid(SolidComponent solid)
    {
        if (solid is MovingPlatform)
            GD.Print("Adding moving platform");

        Solids.Add(solid);
    }

    public void RemoveSolid(SolidComponent solid)
    {
        Solids.Remove(solid);
    }

    public List<ActorComponent> GetRidingActors(SolidComponent solid)
    {
        var actors = new List<ActorComponent>();
        foreach (var actor in Actors)
        {
            if (actor.IsRiding(solid, Vector2i.Down))
                actors.Add(actor);
        }

        return actors;
    }

    public bool CheckWithSolidsCollisionAt(AxisAlignedBoundingBoxComponent boundingBox, Vector2i offset)
    {
        foreach (var solid in Solids)
        {
            if (!solid.IsCollidable)
                continue;

            if (boundingBox.IntersectsRel(solid.BoundingBox, offset))
                return true;
        }

        return false;
    }

    public List<AxisAlignedBoundingBoxComponent> GetCollisionsWithSolidsAt(AxisAlignedBoundingBoxComponent boundingBox, Vector2i offset)
    {
        List<AxisAlignedBoundingBoxComponent> collisions = new();

        foreach (var solid in Solids)
        {
            if (!solid.IsCollidable)
                continue;

            if (boundingBox.IntersectsRel(solid.BoundingBox, offset))
                collisions.Add(solid.BoundingBox);
        }

        return collisions;
    }
}