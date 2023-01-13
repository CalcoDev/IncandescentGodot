using System;
using System.Reflection.Metadata.Ecma335;
using Eyes.Components;
using Godot;
using Incandescent.Utils;

namespace Eyes.GameObjects;

public partial class Player : Node2D
{
    [ExportGroup("Refs")]
    [Export] private VelocityComponent _vel;
    [Export] private CharacterBody2D _actor;

    [ExportGroup("Gravity")]
    [Export] private float Gravity = 140f * 5f;
    [Export] private float MaxFall = 25f * 6f;

    [ExportGroup("Jump")]
    [Export] private float JumpForce = 204f + 8f;
    [Export] private float JumpHBoost = 13f * 5f;
    [Export(PropertyHint.Range, "0, 5")] private float JumpApexControl = 3f;
    [Export(PropertyHint.Range, "0, 1")] private float JumpApexControlMultiplier = 0.5f;

    [ExportGroup("Run")]
    [Export] private float MaxRunSpeed = 14f * 8.5f;
    [Export] private float RunAccel = 200f * 8f;
    [Export] private float RunReduce = 62f * 8f;

    // Input
    private float _inputX;
    private bool _inputJumpPressed;
    private bool _inputJumpReleased;
    private bool _inputJumpHeld;

    private bool _inputDashPressed;

    private Vector2 _lastNonZeroDir;

    private bool _isJumping;

    public override void _Ready()
    {
        Vector2 t = _actor.GlobalPosition;
        _actor.TopLevel = true;
        _actor.GlobalPosition = t;
    }

    private bool _test = true;

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("btn_rand_toggle"))
            _test = !_test;

        _inputX = Input.GetAxis("axis_horizontal_negative", "axis_horizontal_positive");
        _inputJumpPressed = Input.IsActionJustPressed("btn_space");
        _inputJumpReleased = Input.IsActionJustReleased("btn_space");
        _inputJumpHeld = Input.IsActionPressed("btn_space");

        _inputDashPressed = Input.IsActionJustPressed("btn_shift");

        if (Mathf.Abs(_inputX) > 0f)
            _lastNonZeroDir = new Vector2(_inputX, 0f);

        // Handle Jump.
        if (_inputJumpPressed) // _jumpBufferTimer.IsRunning() && _coyoteTimer.IsRunning()
        {
            // _coyoteTimer.SetTime(0f);
            // _jumpBufferTimer.SetTime(0f);

            GD.Print("Jump");
            _vel.AddX(JumpHBoost * _inputX);
            _vel.SetY(-JumpForce);

            // _variableJumpTimer.SetTime(VariableJumpTime);
            _isJumping = true;
        }

        // Test((float)delta);
    }

    public override void _PhysicsProcess(double delta)
    {
        Test((float)delta);
    }

    private void Test(float delta)
    {
        // Add the gravity.
        if (!_actor.IsOnFloor())
        {
            if (_isJumping && _inputJumpHeld && Mathf.Abs(_vel.Y) < JumpApexControl)
                _vel.ApproachY(MaxFall, Gravity * JumpApexControlMultiplier * delta);
            else
                _vel.ApproachY(MaxFall, Gravity * delta);
        }

        // Horizontal
        float accel = RunAccel;
        bool sameDir = Calc.SameSign(_inputX, _vel.X);
        if (Mathf.Abs(_vel.X) > MaxRunSpeed && sameDir)
            accel = RunReduce;
        if (Mathf.Abs(_inputX) > 0f && !sameDir)
            accel *= 2f;

        _vel.ApproachX(_inputX * MaxRunSpeed, accel * delta);

        _actor.Velocity = _vel.Value;

        // TODO(calco): What in the world is this.
        Vector2i pos = Vector2i.Zero;

        if (_test)
            _actor.MoveAndSlide();
        GD.Print($"Moved player actor on frame: {Time.GetTicksMsec()}.");

        pos.x = Calc.FloorToIntButCeilIfClose(_actor.GlobalPosition.x);
        pos.y = Calc.FloorToIntButCeilIfClose(_actor.GlobalPosition.y);

        GlobalPosition = pos;
    }
}
