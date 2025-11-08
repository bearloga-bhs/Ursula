using System;
using Godot;

public class RectangleDetectorShape : IDetectorShape
{
    private Vector3 left_down;
    private Vector3 left_up;
    private Vector3 right_down;
    private Vector3 right_up;

    private Node3D anchor;
    
    public RectangleDetectorShape(Node3D relativeTo, float width, float height, Vector3 offset)
    {
        left_down = new Vector3(-width / 2, 0, -height / 2) + offset;
        left_up = new Vector3(-width / 2, 0, height / 2) + offset;
        right_down = new Vector3(width / 2, 0, -height / 2) + offset;
        right_up = new Vector3(width / 2, 0, height / 2) + offset;
        anchor = relativeTo;
    }

    public bool IsDetected(Vector3 point)
    {
        //NOTE Pitch rotation is not ignored. If 3D actor is rotated in Pitch axis rectangle will be shrinked
        //NOTE make detection like box collider? Need to discuss 
        
        Vector3 point_copy = new Vector3(point.X, 0, point.Z);
            
        Vector3 left_down_after_rotation = anchor.GlobalTransform * left_down;
        Vector3 left_up_after_rotation = anchor.GlobalTransform * left_up;
        Vector3 right_down_after_rotation = anchor.GlobalTransform * right_down;
        Vector3 right_up_after_rotation = anchor.GlobalTransform * right_up;
        
        Vector3 left_side = left_up_after_rotation - left_down_after_rotation;
        Vector3 up_side = right_up_after_rotation - left_up_after_rotation;
        Vector3 right_side = right_down_after_rotation - right_up_after_rotation;
        Vector3 down_side = left_down_after_rotation - right_down_after_rotation;
        
        Vector3 point_left = left_down_after_rotation - point_copy;
        Vector3 point_up = left_up_after_rotation - point_copy;
        Vector3 point_right = right_up_after_rotation - point_copy;
        Vector3 point_down = right_down_after_rotation - point_copy;

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