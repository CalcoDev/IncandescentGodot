using Godot;
using Incandescent.Components.Logic;

namespace Incandescent.Components.Graphics;

// TODO(calco): Make an equivalent class for GPUParticles2D, or make a more generic base implementation.
public partial class CallbackParticlesComponent : CPUParticles2D
{
    [Signal]
    public delegate void OnParticlesFinishedEventHandler();

    public override void _Ready()
    {
        Emitting = false;
    }

    // Hack.
    public void Init(bool oneShot)
    {
        OneShot = oneShot;

        // TODO(calco): Research lifetime randomness and stuff to create *AN ACTUAL FORMULA*
        float maxLifetime = (float)Lifetime * 2f;

        CustomTimerComponent timer = CustomTimerComponent.Create(this, maxLifetime, true);
        timer.OnTimeout += () => EmitSignal(SignalName.OnParticlesFinished);
    }
}