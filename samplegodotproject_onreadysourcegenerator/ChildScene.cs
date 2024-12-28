using Godot;
using System;

public partial class ChildScene : Control
{

    [OnReady("LabelFromChildScene")] private Label _myLabelFromChildScene;

    //public override void _Notification(int what) => this.OnReady(this);

    [OnReady("..")] private Node? _myParent; //Using.. = Parent Node


    public override void _Ready()
    {
        GD.PrintT("Node _Ready() called: " + this.Name);
        _myLabelFromChildScene.Text = "Initiated via OnReady - Working - Label from ChildScene";

        if (_myParent != null)
        {
            GD.PrintT("Parent Node from ", this.Name.ToString(), "Parent Name =", _myParent.Name);
        }



    }
}
