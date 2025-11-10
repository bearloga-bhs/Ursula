using Godot;

public class SphereDetectorShape : IDetectorShape
{
    private Vector3 center;
    private float radius;
    private Node3D anchor = null;
    
    public SphereDetectorShape(Node3D relativeTo, float radius, Vector3 center) 
    {
        anchor = relativeTo;
        this.radius = radius;
        this.center = center;
    }

    public bool IsDetected(Vector3 point)
    {
        Vector3 center_after_rotation = anchor.GlobalTransform * center;
        
        float dist2 = point.DistanceSquaredTo(center_after_rotation);
        if (dist2 <= radius * radius)
        {
            return true;
        }
        
        return false;
    }
}
