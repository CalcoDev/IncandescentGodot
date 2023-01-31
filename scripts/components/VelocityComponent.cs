using Godot;

namespace Incandescent.Components;

public partial class VelocityComponent : Node
{
    [Export] private Vector2 _velocity;

    public Vector2 Value => _velocity;

    public float X => _velocity.x;
    public float Y => _velocity.y;

    public Vector2 GetVelocity()
    {
        return _velocity;
    }

    public void Approach(Vector2 target, float maxDelta)
    {
        ApproachX(target.x, maxDelta);
        ApproachY(target.y, maxDelta);
    }

    public void ApproachX(float target, float maxDelta)
    {
        _velocity.x = _velocity.x < target ? Mathf.Min(_velocity.x + maxDelta, target) : Mathf.Max(_velocity.x - maxDelta, target);
    }

    public void ApproachY(float target, float maxDelta)
    {
        _velocity.y = _velocity.y < target ? Mathf.Min(_velocity.y + maxDelta, target) : Mathf.Max(_velocity.y - maxDelta, target);
    }

    public void SetVelocityX(float value)
    {
        _velocity.x = value;
    }

    public void SetVelocityY(float value)
    {
        _velocity.y = value;
    }

    public void SetVelocity(float x, float y)
    {
        _velocity.x = x;
        _velocity.y = y;
    }

    public void SetVelocity(Vector2 v)
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