using System;
using System.Linq;
using System.Collections.Generic;
using Exp.Spans;
using System.Collections;
using Exp.Operations;
//using Exp.Compiler;

namespace Exp;

public partial class Interpreter
{
    private IReadingOperation ReadReadingOperation(out Span[] src, Span firstSpan = null)
    {
        bool deleteRecord = readValue_codeRecord == null;
        readValue_codeRecord ??= [];

        IReadingOperation ReadSingle(out bool wasval, out bool bracketWasRead)
        {
            wasval = false;
            bracketWasRead = false;
            IReadingOperation value = null;
            Span span = firstSpan ?? ReadSpan();
            firstSpan = null;
            if (span is null)
                return null;

            if (span is NumberSpan num)
                value = ConstValueReadingOperation.For(num.Number.ToExp());
            else if (span is StringSpan)
                value = ConstValueReadingOperation.For(span.Text.ToExpString());
            else if (span is CharSpan)
                value = ConstValueReadingOperation.For(span.Text[0].ToExp());
            else if (span is TrueWordSpan)
                value = ConstValueReadingOperation.For(true.ToExp());
            else if (span is FalseWordSpan)
                value = ConstValueReadingOperation.For(false.ToExp());
            else if (span is NotSymbolSpan)
                value = new NotOperation(ReadSingle(out wasval, out bracketWasRead));
            else if (span is NullWordSpan)
                value = ConstValueReadingOperation.For(null);
            else if (span is FuncDefSpan func)
            {
                if (func.Name != null)
                    Error($"A {FuncDefSpan.ItemName} that is get readed as a value shouldn't have a name.");
                func.SpanItselfIsReadedAsValue = true;
                func.Operations ??= ReadOperations(func.InnerSource, func);
                value = new ReadingOperation(new FuncPntr(func, null));
            }
            else if (span is LenofWordSpan lenof)
                value = new LenofReadingOperation(ReadReadingOperation(), lenof);
            else if (span is TypeOfSpan type)
                value = ConstValueReadingOperation.For(type.Value);
            else if (span is OpeningBracketSpan)
            {
                bracketWasRead = true;
                value = null;
            }
            else if (span is ArrayOpenerSpan)
            {
                IReadingOperation[] readings = ReadParamListOps(true, true, false);

                // if all of the items are constants, return a constant array
                if (readings.All(r => r is ConstValueReadingOperation))
                {
                    value = new ConstArrayReadingOperation([.. readings.Select(r => r as ConstValueReadingOperation)]);
                }
                else
                {
                    value = new ArrayReadingOperation(readings);
                }
            }
            else if (span is InstInitSpan init)
            {
                if (init.DefName.Class != null)
                    value = new InitOperation(init.DefName.Class, ReadParamListOps());
                else if (init.DefName.Extern != null)
                    value = new ExternTypeInitOperation(init.DefName.Extern, ReadParamListOps());
                else
                    Error($"Class name was expected, but function received.");
            }
            else if (span is WordSpan word)
            {
                // on "?" and "??" checks, allow throwing
                if (word is ThrowWordSpan thr)
                {
                    if (!allowThrow)
                        Error("Throwing is not allowed where a value is expected unless it's a boolean selection ('?') or the right side of a '??' symbol.");
                    value = new Throwing(ReadReadingOperation());
                }
                else
                {
                    value = ReadPointingOrFuncCall(false, word, readValue: true);
                }
            }
            else if (span is OperatorSpan { Action: true } op)
            {
                throw new NotImplementedException();
                /*

                value = ReadNamedValueOrPointer(out bool isArrPntr, out int arrPntrInd, null, allowUnknownVars);
                if (value is Variable || (value is Instance && isArrPntr))
                {
                    if (op.TwoSides)
                        Throw($"Unexpected operation {op.Text}.");
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
                    Throw("Variable name was expected.");
                */
            }
            else if (span is WhiteSpaceSpan)
                return ReadSingle(out wasval, out bracketWasRead);
            else
                Error($"Unexpected symbol in value reading: {span.GetType().Name}.", span);


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
                string[] builtins = [number, "char", "bool", "function", "object"];
                if (!builtins.Contains(typeName))
                {
                    defName = ReadDefName(type); // will throw if not found
                    if (defName.Class == null && defName.Extern == null)
                        Error($"A class name was expected ({defName.FullText} is not a class, but {(defName.Defination as IExpItem)?.GetItemName()}).");
                }

                // check
                var single = value;
                value = new CustomReadingOperation<BoolValue>(() =>
                {
                    bool iz;
                    IValue? val = single?.Read();
                    if (defName != null)
                    {
                        if (defName.Class != null && val is Instance ins)
                            iz = ins.def == defName.Class;
                        else if (defName.Extern != null && val is ExternTypeInstance ext)
                        {
                            Type valType = ext.ExternInstance.GetType();
                            iz = valType == defName.Extern.Type || valType.IsSubclassOf(defName.Extern.Type) || ext.ExternInstance.GetType().GetInterfaces().Contains(defName.Extern.Type);
                        }
                        else
                            iz = false;
                    }
                    else if (val is NumberValue)
                        iz = typeName.Equals(number);
                    else if (val is BoolValue)
                        iz = typeName.Equals(builtins[2]);
                    else if (val is CharValue)
                        iz = typeName.Equals(builtins[1]);
                    else if (val is FuncPntr)
                        iz = typeName.Equals(builtins[3]);
                    else if (val is Instance)
                        iz = typeName.Equals(builtins[4]);
                    else // when val is null or SpecialValue<?>
                        iz = false;
                    if (not)
                        iz = !iz;
                    return iz;
                });
            }

            wasval = true;
            return value;
        }

        // values operations
        IReadingOperation val;
        bool valread;
        Instance nullCoalscingEx = null;
        allowThrow = false;

        var nums = new List<IReadingOperation>();
        var ops = new List<OperatorSpan>();

        // fill arrays
        do
        {
            val = ReadSingle(out valread, out bool nextIsBracket);

            if (nextIsBracket)
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
                var subRes = ReadReadingOperation(sub.ToArray());
                readValue_codeRecord = rec_bu;
                val = subRes;
            }

            nums.Add(val);

            Span spoiler = Spoiler();
            if (spoiler is OperatorSpan)
            {
                allowThrow = spoiler is NullCoalescingOperatorSpan;
                var oprtor = Read<OperatorSpan>();
                if (oprtor is NullCoalescingOperatorSpan ncos)
                    ncos.NullCoalsingEx = nullCoalscingEx;
                ops.Add(oprtor);
            }
            else
                break;
        } while (valread);

        if (nums.Count == 0)
            Error($"A value was expected.");

        // calculate math-first operations
        for (int i = 0; i < ops.Count; i++)
        {
            if (ops[i] is MultiplyOperatorSpan or DivideOperatorSpan or ModuluOperatorSpan)
            {
                nums[i] = CreateOperation(nums[i], ops[i], nums[i + 1]);
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
                nums[i] = CreateOperation(nums[i], ops[i], nums[i + 1]);
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
                nums[i] = CreateOperation(nums[i], ops[i], nums[i + 1]);
                nums.RemoveAt(i + 1);
                ops.RemoveAt(i);
                i--;
            }
        }

        // calculate the final result
        val = nums[0];
        for (int i = 1; i < nums.Count; i++)
        {
            val = CreateOperation(val, ops[i - 1], nums[i]);
        }

        IReadingOperation CreateOperation(IReadingOperation left, OperatorSpan @operator, IReadingOperation right)
        {
            return new OperatorResultOperation(@operator, left, right);
        }

        // ? check
        if (Spoiler() is QuestionMarkSpan)
        {
            ReadSpan();

            allowThrow = true;
            var tval = ReadReadingOperation();

            ReadWord(":");

            var fval = ReadReadingOperation();
            allowThrow = false;
            var booleanReading = val;
            val = new CustomReadingOperation<IValue>(() =>
            {
                var obj = booleanReading.Read();
                if (obj.IsBool)
                    return obj.Bool ? tval.Read() : fval.Read();

                ThrowRuntime($"A bool was expected, but {Extensions.GetExpTypeName(obj, true)} was read.", RuntimeException.INVALID_ARGUMENT);
                throw null;
            });
        }

        src = readValue_codeRecord.ToArray();
        if (deleteRecord)
            readValue_codeRecord = null;

        return val;
    }

    private IReadingOperation ReadReadingOperation(Span[] src)
    {
        IReadingOperation res = null;
        SwitchSpans(src, () => res = ReadReadingOperation());
        return res;
    }

    private IReadingOperation ReadReadingOperation() => ReadReadingOperation(out var _);

    /*
        private object ReadInstInitSpan(InstInitSpan init)
        {
            // get the class span
            ClassDefSpan cls = init.DefName.Class;
            ExternWordSpan ext = init.DefName.Extern;
            if (cls == null && ext == null)
                Throw($"Class name was expected, but {(init.DefName as IExpItem).GetItemName()} received.");

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
                    Throw($"'{cls.Name}' does not implement a constructor with this number of parameters.");

                // validate access to constructor
                if (func.Private)
                    ValidateAccess(func.Name, func.DefinedAt, CurrentVarSystem);

                // built in classes:
                // array
                bool isArr = cls.Name.Equals("Array");
                object[] arr = null;
                if (isArr)
                {
                    if (param[0] is not IReadingOperation<double>)
                        ThrowRuntime("A round non-negative number was expected as array length.", RuntimeException.INVALID_OPERATION);
                    int len = (int)((IReadingOperation<double>)param[0]).Read();
                    arr = new object[len];
                }

                // create instance and call constructor
                Instance value = new Instance(cls, arr);
                FuncCall(value, func.Name, currentContext, out bool _, cls.Funcs, param);
                return value;
            }
            else
            {
                if (param.Length > 0)
                    Throw("There are parameters attached, but no constructor is implemented for this type.");
                return new Instance(cls);
            }
        }
    */

    internal INamedValue GetNamedValueItem(IVarSystem vs, string name, Span span, bool first, int argsNum, string nsSpec = null)
    {
        if (nsSpec == null)
        {
            // if it's a var
            var pntr = GetPointer(name, vs, out var foundAt);

            if (pntr != null)
                ValidateAccess(pntr, foundAt, span.GetVS());

            if (pntr != null)
                return pntr;

            // if it's a function
            var funcLs = new List<FuncDefSpan>();
            if (first)
                funcLs.AddRange(UsedFuncs(span));

            if (vs is Instance inst)
            {
                funcLs.AddRange(inst.def.Funcs.Where(f => (!f.Static) || first));
            }

            var matchFuncs = funcLs.Where(f => f.Name == name && (argsNum < 0 || argsNum == f.Args.Length));
            if (matchFuncs.Count() > 1)
            {
                if (argsNum < 0)
                    ThrowRuntime($"More than 1 functions named '{name}' were found, specify number of parameters.", RuntimeException.INVALID_SYNTAX, span);
                return matchFuncs.FirstOrDefault(f => f.Args.Length == argsNum) ?? ThrowRuntime<INamedValue>($"No function named '{name}' and taking {argsNum} parameters was found.", RuntimeException.INVALID_SYNTAX, span);
            }
            else
                return matchFuncs.FirstOrDefault();
        }
        else
        {
            foreach (var def in definations)
            {
                if (def.Namespace == nsSpec && def.Name == name)
                {
                    if (def is INamedValue result)
                        return result;
                    else
                        ThrowRuntime($"A function or variable was expected, but {def.FullName} read.", RuntimeException.INVALID_SYNTAX, span);
                }
            }
        }
        return null;
    }

    internal PointingOrFuncCall ReadPointingOrFuncCall(bool isOp, WordSpan firstWord = null, bool readValue = false) => ReadPointingOrFuncCall(out bool _, isOp, firstWord, readValue);

    internal PointingOrFuncCall ReadPointingOrFuncCall(out bool isCall, bool isOp, WordSpan firstWord = null, bool readValue = false)
    {
        PointingOrFuncCall pnt = null, first = null;
        IVarSystem vs = null;
        while (true)
        {
            // read name
            isCall = false; // keep it inside the while, or expect a bug in extern class static property set
            var word = firstWord ?? ReadWord();
            firstWord = null;
            var name = word.FullText;
            string nsSpec = null;
            vs ??= word.GetVS();
            IEnumerable<INamedValue> possibilities = null;
            List<IReadingOperation[]> argLists = [];
            int? paramsCounter = null;
            PointingOrFuncCall newp = null;
            bool setter = false; // on array index / extern property set, this becomes true meaning the while loop won't continue bc we're converting these to func calls (array.set(...) / extern.pgetset(...)) so without it something like this: "arr[0] = true.next" would pe possible

            // if the name is in format "ns::name", read the namespace specifier and the real name
            if (first == null && Spoiler() is NamespaceSpecificationSpan)
            {
                ReadSpan();
                nsSpec = name;
                name = ReadWord().FullText;
            }

            // check if it's an extern invocation
            /*
            if (first == null && externs.FirstOrDefault(e => e.RefName == name) is ExternClassDefSpan extrn)
            {
                Read<DotSpan>();
                word = ReadWord();
                name = word.FullText;
                argLists.Add(ReadParamListOps());

                // point to the FundDefSpan.ExternInvoker function, with the invocation data as arguments
                newp = new PointingOrFuncCall(isOp, name, [[ConstValueReadingOperation.For(SpecialValue.From(extrn.Type)), ConstValueReadingOperation.For(SpecialValue.From(name)), ConstValueReadingOperation.For(null), ConstValueReadingOperation.For(SpecialValue.From(argLists.Last()))]], null, null, word, true, true) { KnownFunc = FuncDefSpan.ExternInvoker };
            }*/

            // check params counter (for function pointers like "const fn = myFunc(..3)")
            if (Spoiler() is OpeningBracketSpan && Spoiler(1) is CountSpan count)
            {
                ReadSpan();
                ReadSpan();
                Read<ClosingBracketSpan>();
                paramsCounter = count.Count;
            }

            // read argument list(s)
            else while (Spoiler() is OpeningBracketSpan)
                argLists.Add(ReadParamListOps());

            // if it is the first in the raw, then the var / func is already known (unless it's a property, in which case it will be read later as a pointing)
            var funcCtx = FindParentContext<FuncDefSpan>(word);
            if (newp == null && !(funcCtx != null && funcCtx.DefinedAt is ClassDefSpan cls && cls.Props.Any(p => p.Name == name))) // if it's not a property
            {
                if (first == null)
                {
                    possibilities = GetNamedValueItem(vs, name, word, true, paramsCounter ?? argLists.FirstOrDefault()?.Length ?? -1, nsSpec)?.PackAsArray(false);

                    // if it's first and it's a non static function of instance, the GetNamedValueItem(...) above won't find it bc vs is not an insatnce
                    possibilities ??= funcCtx?.DefinedAt?.Funcs.Where(f => f.Name == name && ((!f.Static) || first == null));
                }

                // if it's a class name, read static name
                if ((possibilities == null || !possibilities.Any()) && first == null && name != ThisWordSpan.Keyword && argLists.Count == 0 && paramsCounter == null)
                {
                    DefNameSpan defname = new DefNameSpan(nsSpec, name, word.Document, word.DocumentLocation, this);
                    if (defname == null)
                    {
                        Error($"Unknown item '{(nsSpec == null ? "" : (nsSpec + NamespaceSpecificationSpan.Symbol)) + name}'.");
                        ReadErrorPointing(true);
                        return null;
                    }

                    Read<DotSpan>();
                    name = ReadWord()?.FullText;

                    // read argument lists again
                    while (Spoiler() is OpeningBracketSpan)
                        argLists.Add(ReadParamListOps());

                    // normal class
                    if (defname.Class != null)
                    {
                        possibilities = (IEnumerable<INamedValue>)defname?.Class?.Vars.FirstOrDefault(v => v.Name == name).PackAsArray(false) ?? defname?.Class?.Funcs.Where(f => f.Name == name);
                        if (defname?.Class != null && (possibilities == null || !possibilities.Any()))
                            Error($"'{Extensions.GetExpTypeName(defname.Class, false)}' does not contain a static variable or function named '{name}'.");
                    }

                    // extern class
                    else if (defname.Extern != null)
                    {
                        // if there are arg lists, point to the FuncDefSpan.ExternInvoker function, with the invocation data as arguments (type, static, methodName, instance, args)
                        if (argLists.Any())
                            newp = new PointingOrFuncCall(isOp, name, [[ConstValueReadingOperation.For(SpecialValue.From(defname.Extern.Type)), ConstValueReadingOperation.For(true.ToExp()), ConstValueReadingOperation.For(SpecialValue.From(name)), ConstValueReadingOperation.For(null), ConstValueReadingOperation.For(SpecialValue.From(argLists.First()))]], null, null, word, true, true) { KnownFunc = FuncDefSpan.ExternInvoker };
                        else
                        {
                            // get property info
                            System.Reflection.PropertyInfo pinfo = null;
                            IValue enumVal = null;

                            if (defname.Extern.IsEnum)
                                enumVal = defname.Extern.EnumValues.GetValueOrDefault(name);
                            else
                                pinfo = defname.Extern.Props.FirstOrDefault(p => p.Name.StartWithLowerCase() == name);
                            IReadingOperation rval = null;
                            if (pinfo == null && enumVal == null)
                            {
                                Error($"Extern class '{defname.Defination.FullName} ({defname.Extern.Type})' does not contain a static property with the name '{name}'.");
                            }

                            else
                            {
                                // check set value
                                if (isOp && Spoiler() is SetSymbolSpan)
                                {
                                    ReadSpan();
                                    rval = ReadReadingOperation();
                                    setter = true;
                                    isCall = true;
                                    if (enumVal != null)
                                        Error($"Cannot set enum values ('{defname.Extern.Type.FullName}.{name}').");
                                    else if (pinfo.SetMethod == null)
                                        Error($"Extern class property '{defname.Extern.Type.FullName}.{pinfo.Name}' is read-only and cannot be set.");
                                }
                                else if (pinfo != null && pinfo.GetMethod == null)
                                    Error($"Extern class property '{defname.Extern.Type.FullName}.{pinfo.Name}' is write-only and cannot be read.");

                            }

                            if (pinfo != null)
                                // point to the FuncDefSpan.ExternPropGetSet function, with the getset data as arguments (type, pinfo, instance, valToSet, set)
                                newp = new PointingOrFuncCall(isOp, name, [[ConstValueReadingOperation.For(SpecialValue.From(defname.Extern.Type)), ConstValueReadingOperation.For(SpecialValue.From(pinfo)), ConstValueReadingOperation.For(null), rval, ConstValueReadingOperation.For((rval != null).ToExp())]], null, null, word, true, true) { KnownFunc = FuncDefSpan.ExternPropGetSet };
                            else
                                newp = new PointingOrFuncCall(isOp, name, [], null, null, word, true, true) { KnownPointer = new Variable(name, enumVal, null, false, true) };
                        }
                    }

                    // unknown / unexpected def
                    else
                    {
                        if (defname.Defination != null)
                            Error($"Unexpected item name '{defname.Defination.FullName}'.");
                        //ReadErrorPointing(true);
                        return null;
                    }

                }
            }

            // if there are multiple possibilities, try to find the right one by the num of arguments (if there are arg lists)
            INamedValue known = possibilities?.FirstOrDefault(p => p is not FuncDefSpan func || func.Args.Length == argLists.FirstOrDefault()?.Length);

            // create the pointing
            newp ??= new PointingOrFuncCall(isOp, name, argLists, paramsCounter, first == null ? vs : null, word, readValue, first == null) { Known = known };
            if (pnt != null) pnt.Next = newp;
            pnt = newp;
            first ??= newp;

            // read array brackets
            bool arrBrackets = false;
            while (Spoiler() is ArrayOpenerSpan)
            {
                arrBrackets = true;
                var getsetArgs = (IEnumerable<IReadingOperation>)ReadParamListOps(arrayBrackets: true);

                if (isOp && Spoiler() is OperatorSpan { Action: true } operatr)
                {
                    ReadSpan();

                    // convert operatr to ActionOperator enum
                    ActionOperator opEnum = ActionOperator.Reset;
                    if (operatr is SetPlusOperatorSpan)
                        opEnum = ActionOperator.Add;
                    else if (operatr is SetMinusOperatorSpan)
                        opEnum = ActionOperator.Subtract;
                    else if (operatr is PlusPlusOperatorSpan)
                        opEnum = ActionOperator.PlusPlus;
                    else if (operatr is MinusMinusOperatorSpan)
                        opEnum = ActionOperator.MinusMinus;

                    getsetArgs = getsetArgs.Append(ConstValueReadingOperation.For(((double)opEnum).ToExp())).Append(operatr.TwoSides ? ReadReadingOperation() : ConstValueReadingOperation.For(null));
                    setter = true;
                }

                newp = new PointingOrFuncCall(isOp, (setter ? FuncDefSpan.ArrayIndexSetter : FuncDefSpan.ArrayIndexGetter).Name, [[.. getsetArgs]], null, null, word, false, first: false);
                pnt.Next = newp;
                pnt = newp;
            }

            // if . read next
            bool nextOrNull = false;
            var spoiler = Spoiler();
            if (spoiler is QuestionMarkSpan)
            {
                nextOrNull = true;
                spoiler = Spoiler(1);
            }
            if (!setter && spoiler is DotSpan)
            {
                ReadSpan(); // ? or .
                if (nextOrNull)
                {
                    ReadSpan(); // .
                    pnt.NextOrNull = true;
                }
            }
            else
            {
                isCall = isCall || arrBrackets || argLists.Count > 0;
                break;
            }
        }

        return first;
    }

    private void ReadErrorPointing(bool lastSpanWasWord)
    {
        while (true)
        {
            var spoiler = Spoiler();

            if (lastSpanWasWord && spoiler is DotSpan)
            {
                lastSpanWasWord = false;
                ReadSpan();
                ReadWord();
            }
            else
            {
                return;
            }

            if (spoiler is WordSpan)
            {
                ReadWord();
                spoiler = Spoiler();
                while (spoiler != null)
                {
                    if (spoiler is OpeningBracketSpan or ArrayOpenerSpan)
                    {
                        ReadSpan();
                        if (Spoiler() is CountSpan && spoiler is not ArrayOpenerSpan)
                        {
                            ReadSpan();
                            Read<ClosingBracketSpan>();
                        }
                        else
                            ReadParamList(arrayBrackets: spoiler is ArrayOpenerSpan, true);
                    }
                    spoiler = Spoiler();
                }

                if (Spoiler() is not DotSpan)
                    return;
            }
        }
    }
}