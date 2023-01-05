using Godot;
using Incandescent.GameObjects;
using Incandescent.Utils;

namespace Incandescent.Components;

public partial class CustomTimerComponent : Node
{
    [ExportGroup("Settings")]
    [Export] private bool _updateAutomatically;
    
    [ExportGroup("Debug")]
    [Export] private float _time;
    
    [Signal]
    public delegate void TimeoutEventHandler();
    
    public float Time => _time;

    private bool _triggeredEvent;

    /// <summary>
    /// If true, the timer will update in the Update method.
    /// </summary>
    public bool UpdateAutomatically
    {
        get => _updateAutomatically;
        set => _updateAutomatically = value;
    }

    public override void _Process(double delta)
    {
        if (_updateAutomatically)
            Update((float) delta);
    }

    public void Update(float deltaTime)
    {
        _time = Mathf.Max(_time - deltaTime, 0f);
        if (HasFinished() && !_triggeredEvent)
        {
            EmitSignal(SignalName.Timeout);
            _triggeredEvent = true;
        }
    }
        
    /// <summary>
    /// Sets the timer to the given value, and sets UpdateAutomatically to true.
    /// </summary>
    public void Start(float time)
    {
        _updateAutomatically = true;
        _time = time;
    }
        
    /// <summary>
    /// Only works if UpdateAutomatically is true.
    /// </summary>
    public void Pause()
    {
        _updateAutomatically = false;
    }

    public void SetTime(float time)
    {
        _triggeredEvent = Calc.FloatEquals(time, 0f);
        _time = time;
    }
        
    public bool HasFinished()
    {
        return Calc.FloatEquals(_time, 0f);
    }
        
    public bool IsRunning()
    {
        return _time > 0f;
    }
}