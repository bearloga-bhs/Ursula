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

    private enum ScanType { Player, Object, Sound, Interaction }
    private ScanType currentScanType;
    private IScanner scanner;
    private bool isScanning = false;

    private float timeAccumulator = 0f;
    private const float SCAN_INTERVAL = 0.25f;

    public Action onObjectDetected;
    public Action onPlayerDetected;
    public Action onSoundDetected;
    public Action onPlayerInteractionObject;  // Зачем отдельный Action? Оставил для совместимости.

    public Action onAnyObjectsNotDetected;

    public string playerName = "Player";

    public object StartPlayerScan(float radius)
    {
        ScanningSetup(ScanType.Player);
        IScannerShape shape = new SphereScannerShape(Vector3.Zero, radius, this);
        ConditionBuilder<PlayerScript> conditionBuilder = new ConditionBuilder<PlayerScript>();
        conditionBuilder.SetShape(shape);
        Func<PlayerScript, bool> condition = conditionBuilder.Build();
        scanner = ScannerFactory.Create<PlayerScript>(condition);

        return null;
    }

    public object StartObjectScan(string objectName, float radius)
    {
        ScanningSetup(ScanType.Object);
        string targetObjectName = objectName;
        int targetObjectNameHash = objectName.GetHashCode();
        IScannerShape shape = new SphereScannerShape(Vector3.Zero, radius, this);
        conditionBuilder.SetShape(shape);
        Func<ItemPropsScript, bool> condition = conditionBuilder.Build<ItemPropsScript>(
            ips => ips.GameObjectSampleHash == targetObjectNameHash);
        scanner = ScannerFactory.Create(condition);

        return null;
    }

    public object StartPlayerObjectInteractionScan(string objectName, float radius)
    {
        ScanningSetup(ScanType.Interaction);
        string targetObjectName = objectName;
        int targetObjectNameHash = objectName.GetHashCode();
        IScannerShape shape = new SphereScannerShape(Vector3.Zero, radius, this);
        conditionBuilder.SetShape(shape);
        Func<Node3D, bool> condition = conditionBuilder.Build<Node3D>(node => node.Name.ToString().Contains(targetObjectName));
        scanner = ScannerFactory.Create<Node3D>(condition);
        GameManager.onPlayerInteractionObjectAction += PlayerInteractionObject;

        return null;
    }

    public object StartSoundScan(string soundName, float radius)
    {
        ScanningSetup(ScanType.Sound);
        string targetSoundName = soundName;
        IScannerShape shape = new SphereScannerShape(Vector3.Zero, radius, this);
        scanner = ScannerFactory.Create<InteractiveObjectAudio>(shape, IOAudio => IOAudio.currentAudioKey == targetSoundName && IOAudio.isPlaying);

        return null;
    }

    private void ScanningSetup(ScanType scanType)
    {
        CleanUp();
        isScanning = true;
        currentScanType = scanType;
        GD.Print($"Scanning for {scanType} started...");
    }

    private void CleanUp()
    {
        if (isScanning == true && currentScanType == ScanType.Interaction)
            GameManager.onPlayerInteractionObjectAction -= PlayerInteractionObject;
    }

    public object StopScanning()
    {
        CleanUp();
        isScanning = false;
        scanner = null;
        GD.Print("Scanning stopped.");
        return null;
    }

    public override void _Process(double delta)
    {
        if (isScanning && scanner != null)
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
        detectedObject = scanner.FindNode();
        if (detectedObject != null && Node.IsInstanceValid(detectedObject))
        {
            onObjectDetected?.Invoke();
        }
        else
            onAnyObjectsNotDetected?.Invoke();
    }

    // Оставил для совместимости.
    public void PlayerInteractionObject()
    {
        detectedObject = scanner.FindNode();
        if (detectedObject != null)
        {
            onPlayerInteractionObject?.Invoke();
        }
        else
            onAnyObjectsNotDetected?.Invoke();
    }

    public override void _ExitTree()
    {
        CleanUp();
    }
}
