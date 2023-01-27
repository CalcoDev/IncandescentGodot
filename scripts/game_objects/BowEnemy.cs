using Godot;
using GodotUtilities;
using Incandescent.Components;
using Incandescent.Components.Logic;
using Incandescent.GameObjects.Base;
using Incandescent.Managers;

namespace Incandescent.GameObjects.Enemies;

public partial class BowEnemy : Actor
{
    #region Constants

    [Node("VelocityComponent")]
    private VelocityComponent _vel;
    [Node("StateMachineComponent")]
    private StateMachineComponent _stateMachine;
    [Node("PathfindingComponent")]
    private PathfindingComponent _pathfinding;

    [Node("DashTimer")]
    private CustomTimerComponent _dashTimer;
    [Node("DashCooldownTimer")]
    private CustomTimerComponent _dashCooldownTimer;

    private const int StNormal = 0;
    private const int StAttack = 1;
    private const int StDash = 2;

    private const float Acceleration = 1600f;

    private const float FollowRange = 200f;
    private const float FollowSpeed = 100f;

    private const float AttackRange = 125f;

    private const float DashRange = 50f;
    private const float DashCooldown = 0.5f;
    private const float DashDuration = 0.2f;
    private const float DashSpeed = 400f;
    private const float DashTravelMax = 50f;

    #endregion

    #region Variables

    // TODO(calco): Should really make this a global thing.
    private float _delta;

    // Dash
    private Vector2 _dashDir;
    private Vector2 _dashStartPos;

    #endregion

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
        // _stateMachine.SetCallbacks(StAttack, AttackUpdate, null, null, null);
        _stateMachine.SetCallbacks(StDash, DashUpdate, DashEnter, DashExit, null);

        _pathfinding.OnVelocityChanged += vel => _vel.Set(vel);
    }

    public override void _PhysicsProcess(double delta)
    {
        _delta = (float)delta;

        int newSt = _stateMachine.Update();
        _stateMachine.SetState(newSt);
    }

    #region States

    private int NormalUpdate()
    {
        // Check if the player is within the dash range, then attack range and then follow range.
        PhysicsPlayer player = GameManager.Player;

        // Timers
        _dashCooldownTimer.Update(_delta);

        float sqrDist = player.GlobalPosition.DistanceSquaredTo(GlobalPosition);
        // TODO(calco): Move this to the game class.
        var space = GetWorld2d().DirectSpaceState;
        var query = PhysicsRayQueryParameters2D.Create(
            GlobalPosition, player.GlobalPosition,
            1 << 0, null
        );
        var res = space.IntersectRay(query);
        bool playerInSight = res.Count == 0;

        if (sqrDist < DashRange * DashRange && _dashCooldownTimer.HasFinished() && playerInSight)
        {
            return StDash;
        }
        // else if (sqrDist < AttackRange * AttackRange)
        // {
        //     return StAttack;
        // }
        else if (sqrDist < FollowRange * FollowRange)
        {
            var targetPos = player?.GlobalPosition ?? GlobalPosition;
            _pathfinding.SetTargetInterval(targetPos);

            _pathfinding.Agent.TargetLocation = targetPos;
            var next = _pathfinding.Agent.GetNextLocation();

            var desiredVelocity = (next - GlobalPosition).Normalized() * FollowSpeed;
            _vel.Set(desiredVelocity);
            _pathfinding.Agent.SetVelocity(_vel.GetVelocity());

            // TODO(calco): Acceleration.
            MoveX(_vel.X * _delta);
            MoveY(_vel.Y * _delta);
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

    private void DashEnter()
    {
        _dashDir = (GlobalPosition - GameManager.Player.GlobalPosition).Normalized();
        _dashStartPos = GlobalPosition;

        _dashTimer.SetTime(DashDuration);
        _dashCooldownTimer.SetTime(DashCooldown);
    }

    private int DashUpdate()
    {
        _dashTimer.Update(_delta);

        if (_dashTimer.HasFinished())
            return StNormal;

        bool collidedWithAnything = false;
        MoveX(_dashDir.x * DashSpeed * _delta, (_) => collidedWithAnything = true);
        MoveY(_dashDir.y * DashSpeed * _delta, (_) => collidedWithAnything = true);

        float dashTravel = GlobalPosition.DistanceSquaredTo(_dashStartPos);

        if (collidedWithAnything || (dashTravel > DashTravelMax * DashTravelMax))
            return StNormal;

        return StDash;
    }

    private void DashExit()
    {
        _dashCooldownTimer.SetTime(DashCooldown);
    }

    #endregion
}
