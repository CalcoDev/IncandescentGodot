using Godot;

namespace Incandescent.Components.Logic;

public partial class FollowerComponent : Node
{
    [Export] public Node2D Follower { get; set; }
    [Export] public Node2D Followee { get; set; }
    [Export] public bool UpdateSelf { get; set; }

    public override void _Process(double delta)
    {
        if (UpdateSelf)
            Update();
    }

    public void Update()
    {
        Follower.GlobalPosition = Followee.GlobalPosition;
    }
}