using Exp.Spans;
using System;

namespace Exp.Builtins.Reflection;

static partial class Impl
{
    [Overrides(NS, null)]
    public static Instance GetAttr(Instance? _, IValue?[] args)
    {
        Instance type = args[0]!.Inst;
        type.ThrowIfNotType();
        
        if (args.Length == 2) // for property
        {
            ClassDefSpan cls = type.GetClassFromExpTypeInstanceOrThrowRuntime(Interpreter.Activated);

            string propName = args[1]!.ToString()!;
            ICanSetAttr? prop = (ICanSetAttr)cls.Props.FirstOrDefault(p => p.Name == propName)! ?? cls.Vars.OfType<ClassStaticVar>().FirstOrDefault(p => p.Name == propName);
            if (prop == null)
                Interpreter.Activated.ThrowRuntime($"'{((IDefination)cls).FullName}' does not contain a property named '{propName}'.", RuntimeException.INVALID_ARGUMENT);
            if (prop.AttrInfo == null)
                Interpreter.Activated.ThrowRuntime($"{((IDefination)cls).FullName}.{propName} does not have tags.", RuntimeException.INVALID_ARGUMENT);

            return prop.AttrInfo.ToExpArray();
        }
        else if (args.Length == 3) // for funcs
        {
            ClassDefSpan cls = type.GetClassFromExpTypeInstanceOrThrowRuntime(Interpreter.Activated);

            string funcName = args[1]!.ToString()!;
            int paramsCount = (int)args[2]!.Number;
            FuncDefSpan fn = cls.Funcs.FirstOrDefault(p => p.Args.Length == paramsCount && p.Name == funcName);
            if (fn == null)
                Interpreter.Activated.ThrowRuntime($"'{((IDefination)cls).FullName}' does not contain a function named '{funcName}' taking {paramsCount} parameters.", RuntimeException.INVALID_ARGUMENT);
            if (fn.AttrInfo == null)
                Interpreter.Activated.ThrowRuntime($"{((IDefination)cls).FullName}.{funcName}(..{paramsCount}) does not have tags.", RuntimeException.INVALID_ARGUMENT);

            return fn.AttrInfo.ToExpArray();
        }
        else if (args.Length == 1) // for class
        {
            ClassDefSpan cls = type.GetClassFromExpTypeInstanceOrThrowRuntime(Interpreter.Activated);

            return cls.AttrInfo.ToExpArray();
        }

        throw new Exception("Args length must be lower than 4.");
    }

    private static void ThrowIfNotType(this Instance inst)
    {
        if (inst.def != ClassDefSpan.ExpTypeDef)
            Interpreter.Activated.ThrowRuntime("Argument 'type' must be of type " + ClassDefSpan.ExpTypeDef.GetExpTypeName(false), RuntimeException.INVALID_ARGUMENT);
    }
}
