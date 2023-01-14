using Eyes.Utils;
using Godot;

namespace Eyes.Extensions;

public static class Vector2Extensions
{
    public static Vector2i FloorButCeilIfClose(this Vector2 v, float epsilon = 0.05f)
    {
        return new Vector2i(
            Calc.FloorToIntButCeilIfClose(v.x, epsilon),
            Calc.FloorToIntButCeilIfClose(v.y, epsilon)
        );
    }
}