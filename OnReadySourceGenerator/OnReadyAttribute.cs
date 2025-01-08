using System;
using System.Numerics;

[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public class OnReadyAttribute : Attribute
{
    public string NodePath { get; }

    public OnReadyAttribute(string nodePath)
    {
        NodePath = nodePath;
       
    }
}


[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
public class OnReadyCallableAttribute : Attribute
{
    public Object Node { get; } //TODO MAKE THIS A CALLABLE? OR SOMETHING WE COULD DO TO CALL A FUNCTION

    public OnReadyCallableAttribute(Object _node)
    {
        Node = _node;

    }
}

public static class OnReadyInterface
{
    public interface IOnReady
    {
        //Just an empty interface to be used as a marker
    }
}


