using System;
using System.Collections;
using Godot;
using Incandescent.Components.Logic.Coroutines;

namespace Incandescent.Components.Logic;

public partial class StateMachineComponent : Node
{
    [Export] public bool UpdateSelf { get; set; } = false;

    [ExportGroup("Debug")]
    [Export] private int _state;
    [Export] private int _previousState;

    public int State => _state;

    private int _stateCount;

    private Action[] _enters;
    private Func<int>[] _updates;
    private Action[] _exits;

    private Func<IEnumerator>[] _coroutines;
    private CoroutineComponent _currentCoroutine;

    public void Init(int maxStates, int defaultState)
    {
        _stateCount = maxStates;

        _enters = new Action[maxStates];
        _updates = new Func<int>[maxStates];
        _exits = new Action[maxStates];

        _coroutines = new Func<IEnumerator>[maxStates];
        _currentCoroutine = CoroutineComponent.Create(this, null,
            false, false, "StateMachineCoroutine");

        _previousState = _state = defaultState;
    }

    public override void _Process(double delta)
    {
        if (UpdateSelf)
        {
            int newState = Update();
            SetState(newState);
        }

        _currentCoroutine?.Update((float)delta);
    }

    public void SetState(int state)
    {
        if (_state == state)
            return;

        if (state < 0 || state >= _stateCount)
            throw new ArgumentOutOfRangeException(nameof(state), "StateMachineComponent: State out of range.");

        _previousState = _state;
        _state = state;

        if (_previousState != -1 && _exits[_previousState] != null)
            _exits[_previousState].Invoke();

        _enters[_state]?.Invoke();

        if (_coroutines[_state] != null)
        {
            if (!_currentCoroutine.Finished)
                _currentCoroutine.Cancel();

            _currentCoroutine.Replace(_coroutines[_state].Invoke());
        }
    }

    public int Update()
    {
        if (_updates[_state] != null)
            return _updates[_state].Invoke();

        return _state;
    }

    public void SetCallbacks(int index, Func<int> update, Action enter = null, Action exit = null, Func<IEnumerator> coroutine = null)
    {
        _enters[index] = enter;
        _updates[index] = update;
        _exits[index] = exit;

        _coroutines[index] = coroutine;
    }
}