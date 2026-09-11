using System;
using Exp.Spans;

namespace Exp.Builtins.Reflection;

static partial class Impl
{
    [Overrides(NS, null)]
    public static Instance GetTypeByName(Instance? _, IValue?[] args)
    {
        // get type name
        string[] tname = args[0]!.ToString()!.Split(NamespaceSpecificationSpan.Symbol);

        // get type
        if (tname.Length > 2 || (tname.Length == 2 && string.IsNullOrWhiteSpace(tname[1])))
            Interpreter.Activated.ThrowRuntime("Namespace was not specified correctly.", RuntimeException.INVALID_ARGUMENT);
        ClassDefSpan? cls = Interpreter.Activated.definations.FirstOrDefault(d => d is ClassDefSpan && d.Namespace == (tname.Length == 2 ? tname[0] : null) && d.Name == (tname.Length == 0 ? tname[0] : tname[1])) as ClassDefSpan;
        if (cls == null)
            Interpreter.Activated.ThrowRuntime($"Class '{tname[0] + (tname.Length == 2 ? $"::{tname[1]}" : "")}' was not found.", RuntimeException.NOT_FOUND);
        Instance type = cls.ExpType;

        return type;
    }
}
