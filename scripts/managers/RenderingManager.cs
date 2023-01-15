using Godot;
using Godot.Collections;
using Incandescent.Components.Logic;

namespace Incandescent.Managers;

public partial class RenderingManager : Node
{
    public class PixelatedRenderingLayer
    {
        public SubViewport SubViewport { get; set; }
        public TextureRect TextureRect { get; set; }
    }

    public static RenderingManager Instance { get; private set; }

    private readonly System.Collections.Generic.Dictionary<float, PixelatedRenderingLayer> _layers = new();

    public override void _Ready()
    {
        Instance = this;
        SetUpViewports();
    }

    /// <summary>
    /// Adds [<paramref name="node"/>] to the layer with the same ZIndex as itself. If the layer does not exist, it will be created.
    /// </summary>
    /// <param name="node">The node to add</param>
    public void TryAddNodeToLayer(Node2D node)
    {
        if (!_layers.ContainsKey(node.ZIndex))
            CreateLayer(node.ZIndex);

        node.GetParent().RemoveChild(node);
        _layers[node.ZIndex].SubViewport.AddChild(node);
    }

    public void SetUpViewports()
    {
        foreach (Node node in GetTree().GetNodesInGroup("pixelated"))
        {
            if (node is not Node2D node2d)
                continue;

            if (NodeIsEligibleForFollower(node2d))
                AddFollowerToNode(node2d);

            CallDeferred(nameof(TryAddNodeToLayer), node2d);
        }
    }

    private void CreateLayer(int zIndex)
    {
        SubViewport subViewport = new SubViewport
        {
            Size = new Vector2i(320, 180),
            TransparentBg = true,
            Disable3d = true,
            Snap2dTransformsToPixel = true,
            Snap2dVerticesToPixel = true
        };

        TextureRect textureRect = new TextureRect
        {
            Texture = subViewport.GetTexture(),
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            ZAsRelative = false,
            ZIndex = zIndex
        };

        PixelatedRenderingLayer layer = new PixelatedRenderingLayer()
        {
            SubViewport = subViewport,
            TextureRect = textureRect
        };
        _layers.Add(zIndex, layer);

        Node layerRoot = new Node();
        layerRoot.Name = $"Layer {zIndex}";

        layerRoot.AddChild(subViewport);
        layerRoot.AddChild(textureRect);
        AddChild(layerRoot);
    }

    private bool NodeIsEligibleForFollower(Node2D node)
    {
        return node.GetParent() is Node2D && node.GetParent().Name != "root" && !node.GetParent().Name.ToString().StartsWith("Scene");
    }

    private void AddFollowerToNode(Node2D node)
    {
        FollowerComponent followerComp = new FollowerComponent
        {
            Follower = node,
            Followee = node.GetParent<Node2D>(),
            UpdateSelf = true
        };

        node.AddChild(followerComp);
    }
}