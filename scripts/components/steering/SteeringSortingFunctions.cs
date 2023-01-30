using Incandescent.Utils;

namespace Incandescent.Components.Steering;

public static class SteeringSortingFunctions
{
    public static int ClosestHighestWeight(SteeringBehaviourComponent steering, int a, int b)
    {
        float wA = steering.GetWeight(a);
        float wB = steering.GetWeight(b);

        // TODO(calco): Play around with EPSILON.
        if (Calc.FloatEquals(wA, wB, 0.05f))
        {
            float dA = (steering.GlobalPosition + steering.GetDir(a)).DistanceTo(steering.Target);
            float dB = (steering.GlobalPosition + steering.GetDir(a)).DistanceTo(steering.Target);

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