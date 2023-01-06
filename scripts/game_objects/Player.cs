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
    [Export] private ActorComponent _actorComponent;
    [Export] private CollisionCheckerComponent _groundedChecker;
    [Export] private StateMachineComponent _stateMachine;

    [ExportGroup("Gravity")]
    [Export] private float Gravity = 140f   * 5f;
    [Export] private float MaxFall = 25f    * 6f;

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

    [ExportGroup("Run")]
    [Export] private float MaxRunSpeed = 14f* 8.5f;
    [Export] private float RunAccel = 200f  * 8f;
    [Export] private float RunReduce = 62f  * 8f;
    
    [ExportGroup("Forgiveness")]
    [Export(PropertyHint.Range, "0, 4, 1")] private int CornerCorrectionPixels = 2;

    // Input
    private float _inputX;
    private bool _inputJumpPressed;
    private bool _inputJumpReleased;
    private bool _inputJumpHeld;

    // Jumping
    private bool _isJumping;
    
    // State
    private const int StNormal = 0;
    private const int StDash = 1;

    private float _delta;
    
    public override void _Ready()
    {
        _stateMachine.Init(1, -1);
        _stateMachine.SetCallbacks(StNormal, NormalUpdate, null, null, NormalCoroutine);
        _stateMachine.SetState(StNormal);
        
        _groundedChecker.OnCollide += () =>
        {
            _coyoteTimer.SetTime(CoyoteTime);

            var inst = _hitGroundParticles.Instantiate<CPUParticles2D>();
            inst.GlobalPosition = GlobalPosition;
            inst.Position += new Vector2(6f, 16f);
            GetNode("/root").AddChild(inst);
            
            _isJumping = false;
        };
        
        _groundedChecker.OnSeparate += () =>
        {
            _coyoteTimer.SetTime(CoyoteTime);
        };
    }

    public override void _Process(double delta)
    {
        _delta = (float) delta;
        
        // Input
        _inputX = Input.GetAxis("axis_horizontal_negative", "axis_horizontal_positive");
        _inputJumpPressed = Input.IsActionJustPressed("btn_jump");
        _inputJumpReleased = Input.IsActionJustReleased("btn_jump");
        _inputJumpHeld = Input.IsActionPressed("btn_jump");

        // Collisions
        _groundedChecker.Update();

        int newState = _stateMachine.Update();
        _stateMachine.SetState(newState);
        
        GlobalPosition = _actorComponent.IntPosition;
    }

    #region States

    private int NormalUpdate()
    {
        // Timers
        if (!_groundedChecker.IsColliding)
            _coyoteTimer.Update(_delta);
        
        _jumpBufferTimer.Update(_delta);
        if (_inputJumpPressed)
            _jumpBufferTimer.SetTime(JumpBufferTime);
        
        if (_isJumping)
            _variableJumpTimer.Update(_delta);

        // Movement
        Vector2 vel = _actorComponent.Velocity;

        // Gravity
        if (!_groundedChecker.IsColliding)
        {
            if (_isJumping && _inputJumpHeld && Mathf.Abs(vel.y) < JumpApexControl)
                vel.y = Calc.Approach(vel.y, MaxFall, Gravity * JumpApexControlMultiplier * _delta);
            else
                vel.y = Calc.Approach(vel.y, MaxFall, Gravity * _delta);
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
        if (Mathf.Abs(_inputX) > 0f && !Calc.SameSign(vel.x, _inputX))
            accel *= 2f;
        
        vel.x = Calc.Approach(vel.x, _inputX * MaxRunSpeed, accel * _delta);

        _actorComponent.Velocity = vel;
        
        _actorComponent.MoveX(_actorComponent.Velocity.x * _delta, OnCollideX);
        _actorComponent.MoveY(_actorComponent.Velocity.y * _delta, OnCollideY);
        
        return StNormal;
    }
    
    private IEnumerator NormalCoroutine()
    {
        GD.Print("Hello normal");

        yield return Test();

        yield return 1f;
        
        GD.Print("yo it's me normal again");
    }

    private IEnumerator Test()
    {
        GD.Print("Test here yoyoyoy");
        yield return 1f;
        GD.Print("Test out yoyoyoyo");
    }

    #endregion

    #region Extra Movement

    private void OnCollideX(AxisAlignedBoundingBoxComponent _)
    {
        // TODO(calco): Maybe do horizontal corner correction?
        _actorComponent.ZeroRemainderX();
        _actorComponent.Velocity = new Vector2(0, _actorComponent.Velocity.y);
    }
    
    private void OnCollideY(AxisAlignedBoundingBoxComponent other)
    {
        // Check if it was a head collision
        if (_actorComponent.Velocity.y <= 0 && other.Bottom <= _actorComponent.BoundingBox.Top) // Frick inverted Y axis
        {
            Vector2i offset = AttemptVerticalCornerCorrection();
            if (offset != Vector2i.Zero)
            {
                _actorComponent.MoveXExact(offset.x);
                _actorComponent.MoveYExact(offset.y);
                GD.Print("Corrected");
                return;
            }
        }

        _actorComponent.ZeroRemainderY();
        _actorComponent.Velocity = new Vector2(_actorComponent.Velocity.x, 0);
    }

    private Vector2i AttemptVerticalCornerCorrection()
    {
        for (int i = 0; i < CornerCorrectionPixels; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                Vector2i offset = new Vector2i((i + 1) * j, -1);
                bool collided = SolidManager.Instance.CheckCollisionAt(_actorComponent.BoundingBox, offset);
                if (collided)
                    continue;

                return offset;
            }
        }

        return Vector2i.Zero;
    }

    #endregion
}