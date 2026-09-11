using System;
using Exp.Spans;

namespace Exp.Builtins.STD;

static partial class Impl
{
    [Overrides(Interpreter.STD_NAMESPACE, "cs")]
    public static Instance Int(Instance? _, IValue?[] args) => ((int)args[0]!.Number).AsExtern();

    [Overrides(Interpreter.STD_NAMESPACE, "cs")]
    public static Instance Byte(Instance? _, IValue?[] args) => ((byte)args[0]!.Number).AsExtern();

    [Overrides(Interpreter.STD_NAMESPACE, "cs")]
    public static Instance Float(Instance? _, IValue?[] args) => ((float)args[0]!.Number).AsExtern();

    [Overrides(Interpreter.STD_NAMESPACE, "cs")]
    public static Instance Long(Instance? _, IValue?[] args) => ((long)args[0]!.Number).AsExtern();

    [Overrides(Interpreter.STD_NAMESPACE, "cs")]
    public static Instance Action(Instance? _, IValue?[] args)
    {
        var f = args[0]!.FuncPntr;
        return f.Func.Args.Length > 0 ?
                   new Action<object>((object args) => f.Call(Interpreter.Activated, Interpreter.CsValToExpVal(args) is ArrayInstance arr ? arr.ArrayValues : [])).AsExtern() :
                   new Action(() => f.Call(Interpreter.Activated, [])).AsExtern();
    }

    [Overrides(Interpreter.STD_NAMESPACE, "cs")]
    public static IValue Exp(Instance? _, IValue?[] args)
    {
        NumberValue val = 0;

        object ext = args[0] is ExternTypeInstance eti ? eti.ExternInstance : Interpreter.Activated.ThrowRuntime<object>("An extern value was expected.", RuntimeException.INVALID_ARGUMENT);
        
        if (ext is double i)
            val = i;
        else if (ext is byte b)
            val = b;
        else if (ext is float f)
            val = (double)f;
        else if (ext is long l)
            val = l;
        else if (ext is decimal d)
            val = (double)d;
        else
        {
            try
            {
                val = Convert.ToDouble(ext);
            }
            catch (Exception ex)
            {
                Interpreter.Activated.ThrowRuntime($"Could not cast the given value to {ValueHelper.tnum} (Error: {ex.Message}).", RuntimeException.INVALID_ARGUMENT);
            }
        }

        return (IValue)val ?? SpecialValue.From(ext);
    }
}
