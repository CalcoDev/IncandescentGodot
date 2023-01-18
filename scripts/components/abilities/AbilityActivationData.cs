using Godot;
using Incandescent.Components.Physics;

namespace Incandescent.Components.Abilities;

// TODO(calco): Should probbaly have some sort of inhertiance / composition absed structure for this, in order to not waste memory.
/// <summary>
/// A data class that contains all the information needed to activate an ability.
/// </summary>
public class AbilityActivationData
{
    public ActorComponent Sender { get; }
    public VelocityComponent SenderVelocity { get; }
    public Vector2 Direction { get; }

    public AbilityActivationData(ActorComponent sender, VelocityComponent senderVelocity, Vector2 direction)
    {
        Sender = sender;
        SenderVelocity = senderVelocity;
        Direction = direction;
    }
}