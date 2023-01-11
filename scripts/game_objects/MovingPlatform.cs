using System;
using System.Collections.Generic;
using Godot;
using Incandescent.Utils;

namespace Eyes.GameObjects;

public partial class MovingPlatform : Node2D
{
    [Export] private StaticBody2D _solid;
    [Export] private Area2D _area;

    [Export] private Node _path;
    [Export] private float _duration = 1f;
    [Export] private float _delay = 0.5f;

    private List<Node2D> _points;
    private int _currentPoint = -1;

    private Tween _tween;
    private Vector2 _targetPos;

    private List<CharacterBody2D> _ridingActors;

    public override void _EnterTree()
    {
        _ridingActors = new();
    }

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

        _area.BodyEntered += OnBodyEntered;
        _area.BodyExited += OnBodyExited;
    }

    private void OnBodyEntered(Node2D body)
    {
        CharacterBody2D actor = (CharacterBody2D)body;

        // TODO(calco): maybe make this not check equality but other sth
        var shape = actor.GetNode<CollisionShape2D>("CollisionShape2D");
        Vector2 size = (Vector2)shape.Shape.Get("size");
        float bottom = actor.GlobalPosition.y + size.y;
        float top = GlobalPosition.y;
        if (Mathf.RoundToInt(bottom) != Mathf.RoundToInt(top))
            return;

        _ridingActors.Add(actor);
        GD.Print($"Added: {actor.Name}");
    }

    private void OnBodyExited(Node2D body)
    {
        CharacterBody2D actor = (CharacterBody2D)body;

        if (!_ridingActors.Contains(actor))
            return;

        GD.Print($"Removed {actor.Name}.");
        _ridingActors.Remove(actor);
    }

    public override void _Process(double delta)
    {
        // _solid.MoveAndCollide(_targetPos - _solid.GlobalPosition);
        // GlobalPosition = _solid.GlobalPosition;

        GD.Print(_ridingActors.Count);
        foreach (CharacterBody2D actor in _ridingActors)
        {
            actor.GlobalPosition += _targetPos - GlobalPosition;
        }
        GlobalPosition = _targetPos;
    }
}