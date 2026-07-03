
using Exp.Operations;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Exp.Spans;

interface IKeyword
{
    static abstract string Keyword { get; }
}

interface ISymbol
{
    static abstract string Symbol { get; }
}

interface IExpItem
{
    static abstract string ItemName { get; }
    string GetItemName()
    {
        if (this is ClassDefSpan)
            return ClassDefSpan.ItemName;
        if (this is ExternClassDefSpan)
            return ExternClassDefSpan.ItemName;
        if (this is AttributeDefSpan)
            return AttributeDefSpan.ItemName;
        if (this is ConstructorDefSpan)
            return ConstructorDefSpan.ItemName;
        if (this is FuncDefSpan)
            return FuncDefSpan.ItemName;
        if (this is IfConditionSpan)
            return IfConditionSpan.ItemName;
        if (this is WhileConditionSpan)
            return WhileConditionSpan.ItemName;
        if (this is ForLoopSpan)
            return ForLoopSpan.ItemName;
        if (this is ForEachLoopSpan)
            return ForEachLoopSpan.ItemName;
        return GetType().GetProperty(nameof(ItemName), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)?.GetValue(null) as string ?? "Item";
    }
}

public abstract class Span
{
    public ScriptDocument Document { get; set; }
    public int DocumentLocation { get; set; }
    internal string Text { get; }
    internal virtual Span Container { get; set; }

    internal IVarSystem GetVS()
    {
        return Container as IVarSystem ?? Interpreter.Activated;
    }

    internal virtual string FullText
    {
        get => Text;
    }

    internal Span(string text)
    {
        this.Text = text;
    }

    internal void SetContainer(Span[] spans, Span container = null)
    {
        ArgumentNullException.ThrowIfNull(spans);
        container ??= this;

        foreach (var span in spans)
        {
            span.Container = container;
            if (container is IContext parent && span is IContext vs)
                vs.Parent = parent;
        }
    }

    public override string ToString()
    {
        return Text;
    }
}

public class EndOfCodeSpan : Span
{
    public EndOfCodeSpan(ScriptDocument doc) : base("<<EndOfCode>>")
    {
        base.Document = doc;
        base.DocumentLocation = doc.Script.Length;
    }
}

public class WordSpan : Span
{
    internal WordSpan(string text) : base(text) { }
}

abstract class OperatorSpan : Span
{
    internal OperatorSpan(string op) : base(op) { }
    internal abstract IValue Result(IValue left, IValue right);
    internal virtual IValue Result(IReadingOperation left, IReadingOperation right) => Result(left?.Read(), right?.Read());
    internal abstract bool TwoSides { get; }
    internal virtual bool Action { get; } = false;
    protected static string TypeOrNull(IValue obj)
    {
        return Extensions.GetExpTypeName(obj, true);
    }
    protected static void OperationFailed(string err, Span throwing = null)
    {
        Interpreter.Activated.ThrowRuntime(err, RuntimeException.INVALID_OPERATION, throwing);
    }
    protected void OperationFailed(string err)
    {
        Interpreter.Activated.ThrowRuntime(err, RuntimeException.INVALID_OPERATION, this);
    }
}

class PrintWordSpan : WordSpan, IKeyword
{
    public static string Keyword { get; } = "print";
    internal PrintWordSpan() : base(Keyword) { }
}

class LenofWordSpan : WordSpan, IKeyword
{
    public static string Keyword { get; } = "lenof";
    internal LenofWordSpan() : base(Keyword) { }
}

class NumberSpan : Span
{
    internal double Number;
    internal NumberSpan(double num) : base(num.ToString())
    {
        this.Number = num;
    }
    internal NumberSpan(string text) : base(text)
    {
        this.Number = Convert.ToDouble(text);
    }
}

class StringSpan : Span
{
    private readonly bool escaped;
    internal override string FullText
    {
        get => (escaped ? "@" : "") + "\"" + Text + "\"";
    }
    internal StringSpan(string text, bool escaped = false) : base(text)
    {
        this.escaped = escaped;
    }
}

class CharSpan : Span
{
    internal override string FullText => $"'{Text.Replace("\n", @"\n")}'";
    internal CharSpan(char c) : base("" + c) { }
}

class CountSpan : Span
{
    internal int Count { get; }
    internal CountSpan(int count) : base(".." + count)
    {
        this.Count = count;
    }
}

class NamespaceWordSpan : WordSpan, IKeyword
{
    public static string Keyword { get; } = "namespace";
    internal string Namespace { get; }
    internal NamespaceWordSpan(string ns) : base(Keyword)
    {
        this.Namespace = ns;
    }
    internal override string FullText => $"{Keyword} {Namespace}:";
}

class UsingWordSpan : WordSpan, IKeyword
{
    public static string Keyword { get; } = "using";
    internal string Namespace { get; }
    internal UsingWordSpan(string ns) : base(Keyword)
    {
        this.Namespace = ns;
    }
    internal override string FullText => $"{Keyword} {Namespace}";
}

class ExternClassDefSpan : WordSpan, IDefination, IKeyword, IExpItem
{
    public static string Keyword { get; } = "extern";
    public static string ItemName { get; } = "extern class";
    public string Name => RefName;
    public string Namespace { get; set; }
    internal string RefName { get; }
    internal Type Type { get; }
    internal MethodInfo[] Methods { get; }
    internal ConstructorInfo[] Constructors { get; }
    internal PropertyInfo[] Props { get; }
    public bool IsEnum { get; }
    public Dictionary<string, IValue> EnumValues { get; } = [];
    internal ExternClassDefSpan(string refName, Type type) : base(Keyword)
    {
        this.RefName = refName;
        this.Type = type;
        this.IsEnum = type.IsEnum;

        if (IsEnum)
        {
            Array vals = type.GetEnumValues();
            type.GetEnumNames().ForEach((name, index) => EnumValues.Add(name.StartWithLowerCase(), Interpreter.CsValToExpVal(vals.GetValue(index))));
        }

        // get all public static methods / properties of the type
        this.Methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);
        this.Constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
        this.Props = type.GetProperties(BindingFlags.Public | BindingFlags.Static);
    }
    internal override string FullText => $"{Keyword} {RefName} = \"{Type}\"";
}

class DefNameSpan : WordSpan
{
    internal string SpecificNs { get; }
    internal string Name { get; }
    internal ClassDefSpan Class { get; private set; }
    internal FuncDefSpan Func { get; private set; }
    internal ExternClassDefSpan Extern { get; private set; }
    internal AttributeDefSpan Attr { get; private set; }
    internal bool IsUnknownItem { get; private set; }
    internal IDefination Defination => Class ?? Func ?? (IDefination)Extern ?? Attr;
    internal event EventHandler Resolved;
    internal bool CancelResolve { get; set; }
    internal static bool CancelResolveForNewOnes { get; set; }
    internal DefNameSpan(string specNs, string name, ScriptDocument doc, int docLoc, Interpreter compiler) : base(name)
    {
        (this.SpecificNs, this.Name, Document, DocumentLocation) = (specNs, name, doc, docLoc);
        void Resolve(object sender, EventArgs e)
        {
            if (CancelResolve)
                return;

            compiler ??= Interpreter.Activated;
            var defs = compiler.definations.Where(d => specNs == null ? d.Namespace == null || d.Namespace == Document.Namespace || (Document.Usings?.Contains(d.Namespace) == true) : d.Namespace == specNs);
            var matches = defs.Where(d => d.Name == name);

            // check ambiguous reference
            IDefination def = matches.FirstOrDefault(d => d.Namespace == null);
            int matchesCount = matches.Count();
            if (specNs == null && matchesCount >= 2 && def == null)
            {
                const string advice = "Specify namespace to select the wanted one";
                if (matchesCount == 2)
                    compiler.Error($"'{name}' is an ambiguous reference between '{matches.First().FullName}' and '{matches.ElementAt(1).FullName}'. {advice}.", this);
                else if (matchesCount >= 3)
                    compiler.Error($"'{name}' is an ambiguous reference between '{matches.Map(m => m.FullName).ToString("', '")}'. {advice}.", this);
            }
            def ??= matches.FirstOrDefault();

            if (def is ClassDefSpan cls)
                Class = cls;
            else if (def is AttributeDefSpan attr)
                Attr = attr;
            else if (def is ExternClassDefSpan ext)
                Extern = ext;
            else
            {
                compiler.Error($"Unknown type '{FullText}'.", this);
                IsUnknownItem = true;
            }
            Resolved?.Invoke(this, null);
            compiler.CollectDefsCompleted -= Resolve;
        };

        if (!CancelResolveForNewOnes)
        {
            if (compiler.CollectedDefs)
                Resolve(compiler, null);
            else
                compiler.CollectDefsCompleted += Resolve;
        }
    }

    internal override string FullText => (SpecificNs == null ? "" : (SpecificNs + NamespaceSpecificationSpan.Symbol)) + Name;
}

class TypeOfSpan : WordSpan
{
    internal Instance Value { get; set; }
    private TypeOfSpan(Instance value) : base("<>") => this.Value = value;
    internal TypeOfSpan(ExternClassDefSpan ext) : this(ext.Type.AsExtern()) { }
    internal TypeOfSpan(ClassDefSpan cls) : this(cls.ExpType) { }
    internal TypeOfSpan(AttributeDefSpan attr) : this(attr.ExpType) { }
    internal TypeOfSpan() : this(value: null) { }
    internal override string FullText => LowerThanOperatorSpan.Symbol + Value?.def.Namespace + NamespaceSpecificationSpan.Symbol + Value.Vars.Find(v => v.Name == "name").Value.Inst.ToString() + GreaterThanOperatorSpan.Symbol;
}

public class NamespaceSpecificationSpan : WordSpan, ISymbol
{
    public static string Symbol { get; } = "::";
    internal NamespaceSpecificationSpan() : base(Symbol) { }
}

class SetWordSpan : WordSpan, IKeyword
{
    public static string Keyword { get; } = "var";
    internal SetWordSpan() : base(Keyword) { }
}

class StaticWordSpan : WordSpan, IKeyword
{
    public static string Keyword { get; } = "static";
    internal StaticWordSpan() : base(Keyword) { }
}

class SetSymbolSpan : OperatorSpan, ISymbol
{
    public static string Symbol { get; } = "=";
    internal SetSymbolSpan() : base(Symbol) { }
    internal override bool Action { get; } = true;
    internal override bool TwoSides { get; } = true;
    internal override IValue Result(IValue left, IValue right) => right;
    internal override IValue Result(IReadingOperation left, IReadingOperation right) => Result(null, right.Read());
}

class PlusPlusOperatorSpan : OperatorSpan, ISymbol
{
    public static string Symbol { get; } = "++";
    internal PlusPlusOperatorSpan() : base(Symbol) { }
    internal override bool TwoSides => false;
    internal override bool Action => true;
    internal override IValue Result(IValue left, IValue right = null)
    {
        if (left == null)
            OperationFailed("Cannot apply {Symbol} on null.");
        return (left.Number + 1).ToExp();
    }
}

class MinusMinusOperatorSpan : OperatorSpan, ISymbol
{
    public static string Symbol { get; } = "--";
    internal MinusMinusOperatorSpan() : base(Symbol) { }
    internal override bool TwoSides => false;
    internal override bool Action => true;
    internal override IValue Result(IValue left, IValue right = null)
    {
        if (left == null)
            OperationFailed("Cannot apply {Symbol} on null.");
        return (left.Number - 1).ToExp();
    }
}

class PlusOperatorSpan : OperatorSpan, ISymbol
{
    public static string Symbol { get; } = "+";
    internal override bool TwoSides { get; } = true;
    internal PlusOperatorSpan() : base(Symbol) { }
    internal override IValue Result(IValue left, IValue right) => GetResult(left, right, this);
    internal static IValue GetResult(IValue left, IValue right, Span thrower)
    {
        if (left != null || right != null)
        {
            if (left == null && right != null && right.IsInst)
                return (null as object) + right.Inst;
            else if (left != null && left.IsInst && right == null)
                return left.Inst + (null as object);

            if (left != null && right != null)
            {
                if (left.IsNumber && right.IsNumber)
                    return (left.Number + right.Number).ToExp();
                else if (left.IsChar && right.IsChar)
                    return ((double)left.Char + right.Char).ToExp();
                else if (left.IsInst)
                    return left.Inst + right;
                else if (right.IsInst)
                    return left + right.Inst;
            }
        }

        OperationFailed($"Cannot add {TypeOrNull(right)} to {TypeOrNull(left)}.", thrower);
        return null;
    }
}

class MinusOperatorSpan : OperatorSpan, ISymbol
{
    public static string Symbol { get; } = "-";
    internal override bool TwoSides { get; } = true;
    internal MinusOperatorSpan() : base(Symbol) { }
    internal override IValue Result(IValue left, IValue right) => GetResult(left, right, this);
    internal static IValue GetResult(IValue left, IValue right, Span throwing)
    {
        if (left != null && right != null)
        {
            if (left.IsNumber && right.IsNumber)
                return (left.Number - right.Number).ToExp();
            else if (left.IsChar && right.IsChar)
                return ((char)(left.Char - right.Char)).ToExp();
            else if (left.IsNumber && right.IsChar)
                return ((char)(left.Number - right.Char)).ToExp();
            else if (left.IsChar && right.IsNumber)
                return ((char)(left.Char - right.Number)).ToExp();
        }
        OperationFailed($"Cannot subtract {TypeOrNull(right)} from {TypeOrNull(left)}.", throwing);
        throw null;
    }
}

class MultiplyOperatorSpan : OperatorSpan, ISymbol
{
    public static string Symbol { get; } = "*";
    internal override bool TwoSides { get; } = true;
    internal MultiplyOperatorSpan() : base(Symbol) { }
    internal override IValue Result(IValue left, IValue right)
    {
        if (left != null && right != null)
        {
            if (left.IsNumber && right.IsNumber)
                return (left.Number * right.Number).ToExp();
            if (left.IsString() && right.IsNumber)
            {
                string str = "", original = Interpreter.ExpStringToString(left.Inst);
                for (int i = 0; i < Math.Floor(right.Number); i++)
                    str += original;
                return str.ToExpString();
            }
        }

        OperationFailed($"Cannot multiply {TypeOrNull(left)} by {TypeOrNull(right)}.");
        throw null;
    }
}

class DivideOperatorSpan : OperatorSpan, ISymbol
{
    public static string Symbol { get; } = "/";
    internal override bool TwoSides { get; } = true;
    internal DivideOperatorSpan() : base(Symbol) { }
    internal override IValue Result(IValue left, IValue right)
    {
        if (left != null && right != null && left.IsNumber && right.IsNumber)
        {
            if (right.Number == 0)
                Interpreter.Activated.ThrowRuntime("Divide by zero.", RuntimeException.DIVIDE_BY_ZERO);
            return (left.Number / right.Number).ToExp();
        }
        else
            OperationFailed($"Cannot divide {TypeOrNull(left)} by {TypeOrNull(right)}.");
        throw null;
    }
}

class ModuluOperatorSpan : OperatorSpan, ISymbol
{
    public static string Symbol { get; } = "%";
    internal override bool TwoSides { get; } = true;
    internal ModuluOperatorSpan() : base(Symbol) { }
    internal override IValue Result(IValue left, IValue right)
    {
        if (left != null && right != null && left.IsNumber && right.IsNumber)
        {
            if (right.Number == 0)
                Interpreter.Activated.ThrowRuntime("Divide by zero.", RuntimeException.DIVIDE_BY_ZERO);
            return (left.Number % right.Number).ToExp();
        }
        else
            OperationFailed($"Cannot divide {TypeOrNull(left)} by {TypeOrNull(right)}.");
        throw null;
    }
}

class EqualsOperatorSpan : OperatorSpan, ISymbol
{
    public static string Symbol { get; } = "==";
    internal override bool TwoSides { get; } = true;
    internal EqualsOperatorSpan() : base(Symbol) { }
    internal override IValue Result(IValue left, IValue right) => GetResult(left, right);
    internal static BoolValue GetResult(IValue left, IValue right)
    {
        if (left == null)
            return right == null;
        else if (right == null)
            return left?.IsInst == true ? left.Equals(right) : left == right;
        else if (left.IsBool && right.IsBool)
            return left.Bool == right.Bool;
        else if (left.IsNumber && right.IsNumber)
            return left.Number == right.Number;
        else if (left.IsChar && right.IsChar)
            return left.Char == right.Char;
        else if (left.IsNumber && right.IsChar)
            return left.Number == right.Char;
        else if (left.IsChar && right.IsNumber)
            return left.Char == right.Number;
        //else if (left.IsInst && right.IsInst)
        //    return (left.Inst.IsString() && right.Inst.IsString()) ? left.Inst.ExpStringEquals(right.Inst) : left.Inst.Equals(right.Inst);
        else if (left.Equals(right)) // for funcs. and for instances that wouldn't work as Instance == Instance operator is overrided
            return true;
        else if (left is SpecialValue<object> l && right is SpecialValue<object> r)
            return l.Value == r.Value;
        return false;
    }
}

class NotEqualsOperatorSpan : OperatorSpan, ISymbol
{
    public static string Symbol { get; } = "!=";
    internal override bool TwoSides { get; } = true;
    internal NotEqualsOperatorSpan() : base(Symbol) { }
    internal override BoolValue Result(IValue left, IValue right) => !((IValue)EqualsOperatorSpan.GetResult(left, right)).Bool;
}

class GreaterThanOperatorSpan : OperatorSpan, ISymbol
{
    public static string Symbol { get; } = ">";
    internal override bool TwoSides { get; } = true;
    internal GreaterThanOperatorSpan() : base(Symbol) { }
    internal override BoolValue Result(IValue left, IValue right)
    {
        if (left != null && right != null)
        {
            if (left.IsNumber && right.IsNumber)
                return left.Number > right.Number;
            else if (left.IsChar && right.IsChar)
                return left.Char > right.Char;
            else if (left.IsNumber && right.IsChar)
                return left.Number > right.Char;
            else if (left.IsChar && right.IsNumber)
                return left.Char > right.Number;
        }

        OperationFailed($"Cannot compare {TypeOrNull(left)} to {TypeOrNull(right)}.");
        throw null;
    }
}

class LowerThanOperatorSpan : OperatorSpan, ISymbol
{
    public static string Symbol { get; } = "<";
    internal override bool TwoSides { get; } = true;
    internal LowerThanOperatorSpan() : base(Symbol) { }

    internal override BoolValue Result(IValue left, IValue right)
    {
        if (left != null && right != null)
        {
            if (left.IsNumber && right.IsNumber)
                return left.Number < right.Number;
            else if (left.IsChar && right.IsChar)
                return left.Char < right.Char;
            else if (left.IsNumber && right.IsChar)
                return left.Number < right.Char;
            else if (left.IsChar && right.IsNumber)
                return left.Char < right.Number;
        }

        OperationFailed($"Cannot compare {TypeOrNull(left)} to {TypeOrNull(right)}.");
        throw null;
    }
}

class EqualsOrGreaterOperatorSpan : OperatorSpan, ISymbol
{
    public static string Symbol { get; } = ">=";
    internal override bool TwoSides { get; } = true;
    internal EqualsOrGreaterOperatorSpan() : base(Symbol) { }
    internal override BoolValue Result(IValue left, IValue right)
    {
        if (left != null && right != null)
        {
            if (left.IsNumber && right.IsNumber)
                return left.Number >= right.Number;
            else if (left.IsChar && right.IsChar)
                return left.Char >= right.Char;
            else if (left.IsNumber && right.IsChar)
                return left.Number >= right.Char;
            else if (left.IsChar && right.IsNumber)
                return left.Char >= right.Number;
        }
        OperationFailed($"Cannot compare {TypeOrNull(left)} to {TypeOrNull(right)}.");
        throw null;
    }
}

class EqualsOrLowerOperatorSpan : OperatorSpan, ISymbol
{
    public static string Symbol { get; } = "<=";
    internal override bool TwoSides { get; } = true;
    internal EqualsOrLowerOperatorSpan() : base(Symbol) { }
    internal override BoolValue Result(IValue left, IValue right)
    {
        if (left != null && right != null)
        {
            if (left.IsNumber && right.IsNumber)
                return left.Number <= right.Number;
            else if (left.IsChar && right.IsChar)
                return left.Char <= right.Char;
            else if (left.IsNumber && right.IsChar)
                return left.Number <= right.Char;
            else if (left.IsChar && right.IsNumber)
                return left.Char <= right.Number;
        }
        OperationFailed($"Cannot compare {TypeOrNull(left)} to {TypeOrNull(right)}.");
        throw null;
    }
}

class AndOperatorSpan : OperatorSpan, ISymbol
{
    public static string Symbol { get; } = "&";
    internal override bool TwoSides { get; } = true;
    internal AndOperatorSpan() : base(Symbol) { }
    internal override IValue Result(IReadingOperation left, IReadingOperation right)
    {
        /*
        if (left.IsBool && right.IsBool)
            return left.Bool && right.Bool;
        else
            OperationFailed($"The {Text} symbol must appear between 2 booleans.");
        return null;*/
        return (left?.Read()?.Bool == true && right?.Read()?.Bool == true).ToExp();
    }
    internal override BoolValue Result(IValue left, IValue right) => left.Bool && right.Bool;
}

class OrOperatorSpan : OperatorSpan, ISymbol
{
    public static string Symbol { get; } = "|";
    internal override bool TwoSides { get; } = true;
    internal OrOperatorSpan() : base(Symbol) { }
    internal override BoolValue Result(IReadingOperation left, IReadingOperation right)
    {
        /*
        if (left.IsBool && right.IsBool)
            return left.Bool && right.Bool;
        else
            OperationFailed($"The {Text} symbol must appear between 2 booleans.");
        return null;*/
        return left?.Read()?.Bool == true || right?.Read()?.Bool == true;
    }
    internal override BoolValue Result(IValue left, IValue right) => left.Bool || right.Bool;
}

class NullCoalescingOperatorSpan : OperatorSpan, ISymbol
{
    public static string Symbol { get; } = "??";
    internal override bool TwoSides { get; } = true;
    internal NullCoalescingOperatorSpan() : base(Symbol) { }
    internal Instance NullCoalsingEx { get; set; }
    internal override IValue Result(IValue left, IValue right)
    {
        IValue value = left ?? right;
        if (value is ThrowWordSpan)
            Interpreter.Activated.ThrowRuntime(NullCoalsingEx.Vars[0].Value.ToString(), NullCoalsingEx.Vars[1].Value.ToString(), this);
        return value;
    }
}

class SetPlusOperatorSpan : OperatorSpan, ISymbol
{
    public static string Symbol { get; } = "+=";
    internal SetPlusOperatorSpan() : base(Symbol) { }
    internal override bool TwoSides => true;
    internal override bool Action => true;
    internal override IValue Result(IValue left, IValue right) => PlusOperatorSpan.GetResult(left, right, this);
}

class SetMinusOperatorSpan : OperatorSpan, ISymbol
{
    public static string Symbol { get; } = "-=";
    internal SetMinusOperatorSpan() : base(Symbol) { }
    internal override bool TwoSides => true;
    internal override bool Action => true;
    internal override IValue Result(IValue left, IValue right) => MinusOperatorSpan.GetResult(left, right, this);
}

class QuestionMarkSpan : WordSpan, ISymbol
{
    public static string Symbol { get; } = "?";
    internal QuestionMarkSpan() : base(Symbol) { }
}

class PrivateWordSpan : WordSpan, IKeyword
{
    public static string Keyword { get; } = "private";
    internal PrivateWordSpan() : base(Keyword) { }
}

class BaseArrayWordSpan : WordSpan, IKeyword
{
    public static string Keyword { get; } = "basearray";
    internal BaseArrayWordSpan() : base(Keyword) { }
}

class ConstWordSpan : WordSpan, IKeyword
{
    public static string Keyword { get; } = "const";
    internal ConstWordSpan() : base(Keyword) { }
}

class IsWordSpan : WordSpan, IKeyword
{
    public static string Keyword { get; } = "is";
    internal IsWordSpan() : base(Keyword) { }
}

class TrueWordSpan : WordSpan, IKeyword
{
    public static string Keyword { get; } = "true";
    internal TrueWordSpan() : base(Keyword) { }
}

class FalseWordSpan : WordSpan, IKeyword
{
    public static string Keyword { get; } = "false";
    internal FalseWordSpan() : base(Keyword) { }
}

class NotSymbolSpan : WordSpan, ISymbol
{
    public static string Symbol { get; } = "!";
    internal NotSymbolSpan() : base(Symbol) { }
}

// an item that has its own vars list
public interface IVarSystem
{
    internal List<Variable> Vars { get; }
    internal IVarSystem Parent { get; set; }

    internal IEnumerable<IValue> BackupValues() => Vars.Map(v => v.Value);
    internal void RestoreValues(IEnumerable<IValue> values)
    {
        int i = 0;
        foreach (var val in values)
            Vars[i++].SetSkippingConstant(val);
    }
}

public interface IContext : IVarSystem
{
    //IVarSystem OuterVarSystem { get; }
    Span[] InnerSource { get; set; }
    IOperation[] Operations { get; set; }
    //IContext Context { get; set; }
}

interface ILoopContext : IContext
{
    bool Break { get; set; }
    bool Continue { get; set; }
    string Counter { get; set; }
    string Id { get; set; }
}

public interface IDefination
{
    string Name { get; }
    string? Namespace { get; set; }
    public string FullName { get => (Namespace == null ? "" : (Namespace + NamespaceSpecificationSpan.Symbol)) + Name; }
}

abstract class ConditionSpan : WordSpan, IContext
{
    public List<Variable> Vars { get; } = [];
    public IVarSystem Parent { get; set; }
    public IVarSystem OuterVarSystem { get; }
    public Span[] InnerSource { get; set { field = value; SetContainer(value); } }
    public IOperation[] Operations { get; set; }
    internal Span[] Condition { get; }
    public IContext Context { get; set; }
    internal bool ConditionWasTrue { get; set; }
    internal override string FullText => Text + ' ' + Condition + "\n{\n\t" + InnerSource + "\n}";
    internal ConditionSpan(string text, Span[] condition, Span[] innerSource, IVarSystem varSystem) : base(text)
    {
        this.InnerSource = innerSource;
        this.Condition = condition;
        this.OuterVarSystem = varSystem;
    }
    internal override Span Container
    {
        get;
        set { field = value; SetContainer(Condition, value); }
    }
}

class IfConditionSpan : ConditionSpan, IKeyword, IExpItem
{
    public static string Keyword { get; } = "if";
    public static string ItemName { get; } = "if statement";
    internal IfConditionSpan(Span[] condition, Span[] innerSource, IVarSystem varSystem) : base(Keyword, condition, innerSource, varSystem) { }
}

class ElseConditionSpan : WordSpan, IContext, IKeyword, IExpItem
{
    public static string Keyword { get; } = "else";
    public static string ItemName { get; } = "else statement";
    public List<Variable> Vars { get; } = []; public IVarSystem Parent { get; set; }
    public IVarSystem OuterVarSystem { get; }
    public Span[] InnerSource { get; set { field = value; SetContainer(value); } }
    public IOperation[] Operations { get; set; }
    public IContext Context { get; set; }

    internal override string FullText => $"{Keyword}\n{{\n\t{InnerSource.ToString(" ")}\n}}";
    internal ElseConditionSpan(Span[] innerSource, IVarSystem varSystem) : base(Keyword)
    {
        this.InnerSource = innerSource;
        this.OuterVarSystem = varSystem;
        SetContainer(InnerSource);
    }
}

class WhileConditionSpan : ConditionSpan, ILoopContext, IKeyword, IExpItem
{
    public static string Keyword { get; } = "while";
    public static string ItemName { get; } = "while loop";
    public bool Break { get; set; }
    public bool Continue { get; set; }
    public string Counter { get; set; }
    public string Id { get; set; }
    internal override string FullText
    {
        get
        {
            string s = $"{Keyword} {Condition}";
            if (Id != null || Counter != null)
                s += " : ";
            if (Id != null)
                s += "id " + Id;
            if (Id != null && Counter != null)
                s += " , ";
            if (Counter != null)
                s += "counter " + Counter;
            s += $"\n{{\n\t{InnerSource.ToString(" ")}\n}}";
            return s;
        }
    }
    internal WhileConditionSpan(Span[] condition, Span[] innerSource, IVarSystem varSystem) : base(Keyword, condition, innerSource, varSystem) { }
}

class ForLoopSpan : ConditionSpan, ILoopContext, IKeyword, IExpItem
{
    public static string Keyword { get; } = "for";
    public static string ItemName { get; } = "for loop";
    public bool Break { get; set; }
    public bool Continue { get; set; }
    public string Counter { get; set; }
    internal Span[] InitExe { get; set; }
    internal Span[] StepExe { get; }
    public string Id { get; set; }
    internal override string FullText
    {
        get
        {
            string s = $"{Keyword} ({InitExe}; {Condition}; {StepExe})";
            if (Id != null || Counter != null)
                s += " : ";
            if (Id != null)
                s += "id " + Id;
            if (Id != null && Counter != null)
                s += ", ";
            if (Counter != null)
                s += "counter " + Counter;
            s += $"\n{{\n\t{InnerSource.ToString(" ")}\n}}";
            return s;
        }
    }
    internal ForLoopSpan(Span[] initExe, Span[] condition, Span[] stepExe, Span[] innerSource, IVarSystem varSystem) : base(Keyword, condition, innerSource, varSystem)
    {
        this.InitExe = initExe;
        this.StepExe = stepExe;
        SetContainer(InitExe);
        SetContainer(StepExe);
        SetContainer(condition);
    }


    internal override Span Container
    {
        get;
        set;//{ field = value; SetContainer(InitExe, value); SetContainer(StepExe, value); base.Container = value; }
    }
}

class ForEachLoopSpan : WordSpan, ILoopContext, IKeyword, IExpItem
{
    public static string Keyword { get; } = "foreach";
    public static string ItemName { get; } = "foreach loop";

    public const string IteratableFunc_HasNext = "it_hasNext";
    public const string IteratableFunc_Next = "it_next";
    public const string IteratableFunc_Reset = "it_reset";

    public List<Variable> Vars { get; } = [];
    public IVarSystem Parent { get; set; }
    public IVarSystem OuterVarSystem { get; }
    public Span[] InnerSource { get; set { field = value; SetContainer(value); } }
    public IOperation[] Operations { get; set; }
    public IContext Context { get; set; }
    public bool Break { get; set; }
    public bool Continue { get; set; }
    public string Counter { get; set; }
    internal Span[] ArrReadText { get; }
    internal string VarName { get; }
    internal bool ConstVar { get; }
    public string Id { get; set; }
    internal override string FullText
    {
        get
        {
            string s = $"{Keyword} {VarName} in {ArrReadText}";
            if (Id != null || Counter != null)
                s += " : ";
            if (Id != null)
                s += "id " + Id;
            if (Id != null && Counter != null)
                s += " , ";
            if (Counter != null)
                s += "counter " + Counter;
            s += $"\n{{\n\t{InnerSource.ToString(" ")}\n}}";
            return s;
        }
    }

    internal ForEachLoopSpan(string varName, bool constVar, Span[] arrReadText, Span[] innerSource, IVarSystem varSystem) : base(Keyword)
    {
        this.VarName = varName;
        this.ConstVar = constVar;
        this.ArrReadText = arrReadText;
        this.InnerSource = innerSource;
        this.OuterVarSystem = varSystem;
    }

    internal override Span Container
    {
        get;
        set { field = value; SetContainer(ArrReadText, value); }
    }
}

class RangeLoopSpan : WordSpan, ILoopContext, IKeyword, IExpItem
{
    public static string Keyword { get; } = "foreach";
    public static string ItemName { get; } = "foreach loop";
    public List<Variable> Vars { get; } = [];
    public IVarSystem Parent { get; set; }
    public IVarSystem OuterVarSystem { get; }
    public Span[] InnerSource { get; set { field = value; SetContainer(value); } }
    public IOperation[] Operations { get; set; }
    public IContext Context { get; set; }
    public bool Break { get; set; }
    public bool Continue { get; set; }
    public string Counter { get; set; }
    internal Span[] FromReadText { get; }
    internal Span[] ToReadText { get; }
    internal string VarName { get; }
    internal bool ConstVar { get; }
    public string Id { get; set; }
    internal override string FullText
    {
        get
        {
            string s = $"{Keyword} {VarName} from {FromReadText} to {ToReadText}";
            if (Id != null || Counter != null)
                s += " : ";
            if (Id != null)
                s += "id " + Id;
            if (Id != null && Counter != null)
                s += " , ";
            if (Counter != null)
                s += "counter " + Counter;
            s += $"\n{{\n\t{InnerSource.ToString(" ")}\n}}";
            return s;
        }
    }

    internal RangeLoopSpan(string varName, bool constVar, Span[] fromReadText, Span[] toReadText, Span[] innerSource, IVarSystem varSystem) : base(Keyword)
    {
        this.VarName = varName;
        this.ConstVar = constVar;
        this.FromReadText = fromReadText;
        this.ToReadText = toReadText;
        this.InnerSource = innerSource;
        this.OuterVarSystem = varSystem;
    }

    internal override Span Container
    {
        get;
        set { field = value; SetContainer(FromReadText, value); SetContainer(ToReadText, value); }
    }
}


class BreakWordSpan : WordSpan, IKeyword
{
    public static string Keyword { get; } = "break";
    internal BreakWordSpan() : base(Keyword) { }
}

class ContinueWordSpan : WordSpan, IKeyword
{
    public static string Keyword { get; } = "continue";
    internal ContinueWordSpan() : base(Keyword) { }
}

class OpeningBracketSpan : WordSpan, ISymbol
{
    public static string Symbol { get; } = "(";
    internal OpeningBracketSpan() : base("(") { }
}

class ArrayOpenerSpan : WordSpan, ISymbol
{
    public static string Symbol { get; } = "[";
    internal ArrayOpenerSpan() : base(Symbol) { }
}

class ArrayCloserSpan : WordSpan, ISymbol
{
    public static string Symbol { get; } = "]";
    internal ArrayCloserSpan() : base(Symbol) { }
}

class ClosingBracketSpan : WordSpan, ISymbol
{
    public static string Symbol { get; } = ")";
    internal ClosingBracketSpan() : base(Symbol) { }
}

class SourceOpenerSpan : WordSpan, ISymbol
{
    public static string Symbol { get; } = "{";
    internal SourceOpenerSpan() : base(Symbol) { }
}

class SourceCloserSpan : WordSpan, ISymbol
{
    public static string Symbol { get; } = "}";
    internal SourceCloserSpan() : base(Symbol) { }
}

class CommaSpan : WordSpan, ISymbol
{
    public static string Symbol { get; } = ",";
    internal CommaSpan() : base(Symbol) { }
}

class SemicolonSpan : WordSpan, ISymbol
{
    public static string Symbol { get; } = ";";
    internal SemicolonSpan() : base(Symbol) { }
}

class DotSpan : WordSpan, ISymbol
{
    public static string Symbol { get; } = ".";
    internal DotSpan() : base(Symbol) { }
}

class WhiteSpaceSpan : Span
{
    internal WhiteSpaceSpan(string txt) : base(txt) { }
}

class CommentSpan : Span
{
    private readonly bool multiLine;
    internal CommentSpan(string comment, bool multiLine = false) : base(comment)
    {
        this.multiLine = multiLine;
    }

    internal override string FullText => multiLine ? ("/*" + Text + "*/") : ("//" + Text);
}

public class FuncDefSpan : WordSpan, IContext, IDefination, IKeyword, IClassMember, IExpItem, INamedValue
{
    public static string Keyword { get; } = "func";
    public static string ItemName { get; } = "function";
    public string Namespace { get; set; }
    public List<Variable> Vars { get; } = [];
    public IVarSystem Parent { get; set; } //= Interpreter.Activated.ParentVs;
    public IVarSystem OuterVarSystem { get; set; }
    public Span[] InnerSource { get; set { field = value; SetContainer(value); } }
    public IOperation[] Operations { get; set; }
    public IContext Context { get; set; }
    internal bool IsRunning { get; set; }
    public bool ReadOnly { get; internal set; }
    public string Name { get; }
    public bool Private { get; set; }
    public IValue Value => throw new Exception("A function value can only be accessed by calling it.");
    public bool IsVar => false;
    internal bool Static { get; set; }
    internal ArgumentSpan[] Args { get; }
    internal bool Return { get; set; } = false;
    internal IValue? Returns { get; set; }
    internal bool SpanItselfIsReadedAsValue { get; set; }

    internal ClassDefSpan DefinedAt { get; }
    public ClassDefSpan Def => DefinedAt;

    public string TypeName => ItemName;

    public List<Span[]> TagsCode { get; set; } = [];
    public Instance[] AttrInfo { get; set; }

    internal static FuncDefSpan ArrayIndexGetter, ArrayIndexSetter;
    internal static FuncDefSpan ExternInvoker = new("extern.invoker", [new ArgumentSpan("extrn", notNull: true), new ArgumentSpan("statc", notNull: true), new ArgumentSpan("memberName", true), new ArgumentSpan("inst"), new ArgumentSpan("args", notNull: true)], [], null);
    internal static FuncDefSpan ExternPropGetSet = new("extern.pgetset", [new ArgumentSpan("extrn", notNull: true), new ArgumentSpan("pinfo", true), new ArgumentSpan("inst"), new ArgumentSpan("value", notNull: false), new ArgumentSpan("set", notNull: true)], [], null);

    internal Variable[] ParamVariables { get; }
    internal FuncDefSpan(string name, ArgumentSpan[] args, Span[] innerSource, ClassDefSpan definedAt, string text = null) : base(text ?? Keyword)
    {
        this.Name = name;
        this.Args = args;
        this.InnerSource = innerSource;
        this.DefinedAt = definedAt;

        // create variables for parameter
        ParamVariables = new Variable[Args.Length];
        for (int i = 0; i < ParamVariables.Length; i++)
        {
            Variable pv = new(Args[i].Name, null, null, false);
            Vars.Add(pv);
            ParamVariables[i] = pv;
        }

        if (DefinedAt == ClassDefSpan.ExpArrayDef)
        {
            if (name == "get")
            {
                ArrayIndexGetter = this;
                this.Name = "array." + name;
            }
            else if (name == "set")
            {
                ArrayIndexSetter = this;
                this.Name = "array." + name;
            }
        }
    }

    internal override string FullText
    {
        get
        {
            string argsStr = "";
            foreach (var a in Args)
                argsStr += a.ToString() + " , ";
            if (Args.Length > 0)
                argsStr = argsStr.Substring(0, argsStr.Length - 3);
            string s = Static ? "static " : "";
            s += $"{Keyword} {Name} ( {argsStr} )\n{{\n\t{InnerSource.ToString(" ")}\n}}";
            return s;
        }
    }

    string IDefination.FullName => (DefinedAt != null ? (DefinedAt.GetExpTypeName(false) + ".") : (Namespace == null ? "" : (Namespace + NamespaceSpecificationSpan.Symbol))) + Name + "(.." + Args.Length + ")";
    public override string ToString() => Keyword + " " + ((IDefination)this).FullName;
}

public class ConstructorDefSpan : FuncDefSpan, IKeyword, IExpItem
{
    public static string Keyword { get; } = "constructor";
    public static string ItemName { get; } = "constructor";
    public ConstructorDefSpan(ArgumentSpan[] args, Span[] innerSource, ClassDefSpan definedAt, Interpreter toThrowWith) : base(definedAt == null ? null : $"{definedAt.Name}.ctor", args, innerSource, definedAt, Keyword)
    {
        if (definedAt == null)
            toThrowWith.Error("Constructor must be defined inside a class.");

        base.ReadOnly = true;
    }

    internal override string FullText
    {
        get
        {
            string argsStr = "";
            foreach (var a in Args)
                argsStr += a.ToString() + " , ";
            if (Args.Length > 0)
                argsStr = argsStr.Substring(0, argsStr.Length - 3);
            string s = "";
            s = $"{Keyword} ({argsStr})\n{{\n\t{InnerSource.ToString(" ")}\n}}";
            return s;
        }
    }
}

public class ArgumentSpan : WordSpan, IExpItem
{
    public static string ItemName { get; } = "argument";
    internal string Name { get; }
    internal bool NotNull { get; }
    internal ArgumentSpan(string name, bool notNull = false) : base(name)
    {
        this.Name = name;
        this.NotNull = notNull;
    }

    internal override string FullText
    {
        get
        {
            string s = Name;
            if (!NotNull)
                s += "?";
            return s;
        }
    }
}

class NotNullWordSpan : WordSpan, IKeyword
{
    public static string Keyword { get; } = "notnull";
    internal NotNullWordSpan() : base(Keyword) { }
}

class ReturnWordSpan : WordSpan, IKeyword
{
    public static string Keyword { get; } = "return";
    internal ReturnWordSpan() : base(Keyword) { }
}

class ReturnSymbolSpan : Span, ISymbol
{
    public static string Symbol { get; } = "=>";
    internal ReturnSymbolSpan() : base(Symbol) { }
}

class DoSymbolSpan : Span, ISymbol
{
    public static string Symbol { get; } = "->";
    internal DoSymbolSpan() : base(Symbol) { }
}

class NullWordSpan : WordSpan, IKeyword
{
    public static string Keyword { get; } = "null";
    internal NullWordSpan() : base(Keyword) { }
}

public class ClassDefSpan : WordSpan, IDefination, IVarSystem, IKeyword, ICanSetAttr, IExpItem
{
    private static ClassDefSpan Create(string name, Property[] props)
    {
        var n = new ClassDefSpan(name, props);
        //n.Props.ForEach(pr => pr.Def = n); now done at constructor
        return n;
    }
    public static ClassDefSpan ExpArrayDef { get; private set; } = new("Array", []);
    public static ClassDefSpan ExpStringDef { get; private set; } = Create("string", [new Property(null, true, "chars", true, true)]);
    public static ClassDefSpan ExpExceptionDef { get; private set; } = new("Exception", []);
    public static ClassDefSpan ExternTypeValueDef { get; private set; } = new("ExternTypeValue", []);
    public static ClassDefSpan ExpTypeDef { get; private set; } = new("Type", []);
    public static ClassDefSpan ExpAttrInfoDef { get; private set; } = new("AttrInfo", []);

    public static string Keyword { get; } = "class";
    public static string ItemName { get; } = "class";
    public string Namespace { get; set; }
    public string Name { get; }
    public List<Variable> Vars { get; } = []; // static props
    public IVarSystem Parent { get; set; }
    public Property[] Props { get; }
    public FuncDefSpan[] Funcs { get; set; }
    internal Property BaseArrayProp => Props.FirstOrDefault(p => p.BaseArray);
    public Instance ExpType
    {
        get
        {
            if (_expType == null)
            {
                _expType = new Instance(ExpTypeDef ?? throw new Exception("Trying to get type instance before Type def was collected."));
                _expType.Vars[0].SetSkippingConstant(Interpreter.StringToExpString(Name));
                _expType.Vars[1].SetSkippingConstant(Interpreter.StringToExpString(Namespace + NamespaceSpecificationSpan.Symbol + Name));
                _expType.Vars[2].SetSkippingConstant(SpecialValue.From(this));
            }
            return _expType;
        }
    }
    private Instance _expType;
    public List<Span[]> TagsCode { get; set; } = [];
    public Instance[] AttrInfo { get; set; }

    internal new FuncDefSpan ToString { get; set; }
    internal new FuncDefSpan Equalizer { get; set; }

    public ClassDefSpan(string name, Property[] props, FuncDefSpan[] funcs = null, Instance[] attr = null) : base(Keyword)
    {
        this.Name = name;
        this.Props = props;
        props.ForEach(p => p.Def = this);
        this.Funcs = funcs;
        this.AttrInfo = attr;

        if (name == "Array")
            ExpArrayDef = this;
        else if (name == "string")
            ExpStringDef = this;
        else if (name == "Exception")
            ExpExceptionDef = this;
        else if (name == "ExternTypeValue")
            ExternTypeValueDef = this;
        else if (name == "Type")
            ExpTypeDef = this;
        else if (name == "AttributeInfo")
            ExpAttrInfoDef = this;
    }

    internal override string FullText
    {
        get
        {
            string propsStr = "", funcsStr = "", staticsStr = "";
            foreach (var a in Props)
                propsStr += (a.Const ? "const " : "") + a.Name + (a.Private ? " private" : "") + " , ";
            if (Props.Length > 0)
                propsStr = propsStr.Substring(0, propsStr.Length - 3);

            foreach (var v in Vars)
                staticsStr += $"static " + (v.Private ? "private " : "") + (v.Const ? "const " : "") + v.Name + "\n";

            foreach (var func in Funcs)
                funcsStr += func.FullText + "\n";
            string s = $"{Keyword} {Name} ( {propsStr} )\n{{\n\t{funcsStr}\n}}";
            return s;
        }
    }
}

class EnumDefSpan : WordSpan, IDefination, ICanSetAttr, IKeyword, IExpItem
{
    public static string Keyword { get; } = "enum";
    public static string ItemName { get; } = "enum";
    public string Name { get; }
    public string Namespace { get; set; }
    internal EnumValueSpan[] Values { get; }
    public List<Span[]> TagsCode { get; set; } = [];
    public Instance[] AttrInfo { get; set; }
    internal EnumDefSpan(string name, EnumValueSpan[] values) : base(Keyword)
    {
        this.Name = name;
        this.Values = values;
    }
    internal override string FullText
    {
        get
        {
            string vals = "";
            int index = 0;
            foreach (var val in Values)
                vals += val.FullText + (++index >= Values.Length ? "" : ",") + "\n";
            return Keyword + $" {Name}\n{{\n\t{vals}\n}}";
        }
    }
}

class EnumValueSpan : WordSpan, ICanSetAttr, IExpItem
{
    public static string ItemName { get; } = "enum value";
    internal string Name { get; }
    internal double Value { get; set; }
    internal bool CustomValue { get; }
    public List<Span[]> TagsCode { get; set; } = [];
    public Instance[] AttrInfo { get; set; }
    internal EnumValueSpan(string name, double value, bool customValue) : base(name)
    {
        this.Name = name;
        this.Value = value;
        this.CustomValue = customValue;
    }

    internal override string FullText => Name + (CustomValue ? $" = {Value}" : "");
}

class InstInitSpan : WordSpan, IKeyword, IExpItem
{
    public static string Keyword { get; } = "new";
    public static string ItemName { get; } = "new statement";
    internal DefNameSpan DefName { get; }
    internal InstInitSpan(DefNameSpan defName) : base(Keyword)
    {
        ArgumentNullException.ThrowIfNull(defName, nameof(defName));
        this.DefName = defName;
    }

    internal override string FullText
    {
        get => Keyword + " " + DefName;
    }
}

class ThisWordSpan : WordSpan, IKeyword
{
    public static string Keyword { get; } = "this";
    internal ThisWordSpan() : base(Keyword) { }
}

class ThrowWordSpan : WordSpan, IKeyword
{
    public static string Keyword { get; } = "throw";
    internal ThrowWordSpan() : base(Keyword) { }
}

class TryWordSpan : WordSpan, IContext, IKeyword, IExpItem
{
    public static string Keyword { get; } = "try";
    public static string ItemName { get; } = "try block";
    public IContext Context { get; set; }
    public List<Variable> Vars { get; } = []; public IVarSystem Parent { get; set; }
    public IVarSystem OuterVarSystem { get; }
    internal CatchWordSpan Catch { get; set; }
    internal FinallyWordSpan Finally { get; set; }
    public Span[] InnerSource { get; set { field = value; SetContainer(value); } }
    public IOperation[] Operations { get; set; }
    internal TryWordSpan(Span[] innerSource, IVarSystem vs, CatchWordSpan catc, FinallyWordSpan finaly) : base(Keyword)
    {
        this.InnerSource = innerSource;
        this.Catch = catc;
        this.Finally = finaly;
        this.OuterVarSystem = vs;
    }
    internal override string FullText => $"{Keyword}\n{{\n\t{InnerSource.ToString(" ")}\n}}";
}

class CatchWordSpan : WordSpan, IContext, IKeyword, IExpItem
{
    public static string Keyword { get; } = "catch";
    public static string ItemName { get; } = "catch block";
    public IContext Context { get; set; }
    public List<Variable> Vars { get; set; } = [];
    public IVarSystem Parent { get; set; }
    public IVarSystem OuterVarSystem { get; }
    internal string VarName { get; }
    internal WhenWordSpan When { get; }
    public Span[] InnerSource { get; set { field = value; SetContainer(value); } }
    public IOperation[] Operations { get; set; }
    internal CatchWordSpan(string varname, WhenWordSpan when, Span[] innerSource, IVarSystem vs) : base(Keyword)
    {
        this.VarName = varname;
        this.When = when;
        this.InnerSource = innerSource;
        this.OuterVarSystem = vs;
    }
    internal override string FullText
    {
        get
        {
            string s = $"{Keyword} ";
            if (VarName != null)
                s += VarName;
            if (When != null)
                s += When.FullText;
            s += $"\n{{\n\t{InnerSource.ToString(" ")}\n}}";
            return s;
        }
    }
}

class FinallyWordSpan : WordSpan, IContext, IKeyword, IExpItem
{
    public static string Keyword { get; } = "finally";
    public static string ItemName { get; } = "finally block";
    public IContext Context { get; set; }
    public List<Variable> Vars { get; set; } = [];
    public IVarSystem Parent { get; set; }
    public IVarSystem OuterVarSystem { get; }
    public Span[] InnerSource { get; set { field = value; SetContainer(value); } }
    public IOperation[] Operations { get; set; }
    internal FinallyWordSpan(Span[] innerSource, IVarSystem vs) : base(Keyword)
    {
        this.InnerSource = innerSource;
        this.OuterVarSystem = vs;
    }
    internal override string FullText => $"{Keyword}\n{{\n\t{InnerSource.ToString(" ")}\n}}";
}

class WhenWordSpan : WordSpan, IKeyword
{
    public static string Keyword { get; } = "when";
    internal Span[] Condition { get; }
    internal WhenWordSpan(Span[] condition) : base(Keyword)
    {
        this.Condition = condition;
        SetContainer(condition, Container);
    }

    internal override string FullText => $"{Keyword}\n{{\n\t{Condition.ToString(" ")}\n}}";
}

class SectionWordSpan : WordSpan, IContext, IKeyword, IExpItem
{
    public static string Keyword { get; } = "section";
    public static string ItemName { get; } = "section block";
    public IContext Context { get; set; }
    public List<Variable> Vars { get; } = []; public IVarSystem Parent { get; set; }
    public IVarSystem OuterVarSystem { get; }
    public Span[] InnerSource { get; set { field = value; SetContainer(value); } }
    public IOperation[] Operations { get; set; }

    internal SectionWordSpan(Span[] innerSource, IVarSystem vs) : base(Keyword)
    {
        this.InnerSource = innerSource;
        this.OuterVarSystem = vs;
    }

    internal override string FullText => $"{Keyword}\n{{\n\t{InnerSource.ToString(" ")}\n}}";
}

class AttributeDefSpan : WordSpan, IDefination, IKeyword, ICanSetAttr, IExpItem
{
    internal static new AttributeDefSpan ToString = new("Translator", [])
    { AllowFor_Class = false, AllowFor_Constructor = false, AllowFor_Func = true, AllowFor_Property = false, AllowFor_Attr = false, LimitTo1InCls = true, Func_StaticRequirement = StaticRequirement.NonStatic, Func_ParamsCountRequirement = 0 };

    internal static new AttributeDefSpan EqualizerAttr = new("Equalizer", [])
    { AllowFor_Class = false, AllowFor_Constructor = false, AllowFor_Func = true, AllowFor_Property = false, AllowFor_Attr = false, LimitTo1InCls = true, Func_StaticRequirement = StaticRequirement.NonStatic, Func_ParamsCountRequirement = 1 };

    internal static AttributeDefSpan AllowFor = new("AllowFor",
        [new(typeof(BoolValue), "class"),
        new(typeof(BoolValue), "property"),
        new(typeof(BoolValue), "func"),
        new(typeof(BoolValue), "ctor"),
        new(typeof(BoolValue), "attr")])
    { AllowFor_Class = false, AllowFor_Constructor = false, AllowFor_Func = false, AllowFor_Property = false };

    internal static AttributeDefSpan AllowMultipleAttr = new("AllowMultiple", [])
    { AllowFor_Class = false, AllowFor_Constructor = false, AllowFor_Func = false, AllowFor_Property = false };

    internal static AttributeDefSpan LimitTo1InClsAttr = new("OneInClass", [])
    { AllowFor_Class = false, AllowFor_Constructor = false, AllowFor_Func = false, AllowFor_Property = false };

    internal static AttributeDefSpan FuncRequirementsAttr = new("FuncRequirements",
        [
            new(typeof(NumberValue), "stat"),
            new(typeof(NumberValue), "params")
        ])
    { AllowFor_Class = false, AllowFor_Constructor = false, AllowFor_Func = false, AllowFor_Property = false };

    internal static AttributeDefSpan ExpectFuncAttr;
    /*= new("ExpectFunc",
        [new(ClassDefSpan.ExpStringDef, "name"),
        new(typeof(NumberValue), "paramsCount"),
        new(typeof(BoolValue), "static")])
    { AllowFor_Class = false, AllowFor_Constructor = false, AllowFor_Func = false, AllowFor_Property = false, AllowMultiple = true };
    */

    internal static AttributeDefSpan ReadOnlyAttr = new("ReadOnly", [])
    { AllowFor_Class = false, AllowFor_Constructor = false, AllowFor_Func = true, AllowFor_Property = false, AllowFor_Attr = false };

    internal static AttributeDefSpan IteratableAttr /*= new("Iteratable", [])
    { AllowFor_Class = true, AllowFor_Constructor = false, AllowFor_Func = false, AllowFor_Property = false, AllowFor_Attr = false }*/;

    public static string Keyword { get; } = "attribute";
    public static string ItemName { get; } = "attribute";
    public string Name { get; }
    public string Namespace { get; set; }
    internal AttributeParamSpan[] Params { get; }
    public List<Span[]> TagsCode { get; set; } = [];
    public Instance[] AttrInfo { get; set; }
    internal bool AllowFor_Class { get; set; } = true;
    internal bool AllowFor_Property { get; set; } = true;
    internal bool AllowFor_Func { get; set; } = true;
    internal bool AllowFor_Constructor { get; set; } = true;
    internal bool AllowFor_Attr { get; set; } = true;
    internal bool AllowMultiple { get; set; }
    internal bool LimitTo1InCls { get; set; }
    internal enum StaticRequirement
    {
        Static,
        NonStatic,
        Nevermind
    }
    internal StaticRequirement Func_StaticRequirement { get; set; } = StaticRequirement.Nevermind;
    internal int Func_ParamsCountRequirement = -1;
    internal Instance ExpType
    {
        get
        {
            if (_expType == null)
            {
                _expType = new Instance(ClassDefSpan.ExpTypeDef ?? throw new Exception("Trying to get type instance before Type def was collected."));
                _expType.Vars[0].SetSkippingConstant(Interpreter.StringToExpString(Name));
                _expType.Vars[1].SetSkippingConstant(Interpreter.StringToExpString(Namespace + NamespaceSpecificationSpan.Symbol + Name));
                _expType.Vars[2].SetSkippingConstant(SpecialValue.From(this));
            }
            return _expType;
        }
    }
    private Instance _expType;
    internal AttributeDefSpan(string name, AttributeParamSpan[] param) : base(Keyword)
    {
        this.Name = name;
        this.Params = param;
        if (name == "ExpectFunc")
            ExpectFuncAttr ??= this;
        else if (name == "Iteratable")
            IteratableAttr ??= this;
    }
}

class AttributeParamSpan : WordSpan, IExpItem
{
    public static string ItemName { get; } = "attribute parameter";
    internal string Name { get; }
    internal Instance ExpType { get; private set; }
    internal DefNameSpan ExpTypeName { get; }
    internal Type Type { get; }
    private AttributeParamSpan(string name) : base(name)
    {
        this.Name = name;
    }

    internal AttributeParamSpan(Instance type, string name) : this(name)
    {
        this.ExpType = type;
    }

    internal AttributeParamSpan(Type type, string name) : this(name)
    {
        this.Type = type;
    }

    internal AttributeParamSpan(DefNameSpan defName, string name) : this(name)
    {
        ArgumentNullException.ThrowIfNull(defName);

        this.ExpTypeName = defName;
    }

    internal bool ResolveTypeName(IEnumerable<IDefination> defs)
    {
        if (ExpTypeName != null)
        {
            if (ExpTypeName.Class == null)
                Interpreter.Activated.ThrowRuntime<Instance>("Attribute parameter must be of premitive / non-extern class type.", RuntimeException.INVALID_SYNTAX, this);
            else
                ExpType = ExpTypeName.Class.ExpType;
        }
        return true;
    }
}

class TagSpan : WordSpan, IExpItem
{
    public static string ItemName { get; } = "property tag";
    internal Span[] Code { get; }
    internal TagSpan(string name, Span[] code) : base(name) => this.Code = code;
    internal override string FullText => "@" + Text;
}