using System.Collections.Generic;
using System.Linq;
using Godot;

[Tool]
[GlobalClass]
[Icon("res://addons/june_renderer/icons/Camera3D.svg")]
public partial class RaymarchCamera3D : Camera3D
{
    public override void _Ready()
    {
        RaymarchRenderer.Instance.SetFov(Fov);
    }

    public override void _Process(double delta)
    {
        RaymarchRenderer.Instance.cameraPos = GlobalPosition;
        RaymarchRenderer.Instance.cameraDir = new(GlobalBasis);
    }
}
