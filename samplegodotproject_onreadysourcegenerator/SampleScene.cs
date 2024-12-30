using Godot;
using System;

public partial class SampleScene : Node2D
{
	[OnReady("%LabelUniqueName")] private Label _UniqueNameLabel;
    [OnReady("CanvasLayer/PanelContainer/VBoxContainer/LabelNotUniqueName")] private Label _NotUniqueNameLabel;

    [OnReady("..")] private Node _myParent; //Using.. = get_root() in this context

    //TODO - use the $ as a special simbol to retrive all initializer after = equal signal
    [OnReady("$")] private PackedScene _myChildScene = GD.Load<PackedScene>("res://ChildScene.tscn");

    //[OnReady("")] private Texture _myChildSceneWithoutSpecialSymbol = GD.Load<Texture>("res://ChildScene.tscn"); //this works

    private PackedScene _myChildSceneWithoutOnReady = GD.Load<PackedScene>("res://ChildScene.tscn"); //this works - No Need for ONREADY

    [OnReady("$")] private Texture2D texture = GD.Load<Texture2D>("res://icon.svg"); //this works
    private Texture2D texture2 = GD.Load<Texture2D>("res://icon.svg"); //this works - No Need for ONREADY

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


        if (_myChildScene != null)
        {
            GD.PrintT("Child PackedScene from: ", this.Name.ToString(), "Scene Loaded from path =", _myChildScene.ResourcePath.ToString());
        }

        if (_myChildSceneWithoutOnReady != null)
        {
            GD.PrintT("Child _myChildSceneWithoutOnReady from: ", this.Name.ToString(), "Loaded from path =", _myChildScene.ResourcePath.ToString());
        }

        if (texture != null)
        {
            GD.PrintT("texture from: ", this.Name.ToString(), "Loaded from path =", texture.ResourcePath.ToString());
        }


        if (texture2 != null)
        {
            GD.PrintT("texture2 from: ", this.Name.ToString(), "Loaded from path =", texture.ResourcePath.ToString());
        }



    }

}
