using Godot;
using System;
using System.Linq;

class SoundScanner : ScannerBase<InteractiveObjectAudio>, IScanner
{
    public SoundScanner(Func<InteractiveObjectAudio, bool> condition) : base(condition)
    {
    }

    public Node3D FindNode()
    {
        var nodes = GetItemsNodes().ToList();

        foreach (Node node in nodes)
        {
            ItemPropsScript item = (ItemPropsScript)node;
            if (item != null)
            {
                Node IOaudio = (Node)item.IO.audio;

                if (IOaudio is InteractiveObjectAudio targetNode && condition(targetNode))
                {
                    return targetNode;
                }
            }
        }

        return null;
    }
}

