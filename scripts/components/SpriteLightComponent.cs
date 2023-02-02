using System;
using Godot;
using Incandescent.Managers;

namespace Incandescent.Components;

public partial class SpriteLightComponent : CPUParticles2D
{
    [Export]
    public bool Flicker
    {
        get { return _flicker; }
        set
        {
            if (!value)
            {
                ScaleAmountMin = _defaultMinScale;
                ScaleAmountMax = _defaultMaxScale;
            }

            _flicker = value;
        }
    }

    [ExportSubgroup("Flicker Settings")]
    [Export] public float FlickerAmplitude { get; set; } = 0.1f;
    [Export] public float FlickerFrequency { get; set; } = 5f;
    [Export] public bool AlwaysPositive { get; set; } = true;

    private float _defaultMinScale;
    private float _defaultMaxScale;

    private bool _flicker;

    public override void _Ready()
    {
        _defaultMinScale = ScaleAmountMin;
        _defaultMaxScale = ScaleAmountMax;
    }

    public override void _Process(double delta)
    {
        if (Flicker)
        {
            var val = Mathf.Sin(GameManager.FrameCount * 0.001f * FlickerFrequency);
            if (AlwaysPositive)
                val = Mathf.Abs(val);

            var scale = val * FlickerAmplitude;
            ScaleAmountMin = _defaultMinScale + scale;
            ScaleAmountMax = _defaultMaxScale + scale;
        }
    }
}