using System;

namespace Exp.Builtins.STD;

static partial class Impl
{
    private const string StrCmpr = "StringComparer";
    
    [Overrides(Interpreter.STD_NAMESPACE, StrCmpr)]
    public static IValue Ordinal(Instance instance, IValue?[] args)
    {
        string? a = args[0]?.ToString();
        string? b = args[1]?.ToString();
        return StringComparer.Ordinal.Compare(a, b).ToExp();
    }
    
    [Overrides(Interpreter.STD_NAMESPACE, StrCmpr)]
    public static IValue OrdinalIgnoreCase(Instance instance, IValue?[] args)
    {
        string? a = args[0]?.ToString();
        string? b = args[1]?.ToString();
        return StringComparer.OrdinalIgnoreCase.Compare(a, b).ToExp();
    }
}
