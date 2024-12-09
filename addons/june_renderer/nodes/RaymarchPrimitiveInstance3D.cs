using System.Collections.Generic;
using System.Linq;
using Godot;

[Tool]
[GlobalClass]
[Icon("res://addons/june_renderer/icons/RayCast3D.svg")]
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
    [Export] private Aabb boundingBox;

    private MeshInstance3D previewMeshInstance;
    private VisibleOnScreenNotifier3D screenNotifier;

    public RaymarchPrimitiveInstance3D()
    {
        //if (!Engine.IsEditorHint()) return;
        previewMeshInstance = new MeshInstance3D();
        AddChild(previewMeshInstance, false, InternalMode.Front);
    }

    public override void _Ready()
    {
        if (Engine.IsEditorHint() || _primitive == null) return;

        screenNotifier = new VisibleOnScreenNotifier3D();
        AddChild(screenNotifier);

        screenNotifier.Aabb = boundingBox;

        screenNotifier.ScreenEntered += Register;
        screenNotifier.ScreenExited += Deregister;
    }

    public override void _ExitTree()
    {
        if (Engine.IsEditorHint() || _primitive == null) return;

        RaymarchRenderer.Instance.DeregisterPrimitive(this);
    }

    private void Register()
    {
        RaymarchRenderer.Instance.RegisterPrimitive(this);
    }

    private void Deregister()
    {
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