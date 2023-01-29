using System;
using System.Collections.Generic;
using Godot;
using GodotUtilities;
using Incandescent.Components;
using Incandescent.Components.Logic;
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

    private const int StNormal = 0;
    private const int StAttack = 1;
    private const int StDash = 2;

    private const float Acceleration = 1600f;

    private const float FollowRange = 200f;
    private const float FollowSpeed = 75f;

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

    // TODO(calco); Make this it's own component...
    // Steering
    private const int DirCount = 24;
    private static Func<float, float> AttractionShapingFunction = DotProductShapingFunctions.Avoid;
    private static Func<float, float> RepulsionShapingFunction = DotProductShapingFunctions.Normalized;

    private readonly Vector2[] _dirs = new Vector2[DirCount];

    private readonly float[] _interest = new float[DirCount];
    private readonly float[] _danger = new float[DirCount];

    #endregion

    public override void _Notification(long what)
    {
        if (what == NotificationEnterTree)
            this.WireNodes();
    }

    public override void _EnterTree()
    {
        for (int i = 0; i < DirCount; i++)
        {
            float angle = Mathf.Pi * 2 * i / DirCount;
            _dirs[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }
    }

    public override void _Ready()
    {
        _stateMachine.UpdateSelf = false;
        _stateMachine.Init(StDash + 1, StNormal);
        _stateMachine.SetCallbacks(StNormal, NormalUpdate, null, null, null);
        // _stateMachine.SetCallbacks(StAttack, AttackUpdate, null, null, null);
        _stateMachine.SetCallbacks(StDash, DashUpdate, DashEnter, DashExit, null);

        _pathfinding.OnVelocityChanged += vel => _vel.SetVelocity(vel);
    }

    public override void _PhysicsProcess(double delta)
    {
        int newSt = _stateMachine.Update();
        _stateMachine.SetState(newSt);
    }

    private int _steerDirIdx = 0;
    public override void _Draw()
    {
        if (!GameManager.Debug)
            return;

        const float Unit = 20f;

        for (int i = 0; i < DirCount; i++)
        {
            float weight = _interest[i] + _danger[i];
            float length = Mathf.Abs(weight) * Unit;
            Color col = weight > 0f ? Colors.DarkOliveGreen : Colors.Red;
            if (i == _steerDirIdx)
                col = Colors.Green;

            DrawLine(Vector2.Zero, _dirs[i] * length, col, 1f);
        }

        DrawArc(Vector2.Zero, Unit, 0f, Mathf.Pi * 2f, 32, Colors.DarkRed, 1.5f);
    }

    #region States

    private int NormalUpdate()
    {
        // Check if the player is within the dash range, then attack range and then follow range.
        PhysicsPlayer player = GameManager.Player;

        // Timers
        _dashCooldownTimer.Update(GameManager.PhysicsDelta);

        float sqrDist = player.GlobalPosition.DistanceSquaredTo(GlobalPosition);
        bool playerInSight = !GameManager.Raycast(GlobalPosition, player.GlobalPosition, 1 << 0);

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
            _pathfinding.SetTargetInterval(player.GlobalPosition);

            Vector2 nextPos = _pathfinding.Agent.GetNextLocation();
            Vector2 nextPosDir = (nextPos - GlobalPosition).Normalized();
            Vector2 playerDir = (player.GlobalPosition - GlobalPosition).Normalized();

            // Hacky, but it works.
            Vector2 targetVel;
            if (playerInSight)
            {
                // TODO(calco): This is an ugly hack instead of manipulating weights.
                _steerTimer.Update(GameManager.PhysicsDelta);
                if (_steerTimer.HasFinished())
                {
                    ComputeSteeringInterest();
                    ComputeSteeringDanger();
                    ChooseSteeringDirection();
                    _steerTimer.SetTime(0.25f);
                }

                targetVel = _dirs[_steerDirIdx] * FollowSpeed;

                QueueRedraw();
            }
            else
            {
                GD.Print("Player not in sight");
                targetVel = nextPosDir * FollowSpeed;
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


    // TODO(calco): Make these function not dependant on state, but parameters.
    #region Steering

    private void ComputeSteeringInterest()
    {
        Vector2 towards = (GameManager.Player.GlobalPosition - GlobalPosition).Normalized();

        float sqrDist = GameManager.Player.GlobalPosition.DistanceSquaredTo(GlobalPosition);
        for (int i = 0; i < DirCount; i++)
            _interest[i] = 0f;

        // TODO(calco): Especially abstract this away.
        towards = towards.Normalized();
        const float SqrDashDist = (DashRange + 15f) * (DashRange + 15f);
        if (sqrDist < SqrDashDist)
        {
            AttractionShapingFunction = DotProductShapingFunctions.Sideways;
            // towards = -towards;
        }
        else
        {
            AttractionShapingFunction = DotProductShapingFunctions.Normalized;
        }

        for (int i = 0; i < DirCount; i++)
        {
            float dot = _dirs[i].Dot(towards);
            float weight = AttractionShapingFunction(dot);

            _interest[i] = Mathf.Max(weight, _interest[i]);
        }

        float max = 0f;
        for (int i = 0; i < DirCount; i++)
            max = Mathf.Max(max, _interest[i]);
        if (!Calc.FloatEquals(max, 0f))
        {
            for (int i = 0; i < DirCount; i++)
                _interest[i] /= max;
        }

        // Check if that direction is blocked.
        for (int i = 0; i < DirCount; i++)
        {
            // TODO(calco): Do something about this magic number.
            Vector2 target = GlobalPosition + (_dirs[i] * 10f) + (_dirs[i] * FollowSpeed * GameManager.PhysicsDelta);
            if (GameManager.Raycast(GlobalPosition, target, 1 << 0))
                _interest[i] = 0f;
        }
    }

    private void ComputeSteeringDanger()
    {
        for (int i = 0; i < DirCount; i++)
            _danger[i] = 0f;

        float sqrDist = GameManager.Player.GlobalPosition.DistanceSquaredTo(GlobalPosition);
        const float SqrDashDist = (DashRange + 5f) * (DashRange + 5f);
        if (sqrDist < SqrDashDist)
        {
            Vector2 awayFrom = (GameManager.Player.GlobalPosition - GlobalPosition).Normalized();

            for (int i = 0; i < DirCount; i++)
            {
                float dot = _dirs[i].Dot(awayFrom);
                float weight = RepulsionShapingFunction(dot);

                _danger[i] = Mathf.Max(weight, _danger[i]);
            }

            float max = 0f;
            for (int i = 0; i < DirCount; i++)
                max = Mathf.Max(max, _danger[i]);
            if (!Calc.FloatEquals(max, 0f))
            {
                for (int i = 0; i < DirCount; i++)
                {
                    _danger[i] /= max;
                    _danger[i] = -_danger[i];
                }
            }
        }
    }

    private void ChooseSteeringDirection()
    {
        // List<int> maxIndices = new List<int>();

        // float max = float.MinValue;
        // for (int i = 0; i < DirCount; i++)
        // {
        //     float weight = _interest[i] + _danger[i];
        //     if (weight > max)
        //         max = weight;
        // }

        // for (int i = 0; i < DirCount; i++)
        // {
        //     float weight = _interest[i] + _danger[i];
        //     if (Calc.FloatEquals(weight, max))
        //         maxIndices.Add(i);
        // }

        // _steerDirIdx = maxIndices[GD.RandRange(0, maxIndices.Count - 1)];

        // Create a list of all the directions indices in sorted order.
        List<int> sortedIndices = new List<int>();
        for (int i = 0; i < DirCount; i++)
            sortedIndices.Add(i);
        sortedIndices.Sort(SortSteeringDirections);

        // Pick a random direction from the first 25% of the list, or less if one is o.
        List<int> checks = new List<int>();
        int count = Mathf.Max(sortedIndices.Count / 4, 1);
        for (int i = 0; i < count; i++)
        {
            if (_interest[sortedIndices[i]] < 0f)
                break;

            checks.Add(sortedIndices[i]);
        }

        _steerDirIdx = checks[GD.RandRange(0, checks.Count - 1)];
    }

    private int SortSteeringDirections(int a, int b)
    {
        float weightA = _interest[a] + _danger[a];
        float weightB = _interest[b] + _danger[b];

        if (weightA == weightB)
        {
            float distA = GlobalPosition.DistanceSquaredTo(GameManager.Player.GlobalPosition);
            float distB = GlobalPosition.DistanceSquaredTo(GameManager.Player.GlobalPosition);

            // Favour the direction that is closer to the player.
            if (distA < distB)
                return -1;
            else if (distA > distB)
                return 1;
            else
                return 0;
        }

        if (weightA > weightB)
            return -1;
        else
            return 1;
    }

    #endregion
}
