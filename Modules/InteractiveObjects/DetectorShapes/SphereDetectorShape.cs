using Godot;

public class SphereDetectorShape : IDetectorShape
{
    private Vector3 center;
    private float radius;

    private bool relative = false;
    private Node3D anchor = null;

    /// <summary>
    /// Создание области поиска в абсолютных координатах
    /// </summary>
    public SphereDetectorShape(Vector3 center, float radius)
    {
        this.center = center;
        this.radius = radius;
    }

    /// <summary>
    /// Создание области поиска в относительных координатах объекта relativeTo
    /// </summary>
    public SphereDetectorShape(Vector3 center, float radius, Node3D relativeTo) : this(center, radius)
    {
        anchor = relativeTo;
        relative = true;
    }

    public bool IsDetected(Vector3 point)
    {
        Vector3 offset;
        if (relative)
        {
            offset = anchor.GlobalPosition;
        }
        else
        {
            offset = Vector3.Zero;
        }

        float dist2 = point.DistanceSquaredTo(center + offset);
        if (dist2 <= radius * radius)
        {
            return true;
        }
        
        return false;
    }
}