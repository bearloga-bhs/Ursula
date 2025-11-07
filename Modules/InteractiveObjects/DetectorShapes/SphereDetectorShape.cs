using Godot;

public class SphereDetectorShape : IDetectorShape
{
    private Vector3 offset;
    private float radius;
    private Node3D anchor = null;
    
    public SphereDetectorShape(Node3D relativeTo, float radius, Vector3 offset) 
    {
        anchor = relativeTo;
        this.radius = radius;
        this.offset = offset;
    }

    public bool IsDetected(Vector3 point)
    {
        Vector3 offset_after_rotation = anchor.GlobalTransform * offset;
        
        float dist2 = point.DistanceSquaredTo(offset_after_rotation);
        if (dist2 <= radius * radius)
        {
            return true;
        }
        
        return false;
    }
}