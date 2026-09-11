using System;

namespace Exp.Builtins.STD;

static partial class Impl
{
    [Overrides(Interpreter.STD_NAMESPACE, "Type", "get")]
    public static Instance GetTypeOf(Instance? _, IValue?[] args) => args[0]!.Inst.def.ExpType;
}
