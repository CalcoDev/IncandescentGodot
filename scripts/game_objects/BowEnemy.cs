using Godot;
using GodotUtilities;
using Incandescent.Components;
using Incandescent.Components.Logic;
using Incandescent.Components.Physics;
using Incandescent.Managers;

namespace Incandescent.GameObjects.Enemies;

public partial class BowEnemy : Node2D
{
    // References
    [Node("ActorComponent")]
    private ActorComponent _actor;

    [Node("StateMachineComponent")]
    private StateMachineComponent _stateMachine;

    [Node("VelocityComponent")]
    private VelocityComponent _vel;

    // State
    // Enemy should wander about, until player is within a certain distance. Then, it should attack, by firing an arrow.
    // If the player gets too close, it will try dashing away.
    private const int StNormal = 0;
    private const int StAttack = 1;
    private const int StDash = 2;

    private const float Acceleration = 1600f;

    private const float FollowRange = 200f;
    private const float AttackRange = 125f;
    private const float DashRange = 50f;

    public override void _Notification(long what)
    {
        if (what == NotificationEnterTree)
            this.WireNodes();
    }

    public override void _Ready()
    {
        _stateMachine.Init(StDash, StNormal);
        _stateMachine.SetCallbacks(StNormal, NormalUpdate, null, null, null);
        _stateMachine.SetCallbacks(StAttack, AttackUpdate, null, null, null);
        _stateMachine.SetCallbacks(StDash, DashUpdate, null, null, null);
    }

    public override void _Process(double delta)
    {
        int newSt = _stateMachine.Update();
        _stateMachine.SetState(newSt);

        GlobalPosition = _actor.GlobalPosition;
    }

    private int NormalUpdate()
    {
        // Get the player.
        Player player = GameManager.Player;

        Vector2 desiredVelocity = Vector2.Zero;

        Vector2 startPos = player?.GlobalPosition ?? GlobalPosition;
        Vector2 endPos = GlobalPosition;

        float distSqr = startPos.DistanceSquaredTo(endPos);
        if (distSqr < AttackRange * AttackRange)
            return StAttack;

        if (distSqr < DashRange * DashRange)
            return StDash;

        if (distSqr < FollowRange * FollowRange)
            desiredVelocity = (startPos - endPos).Normalized();

        _vel.Apprach(desiredVelocity, Acceleration);

        _actor.MoveX(_vel.X * GameManager.Delta);
        _actor.MoveY(_vel.Y * GameManager.Delta);

        return StNormal;
    }

    private int AttackUpdate()
    {
        return StNormal;
    }

    private int DashUpdate()
    {
        return StNormal;
    }
}
