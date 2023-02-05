namespace Incandescent.Components.Logic.Coroutines;

public class WaitForSeconds : IYieldable
{
    public float Time { get; }

    public WaitForSeconds(float time)
    {
        Time = time;
    }
}