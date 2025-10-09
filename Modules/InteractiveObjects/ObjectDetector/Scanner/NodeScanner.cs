using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class NodeScanner : ScannerBase<Node3D>, IScanner
{
    public NodeScanner(Func<Node3D, bool> condition) : base(condition)
    {

    }

    public Node3D FindNode()
    {
        var nodes = GetItemsNodes().ToList();
        foreach (Node node in nodes)
        {
            if (node is Node3D targetNode && condition(targetNode))
            {
                return targetNode;
            }
        }

        return null;
    }
}

