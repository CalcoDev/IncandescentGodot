using System.Collections.Generic;
using Godot;
using GodotUtilities;
using Incandescent.Utils;

namespace Incandescent.GameObjects;

public partial class PhysicsMovingPlatform : CharacterBody2D
{
    [Node("Path")]
    private Node _path;

    [Node("Area")]
    private Area2D _area;

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
        Velocity = vel;

        var coll = MoveAndCollide(vel * (float)delta);
        if (coll != null)
        {
            if (!Calc.FloatEquals(vel.y, 0f) || !Calc.FloatEquals(coll.GetNormal().y, 0f))
                return;

            if (coll.GetCollider() is PhysicsPlayer player)
            {
                // GD.Print($"Vel: {vel}");
                // player.Slide(vel);
                // TOOD(calco): Replace with mode and collide for more pro
                // player.MoveAndCollide(vel * (float)delta, safeMargin: 0.001f);

                player.Velocity = vel;
                if (player.MoveAndSlide())
                {
                    int cnt = player.GetSlideCollisionCount();
                    for (int i = 0; i < cnt; i++)
                    {
                        var pColl = player.GetSlideCollision(i);
                        player.Squish(pColl);
                    }
                }
            }
        }

        // if (MoveAndSlide())
        // {
        //     int collCount = GetSlideCollisionCount();
        //     for (int i = 0; i < collCount; i++)
        //     {
        //         var coll = GetSlideCollision(i);
        //         // if (!Calc.FloatEquals(coll.GetNormal().x, 0f))
        //         //     OnCollideH(coll);

        //         // if (!Calc.FloatEquals(coll.GetNormal().y, 0f))
        //         //     OnCollideV(coll);
        //         if (!Calc.FloatEquals(vel.y, 0f) || !Calc.FloatEquals(coll.GetNormal().y, 0f))
        //             continue;

        //         if (coll.GetCollider() is PhysicsPlayer player)
        //         {
        //             GD.Print($"Vel: {vel}");
        //             // player.Slide(vel);
        //             player.MoveAndCollide(vel * (float)delta);
        //         }
        //     }
    }

    // var vel = _targetPos - GlobalPosition;
    // GlobalPosition = _targetPos;

    // _area.GlobalPosition = GlobalPosition;
    // var bodies = _area.GetOverlappingBodies();

    // foreach (var body in bodies)
    // {
    //     if (body is PhysicsPlayer player)
    //     {
    //         player.Velocity = vel;
    //         player.MoveAndSlide();
    //     }
    // }
}