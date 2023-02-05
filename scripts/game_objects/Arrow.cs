using Godot;
using GodotUtilities;
using Incandescent.Components;
using Incandescent.GameObjects.Base;
using Incandescent.Managers;

namespace Incandescent.GameObjects;

public partial class Arrow : Actor
{
    [Node("VelocityComponent")]
    private VelocityComponent _vel;

    public override void _Notification(long what)
    {
        if (what == NotificationEnterTree)
            this.WireNodes();
    }

    public void SetVelocity(Vector2 v)
    {
        _vel.SetVelocity(v);
    }

    public override void _Ready()
    {
        _vel.SetVelocity(Vector2.Zero);
    }

    public override void _PhysicsProcess(double delta)
    {
        MoveX(_vel.GetVelocity().x * GameManager.PhysicsDelta);
        MoveY(_vel.GetVelocity().y * GameManager.PhysicsDelta);
    }
}