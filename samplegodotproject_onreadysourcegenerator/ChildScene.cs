using Godot;
using System;

public partial class ChildScene : Control
{

    [OnReady("LabelFromChildScene")] private Label _myLabelFromChildScene;

    public override void _Ready()
    {
        //TODO: on ChildScenes that are added to a node on the main Scene, the AutoLoad isn't working - FIX: Create a Fake RootScene.
        //this.OnReady(this);
        GD.PrintT("Node _Ready() called: " + this.Name);
        _myLabelFromChildScene.Text = "Initiated via OnReady - Working - Label from ChildScene";
    }

}
