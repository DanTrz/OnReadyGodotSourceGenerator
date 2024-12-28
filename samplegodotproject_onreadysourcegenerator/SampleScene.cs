using Godot;
using System;

public partial class SampleScene : Node2D
{
	[OnReady("%LabelUniqueName")] private Label _UniqueNameLabel;
    [OnReady("CanvasLayer/PanelContainer/VBoxContainer/LabelNotUniqueName")] private Label _NotUniqueNameLabel;

    [OnReady("..")] private Node _myParent; //Using.. = get_root() in this context

    //public override void _Notification(int what) => this.OnReady(this);

    public override void _Ready()
    {
        //_myParent = GetParent<Node>();
        GD.PrintT("Node _Ready() called: " + this.Name);
        _UniqueNameLabel.Text = "Initiated via OnReady - Working - Unique name Node";
        _NotUniqueNameLabel.Text = "Initiated via OnReady - Working - Not Unique name Node";

        if (_myParent != null)
        {
            GD.PrintT("Parent Node from " , this.Name.ToString() , "Parent Name =" , _myParent.Name);
        }
          

    }

}
