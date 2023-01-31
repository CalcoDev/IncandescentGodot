using System;
using System.Collections.Generic;
using Godot;
using GodotUtilities;
using Incandescent.Components;
using Incandescent.Components.Logic;
using Incandescent.Components.Steering;
using Incandescent.GameObjects.Base;
using Incandescent.Managers;
using Incandescent.Utils;

namespace Incandescent.GameObjects.Enemies;

public partial class BowEnemy : Actor
{
    #region Constants

    [Export] private float StrafeCoefficient;

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

    [Node("SteerTimer")]
    private CustomTimerComponent _steerTimer;
    [Node("SteeringBehaviourComponent")]
    private SteeringBehaviourComponent _steeringBehaviour;

    private SteeringBehaviourDefinition _seekSteerDef;
    private SteeringBehaviourDefinition _strafeSteerDef;

    private const int StNormal = 0;
    private const int StAttack = 1;
    private const int StDash = 2;

    private const float Acceleration = 1600f;

    private const float FollowRange = 200f;
    private const float FollowSpeed = 100f;
    private const float StrafeSpeed = 65f;

    private const float AttackRange = 125f;

    private const float DashRange = 50f;
    private const float DashCooldown = 0.5f;
    private const float DashDuration = 0.2f;
    private const float DashSpeed = 400f;
    private const float DashTravelMax = 50f;

    #endregion

    #region Variables

    // Dash
    private Vector2 _dashDir;
    private Vector2 _dashStartPos;

    private float _lastSpeed;

    #endregion

    #region Lifecycle

    public override void _Notification(long what)
    {
        if (what == NotificationEnterTree)
            this.WireNodes();
    }

    public override void _Ready()
    {
        // State
        _stateMachine.UpdateSelf = false;
        _stateMachine.Init(StDash + 1, StNormal);
        _stateMachine.SetCallbacks(StNormal, NormalUpdate, null, null, null);
        // _stateMachine.SetCallbacks(StAttack, AttackUpdate, null, null, null);
        _stateMachine.SetCallbacks(StDash, DashUpdate, DashEnter, DashExit, null);

        // Steering & Pathfinding
        _seekSteerDef = new SteeringBehaviourDefinition(9,
            SteeringShapingFunctions.Normalized, SteeringShapingFunctions.Normalized,
            SteeringSortingFunctions.ClosestHighestWeight);

        _strafeSteerDef = new SteeringBehaviourDefinition(24,
            SteeringShapingFunctions.Cosine, SteeringShapingFunctions.Avoid,
            SteeringSortingFunctions.ClosestHighestWeight);

        _steeringBehaviour.SetDefinition(_seekSteerDef);

        _pathfinding.OnVelocityChanged += vel => _vel.SetVelocity(vel);
    }

    public override void _PhysicsProcess(double delta)
    {
        int newSt = _stateMachine.Update();
        _stateMachine.SetState(newSt);
    }

    #endregion

    #region States

    private int NormalUpdate()
    {
        _dashCooldownTimer.Update(GameManager.PhysicsDelta);


        PhysicsPlayer player = GameManager.Player;
        float sqrDist = player.GlobalPosition.DistanceSquaredTo(GlobalPosition);
        bool playerInSight = !GameManager.Raycast(GlobalPosition, player.GlobalPosition, 1 << 0);
        if (false) { }
        else if (sqrDist < DashRange * DashRange && _dashCooldownTimer.HasFinished() && playerInSight)
        {
            return StDash;
        }
        // else if (sqrDist < AttackRange * AttackRange)
        // {
        //     return StAttack;
        // }
        else if (sqrDist < FollowRange * FollowRange)
        {
            _pathfinding.SetTargetInterval(player.GlobalPosition);

            Vector2 targetVel;
            if (playerInSight)
            {

                // TODO(calco): This is an ugly hack instead of manipulating weights.
                _steerTimer.Update(GameManager.PhysicsDelta);

                if (_steerTimer.HasFinished())
                {
                    Vector2 vel = _vel.GetVelocity() * GameManager.PhysicsDelta;
                    Vector2 attraction;
                    Vector2 repulsion;

                    if (sqrDist < (DashRange + 30f) * (DashRange + 30f))
                    {
                        _steeringBehaviour.SetDefinition(_strafeSteerDef);
                        attraction = player.GlobalPosition - GlobalPosition;
                        repulsion = attraction;
                        _lastSpeed = StrafeSpeed;
                        _steerTimer.SetTime(0.15f);
                    }
                    else
                    {
                        _steeringBehaviour.SetDefinition(_seekSteerDef);
                        attraction = player.GlobalPosition - GlobalPosition;
                        repulsion = -attraction;
                        _lastSpeed = FollowSpeed;
                        _steerTimer.SetTime(0.25f);
                    }

                    _steeringBehaviour.GetSteeringDirection(vel, attraction, repulsion);
                }

                targetVel = _steeringBehaviour.LastSteeringDirection * _lastSpeed;
            }
            else
            {
                targetVel = (_pathfinding.Agent.GetNextLocation() - GlobalPosition).Normalized() * FollowSpeed;
            }

            _vel.SetVelocity(targetVel);
            _pathfinding.Agent.SetVelocity(_vel.GetVelocity());

            MoveX(_vel.X * GameManager.PhysicsDelta);
            MoveY(_vel.Y * GameManager.PhysicsDelta);
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
        _dashTimer.Update(GameManager.PhysicsDelta);

        if (_dashTimer.HasFinished())
            return StNormal;

        bool collidedWithAnything = false;
        MoveX(_dashDir.x * DashSpeed * GameManager.PhysicsDelta, (_) => collidedWithAnything = true);
        MoveY(_dashDir.y * DashSpeed * GameManager.PhysicsDelta, (_) => collidedWithAnything = true);

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
