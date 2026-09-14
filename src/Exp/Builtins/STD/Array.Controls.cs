using System;
using Exp.Spans;
using static Exp.Interpreter;

namespace Exp.Builtins.STD;

static partial class Impl
{
    [Overrides(STD_NAMESPACE, "Array", "Array.ctor")]
    public static ArrayInstance NewArray(Instance? _, IValue?[] args) =>
        new(ClassDefSpan.ExpArrayDef, new IValue[(int)args[0]!.Number]);

    [Overrides(STD_NAMESPACE, "Array", "array.get")]
    public static IValue? ArrayGetIndex(Instance arr, IValue?[] args)
    {
        int i = (int)args[0]!.Number;
        if (i < 0 || i >= arr.ArrayValues.Length)
            ThrowOutOfRange(Activated, i, arr.ArrayValues.Length);

        return arr!.ArrayValues[i];
    }

    [Overrides(STD_NAMESPACE, "Array", "array.set")]
    public static Exp.Void ArraySetIndex(Instance arr, IValue?[] args)
    {
        int i = (int)args[0]!.Number;
        if (i < 0 || i >= arr.ArrayValues.Length)
            ThrowOutOfRange(Activated, i, arr.ArrayValues.Length);
        
        ActionOperator op = (ActionOperator)args[1]!.Number;
        var val = args[2];

        if (op == ActionOperator.Reset)
            arr.ArrayValues[i] = val;
        else if (op == ActionOperator.Add)
            arr.ArrayValues[i] = PlusOperatorSpan.GetResult(arr.ArrayValues[i], val, null);
        else if (op == ActionOperator.Subtract)
            arr.ArrayValues[i] = MinusOperatorSpan.GetResult(arr.ArrayValues[i], val, null);
        else if (op == ActionOperator.PlusPlus)
            arr.ArrayValues[i] = PlusOperatorSpan.GetResult(arr.ArrayValues[i], 1d.ToExp(), null);
        else if (op == ActionOperator.MinusMinus)
            arr.ArrayValues[i] = MinusOperatorSpan.GetResult(arr.ArrayValues[i], 1d.ToExp(), null);
        else
            throw new Exception("Unexpected action operator on array index.");

        return Void.Return;
    }

    private static void ThrowOutOfRange(Interpreter interpreter, int i, int length)
    {
        interpreter.ThrowRuntime($"Index [{i}] is out of range. Must be 0 <= index < {length}.", RuntimeException.INDEX_OUT_OF_RANGE);
    }
}
