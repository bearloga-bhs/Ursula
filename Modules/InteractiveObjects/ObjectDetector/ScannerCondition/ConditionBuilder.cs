using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class ConditionBuilder<T> where T: Node3D
{
    private IScannerShape shape;
    private List<string> prefixList;
    private Func<T, bool> condition;

    public ConditionBuilder<T> SetShape(IScannerShape shape)
    {
        this.shape = shape;
        return this;
    }

    public ConditionBuilder<T> AddPrefix(string prefix)
    {
        prefixList.Add(prefix);
        return this;
    }

    public ConditionBuilder<T> RemovePrefix(string prefix)
    {
        prefixList.Remove(prefix);
        return this;
    }

    public ConditionBuilder<T> SetCondition(Func<T, bool> condition)
    {
        this.condition = condition;
        return this;
    }

    public Func<T, bool> Build()
    {
        return node =>
            (shape is null || shape.PointInside(node.GlobalTransform.Origin))
            && (prefixList.Count == 0 || prefixList.Any(prefix => node.Name.ToString().Contains(prefix)))
            && (condition is null || condition(node));
    }
}
