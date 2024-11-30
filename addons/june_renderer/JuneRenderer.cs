#if TOOLS
using Godot;
using System;

[Tool]
public partial class JuneRenderer : EditorPlugin
{
	public override void _EnterTree()
	{
		AddAutoloadSingleton("RaymarchRenderer", "res://addons/june_renderer/nodes/autoload/RaymarchRenderer.cs");
	}

	public override void _ExitTree()
	{
		RemoveAutoloadSingleton("RaymarchRenderer");
	}
}
#endif
