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
    private bool _isJumping;

    #endregion

    public override void _EnterTree()
    {
        this.WireNodes();
    }

    public override void _PhysicsProcess(double delta)
    {
        _delta = (float)delta;

        Normal();
    }

    private void Normal()
    {
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

        MoveX(_vel.X * _delta);
        MoveY(_vel.Y * _delta);

        // GD.Print(GlobalPosition);
    }

    private void MoveY(float amount)
    {
        var space = GetWorld2d().DirectSpaceState;

        Vector2 offset = amount > 0 ? Vector2.Down * 6f : Vector2.Up * 6f;
        var startPos = GlobalPosition + offset;

        var query = PhysicsRayQueryParameters2D.Create(
            startPos + ((amount > 0 ? Vector2.Up : Vector2.Down) * 0.1f),
            startPos + (Vector2.Down * amount)
        );

        var res = space.IntersectRay(query);
        if (res.Count > 0)
        {
            var hitPoint = (Vector2)res["position"];
            GlobalPosition = hitPoint - offset;
        }
        else
        {
            GlobalPosition += Vector2.Down * amount;
        }
    }

    private void MoveX(float amount)
    {
        var space = GetWorld2d().DirectSpaceState;

        Vector2 offset = amount > 0 ? Vector2.Right * 4f : Vector2.Left * 4f;
        var startPos = GlobalPosition + offset;

        var query = PhysicsRayQueryParameters2D.Create(
            startPos + ((amount > 0 ? Vector2.Left : Vector2.Right) * 0.1f),
            startPos + (Vector2.Right * amount)
        );

        var res = space.IntersectRay(query);
        if (res.Count > 0)
        {
            var hitPoint = (Vector2)res["position"];
            GlobalPosition = hitPoint - offset;
        }
        else
        {
            GlobalPosition += Vector2.Right * amount;
        }
    }
}
