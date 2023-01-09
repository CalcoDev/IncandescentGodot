using System.Collections.Generic;
using Godot;
using Incandescent.Components;

namespace Incandescent.GameObjects;

public partial class MovingPlatform : Node2D
{
    [ExportGroup("Refs")]
    [Export] private SolidComponent _solid;

    [ExportGroup("Settings")]
    [Export] private Node _path;
    [Export] private float _duration = 1f;
    [Export] private float _delay = 0.5f;

    private List<Node2D> _points;
    private int _currentPoint = -1;

    private Tween _tween;
    private Vector2 _targetPos;

    public override void _Ready()
    {
        base._Ready();

        int cnt = _path.GetChildCount();
        _points = new List<Node2D>(cnt);
        for (int i = 0; i < cnt; i++)
            _points.Add(_path.GetChild<Node2D>(i));

        _tween = CreateTween();

        _targetPos = _points[0].Position;
        for (int i = 0; i < cnt; i++)
        {
            int next = (i + 1) % cnt;
            _tween
                .TweenProperty(this, "_targetPos", _points[next].GlobalPosition, _duration)
                .SetEase(Tween.EaseType.InOut)
                .SetTrans(Tween.TransitionType.Expo)
                .SetDelay(_delay);
        }
        _tween.SetLoops();
        _tween.Play();
    }

    public override void _Process(double delta)
    {
        _solid.MoveX(_targetPos.x - (GlobalPosition.x + _solid.Remainder.x));
        _solid.MoveY(_targetPos.y - (GlobalPosition.y + _solid.Remainder.y));

        GlobalPosition = _solid.GlobalPosition;
    }
}