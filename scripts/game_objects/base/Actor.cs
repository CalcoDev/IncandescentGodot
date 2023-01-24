using System;
using Godot;

namespace Incandescent.GameObjects.Base;

// TODO(calco): Replace MoveAndCollide to custom position settings and stuff
public abstract partial class Actor : CharacterBody2D
{
    public virtual void Squish(KinematicCollision2D coll) { }

    public void MoveX(float amount, Action<KinematicCollision2D> onCollide = null)
    {
        var collX = MoveAndCollide(Vector2.Right * amount, safeMargin: 0.001f);
        if (collX != null)
            onCollide(collX);
    }

    public void MoveY(float amount, Action<KinematicCollision2D> onCollide = null)
    {
        var collY = MoveAndCollide(Vector2.Down * amount, safeMargin: 0.001f);
        if (collY != null)
            onCollide(collY);
    }
}