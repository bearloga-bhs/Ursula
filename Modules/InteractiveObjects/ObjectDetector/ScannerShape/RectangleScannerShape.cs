using Godot;

class RectangleScannerShape : IScannerShape
{
    private Vector2 min;
    private Vector2 max;

    private bool relative = false;
    private Node3D anchor;

    /// <summary>
    /// Создание области поиска в абсолютных координатах
    /// </summary>
    public RectangleScannerShape(Vector2 min, Vector2 max)
    {
        this.min = min;
        this.max = max;
    }

    /// <summary>
    /// Создание области поиска в относительных координатах объекта relativeTo
    /// </summary>
    public RectangleScannerShape(Vector2 min, Vector2 max, Node3D relativeTo) : this(min, max)
    {
        relative = true;
        anchor = relativeTo;
    }

    public bool PointInside(Vector3 point)
    {
        Vector3 offset;
        if (relative)
            offset = anchor.Transform.Origin;
        else
            offset = Vector3.Zero;

        if (point.X >= min.X + offset.X && 
            point.X <= max.X + offset.X &&
            point.Z >= min.Y + offset.Z &&
            point.Z <= max.Y + offset.Z)
            return true;
        return false;
    }
}

