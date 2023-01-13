using System;
using System.Collections.Generic;
using Eyes.Components.Physics;
using Eyes.Managers;
using Godot;

namespace Eyes.Components.Physics;

public partial class ActorComponent : PhysicsBodyComponent
{
    [Signal]
    public delegate void OnSquishEventHandler(AABBComponent other);

    public override void _Ready()
    {
        Vector2 t = GlobalPosition;
        TopLevel = true;
        GlobalPosition = t;

        LevelManager.Instance.AddActor(this);
    }

    public override void _ExitTree()
    {
        LevelManager.Instance.RemoveActor(this);
    }

    public void Squish(AABBComponent other)
    {
        EmitSignal(SignalName.OnSquish, other);
    }

    public void MoveX(float amount, Action<AABBComponent> onCollide = null)
    {
        _remainder.x += amount;
        int move = Mathf.FloorToInt(_remainder.x);

        if (move == 0)
            return;

        _remainder.x -= move;
        MoveXExact(move, onCollide);
    }

    public void MoveY(float amount, Action<AABBComponent> onCollide = null)
    {
        _remainder.y += amount;
        int move = Mathf.FloorToInt(_remainder.y);

        if (move == 0)
            return;

        _remainder.y -= move;
        MoveYExact(move, onCollide);
    }

    public void MoveXExact(int amount, Action<AABBComponent> onCollide = null)
    {
        int step = Mathf.Sign(amount);
        while (amount != 0)
        {
            if (CollideAt(LevelManager.Instance.Solids, new Vector2i(step, 0)))
            {
                onCollide?.Invoke(AABB);
                break;
            }
            else
            {
                amount -= step;
                Position += new Vector2(step, 0f);
            }
        }
    }

    public void MoveYExact(int amount, Action<AABBComponent> onCollide = null)
    {
        int step = Mathf.Sign(amount);
        while (amount != 0)
        {
            if (CollideAt(LevelManager.Instance.Solids, new Vector2i(0, step)))
            {
                onCollide?.Invoke(AABB);
                break;
            }
            else
            {
                amount -= step;
                Position += new Vector2(0f, step);
            }
        }
    }
}