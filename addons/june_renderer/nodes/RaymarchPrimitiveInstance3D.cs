using System.Collections.Generic;
using System.Linq;
using Godot;

[Tool]
[GlobalClass]
[Icon("res://addons/may_renderer/icons/RayCast3D.svg")]
public partial class RaymarchPrimitiveInstance3D : Node3D
{
    private RaymarchPrimitive _primitive;
    [Export] public RaymarchPrimitive Primitive
    {
        get{return _primitive;}
        set {
            _primitive = value;
            if (value == null) previewMeshInstance.Mesh = null;
            else if (previewMeshInstance != null) previewMeshInstance.Mesh = value.PreviewMesh;
        }
    }

    private MeshInstance3D previewMeshInstance;

    public RaymarchPrimitiveInstance3D()
    {
        if (!Engine.IsEditorHint()) return;
        previewMeshInstance = new MeshInstance3D();
        AddChild(previewMeshInstance, false, InternalMode.Front);
    }

    public override void _Ready()
    {
        if (Engine.IsEditorHint() || _primitive == null) return;

        RaymarchRenderer.Instance.RegisterPrimitive(this);
    }

    public override void _ExitTree()
    {
        if (Engine.IsEditorHint() || _primitive == null) return;

        RaymarchRenderer.Instance.DeregisterPrimitive(this);
    }

    public int GetID()
    {
        return _primitive.MeshID;
    }

    public List<float> GetData()
    {
        List<float> data = new List<float>{Position.X, Position.Y, Position.Z};
        data.AddRange(Primitive.GetParameters());
        return data;
    }
}