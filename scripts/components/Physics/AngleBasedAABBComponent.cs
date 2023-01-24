using System.Collections.Generic;
using Godot;
using Godot.Collections;
using Incandescent.Components.Physics;

namespace Incandescent.Components;

public partial class AngleBasedAABBComponent : Node2D
{
    [Export] public bool UpdateSelf { get; set; }

    [Export] private float _partitions;
    private readonly List<AABBComponent> _aabbs = new List<AABBComponent>();

    [Signal]
    public delegate void OnAABBChangedEventHandler(AABBComponent aabb);

    private AABBComponent _currentAABB;

    public AABBComponent GetAABB(float angle)
    {
        int index = Mathf.FloorToInt(angle / (Mathf.Pi * 2) * _partitions);
        return _aabbs[index];
    }

    public void Update(float angle)
    {
        if (angle < 0)
            angle += Mathf.Pi * 2;

        var curr = GetAABB(angle);
        if (curr != _currentAABB)
        {
            _currentAABB = curr;
            EmitSignal(SignalName.OnAABBChanged, _currentAABB);

            _aabbs.ForEach(aabb => aabb.Visible = aabb == _currentAABB);
        }
    }

    public override void _Ready()
    {
        foreach (Node child in GetChildren())
        {
            if (child is AABBComponent aabb)
                _aabbs.Add(aabb);
        }
    }

    public override void _Process(double delta)
    {
        if (UpdateSelf)
            Update(Rotation);
    }
}