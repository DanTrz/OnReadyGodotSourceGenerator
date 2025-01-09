using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Godot;


public class CodeToCopy:Node
{

    public void TestMethodSource()
    { 
        GD.Print("!!!!GD PRINT CODE ->>>> WEAVER CREATED WORKING Copied from TestMethodSource");

        GD.Print("!!!!CODE 2 -> GD PRINT CODE ->>>> WEAVER CREATED WORKING Copied from TestMethodSource");

        
    }

    public override void _Notification(int what) => this.OnReady(this);

    private void OnReady(Node node)
    {
        throw new NotImplementedException();
    }
}





