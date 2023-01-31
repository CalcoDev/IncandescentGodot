using System.Collections.Generic;
using Godot;
using GodotUtilities;
using Incandescent.GameObjects.Base;
using Incandescent.Utils;

namespace Incandescent.GameObjects;

public partial class MovingPlatform : Solid
{
    [Node("Path")]
    private Node _path;

    [Node("CollisionShape2D")]
    private CollisionShape2D _shape;

    [Export] private float _duration = 1f;
    [Export] private float _delay = 0.5f;

    private List<Node2D> _points;
    private int _currentPoint = -1;

    private Tween _tween;
    private Vector2 _targetPos;

    public override void _EnterTree()
    {
        this.WireNodes();
    }

    public override void _Ready()
    {
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

    public override void _PhysicsProcess(double delta)
    {
        var vel = (_targetPos - GlobalPosition) / (float)delta;

        MoveX(vel.x * (float)delta);
        MoveY(vel.y * (float)delta);

        // GlobalPosition = _targetPos;
    }
}