using Godot;
using System;

public partial class GlobalAutoLoad : Node
{
    // This AutoLoad can be an alternative to writing this.OnReady(this) in every script.
    // Make sure you are EITHER using the AUtoLoad or the OnReady method within _Ready of the scripts
    public override void _Ready()
    {
        // Listen for new nodes added to the scene tree
        GetTree().NodeAdded += OnNodeAdded;

        //Resolver needs to be executed for all nodes already in the scene tree(auto - load nodes)
        foreach (var node in GetTree().Root.GetChildren())
            {
                OnNodeAdded(node);
            }
    }

    private void OnNodeAdded(Node node)
    {
        // Get the runtime type of the node
        var nodeName = node.Name.ToString(); ;

        // Get the runtime type of the node
        var nodeType = node.GetType();;

        // Dynamically look for an "OnReady" method in the node class
        var method = nodeType?.GetMethod("OnReady");

        if (method != null)
        {
            // Invoke the method, passing the node itself as the parameter
            method.Invoke(node, new object[] { node });
            GD.Print("Node resolved via AutoLoad: " +node.Name.ToString());
        }
    }
}
