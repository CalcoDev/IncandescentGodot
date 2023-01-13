using System.Collections.Generic;
using Eyes.Components;
using Eyes.Components.Physics;
using Godot;

namespace Eyes.Managers;

public partial class LevelManager : Node
{
    public static LevelManager Instance { get; private set; }

    public List<ActorComponent> Actors { get; } = new List<ActorComponent>();
    public List<SolidComponent> Solids { get; } = new List<SolidComponent>();

    public override void _EnterTree()
    {
        Instance = this;
    }

    #region Level Object Helpers
    // These are separate methods as maybe we want to do something else in the future,
    // and refactoring would become a pain.
    public void AddSolid(SolidComponent solid)
    {
        Solids.Add(solid);
    }

    public void RemoveSolid(SolidComponent solid)
    {
        Solids.Remove(solid);
    }

    public void AddActor(ActorComponent actor)
    {
        Actors.Add(actor);
    }

    public void RemoveActor(ActorComponent actor)
    {
        Actors.Remove(actor);
    }
    #endregion
}