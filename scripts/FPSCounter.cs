using Godot;
using System;
using System.Linq;

public partial class FPSCounter : Label
{
    const int sampleLength = 10;
    private double[] deltas;
    private int index = 0;

    public override void _Ready()
    {
        deltas = new double[sampleLength];
    }
    public override void _Process(double delta)
    {
        deltas[index] = delta;
        index++;
        index %= sampleLength;
        
        Text = "" + (int)(1.0 / deltas.Average());
    }
}
