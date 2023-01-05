using Godot;
using Incandescent.Components;
using Incandescent.Utils;

namespace Incandescent.GameObjects;

public partial class Player : Node2D
{
    [ExportGroup("Refs")]
    [Export] private ActorComponent _actorComponent;
    [Export] private CollisionCheckerComponent _groundedChecker;

    [ExportGroup("Gravity")]
    [Export] private float Gravity = 140f   * 5f;
    [Export] private float MaxFall = 25f    * 6f;

    [ExportGroup("Jump")] 
    [Export] private CustomTimerComponent _coyoteTimer;
    [Export] private CustomTimerComponent _jumpBufferTimer;
    [Export] private CustomTimerComponent _variableJumpTimer;
    
    [Export] private float JumpForce = 204f + 17f;
    [Export] private float JumpHBoost = 13f * 5f;
    [Export(PropertyHint.Range, "0, 0.2")] private float CoyoteTime = 0.1f;
    [Export(PropertyHint.Range, "0, 0.2")] private float JumpBufferTime = 0.1f;
    [Export(PropertyHint.Range, "0, 0.2")] private float VariableJumpTime = 0.2f;
    [Export(PropertyHint.Range, "0, 1")] private float VariableJumpMultiplier = 0.5f;

    [ExportGroup("Run")]
    [Export] private float MaxRunSpeed = 14f* 8.5f;
    [Export] private float RunAccel = 200f  * 8f;
    [Export] private float RunReduce = 62f  * 8f;

    // Input
    private float _inputX;
    private bool _inputJumpPressed;
    private bool _inputJumpReleased;

    // Jumping
    private bool _isJumping;
    
    public override void _Ready()
    {
        _groundedChecker.OnCollide += () =>
        {
            _coyoteTimer.SetTime(CoyoteTime);

            _isJumping = false;
        };
        
        _groundedChecker.OnSeparate += () =>
        {
            _coyoteTimer.SetTime(CoyoteTime);
        };
    }

    public override void _Process(double delta)
    {
        // Input
        _inputX = Input.GetAxis("axis_horizontal_negative", "axis_horizontal_positive");
        _inputJumpPressed = Input.IsActionJustPressed("btn_jump");
        _inputJumpReleased = Input.IsActionJustReleased("btn_jump");
        
        // Collisions
        _groundedChecker.Update();
        
        // Timers
        if (!_groundedChecker.IsColliding)
            _coyoteTimer.Update((float) delta);
        
        _jumpBufferTimer.Update((float) delta);
        if (_inputJumpPressed)
            _jumpBufferTimer.SetTime(JumpBufferTime);
        
        if (_isJumping)
            _variableJumpTimer.Update((float) delta);

        // Movement
        Vector2 vel = _actorComponent.Velocity;

        // Gravity
        if (!_groundedChecker.IsColliding)
        {
            vel.y = Calc.Approach(vel.y, MaxFall, Gravity * (float) delta);
        }

        // Jumping
        if (_jumpBufferTimer.IsRunning() && _coyoteTimer.IsRunning())
        {
            _coyoteTimer.SetTime(0f);
            _jumpBufferTimer.SetTime(0f);
            
            vel.x += JumpHBoost * _inputX;
            vel.y = -JumpForce;
            
            _variableJumpTimer.SetTime(VariableJumpTime);
            _isJumping = true;
        }
        
        // Variable Jump
        if (_variableJumpTimer.IsRunning() && _inputJumpReleased)
        {
            _variableJumpTimer.SetTime(0f);
            _isJumping = false;
            if (vel.y < 0)
                vel.y *= VariableJumpMultiplier;
        }

        // Horizontal
        float accel = RunAccel;
        if (Mathf.Abs(vel.x) > MaxRunSpeed && Calc.SameSign(_inputX, vel.x))
            accel = RunReduce;
        
        vel.x = Calc.Approach(vel.x, _inputX * MaxRunSpeed, accel * (float) delta);

        _actorComponent.Velocity = vel;
        
        _actorComponent.MoveX(_actorComponent.Velocity.x * (float) delta);
        _actorComponent.MoveY(_actorComponent.Velocity.y * (float) delta);

        GlobalPosition = _actorComponent.IntPosition;
    }

    public void OnCoyoteTimerTimeout()
    {
        GD.Print("test");
    }
}