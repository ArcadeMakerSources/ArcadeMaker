using System;
using System.Linq;
using System.Collections.Generic;
using Exp.Spans;
using System.Diagnostics.CodeAnalysis;

namespace Exp;

public interface IValue
{
    bool IsBool => false;
    bool Bool { get { Unexpected(ValueHelper.tbool); throw null; } } // set => Unexpected(ValueHelper.tbool); }
    bool IsChar => false;
    char Char { get { Unexpected(ValueHelper.tchar); throw null; } } // set => Unexpected(ValueHelper.tchar); }
    bool IsNumber => false;
    double Number { get { Unexpected(ValueHelper.tnum); throw null; } } // set => Unexpected(ValueHelper.tnum); }
    bool IsInst => false;
    Instance Inst { get { Unexpected(ValueHelper.tinst); throw null; } } // set => Unexpected(ValueHelper.tinst); }
    bool IsFunc => false;
    FuncPntr FuncPntr { get { Unexpected(ValueHelper.tfunc); throw null; } } // set => Unexpected(ValueHelper.tfunc); }
    object? Object { get; }


    string TypeName { get; }

    [DoesNotReturn]
    void Unexpected(string expected)
    {
        Interpreter.Activated.ThrowRuntime($"A value of type {expected} was expected, but {TypeName} received.", RuntimeException.INVALID_ARGUMENT);
    }

    IValue Pass()
    {
        // unneccsary since value types (bool, char, number) are now readonly
        //if (IsBool)
        //    return Bool.ToExp();
        //if (IsChar)
        //    return Char.ToExp();
        //if (IsNumber)
        //    return Number.ToExp();
        return this;
    }
}

public class BoolValue(bool value) : IValue
{
    public string TypeName => ValueHelper.tbool;
    bool IValue.IsBool => true;
    bool IValue.Bool=> value;
    public bool Bool => value;
    public object Object => value;

    public static implicit operator BoolValue(bool val) => new(val);
    public static implicit operator bool(BoolValue val) => val.Bool;
    public override string ToString() => value ? "true" : "false";
}

public class CharValue(char value) : IValue
{
    public string TypeName => ValueHelper.tchar;
    bool IValue.IsChar => true;
    char IValue.Char => value;
    public object Object => value;

    public static implicit operator CharValue(char val) => new(val);
    public override string ToString() => value.ToString();
}

public readonly struct NumberValue(double value) : IValue
{
    public string TypeName => ValueHelper.tnum;
    bool IValue.IsNumber => true;
    double IValue.Number => value;
    public object Object => value;

    public static implicit operator NumberValue(double val) => new(val);
    public static implicit operator double(NumberValue val) => ((IValue)val).Number;
    public override string ToString() => value.ToString();
}

public readonly struct FuncPntr(FuncDefSpan func, Instance? instance) : IValue
{
    public string TypeName => ValueHelper.tfunc;
    public FuncDefSpan Func => func;
    bool IValue.IsFunc => true;
    FuncPntr IValue.FuncPntr => this;
    public object Object => func;

    public Instance? Instance => instance;

    public IValue? Call(Interpreter interpreter, IEnumerable<IValue?> args)
    {
        return interpreter.FuncCall(instance, func, null, out bool _, args);
    }

    public override string ToString() => func.ToString();
}

public class SpecialValue<T> : IValue
{
    public string TypeName => typeof(T).Name;
    public T? Value { get; set; }
    public override string ToString() => Value?.ToString() ?? "NULL";
    IValue IValue.Pass()
    {
        return SpecialValue.From(Value);
    }
    object? IValue.Object => Value;
}

public class SpecialValue : SpecialValue<object>
{
    public SpecialValue(object value) => Value = value;
    public static SpecialValue<T> From<T>(T val) => new SpecialValue<T> { Value = val };
}

public static class ValueHelper
{
    public const string tbool = "bool", tchar = "char", tnum = "number", tfunc = "function", tinst = "instance";
    internal static void Unexpected(string exp, string rec)
    {
        Interpreter.Activated.ThrowRuntime($"A value of type {exp} was expected, but {rec} received.", RuntimeException.INVALID_ARGUMENT);
    }
}