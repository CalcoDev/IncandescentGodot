using System.Collections.Generic;
using Godot;

namespace Eyes.Components.Physics;

public partial class PhysicsBodyComponent : Node2D
{
    [Export] public AABBComponent AABB { get; set; }
    [Export] public bool IsCollidable { get; set; } = true;

    public Vector2 Remainder => _remainder;
    protected Vector2 _remainder;

    /// <summary>
    /// Virtually moves the actor by [<paramref name="positionOffset"/>] and checks if it collides with any of the [<paramref name="bodies"/>
    /// </summary>
    /// <param name="bodies">The aabb's to check for collision against.</param>
    /// <param name="positionOffset">The offset from the current position/</param>
    /// <returns>If the actor will collide with anything.</returns>
    public bool CollideAt(IEnumerable<PhysicsBodyComponent> bodies, Vector2i positionOffset)
    {
        foreach (PhysicsBodyComponent body in bodies)
        {
            if (body.IsCollidable && body.AABB != AABB && AABB.IntersectsRel(body.AABB, positionOffset))
                return true;
        }

        return false;
    }

    // TODO(calco): Maybe define movement methods here?
}