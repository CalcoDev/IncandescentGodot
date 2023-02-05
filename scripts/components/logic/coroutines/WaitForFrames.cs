namespace Incandescent.Components.Logic.Coroutines;

public class WaitForFrames : IYieldable
{
    public uint FrameCount { get; }

    public WaitForFrames(uint frameCount)
    {
        FrameCount = frameCount;
    }
}