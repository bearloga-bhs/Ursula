using Godot;

public class RectangleDetectorShape : IDetectorShape
{
    private Vector2 min;
    private Vector2 max;
    
    private Node3D anchor;
    
    public RectangleDetectorShape(Vector2 min, Vector2 max, Node3D relativeTo)
    {
        this.min = min;
        this.max = max;
        anchor = relativeTo;
    }

    public bool IsDetected(Vector3 point)
    {
        Vector3 offset = anchor.GlobalPosition;

        if (point.X >= min.X + offset.X &&
            point.X <= max.X + offset.X &&
            point.Z >= min.Y + offset.Z &&
            point.Z <= max.Y + offset.Z)
        {
            return true;
        }
        return false;
    }
}