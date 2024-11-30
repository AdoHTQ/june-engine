using Godot;
using System;

public partial class Spawner : Node3D
{
    public override void _Ready()
    {
        Timer timer = new Timer();
		AddChild(timer);
		timer.WaitTime = 0.05f;
		timer.Timeout += Spawn;
		timer.Start();
    }

	private void Spawn()
	{
		RandomNumberGenerator rng = new RandomNumberGenerator();
		rng.Randomize();

		RaymarchPrimitiveInstance3D primitive = new();
		primitive.Position = new(rng.RandfRange(-4, 4), rng.RandfRange(-2, 2), 0.0f);
		primitive.Primitive = new RaymarchSphere();
		(primitive.Primitive as RaymarchSphere).Radius = rng.RandfRange(0.2f, 0.7f);
		AddChild(primitive);
	}
}
