using System;

namespace Exp.Builtins.STD;

static partial class Impl
{
    [Overrides(Interpreter.STD_NAMESPACE, null)]
    public static Exp.Void SetTimeout(Instance? _, IValue?[] args)
    {
        double millis = args[0]!.Number;
        FuncPntr action = args[1]!.FuncPntr;

        async Task Make()
        {
            await Task.Delay((int)millis);
            action.Call(Interpreter.Activated, []);
        }

        Interpreter.Activated.RunAsync(Make());

        return Void.Return;
    }
}
