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

    private Action scanAction;
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
        StartScanning(radius);
        scanAction += FindPlayer;
        return null;
    }

    public object StartObjectScan(string objectName, float radius)
    {
        targetObjectName = objectName;
        targetObjectNameHash = objectName.GetHashCode();
        StartScanning(radius);
        scanAction += FindObject;

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
        StartScanning(radius);
        scanAction += FindSound;

        return null;
    }

    private void StartScanning(float radius)
    {
        isScanning = true;
        scanRadius = radius;
        GD.Print($"Scanning started...");
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
        scanAction();
    }

    private IEnumerable<Node> GetItemsNodes()
    {
        foreach (ItemPropsScript ips in VoxLib.mapManager.gameItems)
        {
            yield return ips;
        }
    }
    
    private void PlayerInteractionObject()
    {
        var nodes = GetItemsNodes().ToList();
        Node player = PlayerScript.instance;
        if (player != null) nodes.Add(player);
        foreach (Node node in nodes)
        {
            if (InRadius(node) && node.Name.ToString().Contains(targetObjectName))
            {
                detectedObject = node;
            }
        }

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

    private void FindPlayer()
    {
        Node3D player = PlayerScript.instance;
        if (player != null && InRadius(player))
        {
            detectedObject = player;
        }

        if (detectedObject != null && Node.IsInstanceValid(detectedObject))
        {
            onPlayerDetected?.Invoke();
        }                    
        else
        {
            onAnyObjectsNotDetected?.Invoke();
        }
    }
    
    private void FindObject()
    {
        var nodes = GetItemsNodes().ToList();
        foreach (Node node in nodes)
        {
            if (node is ItemPropsScript item && item.GameObjectSampleHash == targetObjectNameHash)
            {
                if (InRadius(node))
                {
                    detectedObject = node;
                }
            }
        }

        if (detectedObject != null && Node.IsInstanceValid(detectedObject))
        {
            onObjectDetected?.Invoke();
        }
        else
        {
            onAnyObjectsNotDetected?.Invoke();
        }
    }
    
    private void FindSound()
    {
        var nodes = GetItemsNodes().ToList();
        foreach (Node node in nodes)
        {
            if (node is ItemPropsScript item && item.IO.audio.currentAudioKey == targetSoundName && item.IO.audio.isPlaying && InRadius(node))
            {
                detectedObject = node;
            }
        }

        if (detectedObject != null && Node.IsInstanceValid(detectedObject))
        {
            onSoundDetected?.Invoke();
        }
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
