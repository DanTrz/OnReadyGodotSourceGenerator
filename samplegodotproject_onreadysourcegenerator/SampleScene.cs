using Godot;
using System;

public partial class SampleScene : Node2D
{
	private Label _UniqueNameLabel;
    private Label _NotUniqueNameLabel;

    public override void _Ready()
	{
        _UniqueNameLabel.Text = "Initiated via OnReady - Working - Unique name Node";
        _NotUniqueNameLabel.Text = "Initiated via OnReady - Working - Not Unique name Node";

    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
