using Godot;

namespace Incandescent.Components;

public partial class VelocityComponent : Node
{
    [Export] private Vector2 _velocity;

    public float X => _velocity.x;
    public float Y => _velocity.y;

    public static implicit operator Vector2(VelocityComponent velocity)
    {
        return velocity;
    }

    public void ApproachX(float target, float maxDelta)
    {
        _velocity.x = _velocity.x < target ? Mathf.Min(_velocity.x + maxDelta, target) : Mathf.Max(_velocity.x - maxDelta, target);
    }

    public void ApproachY(float target, float maxDelta)
    {
        _velocity.y = _velocity.y < target ? Mathf.Min(_velocity.y + maxDelta, target) : Mathf.Max(_velocity.y - maxDelta, target);
    }

    public void SetX(float value)
    {
        _velocity.x = value;
    }

    public void SetY(float value)
    {
        _velocity.y = value;
    }

    public void Set(float x, float y)
    {
        _velocity.x = x;
        _velocity.y = y;
    }

    public void Set(Vector2 v)
    {
        _velocity = v;
    }

    public void AddX(float addon)
    {
        _velocity.x += addon;
    }

    public void AddY(float addon)
    {
        _velocity.y += addon;
    }

    public void MultiplyX(float multiplier)
    {
        _velocity.x *= multiplier;
    }

    public void MultiplyY(float multiplier)
    {
        _velocity.y *= multiplier;
    }
}