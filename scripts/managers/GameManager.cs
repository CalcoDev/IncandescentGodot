using Godot;

namespace Incandescent.Managers;

public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; }

    public static bool DebugMode => Instance.Debug;
    [Export] public bool Debug { get; set; } = false;

    [Signal]
    public delegate void OnDebugModeChangedEventHandler(bool debugMode);

    public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("btn_toggle_debug"))
        {
            Debug = !Debug;
            EmitSignal(SignalName.OnDebugModeChanged, Debug);
        }
    }
}