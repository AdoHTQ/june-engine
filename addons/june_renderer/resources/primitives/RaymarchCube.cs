using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Godot;
using Godot.Collections;

[Tool]
[GlobalClass]
[Icon("res://addons/june_renderer/icons/BoxMesh.svg")]
public partial class RaymarchCube : RaymarchPrimitive
{
    [Export] public Vector3 Size 
    {
        get
        {
            if (PreviewMesh as BoxMesh != null) return (PreviewMesh as BoxMesh).Size;
            return Vector3.Zero;
        }
        set
        {
            if (PreviewMesh as BoxMesh == null) return;
            (PreviewMesh as BoxMesh).Size = value;
        } 
    }

    public override List<float> GetParameters()
    {
        return new List<float> {Size.X / 2f, Size.Y / 2f, Size.Z / 2f};
    }

    public RaymarchCube() {
        Size = new Vector3(1f, 1f, 1f);
        PreviewMesh = new BoxMesh();
        MeshID = 1;
    }
}