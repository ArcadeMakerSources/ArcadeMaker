using System;
using System.Linq;
using System.Collections.Generic;
using Exp.Spans;

namespace Exp;

public class Instance : IVarSystem, IValue, IExpItem
{
    public static string ItemName { get; } = "object";

    public readonly ClassDefSpan def;
    public List<Variable> Vars { get; }
    public IVarSystem Parent { get; set; }
    public bool IsArray => ArrayValues != null;
    internal IValue?[]? ArrayValues { get; }

    public string TypeName => def.Name;
    bool IValue.IsInst => true;
    Instance IValue.Inst { get => this; }
    public object Object => this;

    public Instance(ClassDefSpan def, IValue?[]? arrVals = null, bool addProperties = true)
    {
        ArgumentNullException.ThrowIfNull(def);

        this.def = def;
        this.Vars = [];
        this.ArrayValues = arrVals;

        if (addProperties)
        {
            foreach (var prop in def.Props)
            {
                var var = new Variable(prop.Name, null, def, prop.Private, prop.Const);
                var.firstSet = true;
                Vars.Add(var);
            }
        }
    }

    /// <summary>
    /// Returns the array on which a foreach loop would iterate, or throws a runtime exception if the <see cref="Instance"/> is not an array and hasn't a basearray property.
    /// </summary>
    /// <returns>The array on which a foreach loop would iterate.</returns>
    internal Instance GetBasearray(bool orNull = true)
    {
        if (IsArray)
            return this;

        var basearr = def.Props.FirstOrDefault(p => p.BaseArray);
        if (basearr == null)
        {
            if (orNull)
                return null;
            Interpreter.Activated.ThrowRuntime($"{((IDefination)def).FullName} does not contain a property which is marked as basearray.", RuntimeException.INVALID_OPERATION);
        }
        Instance val = Vars.FirstOrDefault(v => v.Name == basearr.Name)?.Value?.Inst;
        if (val == null)
            Interpreter.Activated.ThrowRuntime($"Value of {basearr.Name} was null.", RuntimeException.INVALID_OPERATION);
        else if (val == this)
            Interpreter.Activated.ThrowRuntime($"Value of {basearr.Name} was a reference to the same instance in which it declared.", RuntimeException.INVALID_OPERATION);

        return val.GetBasearray();
    }

    public override string ToString()
    {
        if (def == ClassDefSpan.ExpStringDef)
            return Interpreter.ExpStringToString(this);
        if (def.ToString != null)
        {
            if (Interpreter.Activated?.FuncCall(this, def.ToString, null, out var _, []) is Instance str && str.def == ClassDefSpan.ExpStringDef)
                return Interpreter.ExpStringToString(str);
        }
        return def.Name;
    }

    public override bool Equals(object obj)
    {
        if (def.Equalizer != null && Interpreter.Activated != null && obj is IValue arg)
        {
            var result = Interpreter.Activated.FuncCall(this, def.Equalizer, null, out var _, [arg]);
            if (result?.IsBool == true)
                return result.Bool;
            Interpreter.Activated.ThrowRuntime($"Equalizer function must return a boolean, but it returned {result.GetExpTypeName(true)}.", RuntimeException.INVALID_OPERATION, def.Equalizer);
            throw null;
        }
        return ReferenceEquals(this, obj);
    }

    public static Instance operator +(Instance a, object? b)
    {
        if (a.def == ClassDefSpan.ExpStringDef || (b as Instance)?.def == ClassDefSpan.ExpStringDef)
            return (a.ToString() + (b?.ToString() ?? "NULL")).ToExpString();
        throw new InvalidOperationException($"Cannot add {Extensions.GetExpTypeName(b, true)} to {((IDefination)a.def).FullName}.");
    }

    public static Instance operator +(object? a, Instance b)
    {
        if (b.def == ClassDefSpan.ExpStringDef || (a as Instance)?.def == ClassDefSpan.ExpStringDef)
            return ((a?.ToString() ?? "NULL") + b.ToString()).ToExpString();
        throw new InvalidOperationException($"Cannot add {Extensions.GetExpTypeName(b, true)} to {((IDefination)b.def).FullName}.");
    }
    
    public ClassDefSpan GetClassFromExpTypeInstanceOrThrowRuntime(Interpreter interpreter)
    {
        if (def != ClassDefSpan.ExpTypeDef)
            interpreter.ThrowRuntime(ClassDefSpan.ExpTypeDef.GetExpTypeName(false) + " was expected, but " + def.GetExpTypeName(false) + " was received.", RuntimeException.INVALID_ARGUMENT);
        return ((SpecialValue<ClassDefSpan>)Vars[2].Value!).Value!;
    }
}

public class ExternTypeInstance : Instance
{
    public object ExternInstance { get; }
    public ExternTypeInstance(object inst) : base(ClassDefSpan.ExternTypeValueDef)
    {
        ArgumentNullException.ThrowIfNull(inst);

        this.ExternInstance = inst;

        // load properties
        foreach (var prop in inst.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public))
        {
            var var = new ExternPropertyVar(inst, prop);
            Vars.Add(var);
        }
    }
}