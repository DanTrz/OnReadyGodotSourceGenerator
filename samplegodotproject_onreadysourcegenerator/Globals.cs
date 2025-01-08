using Godot;
using System;

public partial class Globals : Node
{
	public static Globals Instance { get; private set; }

    public string MyGlobalVariable = "Hello World from Globals";

    public override void _Ready()
	{
        Instance = this;
    }

}
