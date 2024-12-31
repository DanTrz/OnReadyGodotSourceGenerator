using Godot;
using System;
using Godot.Collections;

public partial class RootScene : Node
{
	[Export(PropertyHint.File, "*.tscn")]
	public string MainScene;

	public override void _Ready()
	{
		Callable.From(() => GetTree().ChangeSceneToFile(MainScene)).CallDeferred();
	}
}