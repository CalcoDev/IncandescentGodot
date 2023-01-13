using Godot;

namespace Eyes.Managers;

public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; }

    public static bool DebugMode => Instance.Debug;
    [Export] public bool Debug { get; set; }

    public override void _EnterTree()
    {
        Instance = this;
    }
}