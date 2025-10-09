using Godot;
using System;
using Talent.Graphs;

class PlayerScanner : ScannerBase<PlayerScript>, IScanner
{
    public PlayerScanner(Func<PlayerScript, bool> condition) : base(condition)
    {
    }

    public Node3D FindNode()
    {
        PlayerScript targetNode = PlayerScript.instance;
        if (condition(targetNode))
        {
            return targetNode;
        }

        return null;
    }
}
