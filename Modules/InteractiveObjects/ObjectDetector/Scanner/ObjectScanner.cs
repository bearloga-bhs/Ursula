using Godot;
using System;
using System.Linq;

class ObjectScanner : ScannerBase<ItemPropsScript>, IScanner
{
    public ObjectScanner(Func<ItemPropsScript, bool> condition) : base(condition)
    {
    }

    public Node3D FindNode()
    {
        var nodes = GetItemsNodes().ToList();
        foreach (Node node in nodes)
        {
            if (node is ItemPropsScript targetNode && condition(targetNode))
            {
                 return targetNode;
            }
        }

        return null;
    }
}

