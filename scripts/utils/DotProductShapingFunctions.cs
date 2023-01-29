using Godot;

namespace Incandescent.Utils;

public static class DotProductShapingFunctions
{
    public static float Linear(float x)
    {
        return x;
    }

    public static float Normalized(float x)
    {
        return Mathf.Clamp((x + 1f) * 0.5f, 0f, 1f);
    }

    public static float Cosine(float x)
    {
        return Mathf.Cos(x * Mathf.Pi * 0.5f);
    }

    public static float Sine(float x)
    {
        return Mathf.Sin(x * Mathf.Pi * 0.5f);
    }

    public static float Avoid(float x)
    {
        return 1f - Mathf.Abs(x - 0.65f);
    }

    // Create a function that will heavily favour moving sideways
    public static float Sideways(float x)
    {
        if (x >= -0.259f && x <= 0.39f)
            return 1f;

        return 0f;
    }
}