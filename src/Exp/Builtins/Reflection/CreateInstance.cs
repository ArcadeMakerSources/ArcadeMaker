using System;
using Exp.Operations;
using Exp.Spans;

namespace Exp.Builtins.Reflection;

static partial class Impl
{
    [Overrides(NS, null)]
    public static Instance CreateInstance(Instance? _, IValue?[] args)
    {
        ClassDefSpan type = args[0]!.Inst.GetClassFromExpTypeInstanceOrThrowRuntime(Interpreter.Activated);

        InitOperation creator = new(type, [.. args[1]!.Inst.ArrayValues.Select(v => new ReadingOperation(v))]);

        return (Instance)creator.Read();
    }
}
