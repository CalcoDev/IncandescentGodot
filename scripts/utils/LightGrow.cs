using System;
using Godot;

namespace Incandescent.Utils;

public partial class LightGrow : PointLight2D
{
    private float _a;
    private float _init;

    public override void _Ready()
    {
        _init = TextureScale;
    }

    public override void _Process(double delta)
    {
        _a = Mathf.Sin(Time.GetTicksMsec() * 0.001f) * 0.05f;

        TextureScale = _init + _a;
        // RotationDegrees += 0.5f;
    }
}
