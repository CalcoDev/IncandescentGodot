using System;
using System.Collections.Generic;
using Godot;
using Incandescent.Managers;
using Incandescent.Utils;

namespace Incandescent.Components.Steering;

public partial class SteeringBehaviourComponent : Node2D
{
    [Export] public float AgentRadius { get; set; } = 10f;

    public SteeringBehaviourDefinition Definition { get; set; }

    public Vector2 Target { get; set; }
    public float Velocity { get; set; }

    public Vector2 LastSteeringDirection { get; private set; }
    public int LastSteeringDirectionIndex { get; private set; }

    private Vector2[] _dirs;
    private float[] _attraction;
    private float[] _repulsion;

    public void Initialize(SteeringBehaviourDefinition definition)
    {
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

    public override void _Process(double delta)
    {
        if (GameManager.Debug)
            QueueRedraw();
    }

    public override void _Draw()
    {
        if (!GameManager.Debug)
            return;

        const float Unit = 20f;

        for (int i = 0; i < Definition.DirCount; i++)
        {
            float weight = _attraction[i] + _repulsion[i];
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

    public Vector2 GetSteeringDirection(Vector2 target, float velocity)
    {
        Target = target;
        Velocity = velocity;

        ComputeAttraction();
        ComputeRepulsion();

        int idx = ChooseDirection();
        LastSteeringDirection = _dirs[idx];
        return LastSteeringDirection;
    }

    #region Computing

    private void ComputeAttraction()
    {
        for (int i = 0; i < Definition.DirCount; i++)
            _attraction[i] = 0f;

        Vector2 towards = (Target - GlobalPosition).Normalized();

        float max = 0f;
        for (int i = 0; i < Definition.DirCount; i++)
        {
            /* (_velocity * GameManager.PhysicsDelta) */
            Vector2 t = GlobalPosition + (_dirs[i] * (Velocity + AgentRadius));
            if (GameManager.Raycast(GlobalPosition, Target, 1 << 0))
            {
                // Direction is obstructed.
                _attraction[i] = 0f;
                continue;
            }

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

    private void ComputeRepulsion()
    {
        for (int i = 0; i < Definition.DirCount; i++)
            _repulsion[i] = 0f;

        Vector2 towards = (Target - GlobalPosition).Normalized();

        float max = 0f;
        for (int i = 0; i < Definition.DirCount; i++)
        {
            /* (_velocity * GameManager.PhysicsDelta) */
            Vector2 t = GlobalPosition + (_dirs[i] * (Velocity + AgentRadius));
            if (GameManager.Raycast(GlobalPosition, Target, 1 << 0))
            {
                // Direction is obstructed.
                _repulsion[i] = 0f;
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

        List<int> checks = new List<int>();
        int count = Mathf.Max(Definition.DirCount / 4, 1);
        for (int i = 0; i < count; i++)
        {
            // TODO(calco): Why did I code it like this??
            if (_attraction[sortedIndices[i]] < 0f)
                break;

            checks.Add(sortedIndices[i]);
        }

        LastSteeringDirectionIndex = checks[GD.RandRange(0, checks.Count - 1)];
        return LastSteeringDirectionIndex;
    }
}