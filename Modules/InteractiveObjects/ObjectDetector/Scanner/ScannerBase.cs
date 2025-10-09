using Godot;
using System;
using System.Collections.Generic;

public abstract class ScannerBase<T> where T : Node3D
{
    protected Func<T, bool> condition;

    public ScannerBase(Func<T, bool> condition)
    {
        this.condition = condition;
    }

    protected IEnumerable<Node> GetItemsNodes()
    {
        foreach (ItemPropsScript ips in VoxLib.mapManager.gameItems)
        {
            Node node = (Node)ips;
            yield return node;
        }
    }
}

