using System;
using System.Collections.Generic;
using Godot;
using Incandescent.Managers;
using Incandescent.Utils;

namespace Incandescent.Components.Steering;

public partial class SteeringBehaviourComponent : Node2D
{
    [Export] public float AgentRadius = 10f;

    public readonly int DirCount = 24;
    public Func<float, float> AttractionShapingFunction = DotProductShapingFunctions.Sideways;
    public Func<float, float> RepulsionShapingFunction = DotProductShapingFunctions.Null;

    public Vector2 Target { get; set; }
    public float Velocity { get; set; }

    public Vector2 LastSteeringDirection { get; private set; }
    public int LastSteeringDirectionIndex { get; private set; }

    private readonly Vector2[] _dirs;

    private readonly float[] _attraction;
    private readonly float[] _repulsion;

    public SteeringBehaviourComponent()
    {
        _dirs = new Vector2[DirCount];
        _attraction = new float[DirCount];
        _repulsion = new float[DirCount];

        for (int i = 0; i < DirCount; i++)
        {
            float angle = Mathf.Pi * 2 * i / DirCount;
            _dirs[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            _attraction[i] = 0;
            _repulsion[i] = 0;
        }

        GD.Print("SteeringBehaviourComponent");
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

        for (int i = 0; i < DirCount; i++)
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

    public Vector2 GetSteeringDirection()
    {
        ComputeAttraction();
        ComputeRepulsion();

        int idx = GetDirection();
        LastSteeringDirection = _dirs[idx];
        return LastSteeringDirection;
    }

    public float GetWeight(int dirIdx)
    {
        return _attraction[dirIdx] + _repulsion[dirIdx];
    }

    public float GetWeight(Vector2 dir)
    {
        return GetWeight(GetDirectionIndex(dir));
    }

    public int GetDirectionIndex(Vector2 dir)
    {
        float angle = Mathf.Atan2(dir.y, dir.x);
        if (angle < 0f)
            angle += Mathf.Pi * 2f;

        return (int)Math.Round(angle * DirCount / (Mathf.Pi * 2f));
    }

    private void ComputeAttraction()
    {
        for (int i = 0; i < DirCount; i++)
            _attraction[i] = 0f;

        Vector2 towards = (Target - GlobalPosition).Normalized();

        float max = 0f;
        for (int i = 0; i < DirCount; i++)
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
            float weight = AttractionShapingFunction(dot);

            _attraction[i] = Mathf.Max(weight, _attraction[i]);
            max = Mathf.Max(max, _attraction[i]);
        }

        if (!Calc.FloatEquals(max, 0f))
        {
            for (int i = 0; i < DirCount; i++)
                _attraction[i] /= max;
        }
    }

    private void ComputeRepulsion()
    {
        for (int i = 0; i < DirCount; i++)
            _repulsion[i] = 0f;

        Vector2 towards = (Target - GlobalPosition).Normalized();

        float max = 0f;
        for (int i = 0; i < DirCount; i++)
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
            float weight = RepulsionShapingFunction(dot);

            _repulsion[i] = Mathf.Max(weight, _repulsion[i]);
            max = Mathf.Max(max, _repulsion[i]);
        }

        if (!Calc.FloatEquals(max, 0f))
        {
            for (int i = 0; i < DirCount; i++)
                _repulsion[i] = -(_repulsion[i] / max);
        }
    }

    private int GetDirection()
    {
        int[] sortedIndices = new int[DirCount];
        for (int i = 0; i < DirCount; i++)
            sortedIndices[i] = i;
        Array.Sort(sortedIndices, SortSteeringDirections);

        List<int> checks = new List<int>();
        int count = Mathf.Max(DirCount / 4, 1);
        for (int i = 0; i < count; i++)
        {
            // TODO(calco): Why did I code it like this??
            // if (_interest[sortedIndices[i]] < 0f)
            if (_attraction[sortedIndices[i]] < 0f)
                break;

            checks.Add(sortedIndices[i]);
        }

        LastSteeringDirectionIndex = checks[GD.RandRange(0, checks.Count - 1)];
        return LastSteeringDirectionIndex;
    }

    private int SortSteeringDirections(int a, int b)
    {
        float wA = GetWeight(a);
        float wB = GetWeight(b);

        // TODO(calco): Play around with EPSILON.
        if (Calc.FloatEquals(wA, wB, 0.05f))
        {
            float dA = (GlobalPosition + _dirs[a]).DistanceTo(Target);
            float dB = (GlobalPosition + _dirs[b]).DistanceTo(Target);

            if (dA < dB)
                return -1;
            else if (dA > dB)
                return 1;
            else
                return 0;
        }

        if (wA > wB)
            return -1;

        return 1;
    }
}