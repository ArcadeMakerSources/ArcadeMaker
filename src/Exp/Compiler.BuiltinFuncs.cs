using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Exp.Operations;
using Exp.Spans;
using System.Threading.Tasks;

namespace Exp
{
    public partial class Interpreter
    {
        bool skipBuiltinFuncs = false;
        private bool BuiltinFuncs(IContext ctx, Instance instance)
        {
            FuncDefSpan func = null;

            T GetArg<T>(int i, Func<T, bool> cond = null, bool allowDefault = false) where T : IValue
            {
                if (allowDefault && func.Vars[i].Value == null)
                    return default;
                if (func.Vars[i].Value is T arg && (cond?.Invoke(arg) ?? true))
                    return arg;
                else
                    ThrowRuntime($"Parameter '{func.Vars[i].Name}' must be a {typeof(T).Name}, but it was '{func.Vars[i].Value?.GetType().ToString() ?? "null"}'.", RuntimeException.INVALID_ARGUMENT);
                return default;
            }
            Instance GetInstArg(int i, ClassDefSpan cls = null)
            {
                if (func.Vars[i].Value.IsInst && (cls == null || func.Vars[i].Value.Inst.def == cls))
                    return func.Vars[i].Value.Inst;
                else
                    ThrowRuntime($"Parameter '{func.Vars[i].Name}' must be of type '{((IDefination)cls)?.FullName ?? "any non-premitive"}'.", RuntimeException.INVALID_ARGUMENT);
                return null;
            }

            //if (skipBuiltinFuncs)
            //    return false;
            skipBuiltinFuncs = true;
            bool bin = false;

            if (ctx is FuncDefSpan binfunc)
            {
                func = binfunc;

                if (ExternInvokers.TryGetValue(func, out ExternFunc? extrn))
                {
                    func.Return = true;
                    func.Returns = extrn.Func?.Invoke(instance, [..func.ParamVariables.Map(p => p.Value)]);
                    bin = true;
                }
                else if (func == FuncDefSpan.ArrayIndexGetter)
                {
                    int i = (int)GetArg<IValue>(0).Number;

                    if (i >= instance.ArrayValues.Length)
                        ThrowRuntime("Index is out of range.", RuntimeException.INDEX_OUT_OF_RANGE);
                    func.Returns = instance.ArrayValues[i];
                    func.Return = true;
                    bin = true;
                }
                else if (func == FuncDefSpan.ArrayIndexSetter)
                {
                    int i = (int)GetArg<IValue>(0).Number;
                    if (i >= instance.ArrayValues.Length)
                        ThrowRuntime("Index is out of range.", RuntimeException.INDEX_OUT_OF_RANGE);
                    ActionOperator op = (ActionOperator)GetArg<IValue>(1).Number;
                    var val = GetArg<IValue>(2, allowDefault: true);
                    if (op == ActionOperator.Reset)
                        instance.ArrayValues[i] = val;
                    else if (op == ActionOperator.Add)
                        instance.ArrayValues[i] = PlusOperatorSpan.GetResult(instance.ArrayValues[i], val, null);
                    else if (op == ActionOperator.Subtract)
                        instance.ArrayValues[i] = MinusOperatorSpan.GetResult(instance.ArrayValues[i], val, null);
                    else if (op == ActionOperator.PlusPlus)
                        instance.ArrayValues[i] = PlusOperatorSpan.GetResult(instance.ArrayValues[i], 1d.ToExp(), null);
                    else if (op == ActionOperator.MinusMinus)
                        instance.ArrayValues[i] = MinusOperatorSpan.GetResult(instance.ArrayValues[i], 1d.ToExp(), null);
                    else
                        throw new Exception("Unexpected action operator on array index.");

                    bin = true;
                }
                else if (func == FuncDefSpan.ExternInvoker)
                {
                    var type = GetArg<SpecialValue<Type>>(0).Value;
                    bool statc = GetArg<IValue>(1).Bool;
                    var method = GetArg<SpecialValue<string>>(2).Value;
                    var obj = GetArg<SpecialValue<object>>(3, allowDefault: true)?.Value;
                    var args = GetArg<SpecialValue<IReadingOperation[]>>(4).Value.Map(r => r.Read());
                    func.Returns = InvokeExtern(type, statc, obj, method, [.. args]);
                    func.Return = true;
                    bin = true;
                }
                else if (func == FuncDefSpan.ExternPropGetSet)
                {
                    var type = GetArg<SpecialValue<Type>>(0).Value;
                    var pinfo = GetArg<SpecialValue<System.Reflection.PropertyInfo>>(1).Value;
                    var obj = GetArg<SpecialValue<object>>(2, allowDefault: true)?.Value;
                    var val = GetArg<IValue>(3, allowDefault: true);
                    bool isSet = GetArg<IValue>(4).Bool;
                    if (isSet)
                        pinfo.SetValue(obj, ExpValToCsVal(val));
                    else
                        func.Returns = CsValToExpVal(pinfo.GetValue(obj));
                    func.Return = !isSet;
                    bin = true;
                }

                if (func.DefinedAt != null)
                {
                    if (func.DefinedAt.Name == "Date" && func.Name == "setToNow")
                    {
                        DateTime now = DateTime.Now;
                        //instance.Vars.First(v => v.Name == "day").Value = (double)now.Day;
                        //instance.Vars.First(v => v.Name == "month").Value = (double)now.Month;
                        //instance.Vars.First(v => v.Name == "year").Value = (double)now.Year;
                        //instance.Vars.First(v => v.Name == "hour").Value = (double)now.Hour;
                        //instance.Vars.First(v => v.Name == "minute").Value = (double)now.Minute;
                        //instance.Vars.First(v => v.Name == "sec").Value = (double)now.Second;
                        //instance.Vars.First(v => v.Name == "millis").Value = (double)now.Millisecond;
                        //instance.Vars.First(v => v.Name == "nanos").Value = (double)now.Nanosecond;
                        instance.Vars[0].Value = ((double)now.Year).ToExp();
                        instance.Vars[1].Value = ((double)now.Month).ToExp();
                        instance.Vars[2].Value = ((double)now.Day).ToExp();
                        instance.Vars[3].Value = ((double)now.Hour).ToExp();
                        instance.Vars[4].Value = ((double)now.Minute).ToExp();
                        instance.Vars[5].Value = ((double)now.Second).ToExp();
                        instance.Vars[6].Value = ((double)now.Millisecond).ToExp();
                        instance.Vars[7].Value = ((double)now.Nanosecond).ToExp();
                        bin = true;
                    }
                    else if (func.Static && func.DefinedAt == ClassDefSpan.ExpTypeDef && func.Name == "get")
                    {
                        var arg = func.Vars[0].Value as Instance;
                        if (arg == null)
                            ThrowRuntime($"Invalid Argument: {func.Args[0].Name} must be a non-premitive type (received: {func.Vars[0].Value}).", RuntimeException.INVALID_ARGUMENT);
                        else
                        {
                            func.Returns = arg.def.ExpType;
                            func.Return = true;
                            bin = true;
                        }
                    }
                    else if (func.DefinedAt?.Name == "cs")
                    {
                        if (func.Name == "int")
                        {
                            func.Returns = ((int)GetArg<IValue>(0).Number).AsExtern();
                            func.Return = true;
                            bin = true;
                        }
                        else if (func.Name == "float")
                        {
                            func.Returns = ((float)GetArg<IValue>(0).Number).AsExtern();
                            func.Return = true;
                            bin = true;
                        }
                        else if (func.Name == "long")
                        {
                            func.Returns = ((long)GetArg<IValue>(0).Number).AsExtern();
                            func.Return = true;
                            bin = true;
                        }
                        else if (func.Name == "byte")
                        {
                            func.Returns = (GetArg<NumberValue>(0)).AsExtern();
                            func.Return = true;
                            bin = true;
                        }
                        else if (func.Name == "action")
                        {
                            var f = GetArg<FuncPntr>(0);
                            func.Returns = f.Func.Args.Length > 0 ?
                                           new Action<object>((object args) => f.Call(this, CsValToExpVal(args) is Instance { IsArray: true } arr ? arr.ArrayValues : [])).AsExtern() :
                                           new Action(() => f.Call(this, [])).AsExtern();
                            func.Return = true;
                            bin = true;
                        }
                        else if (func.Name == "exp")
                        {
                            NumberValue val = 0;
                            var ext = ((ExternTypeInstance)GetInstArg(0, ClassDefSpan.ExternTypeValueDef)).ExternInstance;
                            if (ext is double i)
                                val = i;
                            else if (ext is byte b)
                                val = b;
                            else if (ext is float f)
                                val = (double)f;
                            else if (ext is long l)
                                val = l;
                            else if (ext is decimal d)
                                val = (double)d;
                            else
                            {
                                try
                                {
                                    val = Convert.ToDouble(ext);
                                }
                                catch (Exception ex)
                                {
                                    ThrowRuntime($"Could not cast the given value to {ValueHelper.tnum} (Error: {ex.Message}).", RuntimeException.INVALID_ARGUMENT);
                                }
                            }

                            func.Returns = (IValue)val ?? SpecialValue.From(ext);
                            func.Return = true;
                            bin = true;
                        }
                    }
                }
                else if (func.Name == "refEquals")
                {
                    IValue a = GetArg<IValue>(0), b = GetArg<IValue>(1);
                    if (a == null || b == null)
                        func.Returns = (a == b).ToExp(); // operator == does not fire .Equals(...) so it's fine if one of them is an Instance
                    else
                        func.Returns = (a.IsInst && b.IsInst && ReferenceEquals(a, b)).ToExp();
                    func.Return = true;
                    bin = true;
                }
                else if (func.Name == "setTimeout")
                {
                    double millis = GetArg<IValue>(0).Number;
                    FuncPntr action = GetArg<FuncPntr>(1);

                    async Task Make()
                    {
                        await Task.Delay((int)millis);
                        action.Call(this, []);
                    }

                    RunAsync(Make());

                    func.Return = false;
                    bin = true;
                }
                else if (func.Name == "runAsync" && func.Args.Length == 2)
                {
                    FuncPntr action = GetArg<FuncPntr>(0);
                    FuncPntr onComplete = GetArg<FuncPntr>(1, allowDefault: true);

                    async Task Make()
                    {
                        var res = await Task.Run(() => action.Call(this, []));
                        onComplete.Call(this, [res]);
                    }

                    RunAsync(Make());

                    func.Return = false;
                    bin = true;
                }


                else if (func.Namespace == "reflection")
                {
                    Instance ToExpPropertyInst(Variable v)
                    {
                        ClassDefSpan expPropDef = definations.FirstOrDefault(d => d is ClassDefSpan expPropDef && d.Namespace == "reflection" && d.Name == "Property") as ClassDefSpan;
                        if (expPropDef == null)
                            ThrowRuntime("reflection::Property class not found.", RuntimeException.INIT_ERR);
                        var p = new Instance(expPropDef);
                        p.Vars[0].Value = v.Name.ToExpString();
                        p.Vars[1].Value = v.Value;
                        p.Vars[2].Value = v.Private.ToExp();
                        p.Vars[3].Value = v.Const.ToExp();
                        return p;
                    }

                    if (func.Name == "getAttr")
                    {
                        if (func.Args.Length == 2) // for property
                        {
                            Instance type = GetInstArg(0, ClassDefSpan.ExpTypeDef);
                            ClassDefSpan cls = type.GetClassFromExpTypeInstanceOrThrowRuntime(this);

                            string propName = GetInstArg(1, ClassDefSpan.ExpStringDef).ToString();
                            ICanSetAttr prop = (ICanSetAttr)cls.Props.FirstOrDefault(p => p.Name == propName) ?? cls.Vars.OfType<ClassStaticVar>().FirstOrDefault(p => p.Name == propName);
                            if (prop == null)
                                ThrowRuntime($"'{((IDefination)cls).FullName}' does not contain a property named '{propName}'.", RuntimeException.INVALID_ARGUMENT);
                            if (prop.AttrInfo == null)
                                ThrowRuntime($"{((IDefination)cls).FullName}.{propName} does not have tags.", RuntimeException.INVALID_ARGUMENT);

                            func.Returns = prop.AttrInfo.ToExpArray();
                            func.Return = true;
                            bin = true;
                        }
                        else if (func.Args.Length == 3) // for funcs
                        {
                            Instance type = GetInstArg(0, ClassDefSpan.ExpTypeDef);
                            ClassDefSpan cls = type.GetClassFromExpTypeInstanceOrThrowRuntime(this);

                            string funcName = GetInstArg(1, ClassDefSpan.ExpStringDef).ToString();
                            int paramsCount = (int)GetArg<IValue>(2).Number;
                            FuncDefSpan fn = cls.Funcs.FirstOrDefault(p => p.Args.Length == paramsCount && p.Name == funcName);
                            if (fn == null)
                                ThrowRuntime($"'{((IDefination)cls).FullName}' does not contain a function named '{funcName}' taking {paramsCount} parameters.", RuntimeException.INVALID_ARGUMENT);
                            if (fn.AttrInfo == null)
                                ThrowRuntime($"{((IDefination)cls).FullName}.{funcName}(..{paramsCount}) does not have tags.", RuntimeException.INVALID_ARGUMENT);

                            func.Returns = fn.AttrInfo.ToExpArray();
                            func.Return = true;
                            bin = true;
                        }
                        else if (func.Args.Length == 1) // for class
                        {
                            Instance type = GetInstArg(0, ClassDefSpan.ExpTypeDef);
                            ClassDefSpan cls = type.GetClassFromExpTypeInstanceOrThrowRuntime(this);

                            func.Returns = cls.AttrInfo.ToExpArray();
                            func.Return = true;
                            bin = true;
                        }
                    }
                    else if (func.Name == "getProperties")
                    {
                        List<Instance> props = [];
                        Instance input = null;

                        input = GetInstArg(0);

                        foreach (var prop in input.Vars)
                        {
                            try
                            {
                                ValidateAccess(prop, input.def, currentContext);
                                var p = ToExpPropertyInst(prop);
                                props.Add(p);
                            }
                            catch { } // access denied
                        }

                        func.Return = true;
                        func.Returns = new Instance(ClassDefSpan.ExpArrayDef, props.ToArray());
                        bin = true;
                    }
                    else if (func.Name == "getProperty")
                    {
                        Instance input = null;
                        string pname = null;
                        Variable p = null;

                        input = GetInstArg(0);
                        pname = GetInstArg(1, ClassDefSpan.ExpStringDef).ToString();

                        p = input.Vars.FirstOrDefault(prop => prop.Name == pname);
                        if (p == null)
                            ThrowRuntime($"Property '{pname}' not found.", RuntimeException.NOT_FOUND);
                        ValidateAccess(p, input.def, currentContext);

                        func.Return = true;
                        func.Returns = ToExpPropertyInst(p);
                        bin = true;
                    }
                    else if (func.Name == "setProperty")
                    {
                        Instance input = null;
                        string pname = null;
                        Variable p = null;

                        input = GetInstArg(0);
                        pname = GetInstArg(1, ClassDefSpan.ExpStringDef).ToString();

                        p = input.Vars.FirstOrDefault(prop => prop.Name == pname);
                        if (p == null)
                            ThrowRuntime($"Property '{pname}' not found.", RuntimeException.NOT_FOUND);
                        ValidateAccess(p, input.def, currentContext);

                        p.Value = func.Vars[2].Value;

                        bin = true;
                    }
                    else if (func.Name == "getFunctions" || func.Name == "getConstructors")
                    {
                        bool ctors = func.Name == "getConstructors";

                        Instance type = GetInstArg(0, ClassDefSpan.ExpTypeDef);
                        ClassDefSpan cls = type.GetClassFromExpTypeInstanceOrThrowRuntime(this);

                        var funcDefs = !ctors ? cls.Funcs.Where(ff => ff is not ConstructorDefSpan) : cls.Funcs.OfType<ConstructorDefSpan>();
                        var funcs = new Instance[funcDefs.Count()];
                        for (int i = 0; i < funcs.Length; i++)
                        {
                            var funcInfoDef = definations.FirstOrDefault(d => d is ClassDefSpan && d.Namespace == "reflection" && d.Name == (ctors ? "ConstructorInfo" : "FunctionInfo")) as ClassDefSpan ?? throw new Exception("reflection::(Function/Constructor)Info class not found.");
                            var f = new Instance(funcInfoDef);
                            f.Vars[0].Value = type;
                            if (!ctors)
                                f.Vars[1].Value = StringToExpString(funcDefs.ElementAt(i).Name);
                            f.Vars[ctors ? 1 : 2].Value = new Instance(ClassDefSpan.ExpArrayDef, funcDefs.ElementAt(i).Args.Select(a => a.Name.ToExpString()).ToArray());
                            f.Vars[ctors ? 2 : 3].Value = funcDefs.ElementAt(i).Private.ToExp();
                            f.Vars[ctors ? 3 : 4].Value = funcDefs.ElementAt(i).Static.ToExp();
                            funcs[i] = f;
                        }

                        var results = new Instance(ClassDefSpan.ExpArrayDef, funcs);
                        func.Return = true;
                        func.Returns = results;
                        bin = true;
                    }
                    else if (/*func.DefinedAt.Name == "FunctionInfo" && */func.Name == "invoke")
                    {
                        Instance input = null;
                        string fname = null;
                        FuncDefSpan f = null;
                        Instance args = null;

                        // get instance
                        input = GetInstArg(0);

                        // get func name
                        fname = GetInstArg(1, ClassDefSpan.ExpStringDef).ToString();

                        // get args
                        args = GetInstArg(2, ClassDefSpan.ExpArrayDef);

                        // get func def
                        f = input.def.Funcs.FirstOrDefault(func1 => func1.Name == fname && func1.Args.Length == args.ArrayValues.Length && func1 is not ConstructorDefSpan);
                        if (f == null)
                            ThrowRuntime($"function '{fname}(..{args.ArrayValues.Length})' was not found.", RuntimeException.NOT_FOUND);
                        ValidateAccess(f, input.def, currentContext);

                        // invoke
                        func.Returns = FuncCall(input, f, currentContext, out bool _, args.ArrayValues);
                        func.Return = func.Returns is not Void;
                        bin = true;
                    }
                    else if (func.Name == "createInstance")
                    {
                        Instance type = null;
                        ClassDefSpan cls = null;
                        ConstructorDefSpan c = null;
                        Instance args = null;

                        // get type
                        type = GetInstArg(0, ClassDefSpan.ExpTypeDef);

                        // get class def
                        cls = type.GetClassFromExpTypeInstanceOrThrowRuntime(this);

                        // get args
                        args = GetInstArg(1, ClassDefSpan.ExpArrayDef);

                        // get ctor def
                        c = cls.Funcs.FirstOrDefault(f => f is ConstructorDefSpan && f.Args.Length == args.ArrayValues.Length) as ConstructorDefSpan;
                        if (c == null)
                            ThrowRuntime($"{((IDefination)cls).FullName} does not contain a constructor takes {args.ArrayValues.Length} parameters.", RuntimeException.NOT_FOUND);
                        ValidateAccess(c, cls, currentContext);

                        // invoke
                        Instance create = new Instance(cls);
                        FuncCall(create, c, currentContext, out bool _, args.ArrayValues);
                        func.Returns = create;
                        func.Return = true;
                        bin = true;
                    }
                    else if (func.Name == "getTypeByName")
                    {
                        // get type name
                        string[] tname = GetInstArg(0, ClassDefSpan.ExpStringDef).ToString().Split(NamespaceSpecificationSpan.Symbol);

                        // get type
                        if (tname.Length > 2 || (tname.Length == 2 && string.IsNullOrWhiteSpace(tname[1])))
                            ThrowRuntime("Namespace was not specified correctly.", RuntimeException.INVALID_ARGUMENT);
                        ClassDefSpan cls = definations.FirstOrDefault(d => d is ClassDefSpan && d.Namespace == (tname.Length == 2 ? tname[0] : null) && d.Name == (tname.Length == 0 ? tname[0] : tname[1])) as ClassDefSpan;
                        if (cls == null)
                            ThrowRuntime($"Class '{tname[0] + (tname.Length == 2 ? $"::{tname[1]}" : "")}' was not found.", RuntimeException.NOT_FOUND);
                        Instance type = cls.ExpType;

                        func.Return = true;
                        func.Returns = type;
                        bin = true;
                    }
                }
            }

            skipBuiltinFuncs = false;
            return bin;
        }

        internal enum ActionOperator
        {
            Reset,
            Add,
            Subtract,
            PlusPlus,
            MinusMinus,
            MinusMinusThen,
            PlusPlusThen
        }
    }
}