using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class InteractiveObjectDetector : Area3D
{
    public Node detectedObject; // заданныйОбъект

    private bool isScanning = false;

    private Action scanAction;
    private string targetObjectName;
    private string targetSoundName;
    private float scanRadius;  
    private IDetectorShape detectorShape;

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
        detectorShape = new SphereDetectorShape(this, radius, Vector3.Zero);
        return null;
    }

    public object StartObjectScan(string objectName, float radius)
    {
        targetObjectName = objectName;
        StartScanning(radius);
        scanAction += FindObject;
        detectorShape = new SphereDetectorShape(this, radius, Vector3.Zero);
        return null;
    }

    public object StartPlayerObjectInteractionScan(string objectName, float radius)
    {
        targetObjectName = objectName;
        scanRadius = radius;
        GameManager.onPlayerInteractionObjectAction += PlayerInteractionObject;
        detectorShape = new SphereDetectorShape(this, radius, Vector3.Zero);
        return null;
    }   

    public object StartSoundScan(string soundName, float radius)
    {
        targetSoundName = soundName;
        StartScanning(radius);
        scanAction += FindSound;
        detectorShape = new SphereDetectorShape(this, radius, Vector3.Zero);
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
                scanAction();
            }
        }
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
        Node currentDetectedObject = null;
        
        var nodes = GetItemsNodes().ToList();
        Node player = PlayerScript.instance;
        if (player != null) nodes.Add(player);
        foreach (Node node in nodes)
        {
            if (node is Node3D targetNode3D && detectorShape.IsDetected(targetNode3D.GlobalPosition) && node.Name.ToString().Contains(targetObjectName))
            {
                currentDetectedObject = node;
                break;
            }
        }

        if (currentDetectedObject != null)
        {
            onPlayerInteractionObject?.Invoke();
        }
        else
        {
            onAnyObjectsNotDetected?.Invoke();
        }

        detectedObject = currentDetectedObject;
    }

    public override void _ExitTree()
    {
        GameManager.onPlayerInteractionObjectAction -= PlayerInteractionObject;
    }

    private void FindPlayer()
    {
        Node3D player = PlayerScript.instance;
        if (player != null && Node.IsInstanceValid(player) && detectorShape.IsDetected(player.GlobalPosition))
        {
            detectedObject = player;
        }
        else
        {
            detectedObject = null;
        }

        if (detectedObject != null)
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
        Node currentDetectedObject = null;
        
        var nodes = GetItemsNodes().ToList();
        foreach (Node node in nodes)
        {
            if (node is ItemPropsScript item && item.GameObjectSample == targetObjectName && 
                node is Node3D targetNode3D && detectorShape.IsDetected(targetNode3D.GlobalPosition))
            {
                currentDetectedObject = node;
                break;
            }
        }

        if (currentDetectedObject != null && Node.IsInstanceValid(currentDetectedObject))
        {
            onObjectDetected?.Invoke();
        }
        else
        {
            onAnyObjectsNotDetected?.Invoke();
        }
        
        detectedObject = currentDetectedObject;
    }
    
    private void FindSound()
    {
        Node currentDetectedObject = null;
        
        var nodes = GetItemsNodes().ToList();
        foreach (Node node in nodes)
        {
            if (node is ItemPropsScript item && item.IO.audio.currentAudioKey == targetSoundName && item.IO.audio.isPlaying &&
                node is Node3D targetNode3D && detectorShape.IsDetected(targetNode3D.GlobalPosition))
            {
                currentDetectedObject = node;
                break;
            }
        }

        if (currentDetectedObject != null && Node.IsInstanceValid(currentDetectedObject))
        {
            onSoundDetected?.Invoke();
        }

        detectedObject = currentDetectedObject;
    }
}
