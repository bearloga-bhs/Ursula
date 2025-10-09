using Godot;
using System;
using System.Collections.Generic;

public class ScannerFactory
{
    public static IScanner Create<T>(Func<T, bool> condition) where T : Node3D
    {
        if (typeof(T).IsEquivalentTo(typeof(ItemPropsScript)))
        {
            return new ObjectScanner(ips => condition(ips as T));
        }
        else if (typeof(T).IsEquivalentTo(typeof(InteractiveObjectAudio)))
        {
            return new SoundScanner(io => condition(io as T));
        }
        else if (typeof(T).IsEquivalentTo(typeof(PlayerScript)))
        {
            return new PlayerScanner(p => condition(p as T));
        }
        else if (typeof(T).IsEquivalentTo(typeof(Node3D)))
        {
            return new NodeScanner(node => condition(node as T));
        }
        else
        {
            throw new ArgumentException($"Couldn't find scanner for type {typeof(T)}");
        }
    }
}

