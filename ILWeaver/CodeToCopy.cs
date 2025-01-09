using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Godot;


public class CodeToCopy : Node
{

    public void TestMethodSource()
    {
        GD.Print("!!!!GD PRINT CODE ->>>> WEAVER CREATED WORKING Copied from TestMethodSource");

        GD.Print("!!!!CODE 2 -> GD PRINT CODE ->>>> WEAVER CREATED WORKING Copied from TestMethodSource");

        GD.Print($"this.OnReady(this)");

    }

    public static void ResolveNode(Godot.Node node)
    {
        GD.Print("Trying to resolve Node: " + node.Name.ToString()); ;
        // Check if the node implements the IOnReady interface
        if (node is OnReadyInterface.IOnReady)
        {
            Type nodeType = node.GetType();


            // Dynamically look for the "OnReady" method only once per type
            var method = nodeType.GetMethod("OnReady", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

            // If the method exists, invoke it
            if (method != null)
            {
                method.Invoke(node, new object[] { node });
                GD.Print("Node resolved via WaverCode: " + node.Name.ToString()); ;
            }
        }

    }
}

//System.Type nodeType = this.GetType();
//var method = nodeType.GetMethod("OnReady");
//if (method != null)
//{
//    method.Invoke(this, null);
//    GD.Print("Node resolved via Weaver"); ;
//}






