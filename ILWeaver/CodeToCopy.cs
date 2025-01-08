using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public  class CodeToCopy: CodeBase
{

    public void TestMethodSource()
    {
       Console.WriteLine("WEAVER CRREATED => TestMethodSource");
    }

}

public class GD
{
    public static void PrintT(params object[] args)
    {
        Console.WriteLine(args);
    }
}

public class CodeBase
{
    public virtual void _Notification(int what)
    {
       // this.OnReady(this);
    }

    public void OnReady(CodeToCopy obj)
    {
        //
    }


}




