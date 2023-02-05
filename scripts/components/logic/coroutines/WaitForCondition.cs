using System;

namespace Incandescent.Components.Logic.Coroutines;

public class WaitForCondition : IYieldable
{
    public Func<bool> Condition { get; }

    public WaitForCondition(Func<bool> condition)
    {
        Condition = condition;
    }
}