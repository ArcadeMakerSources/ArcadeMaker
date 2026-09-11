using System;

namespace Exp.Builtins.STD;

static partial class Impl
{
    [Overrides(Interpreter.STD_NAMESPACE, null)]
    public static Exp.Void RunAsync(Instance? _, IValue?[] args)
    {
        FuncPntr action = args[0]!.FuncPntr;
        FuncPntr? onComplete = args[1]?.FuncPntr;

        async Task Make()
        {
            var res = await Task.Run(() => action.Call(Interpreter.Activated, []));
            onComplete?.Call(Interpreter.Activated, [res]);
        }

        Interpreter.Activated.RunAsync(Make());

        return Void.Return;
    }
}
