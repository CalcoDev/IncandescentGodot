using Godot;
using GodotUtilities;
using Incandescent.Components;
using Incandescent.Components.Logic;
using Incandescent.GameObjects.Base;
using Incandescent.Managers;

namespace Incandescent.GameObjects.Enemies;

public partial class BowEnemy : Actor
{
    // References
    [Node("VelocityComponent")]
    private VelocityComponent _vel;

    [Node("StateMachineComponent")]
    private StateMachineComponent _stateMachine;

    [Node("PathfindingComponent")]
    private PathfindingComponent _pathfinding;

    // State
    // Enemy should wander about, until player is within a certain distance. Then, it should attack, by firing an arrow.
    // If the player gets too close, it will try dashing away.
    private const int StNormal = 0;
    private const int StAttack = 1;
    private const int StDash = 2;

    private const float Acceleration = 1600f;

    private const float FollowRange = 200f;
    private const float FollowSpeed = 100f;

    private const float AttackRange = 125f;
    private const float DashRange = 50f;

    // TODO(calco): Should really make this a global thing.
    private float _delta;

    public override void _Notification(long what)
    {
        if (what == NotificationEnterTree)
            this.WireNodes();
    }

    public override void _Ready()
    {
        _stateMachine.UpdateSelf = false;
        _stateMachine.Init(StDash + 1, StNormal);
        _stateMachine.SetCallbacks(StNormal, NormalUpdate, null, null, null);
        _stateMachine.SetCallbacks(StAttack, AttackUpdate, null, null, null);
        _stateMachine.SetCallbacks(StDash, DashUpdate, null, null, null);

        // _pathfinding.OnVelocityChanged += vel =>
        // {
        //     _vel.Set(vel);

        //     MoveX(_vel.X * _delta);
        //     MoveY(_vel.Y * _delta);
        // };
    }

    public override void _PhysicsProcess(double delta)
    {
        _delta = (float)delta;

        int newSt = _stateMachine.Update();
        _stateMachine.SetState(newSt);
    }

    private int NormalUpdate()
    {
        // Check if the player is within the dash range, then attack range and then follow range.
        PhysicsPlayer player = GameManager.Player;

        float sqrDist = player.GlobalPosition.DistanceSquaredTo(GlobalPosition);
        if (true) //sqrDist < FollowRange * FollowRange
        {
            var desiredVelocity = Vector2.Zero;
            var targetPos = player?.GlobalPosition ?? Vector2.Zero;
            // _pathfinding.SetTargetInterval(targetPos);

            _pathfinding.Agent.TargetLocation = targetPos;
            var next = _pathfinding.Agent.GetNextLocation();

            // desiredVelocity = (next - GlobalPosition).Normalized() * FollowSpeed;

            // _vel.Set(desiredVelocity);
            // _pathfinding.Agent.SetVelocity(_vel.GetVelocity());

            GD.Print($"{targetPos}");

            // MoveX(_vel.X * _delta);
            // MoveY(_vel.Y * _delta);

            GlobalPosition = next;
        }
        else
        {
            // Wander
        }

        return StNormal;
    }

    private int AttackUpdate()
    {
        GD.Print("Attack");
        return StNormal;
    }

    private int DashUpdate()
    {
        return StNormal;
    }
}
