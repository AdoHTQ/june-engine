using System.Collections.Generic;
using Godot;

[Tool]
[GlobalClass]
[Icon("res://addons/june_renderer/icons/SphereMesh.svg")]
public partial class RaymarchSphere : RaymarchPrimitive
{
    [Export] public float Radius
    {
        get
        {
            if (PreviewMesh as SphereMesh != null) return (PreviewMesh as SphereMesh).Radius;
            return -1;
        }
        set
        {
            if (PreviewMesh as SphereMesh == null) return;
            (PreviewMesh as SphereMesh).Radius = value;
            (PreviewMesh as SphereMesh).Height = 2 * value;
        }
    }
    
    public override List<float> GetParameters()
    {
        return new List<float> {Radius};
    }

    public RaymarchSphere() {
        Radius = 1f;
        PreviewMesh = new SphereMesh();
        MeshID = 0;
    }
}