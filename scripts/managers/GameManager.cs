using Godot;
using Incandescent.GameObjects;

namespace Incandescent.Managers;

public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; }
    public static Node Root { get; private set; }
    public static PhysicsPlayer Player { get; private set; }

    public static float Delta { get; private set; }

    [Export] public bool Debug { get; set; } = false;

    #region Game Events

    [Signal]
    public delegate void OnDebugModeChangedEventHandler(bool debugMode);

    #endregion

    public override void _EnterTree()
    {
        Instance = this;
        Root = GetTree().Root;
    }

    public override void _Ready()
    {
        Player = GetTree().GetFirstNodeInGroup("player") as PhysicsPlayer;
    }

    public override void _Process(double delta)
    {
        Delta = (float)delta;

        if (Input.IsActionJustPressed("btn_toggle_debug"))
        {
            Debug = !Debug;
            EmitSignal(SignalName.OnDebugModeChanged, Debug);
        }
    }

    public static Node2D SpawnPixelatedFX(PackedScene fx, Vector2 position, bool root = true)
    {
        var fxInstance = fx.Instantiate() as Node2D;

        if (root)
            Root.AddChild(fxInstance);
        else
            Instance.AddChild(fxInstance);

        fxInstance.GlobalPosition = position;
        RenderingManager.Instance.TryAddNodeToLayer(fxInstance);

        return fxInstance;
    }
}