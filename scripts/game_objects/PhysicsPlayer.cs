using System;
using Godot;
using GodotUtilities;
using Incandescent.Components;
using Incandescent.Components.Logic;
using Incandescent.Utils;

namespace Incandescent.GameObjects;

public partial class PhysicsPlayer : CharacterBody2D
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

    #endregion

    #region Variables

    [Node("VelocityComponent")]
    private VelocityComponent _vel;

    [Node("GroundedChecker")]
    private Area2D _groundedChecker;

    [Node("CoyoteTimer")]
    private CustomTimerComponent _coyoteTimer;

    [Node("JumpBufferTimer")]
    private CustomTimerComponent _jumpBufferTimer;

    [Node("VariableJumpTimer")]
    private CustomTimerComponent _variableJumpTimer;

    [Node("CollisionShape2D")]
    private CollisionShape2D _collisionShape;

    private float _inputX;
    private bool _inputJumpPressed;
    private bool _inputJumpReleased;
    private bool _inputJumpHeld;

    private float _delta;
    private bool _isGrounded;
    private bool _wasGrounded;
    private bool _isJumping;

    #endregion

    public override void _EnterTree()
    {
        this.WireNodes();
    }

    public override void _Ready()
    {
        _groundedChecker.BodyEntered += OnEnterGround;
        _groundedChecker.BodyExited += OnExitGround;
    }

    private void OnEnterGround(Node body)
    {

        if (body is not TileMap)
            return;

        GD.Print($"{body} entered.");

        _isGrounded = true;

        _coyoteTimer.SetTime(CoyoteTime);
        _isJumping = false;
    }

    private void OnExitGround(Node body)
    {
        if (body is not TileMap)
            return;

        GD.Print($"{body} exit.");

        _isGrounded = false;

        _coyoteTimer.SetTime(0f);
    }

    public override void _Process(double delta)
    {
        // Normal(delta);
    }

    public override void _PhysicsProcess(double delta)
    {
        Normal(delta);
    }

    private void Normal(double delta)
    {
        _delta = (float)delta;

        _inputX = Input.GetAxis("axis_horizontal_negative", "axis_horizontal_positive");
        _inputJumpPressed = Input.IsActionJustPressed("btn_space");
        _inputJumpReleased = Input.IsActionJustReleased("btn_space");
        _inputJumpHeld = Input.IsActionPressed("btn_space");

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

        Velocity = _vel.Get();
        if (MoveAndSlide())
        {
            int collCount = GetSlideCollisionCount();
            for (int i = 0; i < collCount; i++)
            {
                var coll = GetSlideCollision(i);
                if (!Calc.FloatEquals(coll.GetNormal().x, 0f))
                    OnCollideH(coll);

                if (!Calc.FloatEquals(coll.GetNormal().y, 0f))
                    OnCollideV(coll);
            }
        }
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
}
