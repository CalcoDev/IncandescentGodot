using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace Incandescent.GameObjects;

public partial class MovingPlatform : Node2D
{
    [Export] private Node _path;
    [Export] private float _duration = 1f;
    
    private List<Node2D> _points;
    private int _currentPoint = -1;
    
    private Tween _tween;
    
    public override void _Ready()
    {
        int cnt = _path.GetChildCount();
        _points = new List<Node2D>(cnt);
        for (int i = 0; i < cnt; i++)
            _points.Add(_path.GetChild<Node2D>(i));
        
        _tween = CreateTween();
        for (int i = 0; i < cnt; i++)
        {
            int next = (i + 1) % cnt;
            _tween
                .TweenProperty(this, "global_position", _points[next].GlobalPosition, _duration)
                .SetEase(Tween.EaseType.InOut)
                .SetTrans(Tween.TransitionType.Expo);
        }
        _tween.SetLoops();
        _tween.Play();
    }
}