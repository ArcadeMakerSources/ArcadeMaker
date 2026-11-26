using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Exp.Spans;

namespace Exp;

public class Instance : IVarSystem, IValue, IExpItem //, IEnumerable<IValue>
{
    public static string ItemName { get; } = "object";

    public readonly ClassDefSpan def;
    public List<Variable> Vars { get; }
    public IVarSystem Parent { get; set; }
    public virtual bool IsArray => false;
    public virtual IValue?[] ArrayValues => throw new Exception("This instance is not an array.");
    public string TypeName => def.Name;
    bool IValue.IsInst => true;
    Instance IValue.Inst { get => this; }
    public object Object => this;

    public Instance(ClassDefSpan def, bool addProperties = true)
    {
        ArgumentNullException.ThrowIfNull(def);

        this.def = def;
        this.Vars = [];

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

    //internal bool GetBasearray([NotNullWhen(true)] out Instance? basearr)
    //{
    //    if (IsArray)
    //    {
    //        basearr = this;
    //        return true;
    //    }

    //    var basearrProp = def.Props.FirstOrDefault(p => p.BaseArray);
    //    if (basearrProp == null)
    //    {
    //        basearr = null;
    //        return false;
    //    }
    //    basearr = Vars.FirstOrDefault(v => v.Name == basearrProp.Name)?.Value?.Inst;
    //    if (basearr == null)
    //        Interpreter.Activated.ThrowRuntime($"Value of {def.GetExpTypeName(false)}.{basearrProp.Name} was null.", RuntimeException.INVALID_OPERATION);
    //    else if (basearr == this)
    //        Interpreter.Activated.ThrowRuntime($"Value of {def.GetExpTypeName(false)}.{basearrProp.Name} was a reference to the same instance in which it was declared.", RuntimeException.INVALID_OPERATION);

    //    return true;
    //}
    
    internal static void StartIteration(ForEachLoopSpan loopSpan)
    {
        _enumeratorReqLoop = loopSpan;
    }
    
    private static ForEachLoopSpan? _enumeratorReqLoop;
    //IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public ForEachLoopSpan.Enumerator GetEnumerator()
    {
        var loopSpan = _enumeratorReqLoop;
        _enumeratorReqLoop = null;
        
        var inter = Interpreter.Activated;
        
        if (!def.HasTag(AttributeDefSpan.IterableAttr))
        {
            //if (GetBasearray(out var basearr))
            //    return basearr.GetEnumerator();
            inter.ThrowRuntime("An attempt to iterate over a non-iterable object. In order to execute a foreach loop on an instance, mark its class as @Iterable.", RuntimeException.INVALID_OPERATION);
        }
        
        var getIter = def.Funcs.First(f => f is { Name: "getIterator", Args.Length: 0, Static: false });
        var iterator = inter.FuncCall(this, getIter, null, out bool _, [], loopSpan).Inst;
        return new ForEachLoopSpan.Enumerator(inter, iterator, loopSpan);
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

public class ArrayInstance : Instance
{
    public ArrayInstance(ClassDefSpan cls, IValue[] array, bool addProperties = true) : base(cls, addProperties)
    {
        ArrayValues = array;
    }

    public override bool IsArray => true;
    public override IValue?[] ArrayValues { get; }
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