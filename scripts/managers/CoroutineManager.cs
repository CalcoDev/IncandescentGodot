using System.Collections;
using System.Collections.Generic;
using Eyes.Components.Logic;
using Godot;

namespace Eyes.Managers;

public partial class CoroutineManager : Node
{
    private static CoroutineManager Instance { get; set; }

    private static readonly List<CoroutineComponent> ActiveCoroutines = new List<CoroutineComponent>();

    public override void _Ready()
    {
        Instance = this;
    }

    public static CoroutineComponent StartCoroutine(IEnumerator function, bool removeOnComplete = true,
        bool updateSelf = false, string name = "")
    {
        CoroutineComponent c = CoroutineComponent.Create(Instance, function, removeOnComplete, updateSelf, name);
        ActiveCoroutines.Add(c);
        return c;
    }

    public static void StopCoroutine(CoroutineComponent coroutine)
    {
        coroutine.Finish();
        ActiveCoroutines.Remove(coroutine);
    }
}