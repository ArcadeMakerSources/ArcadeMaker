using System;
using Exp.Spans;

namespace Exp.Builtins.Reflection;

static partial class Impl
{
    [Overrides(NS, null)]
    public static Instance? GetTypeByName(Instance? _, IValue?[] args)
    {
        // get type name
        string fullArgStr = args[0]!.ToString() ?? "NULL";
        string[] tname = fullArgStr.Split(NamespaceSpecificationSpan.Symbol);

        ClassDefSpan? cls = Interpreter.Activated.GetDef<ClassDefSpan>(tname[0], tname[1], out var actualDef);

        if (cls is null)
        {
            if (actualDef is AttributeDefSpan attr)
                return attr.ExpType;
            return null;
        }

        return cls.ExpType;
    }
}
