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

public static class OnReadyInterface
{
    public interface IOnReady
    {
        //Just an empty interface to be used as a marker
    }
}


