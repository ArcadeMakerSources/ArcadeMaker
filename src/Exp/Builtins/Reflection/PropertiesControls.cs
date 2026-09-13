using System;
using Exp.Spans;
using static Exp.Interpreter;

namespace Exp.Builtins.Reflection;

static partial class Impl
{
    internal const string NS = "reflection";

    [Overrides(NS, null)]
    public static ArrayInstance GetProperties(Instance? _, IValue?[] args)
    {
        List<Instance> props = [];
        Instance input = args[0]!.Inst;

        foreach (var prop in input.Vars)
        {
            var p = prop.ToExpPropertyInst();
            props.Add(p);
        }

        return new ArrayInstance(ClassDefSpan.ExpArrayDef, props.ToArray());
    }

    [Overrides(NS, null)]
    public static Void SetProperty(Instance? _, IValue?[] args)
    {
        Instance? input = null;
        string? pname = null;
        Variable? p = null;

        input = args[0]!.Inst;
        pname = args[1]!.ToString();

        p = input.Vars.FirstOrDefault(prop => prop.Name == pname);
        if (p == null)
            Activated.ThrowRuntime($"Property '{pname}' not found.", RuntimeException.NOT_FOUND);

        p.Value = args[2];

        return Void.Return;
    }

    private static Instance ToExpPropertyInst(this Variable v)
    {
        ClassDefSpan? expPropDef = Activated.definations.FirstOrDefault(d => d is ClassDefSpan expPropDef && d.Namespace == NS && d.Name == "Property") as ClassDefSpan;
        if (expPropDef == null)
            Activated.ThrowRuntime($"{NS}::Property class not found.", RuntimeException.INIT_ERR);
        var p = new Instance(expPropDef);
        p.Vars[0].Value = v.Name.ToExpString();
        p.Vars[1].Value = v.Value;
        p.Vars[2].Value = v.Private.ToExp();
        p.Vars[3].Value = v.Const.ToExp();
        return p;
    }
}
