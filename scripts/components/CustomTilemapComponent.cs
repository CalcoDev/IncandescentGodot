using Incandescent.Managers;
using Godot;
using Incandescent.Components.Physics;

namespace Incandescent.Components;

// TODO(calco): This is so bad for performance. MUST FIX
// https://en.wikipedia.org/wiki/Quadtree
// https://jonathanwhiting.com/tutorial/collision/
// Handmade Hero: EP48 - 50
public partial class CustomTilemapComponent : Node
{
    [Export] private TileMap _tilemap;

    public override void _Ready()
    {
        foreach (var cellPosition in _tilemap.GetUsedCells(0))
        {
            SolidComponent solid = new SolidComponent();
            solid.GlobalPosition = _tilemap.ToGlobal(_tilemap.MapToLocal(cellPosition)) - new Vector2(4, 4);

            AABBComponent aabb = new AABBComponent();
            aabb.Size = new Vector2i(8, 8);
            solid.AABB = aabb;

            solid.AddChild(aabb);

            AddChild(solid);
        }
    }
}