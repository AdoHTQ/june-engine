using System;
using System.Collections.Generic;
using Godot;

[Tool]
[GlobalClass]
[Icon("res://addons/may_renderer/icons/MeshInstance3D.svg")]
public partial class RaymarchPrimitive : Resource
{
    public Mesh PreviewMesh {get; protected set;}
    public int MeshID {get; protected set;}
    public virtual List<float> GetParameters() { return null; }
}
