using System;

namespace Exp.Builtins.STD;

static partial class Impl
{
    [Overrides(Interpreter.STD_NAMESPACE, null)]
    public static IValue RefEquals(Instance? _, IValue?[] args)
    {
        IValue? a = args[0], b = args[1];
        if (a == null || b == null)
            return (a == b).ToExp(); // operator == does not fire .Equals(...) so it's fine if one of them is an Instance
        else
            return (a.IsInst && b.IsInst && ReferenceEquals(a, b)).ToExp();
    }
}
