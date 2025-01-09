using Godot;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

public partial class ChildScene : Control
{

    [OnReady("LabelFromChildScene")] private Label _myLabelFromChildScene;
    [OnReady("%Sprite2D")] private Sprite2D _mySprite2D;

    //public override void _Notification(int what) => this.OnReady(this);

    [OnReady("..")] private Node _myParent; //This will work

    //[OnReady("/root/SampleScene")] private SampleScene _myOwner; //This will work
    //[OnReady("$")] private SampleScene _myOwner = GetOwner<SampleScene>(); 

    //[OnReadyCallable("$")] 
    public Node MyOwner => this.GetOwner<Node>(); //This workws without any OnReady needed. It's a getter shortcut.

    //public string _myString => this.GetOwner();
    //[OnReady("$")] private SampleScene _myOwner = GetOwner<SampleScene>(); 

    public override void _Notification(int what) => this.OnReady(this);

    public override void _Ready()
    {
        _myLabelFromChildScene.Text = "Initiated via OnReady - Working - Label from ChildScene";

        //_myOwner = GetOwner<SampleScene>();

        //_mySprite2D.Texture = _myOwner.texture2;

        GD.Print($"My owner is: {MyOwner?.Name}");

        if (MyOwner != null)
        {
            GD.PrintT("MyOwner from: ", this.Name.ToString(), "Owner Name =", MyOwner.Name);
        }


        if (_myParent != null)
        {
            GD.PrintT("Parent Node from ", this.Name.ToString(), "Parent Name =", _myParent.Name);
        }



    }
}
