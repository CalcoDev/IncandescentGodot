using System.Collections.Generic;
using Godot;
using Incandescent.Components;

namespace Incandescent.Managers;

public partial class SolidManager : Node
{
    public static SolidManager Instance { get; private set; }
    
    private readonly List<SolidComponent> _solids = new();

    public override void _EnterTree()
    {
        if (Instance != null)
        {
            GD.PrintErr("Multiple instances of SolidManager detected!");
            return;
        }

        Instance = this;
    }

    public override void _Ready()
    {
        GD.Print("SolidManager: _Ready");
    }

    public void AddSolid(SolidComponent solid)
    {
        GD.Print($"Added solid at position: {solid.GlobalPosition}");
        _solids.Add(solid);
    }
    
    public void RemoveSolid(SolidComponent solid)
    {
        _solids.Remove(solid);
    }

    public bool CheckCollisionAt(AxisAlignedBoundingBoxComponent boundingBox, Vector2i offset)
    {
        foreach (var solid in _solids)
            if (boundingBox.IntersectsRel(solid.BoundingBox, offset))
                return true;

        return false;
    }

    public List<AxisAlignedBoundingBoxComponent> GetCollisionsAt(AxisAlignedBoundingBoxComponent boundingBox, Vector2i offset)
    {
        List<AxisAlignedBoundingBoxComponent> collisions = new();

        foreach (var solid in _solids)
            if (boundingBox.IntersectsRel(solid.BoundingBox, offset))
                collisions.Add(solid.BoundingBox);

        return collisions;
    }
}