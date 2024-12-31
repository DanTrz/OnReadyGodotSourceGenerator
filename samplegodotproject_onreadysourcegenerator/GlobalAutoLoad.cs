using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

// The resolver is not working on the main scene of the project @see https://github.com/godotengine/godot/issues/37813,
// so we need to use a decorator Root scene to instantiate the real main scene of the game, which is covered by the injector <summary>
// ORIGINAL SOLUTION BY: Romain Mouillard https://medium.com/@romain.mouillard.fr
// See: https://medium.com/@romain.mouillard.fr/bringing-gdscripts-onready-magic-to-c-a-quick-custom-solution-5bae074ce799
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

    // Cache dictionary for MethodInfo
    private static Dictionary<Type, MethodInfo> onReadyMethodCache = new Dictionary<Type, MethodInfo>();

    private void OnNodeAdded(Node node)
    {

        //// Check if the node implements the IOnReady interface that we assign via SourceGenerator (Just to filter out the nodes we are interested in
        //if (node is OnReadyInterface.IOnReady)
        //{
        //    // Get the runtime type of the node
        //    //var nodeName = node.Name.ToString(); ;

        //    // Get the runtime type of the node
        //    var nodeType = node.GetType(); ;


        //    // Dynamically look for an "OnReady" method in the node class
        //    //var method = nodeType?.GetMethod("OnReady");
        //    var method = nodeType.GetMethod("OnReady", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

        //    if (method != null)
        //    {
        //        // Invoke the method, passing the node itself as the parameter
        //        method.Invoke(node, new object[] { node });
        //        GD.Print("Node resolved via AutoLoad: " + node.Name.ToString());
        //    }
        //}


        // Check if the node implements the IOnReady interface
        if (node is OnReadyInterface.IOnReady)
        {
            Type nodeType = node.GetType();

            // Try to get the cached MethodInfo, in case it's a node / type that will be added multiple times
            if (!onReadyMethodCache.TryGetValue(nodeType, out var method))
            {
                // Dynamically look for the "OnReady" method only once per type
                method = nodeType.GetMethod("OnReady", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

                // Cache the MethodInfo for future use
                if (method != null)
                {
                    onReadyMethodCache[nodeType] = method;
                }
            }

            // If the method exists, invoke it
            if (method != null)
            {
                method.Invoke(node, new object[] { node });
                GD.Print("Node resolved via AutoLoad: " + node.Name.ToString()); ;
            }
        }

    }
}
