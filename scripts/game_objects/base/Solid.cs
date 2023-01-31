using Godot;
using Incandescent.Managers;

namespace Incandescent.GameObjects.Base;

public abstract partial class Solid : AnimatableBody2D
{
    public bool IsCollidable { get; set; } = true;

    public void MoveX(float amount)
    {
        CollisionLayer = (uint)GameManager.CollisionLayers.Disabled;

        var shape = ShapeOwnerGetShape(0, 0);
        var state = GetWorld2d().DirectSpaceState;
        var query = new PhysicsShapeQueryParameters2D
        {
            Exclude = new Godot.Collections.Array<RID> { this.GetRid() },
            Shape = shape,
            Transform = GlobalTransform,
            Motion = Vector2.Right * amount,
        };

        var colls = state.IntersectShape(query);
        for (int i = 0; i < colls.Count; i++)
        {
            var actor = (Actor)colls[i]["collider"];
            actor.MoveX(amount, actor.Squish);
        }

        // Riders
        query = new PhysicsShapeQueryParameters2D
        {
            Exclude = new Godot.Collections.Array<RID> { this.GetRid() },
            Shape = shape,
            Transform = GlobalTransform,
            Motion = Vector2.Up * 1f,
        };
        colls = state.IntersectShape(query);
        for (int i = 0; i < colls.Count; i++)
        {
            var actor = (Actor)colls[i]["collider"];
            actor.MoveX(amount);
        }

        MoveAndCollide(Vector2.Right * amount, safeMargin: 0.001f);
        CollisionLayer = (uint)GameManager.CollisionLayers.LevelGeometry;
    }

    public void MoveY(float amount)
    {
        CollisionLayer = (uint)GameManager.CollisionLayers.Disabled;

        var old = GlobalTransform;
        MoveAndCollide(Vector2.Down * amount, safeMargin: 0.001f);

        var shape = ShapeOwnerGetShape(0, 0);
        var state = GetWorld2d().DirectSpaceState;
        var query = new PhysicsShapeQueryParameters2D
        {
            Exclude = new Godot.Collections.Array<RID> { this.GetRid() },
            Shape = shape,
            Transform = old,
            Motion = Vector2.Down * amount,
        };

        var colls = state.IntersectShape(query);
        for (int i = 0; i < colls.Count; i++)
        {
            var actor = (Actor)colls[i]["collider"];
            actor.MoveY(amount, actor.Squish);
        }

        // Riders
        query = new PhysicsShapeQueryParameters2D
        {
            Exclude = new Godot.Collections.Array<RID> { this.GetRid() },
            Shape = shape,
            Transform = old,
            Motion = Vector2.Up * 1f,
        };
        colls = state.IntersectShape(query);
        for (int i = 0; i < colls.Count; i++)
        {
            var actor = (Actor)colls[i]["collider"];

            // GD.Print(Time.GetTicksMsec() + ": Moving rider with: " + amount);
            // actor.MoveY(amount + amount < 0f ? 0.5f : 0f);
            // if (amount < 0.01f)
            //     amount -= 0.01f;

            actor.MoveY(amount);
        }

        CollisionLayer = (uint)GameManager.CollisionLayers.LevelGeometry;
    }
}