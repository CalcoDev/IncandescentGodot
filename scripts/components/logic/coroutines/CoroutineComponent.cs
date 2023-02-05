using System;
using System.Collections;
using System.Collections.Generic;
using Godot;
using Incandescent.Utils;

namespace Incandescent.Components.Logic.Coroutines;

// TODO(calco): Make this more customizable
public partial class CoroutineComponent : Node
{
    [Export] public bool Finished { get; private set; } = false;
    [Export] public bool RemoveOnCompletion { get; set; } = true;
    [Export] public bool UpdateSelf { get; set; } = false;

    private Optional<float> _waitTime;
    private Optional<uint> _waitFrames;
    private Optional<Func<bool>> _waitForCondition;

    [Signal]
    public delegate void OnFinishedEventHandler();

    // TODO(calco): Convert this to a singal. Sadly, they don't support interface sharing and idunno if it's worth making this a resource.
    public Action<IYieldable> OnYielded;

    private Stack<IEnumerator> _enumerators;

    public static CoroutineComponent Create(Node parent, IEnumerator function,
        bool removeOnComplete = true, bool updateSelf = false, string name = "")
    {
        CoroutineComponent c = new CoroutineComponent();
        c.Init(function, removeOnComplete, updateSelf, name);

        parent.AddChild(c);
        return c;
    }

    public void Init(IEnumerator function, bool removeOnComplete = true, bool updateSelf = false, string name = "")
    {
        GD.Print("CoroutineComponent: Initializing coroutine ", name);

        _enumerators = new Stack<IEnumerator>();

        if (function != null)
        {
            _enumerators.Push(function);

            if (name != "")
                Name = name;
            Name = nameof(function);
        }

        RemoveOnCompletion = removeOnComplete;
        UpdateSelf = updateSelf;

        _waitTime = new Optional<float>(false);
        _waitFrames = new Optional<uint>(false);
        _waitForCondition = new Optional<Func<bool>>(false);

        Finished = false;
        OnYielded ??= null;
    }

    public override void _Process(double delta)
    {
        if (UpdateSelf)
            Update((float)delta);
    }

    public void Update(float delta)
    {
        if (_waitTime.HasValue && _waitTime.Value > 0)
        {
            _waitTime.Value -= delta;
            return;
        }
        if (_waitFrames.HasValue && _waitFrames.Value > 0)
        {
            _waitFrames.Value -= 1;
            return;
        }
        if (_waitForCondition.HasValue && !_waitForCondition.Value.Invoke())
            return;

        if (_enumerators.Count == 0)
        {
            Finish();
            return;
        }
        Finished = false;

        IEnumerator now = _enumerators.Peek();
        if (now.MoveNext())
        {
            if (now.Current is not IYieldable yieldable)
                return;

            if (yieldable is WaitForSeconds wfs)
                _waitTime.Value = wfs.Time;
            else if (yieldable is WaitForFrames wff)
                _waitFrames.Value = wff.FrameCount;
            else if (yieldable is WaitForCondition wfc)
                _waitForCondition.Value = wfc.Condition;

            // EmitSignal(SignalName.OnYield, yieldable);
            OnYielded?.Invoke(yieldable);
        }
        else
        {
            _enumerators.Pop();
            if (_enumerators.Count == 0)
                Finish();
        }
    }

    public void Finish()
    {
        Finished = true;

        // EmitSignal(SignalName.OnYield);
        OnYielded?.Invoke(null);
        EmitSignal(SignalName.OnFinished);

        if (RemoveOnCompletion)
            QueueFree();
    }

    public void Cancel()
    {
        _enumerators.Clear();
        Finished = true;

        _waitTime.Clear();
        _waitFrames.Clear();
        _waitForCondition.Clear();
    }

    public void Replace(IEnumerator function)
    {
        Finished = false;

        _waitTime.Clear();
        _waitFrames.Clear();
        _waitForCondition.Clear();

        _enumerators.Clear();
        _enumerators.Push(function);
    }
}