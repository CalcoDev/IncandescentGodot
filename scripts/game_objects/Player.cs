using System;
using Eyes.Components;
using Eyes.Components.Physics;
using Eyes.Managers;
using Eyes.Utils;
using Godot;

namespace Eyes.GameObjects;

public partial class Player : Node2D
{
    [ExportGroup("Refs")]
    [Export] private VelocityComponent _vel;
    [Export] private ActorComponent _actor;

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

    private bool _isGrounded;

    public override void _Ready()
    {
        Vector2 t = _actor.GlobalPosition;
        _actor.TopLevel = true;
        _actor.GlobalPosition = t;

        _actor.OnSquish += OnSquish;
    }

    public override void _Process(double delta)
    {
        _inputX = Input.GetAxis("axis_horizontal_negative", "axis_horizontal_positive");
        _inputJumpPressed = Input.IsActionJustPressed("btn_space");
        _inputJumpReleased = Input.IsActionJustReleased("btn_space");
        _inputJumpHeld = Input.IsActionPressed("btn_space");

        _inputDashPressed = Input.IsActionJustPressed("btn_shift");

        if (Mathf.Abs(_inputX) > 0f)
            _lastNonZeroDir = new Vector2(_inputX, 0f);

        // Handle Jump.
        if (_inputJumpPressed)
        {
            GD.Print("Jump");
            _vel.AddX(JumpHBoost * _inputX);
            _vel.SetY(-JumpForce);

            _isJumping = true;
        }

        Test((float)delta);
    }

    private void Test(float delta)
    {
        _isGrounded = _actor.CollideAt(LevelManager.Instance.Solids, Vector2i.Down);

        // Add the gravity.
        if (!_isGrounded)
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

        _actor.MoveX(_vel.X * delta, OnCollideX);
        _actor.MoveY(_vel.Y * delta, OnCollideY);

        // Vector2i pos = Vector2i.Zero;
        // pos.x = Calc.FloorToIntButCeilIfClose(_actor.GlobalPosition.x);
        // pos.y = Calc.FloorToIntButCeilIfClose(_actor.GlobalPosition.y);
        // GlobalPosition = pos;
    }

    private void OnCollideX(AABBComponent other)
    {
    }

    private void OnCollideY(AABBComponent other)
    {
    }

    private void OnSquish(AABBComponent other)
    {
        GD.Print("Player was squished!");
    }
}
