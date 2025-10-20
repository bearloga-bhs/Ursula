using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;

using Talent.Logic.Bus;
using Modules.HSM;

public partial class InteractiveObjectDetector : Area3D
{
    public Node detectedObject; // заданныйОбъект

    private bool isScanning = false;

    private enum ScanType { Player, Object, Sound }
    private ScanType currentScanType; 
    private string targetObjectName;
    private int targetObjectNameHash;
    private string targetSoundName;
    private float scanRadius;  

    private float timeAccumulator = 0f; 
    private const float SCAN_INTERVAL = 0.25f;

    public Action onObjectDetected;
    public Action onPlayerDetected;
    public Action onSoundDetected;
    public Action onPlayerInteractionObject;

    public Action onAnyObjectsNotDetected;

    public string playerName = "Player";

    public object StartPlayerScan(float radius)
    {
        StartScanning(ScanType.Player, radius);

        return null;
    }

    public object StartObjectScan(string objectName, float radius)
    {
        targetObjectName = objectName;
        targetObjectNameHash = objectName.GetHashCode();
        StartScanning(ScanType.Object, radius);

        return null;
    }

    public object StartPlayerObjectInteractionScan(string objectName, float radius)
    {
        targetObjectName = objectName;
        targetObjectNameHash = objectName.GetHashCode();
        scanRadius = radius;
        GameManager.onPlayerInteractionObjectAction += PlayerInteractionObject;

        return null;
    }   

    public object StartSoundScan(string soundName, float radius)
    {
        targetSoundName = soundName;
        StartScanning(ScanType.Sound, radius);

        return null;
    }

    private void StartScanning(ScanType scanType, float radius)
    {
        isScanning = true;
        currentScanType = scanType;
        scanRadius = radius;
        GD.Print($"Scanning for {scanType} started...");
    }
    public object StopScanning()
    {
        isScanning = false;
        GD.Print("Scanning stopped.");
        return null;
    }

    public override void _Process(double delta)
    {
        if (isScanning)
        {
            timeAccumulator += (float)delta;

            if (timeAccumulator >= SCAN_INTERVAL)
            {
                timeAccumulator = 0f;
                PerformScan();
            }
        }
    }

    private void PerformScan()
    {
        if (currentScanType == ScanType.Player)
        {
            detectedObject = FindPlayer();
            if (detectedObject != null && Node.IsInstanceValid(detectedObject))
            {
                onPlayerDetected?.Invoke();
            }                    
            else
            {
                onAnyObjectsNotDetected?.Invoke();
            }
        }
        else if (currentScanType == ScanType.Object)
        {
            detectedObject = FindObject();
            if (detectedObject != null && Node.IsInstanceValid(detectedObject))
            {
                onObjectDetected?.Invoke();
            }
            else
            {
                onAnyObjectsNotDetected?.Invoke();
            }
        }
        else if (currentScanType == ScanType.Sound)
        {
            detectedObject = FindSound();
            if (detectedObject != null && Node.IsInstanceValid(detectedObject))
            {
                onSoundDetected?.Invoke();
            }
        }
    }

    private IEnumerable<Node> GetItemsNodes()
    {
        foreach (ItemPropsScript ips in VoxLib.mapManager.gameItems)
        {
            Node node = (Node)ips; 
            yield return node;
        }
    }

    public void PlayerInteractionObject()
    {
        detectedObject = FindPlayerInteractionObject();

        if (detectedObject != null)
        {
            onPlayerInteractionObject?.Invoke();
        }
        else
        {
            onAnyObjectsNotDetected?.Invoke();
        }
    }

    public override void _ExitTree()
    {
        GameManager.onPlayerInteractionObjectAction -= PlayerInteractionObject;
    }

    private Node FindPlayer()
    {
        Node3D player = PlayerScript.instance;
        if (player != null && InRadius(player))
        {
            return player;
        }

        return null;
    }
    
    private Node FindObject()
    {
        Func<ItemPropsScript, bool> condition = ips => ips.GameObjectSampleHash == targetObjectNameHash;
        var nodes = GetItemsNodes().ToList();
        foreach (Node node in nodes)
        {
            ItemPropsScript item = (ItemPropsScript)node;
            if (item == null) continue;
            if (item is ItemPropsScript targetNode && condition(targetNode))
            {
                if (InRadius(node))
                {
                    return node;
                }
            }
        }

        return null;
    }
    
    private Node FindSound()
    {
        Func<InteractiveObjectAudio, bool> condition = IOAudio => IOAudio.currentAudioKey == targetSoundName && IOAudio.isPlaying;
        var nodes = GetItemsNodes().ToList();
        foreach (Node node in nodes)
        {
            ItemPropsScript item = (ItemPropsScript)node;
            if (item != null)
            {
                Node IOaudio = (Node)item.IO.audio;

                if (IOaudio is InteractiveObjectAudio targetNode && condition(targetNode) && InRadius(node))
                {
                    return node;
                }
            }
        }

        return null;
    }
    
    private Node FindPlayerInteractionObject()
    {
        Func<Node, bool> condition = node => node.Name.ToString().Contains(targetObjectName);
        var nodes = GetItemsNodes().ToList();
        Node player = PlayerScript.instance;
        if (player != null) nodes.Add(player);
        foreach (Node node in nodes)
        {
            if (InRadius(node) && condition(node))
            {
                return node;
            }
        }

        return null;
    }

    private bool InRadius(Node node)
    {
        if (node is Node3D targetNode3D)
        {
            float distance = GlobalTransform.Origin.DistanceSquaredTo(targetNode3D.GlobalPosition);
            if (distance <= scanRadius * scanRadius)
            {
                return true;
            }
        }
        
        return false;
    }
}
