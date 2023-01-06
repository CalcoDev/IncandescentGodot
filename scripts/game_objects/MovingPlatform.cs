using Godot;
using Godot.Collections;

namespace Incandescent.GameObjects;

public partial class MovingPlatform : Node2D
{
    [Export] private Array<Node2D> _points;
    [Export] private float _speed = 100f;
    
    private int _currentPoint = 0;
    
    public override void _Ready()
    {
    }
}