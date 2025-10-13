using Godot;
using Modules.HSM;
using System;

public partial class InteractiveObjectRandomness : Node
{
    public VariableHolder<int> CurrentValue = new(0);

    public Action ValueChanged;

    static Random random;
    private bool valueChanged;

    public Random Random
    {
        get
        {
            if (random is null)
            {
                random = new Random(42);
            }
            return random;
        }
    }

    public object GenerateRange(int a, int b)
    {
        if (a > b)
        {
            int t = a;
            a = b;
            b = t;
        }
        int value = Random.Next(a, b);
        CurrentValue.Value = value;
        valueChanged = true;

        return null;
    }

    public object InvokeIfChanged()
    {
        if (valueChanged)
        {
            ValueChanged?.Invoke();
            valueChanged = false;
        }

        return null;
    }
}

