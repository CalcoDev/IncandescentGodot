using System;
using Godot;
using Incandescent.Utils;

namespace Incandescent.Components.Steering;

public partial class SteeringBehaviourDefinition : RefCounted
{
    public readonly int DirCount = 24;
    public Func<float, float> AttractionShapingFunction { get; set; }
    public Func<float, float> RepulsionShapingFunction { get; set; }
    public Func<SteeringBehaviourComponent, int, int, int> DirSortingFunction { get; set; }

    public SteeringBehaviourDefinition()
    {
    }

    public SteeringBehaviourDefinition(int dirCount)
    {
        DirCount = dirCount;
    }

    public SteeringBehaviourDefinition(int dirCount,
        Func<float, float> attractionShapingFunction,
        Func<float, float> repulsionShapingFunction,
        Func<SteeringBehaviourComponent, int, int, int> dirSortingFunction
    )
    {
        DirCount = dirCount;
        AttractionShapingFunction = attractionShapingFunction;
        RepulsionShapingFunction = repulsionShapingFunction;
        DirSortingFunction = dirSortingFunction;
    }
}