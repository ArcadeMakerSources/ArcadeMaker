using Exp.Spans;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Exp;

/// <summary>
/// Representes a variable.
/// </summary>
public class Variable : IExpItem, INamedValue
{
    /// <summary>
    /// This variable is returned by <see cref="Operations.PointingOrFuncCall.Read()"/> when the experssion is in format <c>x?.y = z</c> and <c>x</c> is <c>null</c>. The <see cref="Assignment"/> operation is cancelling itself if it gets that var.
    /// </summary>
    internal static Variable Futile { get; } = new("<<futile_variable>>", null, null);
    public static string ItemName { get; } = "variable";
    public string Name { get; }
    internal bool firstSet = true;

    public bool IsVar => true;
    public virtual IValue? Value
    {
        get;
        set
        {
            if (Const && !firstSet)
            {
                Interpreter.Activated.ThrowRuntime($"This variable ('{Name}') is marked as constant and cannot be set.", RuntimeException.INVALID_OPERATION);
                return;
            }
            field = value;
            firstSet = false;
        }
    }
    public void SetSkippingConstant(IValue value)
    {
        firstSet = true;
        this.Value = value;
    }

    public bool Private { get; set; }
    internal bool Const { get; }
    internal Span? SettingSpan { get; }

    public Variable(string name, IValue? value, Span? settingSpan = null, bool prvt = false, bool cons = false)
    {
        this.Name = name;
        this.Value = value;
        this.SettingSpan = settingSpan;
        this.Private = prvt;
        this.Const = cons;
    }
}

class ExternPropertyVar : Variable
{
    private readonly PropertyInfo pinfo;
    private bool initSetComplete;
    private readonly object inst;

    public bool IsOptimized => getter != null || setter != null;
    private dynamic? getter = null;
    private dynamic? setter = null;
    
    internal ExternPropertyVar(object inst, PropertyInfo pinfo) : base(pinfo.Name.StartWithLowerCase(), null, null)
    {
        ArgumentNullException.ThrowIfNull(inst);
        ArgumentNullException.ThrowIfNull(pinfo);

        this.pinfo = pinfo;
        this.inst = inst;
    }

    [Obsolete("Performances of this approach have not been tested yet.")]
    public void Optimize()
    {
        if (IsOptimized)
            return;

        // create Func<T> instance for the property getter, which can be used for fast invocation
        Type getterType = typeof(Func<>).MakeGenericType(pinfo.PropertyType);
        getter = pinfo.GetMethod != null ? Delegate.CreateDelegate(getterType, inst, pinfo.GetMethod) : null;

        // create Action<T> instance for the property setter, which can be used for fast invocation
        Type setterType = typeof(Action<>).MakeGenericType(pinfo.PropertyType);
        setter = pinfo.SetMethod != null ? Delegate.CreateDelegate(setterType, inst, pinfo.SetMethod) : null;
    }

    public override IValue? Value
    {
        get { try { return getter?.Invoke() ?? Interpreter.CsValToExpVal(pinfo.GetValue(inst)); } catch (Exception ex) { Catch(ex); throw null; } }
        set
        {
            if (!initSetComplete)
                initSetComplete = true;
            else
            {
                try
                {
                    if (setter == null)
                        pinfo.SetValue(inst, Interpreter.ExpValToCsVal(value));
                    else
                        setter.Invoke(value);
                }
                catch (Exception ex) { Catch(ex); }
            }
        }
    }

    [DoesNotReturn]
    private static void Catch(Exception ex)
    {
        ex = ex.InnerException ?? ex;
        Interpreter.Activated.ThrowRuntime(ex.GetType().ToString() + ": " + ex.Message, RuntimeException.EXTERN_OPERATION_FAILED);
        throw null;
    }
}

class ClassStaticVar : Variable, IClassMember, IExpItem
{
    public static new string ItemName { get; } = "class static variable";
    public List<Span[]> TagsCode { get; set; }
    public Instance[] AttrInfo { get; set; }
    public ClassDefSpan Def { get; set; }
    internal Span[] InitValueCode { get; set; }

    internal ClassStaticVar(string name, IValue value, ClassDefSpan def, Span settingSpan, bool prvt = false, bool cons = false) : base(name, value, settingSpan, prvt, cons) { this.Def = def; }
}

public class TypeVariable(string varName, IValue? initVal, Func<IValue?, bool> checker, string typeName, bool prvt = false, bool constant = false, bool skipCheckOnInit = false) : Variable(varName, initVal, null, prvt, constant)
{
    public override IValue? Value
    {
        get => base.Value;
        set
        {
            if (skipCheckOnInit || checker == null || checker(value))
                base.Value = value;
            else
                Interpreter.Activated.ThrowRuntime($"The value of '{base.Name}' must be of type {typeName} (Given value was {value?.TypeName ?? "null"}).", RuntimeException.INVALID_OPERATION);
            skipCheckOnInit = false;
        }
    }
}

public class CustomVariable : Variable
{
    public override IValue? Value { get => Getter?.Invoke(); set => Setter?.Invoke(value); }
    public Func<IValue?>? Getter { get; set; }
    public Action<IValue?>? Setter { get; set; }

    public CustomVariable(string name, Func<IValue?> getter, Action<IValue?>? setter, bool prvt = false) : base(name, null, null, prvt, setter == null)
    {
        this.Getter = getter;
        this.Setter = setter;
    }
}