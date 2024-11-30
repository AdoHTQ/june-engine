using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Godot;
using Godot.Collections;

[Tool]
[GlobalClass]
[Icon("res://addons/may_renderer/icons/BoxMesh.svg")]
public partial class RaymarchCube : RaymarchPrimitive
{
    [Export] public float Size 
    {
        get
        {
            if (PreviewMesh as BoxMesh != null) return (PreviewMesh as BoxMesh).Size.X;
            return -1;
        }
        set
        {
            if (PreviewMesh as BoxMesh == null) return;
            (PreviewMesh as BoxMesh).Size = new(value, value, value);
        } 
    }

    public override List<float> GetParameters()
    {
        return new List<float> {Size / 2f};
    }

    public RaymarchCube() {
        Size = 1f;
        PreviewMesh = new BoxMesh();
        MeshID = 1;
    }
}