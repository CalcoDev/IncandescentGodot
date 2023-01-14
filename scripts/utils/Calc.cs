using Godot;

namespace Eyes.Utils;

public static class Calc
{
    public static bool SameSign(float a, float b)
    {
        return (a < 0) == (b < 0);
    }

    public static bool SameSignZero(float a, float b)
    {
        if (a == 0 || b == 0)
            return true;

        return (a < 0) == (b < 0);
    }

    public static float Approach(float current, float target, float maxDelta)
    {
        return current < target ? Mathf.Min(current + maxDelta, target) : Mathf.Max(current - maxDelta, target);
    }

    public static bool FloatEquals(float a, float b, float epsilon = 0.001f)
    {
        return Mathf.Abs(a - b) < epsilon;
    }

    public static int FloorToIntButCeilIfClose(float value, float epsilon = 0.05f)
    {
        return Mathf.FloorToInt(value) + (Mathf.Abs(value - Mathf.Ceil(value)) < epsilon ? 1 : 0);
    }
}