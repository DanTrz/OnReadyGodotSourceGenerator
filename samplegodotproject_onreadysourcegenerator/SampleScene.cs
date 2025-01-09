using Godot;
using System;
using System.Reflection;
using static Godot.XmlParser;

public partial class SampleScene : Node2D
{
    //NODE INTIALIZATION
    [OnReady("%LabelUniqueName")] private Label _UniqueNameLabel;
    [OnReady("CanvasLayer/PanelContainer/VBoxContainer/LabelNotUniqueName")] private Label _NotUniqueNameLabel;
    [OnReady("%Sprite2D")] private Sprite2D _mySprite2D;

    //PARENTH INTIALIZATION
    [OnReady("..")] private Node _myParent; //Using.. = get_root() in this context

    //GLOBALS INTIALIZATION
    [OnReady("/root/Globals")] private Globals _Globals; //Using.. = get_root() in this context

    //SCENE LOADING
    [OnReady("$")] private PackedScene _myChildScene = GD.Load<PackedScene>("res://ChildScene.tscn");
    private PackedScene _myChildSceneWithoutOnReady = GD.Load<PackedScene>("res://ChildScene.tscn"); //this works - No Need for ONREADY

    //RESOURCE LOADING
    [OnReady("$")] private Texture2D texture = GD.Load<Texture2D>("res://icon.svg"); //this works
    private Texture2D texture2 = GD.Load<Texture2D>("res://icon.svg"); //this works - No Need for ONREADY

    //TRYIG SOMETHING NEW... TRYING A WAY TO PASS A PARATEMETER TO THIS ONREADY ATTRIBUTE THAT WILL CALL A FUNCTION DEFINED IN IT. 
    //[OnReadyCallable(null)] public Texture2D texture99; //this works

    Func<string> myString0 = () => "string0";

    string MyString1() => "string1";

    public string MyString2 => "string2";


    //public Node MyOwner => this.GetOwner<>();
    //[OnReady("$")] private SampleScene _myOwner = GetOwner<SampleScene>(); 


    public void TestMethod()
    {
        GD.Print("OriginalContent");
    }

    public override void _Process(double delta)
    {

    }

    public override void _Ready()
    {
        TestMethod();

        //_myParent = GetParent<Node>();
        _UniqueNameLabel.Text = "Initiated via OnReady - Working - Unique name Node";
        _NotUniqueNameLabel.Text = "Initiated via OnReady - Working - Not Unique name Node";
        GD.PrintT("Globals OnReady: ", this.Name.ToString(), "Variable: ", _Globals.MyGlobalVariable);


        //if (this.HasMethod("TestMethodSource"))
        //{
        // Get the runtime type of the node
        // var nodeType = this.GetType(); ;

        // // Dynamically look for an "OnReady" method in the node class
        // var method = nodeType?.GetMethod("TestMethod", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

        // if (method != null)
        // {
        //     // Invoke the method, passing the node itself as the parameter
        //     //method.Invoke(this, null);
        //     GD.Print("SUCESS -> FOUND METHOD: " + method.Name.ToString());


        // }
        // else
        // {
        //     GD.Print("FAILED -> NOT FOUND METHOD: " + method.Name.ToString());
        // }
        //}

        _mySprite2D.Texture = texture;


        if (_myParent != null)
        {
            GD.PrintT("Parent Node from ", this.Name.ToString(), "Parent Name =", _myParent.Name);
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
