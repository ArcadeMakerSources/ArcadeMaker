using System;
using Exp.Spans;
using Exp.Converting;

namespace Exp.Builtins.Reflection;

static partial class Impl
{
    [Initializes(Interpreter.STD_NAMESPACE, "Type", "all")]
    public static ArrayInstance GetAllTypes()
    {
        return new(ClassDefSpan.ExpArrayDef, Interpreter.Activated.definations.OfType<ClassDefSpan>().Select(cls => cls.ExpType).ToArray());
    }
}