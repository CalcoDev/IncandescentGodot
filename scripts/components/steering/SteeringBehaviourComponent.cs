using System;
using System.Collections.Generic;
using Godot;
using GodotUtilities;
using Incandescent.Managers;
using Incandescent.Utils;

namespace Incandescent.Components.Steering;

public partial class SteeringBehaviourComponent : Node2D
{
    [Export] public float AgentRadius { get; set; } = 3.5f;

    [Node("ShapeCast2D")]
    private ShapeCast2D _shapeCast;

    public SteeringBehaviourDefinition Definition { get; set; }

    public Vector2 Velocity { get; set; }
    public Vector2 Target { get; set; }

    public Vector2 LastSteeringDirection { get; private set; }
    public int LastSteeringDirectionIndex { get; private set; }

    private Vector2[] _dirs;
    private float[] _attraction;
    private float[] _repulsion;

    public void SetDefinition(SteeringBehaviourDefinition definition)
    {
        if (Definition == definition)
            return;

        Definition = definition;
        _dirs = new Vector2[Definition.DirCount];
        _attraction = new float[Definition.DirCount];
        _repulsion = new float[Definition.DirCount];

        for (int i = 0; i < Definition.DirCount; i++)
        {
            float angle = Mathf.Pi * 2 * i / Definition.DirCount;
            _dirs[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            _attraction[i] = 0;
            _repulsion[i] = 0;
        }
    }

    #region Lifecycle

    public override void _Notification(long what)
    {
        if (what == NotificationEnterTree)
            this.WireNodes();
    }

    public override void _Process(double delta)
    {
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (!GameManager.Debug)
            return;

        const float Unit = 20f;

        float maxWeight = 0f;
        for (int i = 0; i < Definition.DirCount; i++)
            maxWeight = Mathf.Max(maxWeight, Mathf.Abs(GetWeight(i)));

        for (int i = 0; i < Definition.DirCount; i++)
        {
            // TODO(calco): Weights should probably be normalized already ???.
            float weight = GetWeight(i);
            float length = Mathf.Abs(weight) * Unit;
            Color col = weight > 0f ? Colors.DarkOliveGreen : Colors.Red;
            if (i == LastSteeringDirectionIndex)
                col = Colors.Green;

            DrawLine(Vector2.Zero, _dirs[i] * length, col, 1f);
        }

        DrawArc(Vector2.Zero, Unit, 0f, Mathf.Pi * 2f, 32, Colors.DarkRed, 1.5f);
    }

    #endregion

    public float GetWeight(int dirIdx)
    {
        return _attraction[dirIdx] + _repulsion[dirIdx];
    }

    public Vector2 GetDir(int dirIdx)
    {
        return _dirs[dirIdx];
    }

    public Vector2 GetSteeringDirection(Vector2 velocity, Vector2 attraction, Vector2 repulsion)
    {
        Target = attraction;
        Velocity = velocity;

        ComputeAttraction(attraction);
        ComputeRepulsion(repulsion);

        int idx = ChooseDirection();
        LastSteeringDirection = _dirs[idx];
        return LastSteeringDirection;
    }

    // TODO(calco): Replace Target and Velocity, which should stay local to entity with attraction and repulsion vectors.
    #region Computing

    private void ComputeAttraction(Vector2 v)
    {
        for (int i = 0; i < Definition.DirCount; i++)
            _attraction[i] = 0f;

        Vector2 towards = v.Normalized();

        float max = 0f;
        for (int i = 0; i < Definition.DirCount; i++)
        {
            float dot = _dirs[i].Dot(towards);
            float weight = Definition.AttractionShapingFunction(dot);

            _attraction[i] = Mathf.Max(weight, _attraction[i]);
            max = Mathf.Max(max, _attraction[i]);
        }

        if (!Calc.FloatEquals(max, 0f))
        {
            for (int i = 0; i < Definition.DirCount; i++)
                _attraction[i] /= max;
        }
    }

    private void ComputeRepulsion(Vector2 v)
    {
        for (int i = 0; i < Definition.DirCount; i++)
            _repulsion[i] = 0f;

        Vector2 towards = v.Normalized();

        float max = 0f;
        for (int i = 0; i < Definition.DirCount; i++)
        {
            // Vector2 t = GlobalPosition + (_dirs[i] * (Velocity + AgentRadius));
            // if (GameManager.Raycast(GlobalPosition, Target, 1 << 0))
            // {
            //     _repulsion[i] = 1f;
            //     max = Mathf.Max(max, _repulsion[i]);
            //     continue;
            // }

            // TODO(calco): Replace this with GameManagerr.CircleCast.
            _shapeCast.TargetPosition = _dirs[i] * Velocity;
            _shapeCast.Shape.Set("radius", AgentRadius);
            _shapeCast.GlobalPosition = GlobalPosition + (_dirs[i] * AgentRadius * 0.5f);
            _shapeCast.ForceUpdateTransform();
            _shapeCast.ForceShapecastUpdate();
            var b = _shapeCast.IsColliding();

            if (b)
            {
                _attraction[i] = 0f;
                _repulsion[i] = 1f;
                continue;
            }

            float dot = _dirs[i].Dot(towards);
            float weight = Definition.RepulsionShapingFunction(dot);

            _repulsion[i] = Mathf.Max(weight, _repulsion[i]);
            max = Mathf.Max(max, _repulsion[i]);
        }

        if (!Calc.FloatEquals(max, 0f))
        {
            for (int i = 0; i < Definition.DirCount; i++)
                _repulsion[i] = -(_repulsion[i] / max);
        }
    }

    #endregion

    private int ChooseDirection()
    {
        int[] sortedIndices = new int[Definition.DirCount];
        for (int i = 0; i < Definition.DirCount; i++)
            sortedIndices[i] = i;
        Array.Sort(sortedIndices, (a, b) => Definition.DirSortingFunction.Invoke(this, a, b));

        List<int> checks = new List<int> { sortedIndices[0] };
        int count = Mathf.Max(Definition.DirCount / 4, 1);
        for (int i = 1; i < count; i++)
        {
            // TODO(calco): Why did I code it like this??
            if (GetWeight(sortedIndices[i]) < 0f)
                break;

            checks.Add(sortedIndices[i]);
        }

        int idx = GD.RandRange(0, checks.Count - 1);
        LastSteeringDirectionIndex = checks[idx < 0 ? 0 : idx];
        return LastSteeringDirectionIndex;
    }
}