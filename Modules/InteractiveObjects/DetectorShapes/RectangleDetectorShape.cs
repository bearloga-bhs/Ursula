using System;
using Godot;

public class RectangleDetectorShape : IDetectorShape
{
    private Vector3 left_down;
    private Vector3 left_up;
    private Vector3 right_down;
    private Vector3 right_up;
    private Vector3 offset;

    private Node3D anchor;
    
    public RectangleDetectorShape(Node3D relativeTo, float width, float height, Vector3 offset)
    {
        //TODO add support of offset of rectangle points
        left_down = new Vector3(-width / 2, 0, -height / 2);
        left_up = new Vector3(-width / 2, 0, height / 2);
        right_down = new Vector3(width / 2, 0, -height / 2);
        right_up = new Vector3(width / 2, 0, height / 2);
        this.offset = offset;
        anchor = relativeTo;
    }

    public bool IsDetected(Vector3 point)
    {
        //TODO rotate rectangle here, ignore Pitch rotation
        Vector3 point_copy = new Vector3(point.X, 0, point.Z);
        
        Vector3 left_side = left_up - left_down;
        Vector3 up_side = right_up - left_up;
        Vector3 right_side = right_down - right_up;
        Vector3 down_side = left_down - right_down;
        
        Vector3 point_left = left_down - point_copy;
        Vector3 point_up = left_up - point_copy;
        Vector3 point_right = right_up - point_copy;
        Vector3 point_down = right_down - point_copy;

        float cross_left = point_left.Cross(left_side).Y;
        float cross_up = point_up.Cross(up_side).Y;
        float cross_right = point_right.Cross(right_side).Y;
        float cross_down = point_down.Cross(down_side).Y;

        if (cross_left < 0 || cross_up < 0 || cross_right < 0 || cross_down < 0)
        {
            return false;
        }
        
        return true;
    }
}