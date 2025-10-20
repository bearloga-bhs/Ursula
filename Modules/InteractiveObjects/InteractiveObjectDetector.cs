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
        switch (currentScanType)
        {
            case ScanType.Player:
                Node3D player = PlayerScript.instance;
                if (player != null && Node.IsInstanceValid(player))
                {
                    float distance = GlobalTransform.Origin.DistanceSquaredTo(player.GlobalTransform.Origin);
                    if (distance < scanRadius * scanRadius)
                    {
                        detectedObject = player;
                        onPlayerDetected?.Invoke();
                    }
                    else
                    {
                        onAnyObjectsNotDetected?.Invoke();
                    }
                }
                break;
            case ScanType.Object:
                detectedObject = FindNodeInRadius<ItemPropsScript>(scanRadius, ips => ips.GameObjectSampleHash == targetObjectNameHash);
                if (detectedObject != null && Node.IsInstanceValid(detectedObject))
                {
                    onObjectDetected?.Invoke();
                }
                else
                {
                    onAnyObjectsNotDetected?.Invoke();
                }
                break;
            case ScanType.Sound:
                detectedObject = FindNodeInRadius<InteractiveObjectAudio>(scanRadius, IOAudio => IOAudio.currentAudioKey == targetSoundName && IOAudio.isPlaying)?.GetParent();
                if (detectedObject != null && Node.IsInstanceValid(detectedObject))
                {
                    onSoundDetected?.Invoke();
                }
                break;
        }
    }

    private T FindNodeInRadius<T>(float radius, Func<T, bool> condition) where T : Node
    {
        Node root = GetTree().Root;
        
        var nodes = GetItemsNodes().ToList();

        if (typeof(T) == typeof(InteractiveObjectAudio))
        {
            foreach (Node node in nodes)
            {
                ItemPropsScript item = (ItemPropsScript)node;
                if (item != null)
                {
                    Node IOaudio = (Node)item.IO.audio;

                    if (IOaudio is T targetNode && condition(targetNode))
                    {
                        if (node is Node3D targetNode3D)
                        {
                            float distance = GlobalTransform.Origin.DistanceSquaredTo(targetNode3D.GlobalTransform.Origin);
                            if (distance <= radius * radius)
                            {
                                return targetNode;
                            }
                        }
                    }
                }
            }
        }
        else if (typeof(T) == typeof(ItemPropsScript))
        {
            foreach (Node node in nodes)
            {
                ItemPropsScript item = (ItemPropsScript)node;
                if (item == null) continue;

                if (item is T targetNode && condition(targetNode))
                {
                    if (node is Node3D targetNode3D)
                    {
                        float distance = GlobalTransform.Origin.DistanceSquaredTo(targetNode3D.GlobalTransform.Origin);
                        if (distance <= radius * radius)
                        {
                            return targetNode;
                        }
                    }
                }
            }
        }
        else
        {
            Node player = PlayerScript.instance;
            if (player != null) nodes.Add(player);

            foreach (Node node in nodes)
            {
                if (node is T targetNode && condition(targetNode))
                {
                    if (targetNode is Node3D targetNode3D)
                    {
                        float distance = GlobalTransform.Origin.DistanceSquaredTo(targetNode3D.GlobalTransform.Origin);
                        if (distance <= radius * radius)
                        {
                            return targetNode;
                        }
                    }
                }
            }
        }
        return null;
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
        detectedObject = FindNodeInRadius<Node>(scanRadius, node => node.Name.ToString().Contains(targetObjectName));
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
}
