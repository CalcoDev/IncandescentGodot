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

    // Dash
    private Vector2 _dashDir;
    private Vector2 _dashStartPos;

    // private FastNoiseLite _dirNoise = new FastNoiseLite();
    // private Vector2 _pathfindVel;

    private const int DirCount = 16;
    private readonly Vector2[] _dirs = new Vector2[DirCount];
    private readonly float[] _weights = new float[DirCount];

    #endregion

    public override void _Notification(long what)
    {
        if (what == NotificationEnterTree)
            this.WireNodes();
    }

    public override void _EnterTree()
    {
        // GD.Randomize();
        // _dirNoise.Seed = (int)GD.Randi();
        // _dirNoise.FractalOctaves = 4;
        // _dirNoise.Frequency = 1.0f / 20.0f;

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

    private int _maxWeightIdx = 0;
    public override void _Draw()
    {
        const float Unit = 20f;

        for (int i = 0; i < DirCount; i++)
        {
            float length = _weights[i] * Unit;
            Color col = _weights[i] > 0f ? Colors.DarkOliveGreen : Colors.Red;
            if (i == _maxWeightIdx)
                col = Colors.Green;

            DrawLine(Vector2.Zero, _dirs[i] * length, col, 0.33f);
        }

        DrawArc(Vector2.Zero, Unit, 0f, Mathf.Pi * 2f, 32, Colors.DarkRed, 0.5f);
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

        if (false) { }
        // if (sqrDist < DashRange * DashRange && _dashCooldownTimer.HasFinished() && playerInSight)
        // {
        //     return StDash;
        // }
        // else if (sqrDist < AttackRange * AttackRange)
        // {
        //     return StAttack;
        // }
        else if (sqrDist < FollowRange * FollowRange)
        {
            _pathfinding.SetTargetInterval(CalculateTargetPos());

            Vector2 nextPos = _pathfinding.Agent.GetNextLocation();
            Vector2 nextPosDir = (nextPos - GlobalPosition).Normalized();
            Vector2 playerDir = (player.GlobalPosition - GlobalPosition).Normalized();

            // Hacky, but it works.
            Vector2 targetVel = Vector2.Zero;

            if (playerInSight)
            {
                Vector2 desiredDir = playerDir;

                float maxDot = -1f;
                for (int i = 0; i < DirCount; i++)
                {
                    float dot = (_dirs[i].Dot(desiredDir) + 1f) * 0.5f;
                    if (dot > maxDot)
                    {
                        maxDot = dot;
                        _maxWeightIdx = i;
                    }

                    _weights[i] = dot;
                }
                QueueRedraw();

                targetVel = _dirs[_maxWeightIdx] * FollowSpeed;
            }
            else
            {
                targetVel = nextPosDir * FollowSpeed;
            }

            _vel.SetVelocity(targetVel);
            _pathfinding.Agent.SetVelocity(_vel.GetVelocity());

            MoveX(_vel.X * GameManager.PhysicsDelta);
            MoveY(_vel.Y * GameManager.PhysicsDelta);
        }
        else
        {
            // Wander
        }

        return StNormal;
    }

    private Vector2 CalculateTargetPos()
    {

        var targetPos = GameManager.Player?.GlobalPosition ?? GlobalPosition;

        // TODO(calco): Come up with a better check
        // if (!GameManager.Raycast(GlobalPosition, targetPos, 1 << 0))
        //     targetPos += (GlobalPosition - targetPos).Normalized() * (DashRange + 5f); // 10f to give room for strafe

        return targetPos;
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
