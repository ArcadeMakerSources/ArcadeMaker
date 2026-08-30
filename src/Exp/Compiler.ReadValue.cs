using System;
using System.Linq;
using System.Collections.Generic;
using Exp.Spans;
using System.Collections;
//using Exp.Compiler;

namespace Exp;

public partial class Interpreter
{
    private bool allowThrow = false;

    private List<Span> readValue_codeRecord = null;
    private object ReadValue(out Span[] src, Span firstSpan = null, bool allowUnknownVars = false, bool oconst = false)
    {
        bool deleteRecord = readValue_codeRecord == null;
        readValue_codeRecord ??= [];

        void ThrowOnlyConst()
        {
            if (oconst)
                Error("A constant value was expected.");
        }

        object ReadSingle(out bool wasval)
        {
            wasval = false;
            object value = null;
            Span span = firstSpan ?? ReadSpan();
            firstSpan = null;
            if (span is null)
                return null;

            if (span is NumberSpan num)
                value = num.Number;
            else if (span is StringSpan)
                value = span.Text.ToExpString();
            else if (span is CharSpan)
                value = span.Text[0];
            else if (span is TrueWordSpan)
                value = true;
            else if (span is FalseWordSpan)
                value = false;
            else if (span is NotSymbolSpan)
                value = ReadSingle(out wasval);
            else if (span is NullWordSpan)
                value = null;
            else if (span is FuncDefSpan funcpntr)
                value = funcpntr;
            else if (span is LenofWordSpan)
            {
                ThrowOnlyConst();

                object val = ReadValue();
                if (val is Instance inststr && inststr.def == ClassDefSpan.ExpStringDef)
                    value = (inststr.Vars[0].Value as Instance).ArrayValues.Length;
                else if (val is Instance inst && inst.IsArray)
                    value = inst.ArrayValues.Length;
                else if (!allowUnknownVars)
                    Error("Only arrays and strings can be read here.");
            }
            else if (span is TypeOfSpan type)
                value = type.Value;
            else if (span is OpeningBracketSpan)
                value = span;
            else if (span is ArrayOpenerSpan)
            {
                ThrowOnlyConst();

                IValue[] vals = ReadParamList(true, true, false);
                value = new ArrayInstance(ClassDefSpan.ExpArrayDef, vals);
            }
            else if (span is InstInitSpan init)
            {
                ThrowOnlyConst();

                if (allowUnknownVars)
                    ReadParamList(false, false, true);
                value = allowUnknownVars ? 0d : ReadInstInitSpan(init);
            }
            else if (span is WordSpan word)
            {
                if (oconst)
                {
                    // expect constant static property (such as enum value)
                    var defName = ReadDefName(word);
                    if (allowUnknownVars)
                        goto endif;

                    if (defName.Class != null)
                    {
                        Read<DotSpan>();
                        var stpropName = ReadWord().FullText;
                        var val = defName.Class.Vars.FirstOrDefault(v => v.Name == stpropName);
                        if (val == null)
                            Error($"Unknown static constant {(defName.Class as IDefination).FullName}{NamespaceSpecificationSpan.Symbol}{stpropName}");
                        if (!val.Const)
                            Error($"{(defName.Class as IDefination).FullName}{NamespaceSpecificationSpan.Symbol}{stpropName} is not marked as constant and can't be read here.");
                        if ((val.Value.IsInst && val.Value.Inst.def != ClassDefSpan.ExpStringDef) || !(val.Value == null || val.Value.IsBool || val.Value.IsChar || val.Value.IsNumber || val.Value.IsInst))
                            ThrowOnlyConst();
                        value = val.Value;
                        goto endif;
                    }
                    else
                    {
                        Error("A constant value was expected, so only variables marked as constant and defined in a class can be read here.");
                    }
                }

                ThrowOnlyConst();

                if (word is ThrowWordSpan thr)
                {
                    if (!allowThrow)
                        Error("Throwing is not allowed where a value is expected unless it's a boolean selection ('?') or the right side of a '??' symbol.");
                    value = thr;
                    goto endif;
                }

                value = ReadNamedValueOrPointer(out bool isArrPntr, out int arrPntrInd, word, allowUnknownVars);
                if (value is Variable || (value is Instance && isArrPntr))
                {
                    var pointer = value as Variable;
                    var vi = value;
                    value = isArrPntr ? ((Instance)(pointer?.Value ?? value)).ArrayValues[arrPntrInd] : pointer.Value;

                    // take care of operations (++, --, +=, -=, etc.)
                    if (Spoiler() is OperatorSpan { Action: true } op)
                    {
                        ReadSpan();

                        object input = op.TwoSides ? ReadValue() : null;

                        if (isArrPntr)
                        {
                            var arr = (pointer?.Value ?? vi) as Instance;
                            arrPntrInd.Println();
                            arr.ArrayValues[arrPntrInd] = op.Result(arr.ArrayValues[arrPntrInd], null);
                        }
                        else
                        {
                            pointer.Value = op.Result(pointer.Value, 0d.ToExp());
                        }

                        // 2 sides operations should return the new value, 1 side operations should return old value
                        // Example: var num = 0   print num++         >> prints 0
                        //          var num = 0   print num += 5      >> prints 5
                        if (op.TwoSides)
                            value = isArrPntr ? (pointer.Value as Instance).ArrayValues[arrPntrInd] : pointer.Value;
                    }
                }
            endif: object _ = null;
            }
            else if (span is OperatorSpan { Action: true } op)
            {
                ThrowOnlyConst();

                value = ReadNamedValueOrPointer(out bool isArrPntr, out int arrPntrInd, null, allowUnknownVars);
                if (value is Variable || (value is Instance && isArrPntr))
                {
                    if (op.TwoSides)
                        Error($"Unexpected operation {op.Text}.");
                    var pointer = value as Variable;
                    if (isArrPntr)
                    {
                        var arr = (pointer?.Value ?? value) as Instance;
                        arr.ArrayValues[arrPntrInd] = op.Result(arr.ArrayValues[arrPntrInd], null);
                        value = arr.ArrayValues[arrPntrInd];
                    }
                    else
                    {
                        pointer.Value = op.Result(pointer.Value, null);
                        value = pointer.Value;
                    }
                }
                else
                    Error("Variable name was expected.");
            }
            else if (span is WhiteSpaceSpan)
                return ReadSingle(out wasval);
            else
                Error($"Unexpected symbol in value reading: {span.GetType().Name}.", span);

            if (value is int integer)
                value = (double)integer;

            // "is" check
            if (Spoiler() is IsWordSpan)
            {
                ReadSpan();

                bool not = false;

                if (Spoiler().FullText.Equals("not"))
                {
                    not = true;
                    ReadSpan();
                }

                // type to check
                WordSpan type = ReadWord();
                string typeName = type.Text;
                DefNameSpan defName = null;

                // throw if type not exist
                const string number = "number";
                string[] builtins = [number, "char", "bool", "function"];
                if (!builtins.Contains(typeName))
                {
                    if (!allowUnknownVars)
                    {
                        defName = ReadDefName(type); // will throw if not found
                        if (defName.Class == null)
                            Error($"A class name was expected ({defName.FullText} is not a class, but {(defName.Defination as IExpItem)?.GetItemName()}).");
                    }
                }

                // check
                if (value is Instance ins)
                    value = ins.def == defName?.Class;
                else if (value is double)
                    value = typeName.Equals(number);
                else if (value is bool)
                    value = typeName.Equals(builtins[2]);
                else if (value is char)
                    value = typeName.Equals(builtins[1]);
                else if (value is FuncDefSpan)
                    value = typeName.Equals(builtins[3]);
                else
                    throw new Exception($"Unsupported value ({value.GetType()}).");
                if (not)
                    value = !(bool)value;
            }

            wasval = true;
            return value;
        }

        // values operations
        object val;
        bool valread;
        Instance nullCoalscingEx = null;
        allowThrow = false;

        var nums = new List<object>();
        var ops = new List<OperatorSpan>();

        // fill arrays
        do
        {
            val = ReadSingle(out valread);

            if (val is ThrowWordSpan)
            {
                nullCoalscingEx = ReadValue<Instance>(allowUnknownVars: allowUnknownVars);
                if (nullCoalscingEx == null || nullCoalscingEx.def != ClassDefSpan.ExpExceptionDef)
                    Error($"Only instances of type {(ClassDefSpan.ExpExceptionDef as IDefination)?.FullName ?? "system::Exception"} can be thrown.");
            }

            if (val is int i32)
                val = (double)i32;

            if (val is OpeningBracketSpan)
            {
                List<Span> sub = [];
                int openers = 1, closers = 0;
                while (true)
                {
                    Span span = ReadSpan();

                    if (span == null)
                        Error("Missing ')'.");
                    else if (span is ClosingBracketSpan)
                    {
                        if (++closers >= openers)
                            break;
                    }
                    else if (span is OpeningBracketSpan)
                        openers++;

                    sub.Add(span);
                }

                var rec_bu = readValue_codeRecord;
                readValue_codeRecord = null;
                object subRes = Run<object>(sub.ToArray(), allowUnknownVars: allowUnknownVars, oconst: oconst);
                readValue_codeRecord = rec_bu;
                val = subRes;
            }

            Span spoiler = Spoiler();
            if (spoiler is OperatorSpan)
            {
                allowThrow = spoiler is NullCoalescingOperatorSpan;
                nums.Add(val);
                var oprtor = Read<OperatorSpan>();
                if (oprtor is NullCoalescingOperatorSpan ncos)
                    ncos.NullCoalsingEx = nullCoalscingEx;
                ops.Add(oprtor);
            }
            else
            {
                nums.Add(val);
                break;
            }
        } while (valread);

        if (nums.Count == 0)
            Error($"A value was expected.");

        // calculate math-first operations
        for (int i = 0; i < ops.Count; i++)
        {
            if (ops[i] is MultiplyOperatorSpan or DivideOperatorSpan or ModuluOperatorSpan)
            {
                nums[i] = MakeOperation(nums[i], ops[i], nums[i + 1]);
                nums.RemoveAt(i + 1);
                ops.RemoveAt(i);
                i--;
            }
        }

        // calculate math-late operations
        for (int i = 0; i < ops.Count; i++)
        {
            if (ops[i] is PlusOperatorSpan or MinusOperatorSpan)
            {
                nums[i] = MakeOperation(nums[i], ops[i], nums[i + 1]);
                nums.RemoveAt(i + 1);
                ops.RemoveAt(i);
                i--;
            }
        }

        // calculate all the rest, except for & and |
        for (int i = 0; i < ops.Count; i++)
        {
            if (ops[i] is not AndOperatorSpan && ops[i] is not OrOperatorSpan)
            {
                nums[i] = MakeOperation(nums[i], ops[i], nums[i + 1]);
                nums.RemoveAt(i + 1);
                ops.RemoveAt(i);
                i--;
            }
        }

        // calculate the final result
        val = nums[0];
        for (int i = 1; i < nums.Count; i++)
        {
            val = MakeOperation(val, ops[i - 1], nums[i]);
        }

        object MakeOperation(object left, OperatorSpan @operator, object right)
        {
            allowUnknownVars = true;
            if (allowUnknownVars)
                return 0d;

            if (left is Instance inst && inst.def == ClassDefSpan.ExpStringDef)
                left = ExpStringToString(inst);
            if (right is Instance inst1 && inst1.def == ClassDefSpan.ExpStringDef)
                right = ExpStringToString(inst1);

            object value = @operator.Result(0d.ToExp(), 0d.ToExp());

            if (value is int or float or long or decimal or uint or ulong)
                value = Convert.ToDouble(value);
            return value;
        }

        // ? check
        if ((val is bool || allowUnknownVars) && Spoiler() is QuestionMarkSpan)
        {
            ThrowOnlyConst();

            ReadSpan();

            allowThrow = true;
            object tval = ReadValue(allowUnknownVars: allowUnknownVars);
            if (tval is ThrowWordSpan) ReadThrowStmt(readThrowKeyword: false, neutral: allowUnknownVars, thr: !allowUnknownVars && (bool)val);

            ReadWord(":");

            object fval = ReadValue(allowUnknownVars: allowUnknownVars);
            if (fval is ThrowWordSpan) ReadThrowStmt(readThrowKeyword: false, neutral: allowUnknownVars, thr: !allowUnknownVars && !(bool)val);
            allowThrow = false;

            if (!allowUnknownVars)
            {
                val = (bool)val ? tval : fval;
            }
        }

        src = readValue_codeRecord.ToArray();
        if (deleteRecord)
            readValue_codeRecord = null;

        if (val is string s)
            val = StringToExpString(s);

        return val;
    }

    private object ReadValue(bool allowUnknownVars = false)
    {
        return ReadValue(out var _, null, allowUnknownVars);
    }

    private T? ReadValue<T>(bool allowUnknownVars = false, bool oconst = false)
    {
        return ReadValue<T>(out var _, allowUnknownVars: allowUnknownVars, oconst: oconst);
    }

    private T? ReadValue<T>(out Span[] src, bool allowUnknownVars = false, bool oconst = false)
    {
        object v = ReadValue(out src, null, allowUnknownVars: allowUnknownVars, oconst: oconst);

        if (allowUnknownVars)
            return default;

        if (v is not T)
        {
            Error($"An {typeof(T).Name} value was expected (received: {v}).");
            return default;
        }
        return (T)v;
    }

    private object ReadInstInitSpan(InstInitSpan init)
    {
        // get the class span
        ClassDefSpan cls = init.DefName.Class;
        ExternClassDefSpan ext = init.DefName.Extern;
        if (cls == null && ext == null)
            Error($"Class name was expected, but {(init.DefName as IExpItem).GetItemName()} received.");

        // read param list
        var param = ReadParamList();

        // if extern invoke ctor
        if (ext != null)
            return NewExtern(ext.Type, param);

        // if no constructor is defined, skip the ctor search
        if (cls.Funcs.OfType<ConstructorDefSpan>().Any())
        {
            // if there's a constructor with this num of params, create the instance and call the constructor
            var func = cls.Funcs.FirstOrDefault(f => f is ConstructorDefSpan && !f.Static && f.Args.Length == param.Length);
            if (func == null)
                Error($"'{cls.Name}' does not implement a constructor with this number of parameters.");

            // validate access to constructor
            if (func.Private)
                ValidateAccess(func, func.DefinedAt, CurrentVarSystem);

            // built in classes:
            // array
            bool isArr = cls.Name.Equals("Array") && cls.Namespace.Equals(STD_NAMESPACE);
            IValue[]? arr = null;
            if (isArr)
            {
                /*if (param[0] is not double)
                    ThrowRuntime("A round non-negative number was expected as array length.", RuntimeException.INVALID_OPERATION);
                int len = (int)((double)param[0]);
                arr = new object[len];*/
            }

            // create instance and call constructor
            Instance value = isArr ? new ArrayInstance(cls, arr) : new Instance(cls);
            FuncCall(value, func.Name, currentContext, out bool _, cls.Funcs, param);
            return value;
        }
        else
        {
            if (param.Length > 0)
                Error("There are parameters attached, but no constructor is implemented for this type.");
            return new Instance(cls);
        }
    }

    private object ReadNamedValueOrPointer(out bool isArrPointer, out int arrPointerIndex, WordSpan firstSpan = null, bool allowUnknownVars = false, bool allowFuncsToNotReturn = false)
    {
        allowUnknownVars = true; // REMOVE THIS
        isArrPointer = false;
        arrPointerIndex = 0;
        bool nsSpec = false, firstTime = true;
        object val = null;
        Instance inst = null;
        ClassDefSpan clas = null;
        ExternClassDefSpan ext = null;
        while (true)
        {
            var span = firstTime ? (firstSpan ?? ReadWord()) : ReadWord();

            string word = span.FullText;

            // namespace specification
            if (!nsSpec && Spoiler() is NamespaceSpecificationSpan)
            {
                specificNS = word;
                ReadSpan();
                nsSpec = true;
                firstSpan = null;
                continue;
            }

            IVarSystem vs = (IVarSystem)inst ?? (IVarSystem)clas;
            vs ??= currentContext;

            // if it's a func, call it
            bool isFunc = false;
            if (!allowUnknownVars)
            {
                if (inst?.def == ClassDefSpan.ExternTypeValueDef && Spoiler() is OpeningBracketSpan)
                {
                    val = InvokeExtern(inst.Vars[1].Value.GetType(), false, inst.Vars[1].Value, word, ReadParamList());
                    isFunc = true;
                }
                else if (ext == null)
                {
                    // extern funcs provided by the user of the .dll
                    ExternFunc externFunc = inst == null && clas == null && ext == null ? null : null;
                    if (externFunc != null)
                    {
                        val = InvokeExtern(externFunc, ReadParamList());
                        isFunc = true;
                    }
                    else
                    {
                        // a normal func
                        var funcs = inst == null ? (clas == null ? GetFuncLs(vs) : clas.Funcs) : inst.def.Funcs;
                        var func = funcs.FirstOrDefault(f => word.Equals(f.Name));
                        if (funcs != null && func != null)
                        {
                            if (func.Private)
                                ValidateAccess(func, func.DefinedAt, CurrentVarSystem);
                            if (Spoiler() is OpeningBracketSpan)
                            {
                                val = FuncCall(inst, word, currentContext, out isFunc, funcs.ToArray());
                                if (!allowFuncsToNotReturn && val == Void.Return)
                                    Error($"A value was expected, but the function did not return one (function: '{word}').");
                            }
                            else
                            {
                                val = func;
                            }
                            isFunc = true;
                        }
                    }
                }
                else
                {
                    val = InvokeExtern(ext.Type, true, null, word, ReadParamList());
                    isFunc = true;
                    ext = null;
                }
            }
            else
            {
                if (Spoiler() is OpeningBracketSpan)
                {
                    ReadParamList(allowUnknownVars: true);
                    val = 0d;
                    isFunc = true;
                }
            }

            // if it's a var, get it (if not, it will throw for us)
            if (!isFunc)
            {
                val = allowUnknownVars ? 0d : GetPointer(word, vs);
                if (val is Variable varpntr && varpntr.Value is FuncDefSpan funcpntr && Spoiler() is OpeningBracketSpan)
                    val = FuncCall(inst, funcpntr, currentContext, out isFunc, ReadParamList());
                if (val == null && span is ThisWordSpan thisWordSpan)
                {
                    if (FindParentVarSystem<Instance>(false ? null : throw new NotImplementedException()) is Instance thiss)
                        val = thiss;
                    else
                        Error($"'{thisWordSpan.Text}' word can only appear inside a non-static content in a class.");
                }
            }

            // if it's not a var, check static var
            if (val == null && firstTime)
            {
                throw new NotImplementedException("need to respect nsSpec.");
                val = UsedClasses(span).FirstOrDefault(cl => cl.Name.Equals(word));
            }
            if (val == null && firstTime)
                val = externs.FirstOrDefault(e => e.RefName.Equals(word));

            // check if it's an array ref, if it is - read index
            CheckArrayIndex(val is Variable pntr ? pntr.Value : val, out isArrPointer, out arrPointerIndex, allowUnknownVars);

            // maybe it's an instance and it is followed by a dot
            Span spoiler = Spoiler();

            // if spoiler is '.', relate it as an instance
            if (spoiler is DotSpan dot || (spoiler is QuestionMarkSpan && Spoiler(1) is DotSpan))
            {
                if (val is Variable pointer)
                    val = isArrPointer ? ((Instance)pointer.Value).ArrayValues[arrPointerIndex] : pointer.Value;
                else if (val is Instance vi && isArrPointer)
                    val = vi.ArrayValues[arrPointerIndex];

                if (spoiler is QuestionMarkSpan)
                    ReadSpan(); // read ?

                if (val is Instance ival)
                {
                    inst = ival;
                    ReadSpan(); // read .
                }
                else if (allowUnknownVars)
                {
                    val = StringToExpString("");
                    ReadSpan(); // read .
                }
                else if (val is ClassDefSpan cls)
                {
                    clas = cls;
                    ReadSpan(); // read .
                }
                else if (val is ExternClassDefSpan e)
                {
                    ext = e;
                    ReadSpan(); // read .
                }
                else
                {
                    ThrowRuntime($"Unexpected '.': '{word}' is not an instance reference or class name.", RuntimeException.NULL_REFERENCE);
                }
            }
            else if (val == null && !isFunc && !allowUnknownVars && ext == null)
            {
                string beforeDot = inst == null ? (clas == null ? "" : (clas.Name + '.')) : (inst.def.Name + '.');
                Error($"Unknown item '{beforeDot}{word}'.");
            }

            // else, break to return the value
            else
            {
                break;
            }
            firstTime = false;
        }

        specificNS = null;

        return val;
    }


    internal static Instance StringToExpString(string s)
    {
        ArgumentNullException.ThrowIfNull(s, nameof(s));

        var carr = new CharValue[s.Length];
        for (int c = 0; c < s.Length; c++)
            carr[c] = s[c];
        var expCarr = new ArrayInstance(ClassDefSpan.ExpArrayDef, carr);
        var exp = new Instance(ClassDefSpan.ExpStringDef);
        exp.Vars[0].SetSkippingConstant(expCarr);
        return exp;
    }

    internal static string ExpStringToString(Instance exp)
    {
        var oarr = (exp.Vars[0].Value as Instance).ArrayValues;
        string s = "";
        foreach (var c in oarr)
            s += c.Char;
        return s;
    }

    /// <summary>
    /// Get the list of functions available in the context of the given <see cref="Span"/>, including the functions defined in the same class (if the span is inside a function inside a class).
    /// </summary>
    /// <param name="vs"></param>
    /// <returns></returns>
    private IEnumerable<FuncDefSpan> GetFuncLs(IVarSystem vs)
    {
        throw new NotImplementedException();
        /*
        var ls = UsedFuncs(span);
        var func = FindParentContext<FuncDefSpan>(span);
        if (func?.DefinedAt == null)
            return ls;
        foreach (var f in func.DefinedAt.Funcs)
            ls = ls.Append(f);
        return ls;*/
    }

    private object CheckArrayIndex(object value, out bool isArrInd, out int index, bool allowUnknownVars = false)
    {
        isArrInd = false;
        index = -1;
        if (Spoiler() is ArrayOpenerSpan)
        {
            ReadSpan();
            index = (int)ReadValue<double>(allowUnknownVars);

            if (allowUnknownVars)
            {
                isArrInd = true;
            }
            else if (value is Instance inst && inst.IsArray)
            {
                if (index >= inst.ArrayValues.Length || index < 0)
                    ThrowRuntime($"index is out of range (index: {index}, array length: {inst.ArrayValues.Length}).", RuntimeException.INDEX_OUT_OF_RANGE);

                value = inst.ArrayValues[index];
                isArrInd = true;
            }
            else
                Error($"Only arrays can be followed by '['.");
            Read<ArrayCloserSpan>();
        }
        return value;
    }
}