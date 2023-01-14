using Godot;
using Incandescent.Utils;

namespace Incandescent.Components.Logic;

public partial class CustomTimerComponent : Node
{
    [Export] public bool UpdateSelf { get; set; } = false;

    [ExportGroup("Debug")]
    [Export] private float _time;

    [Signal]
    public delegate void TimeoutEventHandler();

    public float Time => _time;

    private bool _triggeredEvent;

    public static CustomTimerComponent Create(Node parent, float time, bool updateAutomatically = false)
    {
        CustomTimerComponent c = new CustomTimerComponent();
        c.Init(time, updateAutomatically);

        parent.AddChild(c);
        return c;
    }

    public void Init(float time, bool updateAutomatically = false)
    {
        _time = time;
        UpdateSelf = updateAutomatically;
    }

    public override void _Process(double delta)
    {
        if (UpdateSelf)
            Update((float)delta);
    }

    public bool HasFinished()
    {
        return Calc.FloatEquals(_time, 0f);
    }

    public bool IsRunning()
    {
        return _time > 0f;
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

    public void Start(float time)
    {
        UpdateSelf = true;
        _time = time;
    }

    public void Pause()
    {
        UpdateSelf = false;
    }

    public void SetTime(float time)
    {
        _triggeredEvent = Calc.FloatEquals(time, 0f);
        _time = time;
    }
}