using System.Collections;
using System.Threading.Tasks;
using Godot;
using Incandescent.Components;
using Incandescent.Managers;
using Incandescent.Utils;

namespace Incandescent.GameObjects;

public partial class Player : Node2D
{
    [ExportGroup("Refs")]
    [Export] private VelocityComponent _vel;
    [Export] private ActorComponent _actor;
    [Export] private StateMachineComponent _stateMachine;
    [Export] private CollisionCheckerComponent _groundedChecker;

    [ExportGroup("Gravity")]
    [Export] private float Gravity = 140f * 5f;
    [Export] private float MaxFall = 25f * 6f;

    [ExportGroup("Jump")]
    [Export] private CustomTimerComponent _coyoteTimer;
    [Export] private CustomTimerComponent _jumpBufferTimer;
    [Export] private CustomTimerComponent _variableJumpTimer;
    [Export] private PackedScene _hitGroundParticles;

    [Export] private float JumpForce = 204f + 8f;
    [Export] private float JumpHBoost = 13f * 5f;
    [Export(PropertyHint.Range, "0, 0.2")] private float CoyoteTime = 0.1f;
    [Export(PropertyHint.Range, "0, 0.2")] private float JumpBufferTime = 0.1f;
    [Export(PropertyHint.Range, "0, 0.2")] private float VariableJumpTime = 0.2f;
    [Export(PropertyHint.Range, "0, 1")] private float VariableJumpMultiplier = 0.5f;
    [Export(PropertyHint.Range, "0, 5")] private float JumpApexControl = 3f;
    [Export(PropertyHint.Range, "0, 1")] private float JumpApexControlMultiplier = 0.5f;

    [ExportGroup("Dash")]
    [Export] private TrailRendererComponent _dashTrail;

    [Export] private CustomTimerComponent _dashCooldownTimer;
    [Export] private float DashCooldown = 0.2f;
    [Export] private float DashSpeed = 14f * 8.5f * 3f;
    [Export] private float DashTime = 0.15f;
    [Export(PropertyHint.Range, "0, 1")] private float DashFinishMultiplier = 0.75f;

    [ExportGroup("Run")]
    [Export] private float MaxRunSpeed = 14f * 8.5f;
    [Export] private float RunAccel = 200f * 8f;
    [Export] private float RunReduce = 62f * 8f;

    [ExportGroup("Forgiveness")]
    [Export(PropertyHint.Range, "0, 4, 1")] private int CornerCorrectionPixels = 2;

    // Input
    private float _inputX;
    private bool _inputJumpPressed;
    private bool _inputJumpReleased;
    private bool _inputJumpHeld;

    private bool _inputDashPressed;

    private Vector2 _lastNonZeroDir;

    // Jumping
    private bool _isJumping;

    // State
    private const int StNormal = 0;
    private const int StDash = 1;

    private float _delta;

    public bool IsColliding { get; private set; }

    // Dashing
    private Vector2 _dashDir;
    private bool _groundDash;
    private bool _queueDashStop;

    public override void _Ready()
    {
        _stateMachine.Init(3, -1);
        _stateMachine.SetCallbacks(StNormal, NormalUpdate, null, null, null);
        _stateMachine.SetCallbacks(StDash, DashUpdate, DashEnter, DashExit, DashCoroutine);
        _stateMachine.SetState(StNormal);

        _dashTrail.Emitting = false;

        _groundedChecker.OnCollide += () =>
        {
            _coyoteTimer.SetTime(CoyoteTime);
            _dashCooldownTimer.SetTime(0f);

            var inst = _hitGroundParticles.Instantiate<CPUParticles2D>();
            inst.GlobalPosition = GlobalPosition;
            inst.Position += new Vector2(6f, 16f);
            GetNode("/root").AddChild(inst);

            _isJumping = false;
        };

        _groundedChecker.OnSeparate += () => _coyoteTimer.SetTime(CoyoteTime);
    }

    public override void _Process(double delta)
    {
        _delta = (float)delta;

        // Input
        _inputX = Input.GetAxis("axis_horizontal_negative", "axis_horizontal_positive");
        _inputJumpPressed = Input.IsActionJustPressed("btn_jump");
        _inputJumpReleased = Input.IsActionJustReleased("btn_jump");
        _inputJumpHeld = Input.IsActionPressed("btn_jump");

        _inputDashPressed = Input.IsActionJustPressed("btn_dash");

        if (Mathf.Abs(_inputX) > 0f)
            _lastNonZeroDir = new(_inputX, 0f);

        // Collisions
        _groundedChecker.Update();

        int newState = _stateMachine.Update();
        _stateMachine.SetState(newState);

        IsColliding = false;

        // GD.Print($"Grounded checker pos: {_groundedChecker.GlobalPosition}");
        // GD.Print($"Velocity: [{_vel.X}, {_vel.Y}] | Actor Pos: {_actor.GlobalPosition}");
        // TODO(calco): Make it so that actors don't require you to do this.
        GlobalPosition = _actor.GlobalPosition;
    }

    #region States

    private int NormalUpdate()
    {
        // State
        if (_inputDashPressed && _dashCooldownTimer.HasFinished())
            return StDash;

        // Timers
        if (!_groundedChecker.IsColliding)
            _coyoteTimer.Update(_delta);

        _jumpBufferTimer.Update(_delta);
        if (_inputJumpPressed)
            _jumpBufferTimer.SetTime(JumpBufferTime);

        if (_isJumping)
            _variableJumpTimer.Update(_delta);

        // Gravity
        if (!_groundedChecker.IsColliding)
        {
            if (_isJumping && _inputJumpHeld && Mathf.Abs(_vel.Y) < JumpApexControl)
                _vel.ApproachY(MaxFall, Gravity * JumpApexControlMultiplier * _delta);
            else
                _vel.ApproachY(MaxFall, Gravity * _delta);
        }

        // Jumping
        if (_jumpBufferTimer.IsRunning() && _coyoteTimer.IsRunning())
        {
            _coyoteTimer.SetTime(0f);
            _jumpBufferTimer.SetTime(0f);

            _vel.AddX(JumpHBoost * _inputX);
            _vel.SetY(-JumpForce);

            _variableJumpTimer.SetTime(VariableJumpTime);
            _isJumping = true;
        }

        // Variable Jump
        if (_variableJumpTimer.IsRunning() && _inputJumpReleased)
        {
            _variableJumpTimer.SetTime(0f);
            _isJumping = false;
            if (_vel.Y < 0)
                _vel.MultiplyY(VariableJumpMultiplier);
        }

        // Horizontal
        float accel = RunAccel;
        bool sameDir = Calc.SameSign(_inputX, _vel.X);
        if (Mathf.Abs(_vel.X) > MaxRunSpeed && sameDir)
            accel = RunReduce;
        if (Mathf.Abs(_inputX) > 0f && !sameDir)
            accel *= 2f;

        _vel.ApproachX(_inputX * MaxRunSpeed, accel * _delta);

        _actor.MoveX(_vel.X * _delta, OnCollideX);
        _actor.MoveY(_vel.Y * _delta, OnCollideY);

        return StNormal;
    }

    private void DashEnter()
    {
        _vel.Set(0f, 0f);
        _dashDir = Vector2.Zero;

        _groundDash = _groundedChecker.IsColliding;

        _dashTrail.ClearPoints();
        _dashTrail.ResetTimerToZero();
        _dashTrail.Emitting = true;
    }

    private int DashUpdate()
    {
        _actor.MoveX(_vel.X * _delta, OnCollideX);
        if (_queueDashStop)
        {
            _queueDashStop = false;
            return StNormal;
        }

        return StDash;
    }

    private void DashExit()
    {
        _dashTrail.Emitting = false;

        CustomTimerComponent.Create(this, 0.2f, true).Timeout += () => _dashTrail.StartClearingPoints();

        GD.Print("Stopped dashing.");
    }

    private IEnumerator DashCoroutine()
    {
        yield return null;

        _dashDir = _lastNonZeroDir;

        Vector2 speed = _dashDir * DashSpeed;

        if (Calc.SameSign(_vel.X, speed.x) && Mathf.Abs(_vel.X) > Mathf.Abs(speed.x))
            speed.x = _vel.X;

        _vel.Set(speed);

        yield return DashTime;

        _vel.MultiplyX(DashFinishMultiplier);
        _stateMachine.SetState(StNormal);
    }

    #endregion

    #region Extra Movement

    private void OnCollideX(AxisAlignedBoundingBoxComponent aabb)
    {
        IsColliding = true;

        if (_stateMachine.State == StDash)
        {
            int dir = (int)_dashDir.x;
            Vector2i offset = AttemptHorizontalCornerCorrection(dir);

            if (offset != Vector2i.Zero)
            {
                _actor.MoveXExact(offset.x);
                _actor.MoveYExact(offset.y);
                return;
            }
            _queueDashStop = true;
        }

        // TODO(calco): Maybe do horizontal corner correction?
        _actor.ClearRemainderX();
        _vel.SetX(0f);
    }

    private void OnCollideY(AxisAlignedBoundingBoxComponent other)
    {
        IsColliding = true;

        // Check if it was a head collision
        if (_vel.Y <= 0 && other.Bottom <= _actor.BoundingBox.Top) // Frick inverted Y axis
        {
            Vector2i offset = AttemptVerticalCornerCorrection();
            if (offset != Vector2i.Zero)
            {
                _actor.MoveXExact(offset.x);
                _actor.MoveYExact(offset.y);
                return;
            }
        }

        _actor.ClearRemainderY();
        _vel.SetY(0f);
    }

    private Vector2i AttemptHorizontalCornerCorrection(int dir)
    {
        for (int i = 0; i < CornerCorrectionPixels; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                Vector2i offset = new Vector2i(dir, (i + 1) * j);
                bool collided = PhysicsManager.Instance.CheckWithSolidsCollisionAt(_actor.BoundingBox, offset);
                if (collided)
                    continue;

                return offset;
            }
        }

        return Vector2i.Zero;
    }

    private Vector2i AttemptVerticalCornerCorrection()
    {
        for (int i = 0; i < CornerCorrectionPixels; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                Vector2i offset = new Vector2i((i + 1) * j, -1);
                bool collided = PhysicsManager.Instance.CheckWithSolidsCollisionAt(_actor.BoundingBox, offset);
                if (collided)
                    continue;

                return offset;
            }
        }

        return Vector2i.Zero;
    }

    #endregion
}