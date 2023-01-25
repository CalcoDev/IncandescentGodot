using System;
using System.Collections;
using Godot;
using GodotUtilities;
using Incandescent.Components;
using Incandescent.Components.Logic;
using Incandescent.GameObjects.Base;
using Incandescent.Utils;

namespace Incandescent.GameObjects;

public partial class PhysicsPlayer : Actor
{
    #region Constants

    [ExportGroup("Constants")]
    [ExportSubgroup("Gravity")]
    [Export] private float Gravity = 140f * 5f;
    [Export] private float MaxFall = 25f * 6f;

    [ExportSubgroup("Run")]
    [Export] private float MaxRunSpeed = 14f * 8.5f;
    [Export] private float RunAccel = 200f * 8f;
    [Export] private float RunReduce = 62f * 8f;

    [ExportSubgroup("Forgiveness")]
    [Export(PropertyHint.Range, "0, 4, 1")] private int CornerCorrectionPixels = 4;

    [ExportSubgroup("Jump")]
    [Export] private float JumpForce = 204f + 8f;
    [Export] private float JumpHBoost = 13f * 5f;
    [Export(PropertyHint.Range, "0, 0.2")] private float CoyoteTime = 0.1f;
    [Export(PropertyHint.Range, "0, 0.2")] private float JumpBufferTime = 0.1f;
    [Export(PropertyHint.Range, "0, 0.2")] private float VariableJumpTime = 0.2f;
    [Export(PropertyHint.Range, "0, 1")] private float VariableJumpMultiplier = 0.5f;
    [Export(PropertyHint.Range, "0, 5")] private float JumpApexControl = 3f;
    [Export(PropertyHint.Range, "0, 1")] private float JumpApexControlMultiplier = 0.5f;

    [ExportSubgroup("Dash")]
    [Export] private float DashCooldown = 0.2f;
    [Export] private float DashSpeed = 14f * 8.5f * 3f;
    [Export] private float DashTime = 0.15f;
    [Export(PropertyHint.Range, "0, 1")] private float DashFinishMultiplier = 0.75f;

    #endregion

    #region Variables

    // General
    [Node("VelocityComponent")] private VelocityComponent _vel;
    [Node("GroundedChecker")] private Area2D _groundedChecker;
    [Node("StateMachine")] private StateMachineComponent _stateMachine;

    // States
    private float _delta;

    private const int StNormal = 0;
    private const int StDash = 1;
    private const int StPrimary = 2;
    private const int StSecondary = 3;

    // Jump
    [Node("CoyoteTimer")] private CustomTimerComponent _coyoteTimer;
    [Node("JumpBufferTimer")] private CustomTimerComponent _jumpBufferTimer;
    [Node("VariableJumpTimer")] private CustomTimerComponent _variableJumpTimer;

    private bool _isGrounded;
    private bool _isJumping;

    // Dash
    [Node("DashCooldownTimer")] private CustomTimerComponent _dashCooldownTimer;

    private Vector2 _dashDir;
    private bool _groundDash;
    private bool _queueDashStop;

    // Input
    private float _inputX;
    private bool _inputJumpPressed;
    private bool _inputJumpReleased;
    private bool _inputJumpHeld;
    private bool _inputDashPressed;
    private Vector2 _lastNonZeroDir;

    private bool _inputPrimaryPressed;
    private bool _inputSecondaryPressed;

    #endregion

    public override void _EnterTree()
    {
        this.WireNodes();
    }

    public override void _Ready()
    {
        _groundedChecker.BodyEntered += OnEnterGround;
        _groundedChecker.BodyExited += OnExitGround;

        _stateMachine.Init(10, -1);
        _stateMachine.SetCallbacks(StNormal, NormalUpdate, null, null, null);
        _stateMachine.SetCallbacks(StDash, DashUpdate, DashEnter, DashExit, DashCoroutine);

        _stateMachine.SetState(StNormal);
    }

    // _PhysicsProcess
    public override void _PhysicsProcess(double delta)
    {
        _delta = (float)delta;

        // Gather Input
        _inputX = Input.GetAxis("axis_horizontal_negative", "axis_horizontal_positive");
        _inputJumpPressed = Input.IsActionJustPressed("btn_space");
        _inputJumpReleased = Input.IsActionJustReleased("btn_space");
        _inputJumpHeld = Input.IsActionPressed("btn_space");

        _inputDashPressed = Input.IsActionJustPressed("btn_shift");

        _inputPrimaryPressed = Input.IsActionJustPressed("btn_primary");
        _inputSecondaryPressed = Input.IsActionJustPressed("btn_secondary");

        if (Mathf.Abs(_inputX) > 0f)
            _lastNonZeroDir = new Vector2(_inputX, 0f);

        int newState = _stateMachine.Update();
        _stateMachine.SetState(newState);
    }

    #region States

    private int NormalUpdate()
    {
        // States
        if (_inputDashPressed && _dashCooldownTimer.HasFinished())
            return StDash;

        // Timers
        if (!_isGrounded)
            _coyoteTimer.Update(_delta);

        _jumpBufferTimer.Update(_delta);
        if (_inputJumpPressed)
            _jumpBufferTimer.SetTime(JumpBufferTime);

        if (_isJumping)
            _variableJumpTimer.Update(_delta);

        // Gravity
        if (!_isGrounded)
        {
            if (_isJumping && _inputJumpHeld && Mathf.Abs(_vel.Y) < JumpApexControl)
                _vel.ApproachY(MaxFall, Gravity * JumpApexControlMultiplier * _delta);
            else
                _vel.ApproachY(MaxFall, Gravity * _delta);
        }

        // Jumping
        GD.Print(_coyoteTimer.Time);
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
        bool sameDir = Calc.SameSignZero(_inputX, _vel.X);
        if (Mathf.Abs(_vel.X) > MaxRunSpeed && sameDir)
            accel = RunReduce;
        if (Mathf.Abs(_inputX) > 0f && !sameDir)
            accel *= 2f;

        _vel.ApproachX(_inputX * MaxRunSpeed, accel * _delta);

        MoveX(_vel.X * _delta, OnCollideH);
        MoveY(_vel.Y * _delta, OnCollideV);

        return StNormal;
    }

    // Dash
    private void DashEnter()
    {
        _vel.Set(0f, 0f);
        _dashDir = Vector2.Zero;

        _groundDash = _isGrounded;
    }

    private int DashUpdate()
    {
        MoveX(_vel.X * _delta, OnCollideH);
        if (_queueDashStop)
        {
            _queueDashStop = false;
            return StNormal;
        }

        return StDash;
    }

    private void DashExit()
    {
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

    #region Collision Events

    public override void Squish(KinematicCollision2D coll)
    {
    }

    private void OnCollideH(KinematicCollision2D coll)
    {
        if (Calc.FloatEquals(_vel.X, 0f))
            return;

        _vel.SetX(0f);
    }

    private void OnCollideV(KinematicCollision2D coll)
    {
        if (Calc.FloatEquals(_vel.Y, 0f))
            return;

        if (_vel.Y <= 0 && Calc.FloatEquals(coll.GetNormal().y, 1f)) // Frick inverted Y axis
        {
            Vector2 offset = AttemptVerticalCornerCorrection();
            if (offset != Vector2i.Zero)
            {
                GlobalPosition += offset;
                return;
            }
        }

        _vel.SetY(0f);
    }

    private void OnEnterGround(Node body)
    {
        if (body is not PhysicsBody2D && body is not TileMap)
        {
            return;
        }

        if (body is CharacterBody2D cB && cB.GetRid().Equals(this.GetRid()))
            return;

        _isGrounded = true;

        _coyoteTimer.SetTime(CoyoteTime);
        _dashCooldownTimer.SetTime(0f);
        _isJumping = false;
    }

    private void OnExitGround(Node body)
    {
        if (body is not PhysicsBody2D && body is not TileMap)
        {
            return;
        }

        if (body is CharacterBody2D cB && cB.GetRid().Equals(this.GetRid()))
            return;

        _isGrounded = false;

        _coyoteTimer.SetTime(CoyoteTime);
    }

    #endregion

    #region Corner Correction

    private Vector2 AttemptVerticalCornerCorrection()
    {
        for (float i = 0; i < CornerCorrectionPixels; i += 0.25f)
        {
            for (int j = -1; j < 2; j++)
            {
                Vector2 offset = new Vector2((i + 1f) * j, 0f);
                var transf = GlobalTransform;
                transf.origin.y -= _vel.Y * _delta;
                bool collided = TestMove(transf.Translated(offset), Vector2.Up, null, 0.001f);
                if (collided)
                    continue;

                return offset;
            }
        }

        return Vector2i.Zero;
    }

    #endregion
}
