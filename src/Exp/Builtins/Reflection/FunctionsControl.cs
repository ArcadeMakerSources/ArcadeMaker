using Exp.Spans;
using Microsoft.VisualBasic;
using System;

namespace Exp.Builtins.Reflection;

static partial class Impl
{
    [Overrides(NS, null)]
    public static ArrayInstance GetFunctions(Instance? _, IValue?[] args) => GetFunctionsOrCtors(false, args);

    [Overrides(NS, null)]
    public static ArrayInstance GetConstructors(Instance? _, IValue?[] args) => GetFunctionsOrCtors(true, args);

    [Overrides(NS, "FunctionInfo", "invoke")]
    public static IValue InvokeFuncInfo(Instance? funcInfo, IValue?[] args)
    {
        Interpreter.Activated.ThrowRuntime($"This function is not implemented yet. Use '{NS}::invoke(inst, name, args)' instead.", "NOT_IMPLEMENTED");
        
        return Void.Return;
    }

    [Overrides(NS, null)]
    public static IValue Invoke(Instance _, IValue?[] args)
    {
        Instance? input = null;
        string? fname = null;
        FuncDefSpan? f = null;
        Instance? _args = null;

        // get instance
        input = args[0]!.Inst;

        // get func name
        fname = args[1]!.ToString();

        // get args
        _args = args[2]!.Inst;

        // get func def
        f = input.def.Funcs.FirstOrDefault(func1 => func1.Name == fname && func1.Args.Length == _args.ArrayValues.Length && func1 is not ConstructorDefSpan);
        if (f == null)
            Interpreter.Activated.ThrowRuntime($"function '{fname}(..{_args.ArrayValues.Length})' was not found.", RuntimeException.NOT_FOUND);

        // invoke
        return Interpreter.Activated.FuncCall(input, f, null, out bool _, _args.ArrayValues);
    }

    private static ArrayInstance GetFunctionsOrCtors(bool ctors, IValue?[] args)
    {
        Instance type = args[0]!.Inst;
        type.ThrowIfNotType();

        ClassDefSpan cls = type.GetClassFromExpTypeInstanceOrThrowRuntime(Interpreter.Activated);

        var funcDefs = !ctors ? cls.Funcs.Where(ff => ff is not ConstructorDefSpan) : cls.Funcs.OfType<ConstructorDefSpan>();
        var funcs = new Instance[funcDefs.Count()];
        for (int i = 0; i < funcs.Length; i++)
        {
            var funcInfoDef = Interpreter.Activated.definations.FirstOrDefault(d => d is ClassDefSpan && d.Namespace == NS && d.Name == (ctors ? "ConstructorInfo" : "FunctionInfo")) as ClassDefSpan ?? throw new Exception($"{NS}::(Function/Constructor)Info class not found.");
            var f = new Instance(funcInfoDef);
            f.Vars[0].Value = type;
            if (!ctors)
                f.Vars[1].Value = Interpreter.StringToExpString(funcDefs.ElementAt(i).Name);
            f.Vars[ctors ? 1 : 2].Value = new ArrayInstance(ClassDefSpan.ExpArrayDef, funcDefs.ElementAt(i).Args.Select(a => a.Name.ToExpString()).ToArray());
            f.Vars[ctors ? 2 : 3].Value = funcDefs.ElementAt(i).Private.ToExp();
            f.Vars[ctors ? 3 : 4].Value = funcDefs.ElementAt(i).Static.ToExp();
            funcs[i] = f;
        }

        return new ArrayInstance(ClassDefSpan.ExpArrayDef, funcs);
    }
}