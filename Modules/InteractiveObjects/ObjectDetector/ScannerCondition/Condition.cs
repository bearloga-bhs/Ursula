
using Godot;
using System.Collections.Generic;
using System;

class Condition<T>
{
    private IScannerShape shape;
    private List<string> prefixList;
    private Func<Node3D, bool> condition;

    public void SetShape(IScannerShape shape)
    {
        this.shape = shape;
    }

    public void AddPrefix(string prefix)
    {
        prefixList.Add(prefix);
    }

    public void RemovePrefix(string prefix)
    {
        prefixList.Remove(prefix);
    }

    public bool Check<T>(T node)
    {

    }
}

