using bearloga.addons.Ursula.Modules.InteractiveObjects.DetectorShapes.DetectorShapeVisualiation;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

using Talent.Logic.Bus;
using Modules.HSM;

public partial class InteractiveObjectDetector : Node
{
    public Node detectedObject; // заданныйОбъект
    public Node previousDetectedObject;
    
    private bool isScanning = false;

    private Action scanAction;
    private string targetObjectName;
    private string targetSoundName;
    private IDetectorShape detectorShape;

    private DetectorShapeVisualization visualization = new DetectorShapeVisualization();

    private float timeAccumulator = 0f; 
    private const float SCAN_INTERVAL = 0.25f;

    public Action onObjectDetected;
    public Action onPlayerDetected;
    public Action onSoundDetected;
    public Action onPlayerInteractionObject;

    public Action onAnyObjectsNotDetected;

    public string playerName = "Player";

    private MoveScript moveScriptCache;

    public MoveScript moveScript
    {
        get
        {
            if (moveScriptCache == null)
            {
                var moveScript = GetParent() as MoveScript;
                moveScriptCache = moveScript;
            }
            return moveScriptCache;
        }
    }

    public override void _Ready()
    {
        CSharpBridgeRegistry.Process += CSProcess;
    }

    public object StartPlayerScan(float radius)
    {
        StartScanning();
        scanAction += FindPlayer;
        detectorShape = new SphereDetectorShape(moveScript, radius, Vector3.Zero);
        visualization.Hide();
        visualization.Draw(detectorShape, this);
        return null;
    }

    public object StartObjectScan(string objectName, float radius)
    {
        targetObjectName = objectName;
        StartScanning();
        scanAction += FindObject;
        detectorShape = new SphereDetectorShape(moveScript, radius, Vector3.Zero);
        visualization.Hide();
        visualization.Draw(detectorShape, this);
        return null;
    }

    public object StartObjectScanSquare(string objectName, float width, float offsetX, float offsetZ)
    {
        targetObjectName = objectName;
        StartScanning();
        scanAction += FindObject;
        detectorShape = new RectangleDetectorShape(moveScript, width, width, new Vector3(offsetX, 0, offsetZ));
        visualization.Hide();
        visualization.Draw(detectorShape, this);
        return null;
    }

    public object StartPlayerObjectInteractionScan(string objectName, float radius)
    {
        targetObjectName = objectName;
        GameManager.onPlayerInteractionObjectAction += PlayerInteractionObject;
        detectorShape = new SphereDetectorShape(moveScript, radius, Vector3.Zero);
        visualization.Hide();
        visualization.Draw(detectorShape, this);
        return null;
    }   

    public object StartSoundScan(string soundName, float radius)
    {
        targetSoundName = soundName;
        StartScanning();
        scanAction += FindSound;
        detectorShape = new SphereDetectorShape(moveScript, radius, Vector3.Zero);
        visualization.Hide();
        visualization.Draw(detectorShape, this);
        return null;
    }

    public object StartSoundScanOffset(string soundName, float radius, float offsetX, float offsetZ)
    {
        targetSoundName = soundName;
        StartScanning();
        scanAction += FindSound;
        detectorShape = new SphereDetectorShape(moveScript, radius, new Vector3(offsetX, 0, offsetZ));
        visualization.Hide();
        visualization.Draw(detectorShape, this);
        return null;
    }

    private void StartScanning()
    {
        isScanning = true;
        GD.Print($"Scanning started...");
    }
    
    public object StopScanning()
    {
        isScanning = false;
        GD.Print("Scanning stopped.");
        return null;
    }

    public void CSProcess(double delta)
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
            if (!IsInstanceValid(node))
            {
                continue;
            }
            
            float distance;
            if (node is Node3D targetNode3D && detectorShape.IsDetected(targetNode3D.GlobalPosition, out distance) && node.Name.ToString().Contains(targetObjectName))
            {
                currentDetectedObject = node;
                break;
            }
        }

        if (currentDetectedObject != null)
        {
            onPlayerInteractionObject?.Invoke();
            previousDetectedObject = currentDetectedObject;
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
        float distance;
        
        Node3D player = PlayerScript.instance;
        if (player != null && Node.IsInstanceValid(player) && detectorShape.IsDetected(player.GlobalPosition, out distance))
        {
            previousDetectedObject = player;
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
        float min_distance = float.MaxValue;
        
        var nodes = GetItemsNodes().ToList();
        foreach (Node node in nodes)
        {
            if (!IsInstanceValid(node))
            {
                continue;
            }
            
            float distance;
            
            if (node is ItemPropsScript item && item.GameObjectSample.StartsWith(targetObjectName) && 
                node is Node3D targetNode3D && detectorShape.IsDetected(targetNode3D.GlobalPosition, out distance))
            {
                if (min_distance > distance)
                { 
                    currentDetectedObject = node;
                    min_distance = distance;
                }
            }
        }

        if (currentDetectedObject != null && Node.IsInstanceValid(currentDetectedObject))
        {
            previousDetectedObject = currentDetectedObject;
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

        float min_distance = float.MaxValue;
        
        var nodes = GetItemsNodes().ToList();
        foreach (Node node in nodes)
        {
            if (!IsInstanceValid(node))
            {
                continue;
            }

            float distance;
            
            if (node is ItemPropsScript item && item.IO.audio.currentAudioKey.StartsWith(targetSoundName) && item.IO.audio.isPlaying &&
                node is Node3D targetNode3D && detectorShape.IsDetected(targetNode3D.GlobalPosition, out distance))
            {
                if (min_distance > distance)
                { 
                    currentDetectedObject = node;
                    min_distance = distance;
                }
            }
        }

        if (currentDetectedObject != null && Node.IsInstanceValid(currentDetectedObject))
        {
            previousDetectedObject = currentDetectedObject;
            onSoundDetected?.Invoke();
        }
        else
        {
            onAnyObjectsNotDetected?.Invoke();
        }
        
        detectedObject = currentDetectedObject;
    }
}
