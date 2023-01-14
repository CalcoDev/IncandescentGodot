using System.Collections;
using System.Collections.Generic;
using Godot;

namespace Eyes.Components.Logic;

public partial class CoroutineComponent : Node
{
    [Export] public bool Finished { get; private set; } = false;
    [Export] public bool RemoveOnCompletion { get; set; } = true;
    [Export] public bool UpdateSelf { get; set; } = false;

    [ExportGroup("Debug")]
    [Export] private float _waitTime;

    [Signal]
    public delegate void OnFinishedEventHandler();

    [Signal]
    public delegate void OnYieldEventHandler();

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
        _waitTime = 0;
    }

    public override void _Process(double delta)
    {
        if (UpdateSelf)
            Update((float)delta);
    }

    public void Update(float delta)
    {
        if (_waitTime > 0)
        {
            _waitTime -= delta;
            return;
        }

        if (_enumerators.Count == 0)
        {
            Finish();
            return;
        }
        Finished = false;

        IEnumerator now = _enumerators.Peek();
        if (now.MoveNext())
        {
            if (now.Current is int ci)
                _waitTime = ci;
            else if (now.Current is float cf)
                _waitTime = cf;
            else if (now.Current is IEnumerator cie)
                _enumerators.Push(cie);
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

        EmitSignal(SignalName.OnYield);
        EmitSignal(SignalName.OnFinished);

        if (RemoveOnCompletion)
            QueueFree();
    }

    public void Cancel()
    {
        _enumerators.Clear();
        Finished = true;
        _waitTime = 0f;
    }

    public void Replace(IEnumerator function)
    {
        Finished = false;
        _waitTime = 0f;

        _enumerators.Clear();
        _enumerators.Push(function);
    }
}