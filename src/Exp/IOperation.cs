using Exp.Spans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Exp.Operations;

public interface IOperation
{
    void Make();
}

interface IReadingOperation
{
    IValue Read();
}

class Operation(Action action) : IOperation
{
    public void Make() => action();
    internal static IOperation Custom(Action action) => new Operation(action);
    internal static IOperation Error => new Operation(() => Interpreter.Activated.ThrowRuntime("Execution reached a build error.", "EXE_REACHED_BUILD_ERR"));
}

class Invocation(PointingOrFuncCall call) : IOperation
{
    public void Make()
    {
        call.Read();
    }
}

class OperatorResultOperation(OperatorSpan opertor, IReadingOperation left, IReadingOperation right) : IReadingOperation
{
    public IValue Read()
    {
        return opertor.Result(left, right);
    }
}

class Assignment(PointingOrFuncCall pointing, OperatorSpan opertor, IReadingOperation? readingOperation) : IOperation
{
    public void Make()
    {
        IValue? readingResult = null;
        var known = pointing.Next == null ? pointing.KnownPointer : null;
        if (known == null)
        {
            readingResult = pointing.Read();
            known = (readingResult as SpecialValue<INamedValue>)?.Value as Variable;
        }

        if (known == Variable.Futile)
            return;
        else if (known != null)
            known.Value = opertor.Result(known.Value, readingOperation?.Read());
        else
            throw new Exception($"Pointing did not return a variable but '{readingResult.GetExpTypeName(true)}'.");
    }
}

class VariablesDeclaration(Dictionary<Variable, IReadingOperation> decs) : IOperation
{
    public void Make()
    {
        foreach (var v in decs)
            v.Key.SetSkippingConstant(v.Value?.Read());
    }
}

interface IOperationWithInnerSource : IOperation
{
    IOperation[] InnerOperations { get; }
    IContext Context { get; }
    bool IsRunning { get; set; }
    void Run();

    void IOperation.Make()
    {
        bool wasRunning = IsRunning;
        IsRunning = true;

        IVarSystem thisVs = Context;
        var backup = wasRunning ? thisVs.BackupValues() : null;

        try
        {
            Run();
        }
        finally
        {
            if (backup != null)
                thisVs.RestoreValues(backup);
            IsRunning = wasRunning;
        }
    }
}

interface IConditionalStatement : IOperationWithInnerSource
{
    ConditionSpan ConditionSpan { get; }
    IReadingOperation ConditionReading { get; }
}

interface ILoopStatement : IOperationWithInnerSource
{
    ILoopContext LoopContext { get; }
}

class IfStatement(IfConditionSpan ctx, IReadingOperation cond, IOperation[] innerOps) : IConditionalStatement
{
    public IContext Context => ctx;
    public bool IsRunning { get; set; }
    public ConditionSpan ConditionSpan => ctx ?? throw new ArgumentNullException();
    public IReadingOperation ConditionReading => cond ?? throw new ArgumentNullException();
    public IOperation[] InnerOperations { get => innerOps; set => innerOps = value; }
    public ElseStatement Else { get; set; }
    public void Run()
    {
        if (cond.Read().Bool)
            foreach (var op in InnerOperations) op.Make();
        else
            (Else as IOperation)?.Make();
    }
}

class ElseStatement(ElseConditionSpan ctx, IOperation[] innerOps) : IOperationWithInnerSource
{
    public IContext Context => ctx;
    public bool IsRunning { get; set; }
    public IOperation[] InnerOperations => innerOps;
    public void Run()
    {
        foreach (var op in InnerOperations) op.Make();
    }
}

class WhileStatement(WhileConditionSpan ctx, IReadingOperation cond, IOperation[] innerOps, Variable counter) : ILoopStatement, IConditionalStatement
{
    public IContext Context => ctx;
    public bool IsRunning { get; set; }
    public ConditionSpan ConditionSpan => ctx ?? throw new ArgumentNullException();
    public ILoopContext LoopContext => ctx ?? throw new ArgumentNullException();
    public IReadingOperation ConditionReading => cond ?? throw new ArgumentNullException();
    public IOperation[] InnerOperations => innerOps;
    public void Run()
    {
        IValue counterNum = 0d.ToExp();
        counter?.SetSkippingConstant(counterNum);

        while (cond.Read().Bool)
        {
            foreach (var op in InnerOperations)
            {
                op.Make();
                if (LoopContext.Break)
                {
                    LoopContext.Break = false;
                    goto break_while;
                }
                if (LoopContext.Continue)
                {
                    LoopContext.Continue = false;
                    goto continue_while;
                }
            }

            continue_while:
            if (counter != null)
            {
                counterNum = (counterNum.Number + 1).ToExp();
                counter.SetSkippingConstant(counterNum);
            }
        }
    break_while: { }
    }
}

class ForStatement(ForLoopSpan ctx, IOperation init, IReadingOperation cond, IOperation step, IOperation[] innerOps, Variable counter) : ILoopStatement, IConditionalStatement
{
    public IContext Context => ctx;
    public bool IsRunning { get; set; }
    public ConditionSpan ConditionSpan => ctx ?? throw new ArgumentNullException();
    public ILoopContext LoopContext => ctx ?? throw new ArgumentNullException();
    internal IOperation Init => init;
    public IReadingOperation ConditionReading => cond ?? throw new ArgumentNullException();
    internal IOperation Step => step;
    public IOperation[] InnerOperations => innerOps ?? throw new ArgumentNullException();

    public void Run()
    {
        // set counter
        IValue counterNum = 0d.ToExp();
        counter?.SetSkippingConstant(counterNum);

        for (init?.Make(); cond.Read().Bool; step?.Make())
        {
            foreach (var op in InnerOperations)
            {
                op.Make();

                if (LoopContext.Break)
                {
                    LoopContext.Break = false;
                    goto break_for;
                }
                if (LoopContext.Continue)
                {
                    LoopContext.Continue = false;
                    goto continue_for;
                }
            }

            if (counter != null)
            {
                counterNum = (counterNum.Number + 1).ToExp();
                counter.SetSkippingConstant(counterNum);
            }
            continue_for:;
        }
    break_for:;
    }
}


class ForeachStatement(ForEachLoopSpan ctx, Variable var, IReadingOperation readingOperation, IOperation[] innerOps, Variable counter) : ILoopStatement
{
    public IContext Context => ctx;
    public bool IsRunning { get; set; }
    internal Variable Var => var ?? throw new ArgumentNullException();
    internal IReadingOperation ReadingOperation => readingOperation ?? throw new ArgumentNullException();
    public IOperation[] InnerOperations => innerOps;
    public ILoopContext LoopContext => ctx ?? throw new ArgumentNullException();
    public void Run()
    {
        IValue counterNum = 0d.ToExp();
        counter?.SetSkippingConstant(counterNum);
        var iter = ReadingOperation.Read()?.Inst;
        if (iter?.GetBasearray() is Instance { IsArray: true } array)
        {
            foreach (var item in array.ArrayValues)
            {
                Var.SetSkippingConstant(item);
                foreach (var op in InnerOperations)
                {
                    op.Make();
                    if (LoopContext.Break)
                    {
                        LoopContext.Break = false;
                        goto break_fe;
                    }
                    if (LoopContext.Continue)
                    {
                        LoopContext.Continue = false;
                        goto continue_fe;
                    }
                }

            continue_fe:
                if (counter != null)
                {
                    counterNum = (counterNum.Number + 1).ToExp();
                    counter.SetSkippingConstant(counterNum);
                }
            }
        break_fe: { }
        }
        else if (iter?.def.HasTag(AttributeDefSpan.IteratableAttr) == true)
        {
            var hasNextFunc = iter.def.Funcs.First(f => f.Name == ForEachLoopSpan.IteratableFunc_HasNext);
            var nextFunc = iter.def.Funcs.First(f => f.Name == ForEachLoopSpan.IteratableFunc_Next);
            var resetFunc = iter.def.Funcs.First(f => f.Name == ForEachLoopSpan.IteratableFunc_Reset);

            Interpreter.Activated.FuncCall(iter, resetFunc, null, out bool _, []);
            while (Interpreter.Activated.FuncCall(iter, hasNextFunc, null, out bool _, [])?.Bool == true)
            {
                Var.SetSkippingConstant(Interpreter.Activated.FuncCall(iter, nextFunc, null, out bool _, []));
                foreach (var op in InnerOperations)
                {
                    op.Make();
                    if (LoopContext.Break)
                    {
                        LoopContext.Break = false;
                        goto break_fe;
                    }
                    if (LoopContext.Continue)
                    {
                        LoopContext.Continue = false;
                        goto continue_fe;
                    }
                }
            
            continue_fe:
                if (counter != null)
                {
                    counterNum = (counterNum.Number + 1).ToExp();
                    counter.SetSkippingConstant(counterNum);
                }
            }
        break_fe:;
        }
        else
        {
            string err = iter == null ? "The object to iterate over was null" : $"The object to iterate over was not an array, basearray-setter or an instance of a class containg a tag of attribute '{AttributeDefSpan.IteratableAttr.GetExpTypeName(false)}', but it was of type '{Extensions.GetExpTypeName(iter, false)}'";
            Interpreter.Activated.ThrowRuntime($"Foreach loop failed: {err}.", RuntimeException.INVALID_OPERATION, ctx);
        }
    }
}

class RangeLoopStatement(RangeLoopSpan ctx, Variable var, IReadingOperation fromReadingOperation, IReadingOperation toReadingOperation, IOperation[] innerOps, Variable counter) : ILoopStatement
{
    public IContext Context => ctx;
    public bool IsRunning { get; set; }
    internal Variable Var => var ?? throw new ArgumentNullException();
    internal IReadingOperation ToReadingOperation => toReadingOperation ?? throw new ArgumentNullException();
    public IOperation[] InnerOperations => innerOps;
    public ILoopContext LoopContext => ctx ?? throw new ArgumentNullException();
    public void Run()
    {
        var from = fromReadingOperation?.Read().Number ?? 0d;
        var to = ToReadingOperation.Read().Number;
        bool toIsHigh = to > from;

        IValue counterNum = 0d.ToExp();
        counter?.SetSkippingConstant(counterNum);
        for (var.SetSkippingConstant(from.ToExp()); toIsHigh ? var.Value.Number < to : var.Value.Number > to; var.Value = toIsHigh ? (var.Value.Number + 1).ToExp() : (var.Value.Number - 1).ToExp())
        {
            foreach (var op in InnerOperations)
            {
                op.Make();
                if (LoopContext.Break)
                {
                    LoopContext.Break = false;
                    goto break_fe;
                }
                if (LoopContext.Continue)
                {
                    LoopContext.Continue = false;
                    goto continue_fe;
                }
            }

        continue_fe:
            if (counter != null)
            {
                counterNum = (counterNum.Number + 1).ToExp();
                counter.SetSkippingConstant(counterNum);
            }

            if (var.Value?.IsNumber != true)
                Interpreter.Activated.ThrowRuntime($"The value of '{var.Name}' must be a number, because it is used by the {RangeLoopSpan.ItemName} (The value was {var.Value.GetExpTypeName(true)}).", RuntimeException.INVALID_ARGUMENT, ctx);
        }
    break_fe: { }
    }
}

class BreakStatement(BreakWordSpan bword, ILoopContext loop) : IOperation
{
    internal ILoopContext Loop => loop ?? throw new ArgumentNullException();
    public void Make()
    {
        // break any loop until the given one
        Span span = bword;
        while (true)
        {
            if (span is ILoopContext loopctx)
            {
                loopctx.Break = true;

                if (loopctx == this.Loop)
                    return;
            }
            else if (span == null)
                throw new Exception("The given loop is not in the stack.");
            span = span.Container;
        }
    }
}

class ContinueStatement(ContinueWordSpan cword, ILoopContext loop) : IOperation
{
    internal ILoopContext Loop => loop ?? throw new ArgumentNullException();
    public void Make()
    {
        // break any loop until the given one, and continue the given one
        Span span = cword;
        while (true)
        {
            if (span is ILoopContext loopctx)
            {
                if (loopctx == this.Loop)
                {
                    loopctx.Continue = true;
                    return;
                }
                loopctx.Break = true;
            }
            else if (span == null)
                throw new Exception("The given loop is not in the stack.");
            span = span.Container;
        }
    }
}

class ReturnStatement(FuncDefSpan func, IReadingOperation readingOperation, ReturnWordSpan rword) : IOperation
{
    internal FuncDefSpan Func => func ?? throw new ArgumentNullException();
    public void Make()
    {
        // break any loop until getting to the function, and return
        Span span = rword;
        while (span != null)
        {
            if (span is ILoopContext loopctx)
            {
                loopctx.Break = true;
            }
            span = span.Container;
        }
        func.Returns = readingOperation.Read();
        func.Return = true;
        Interpreter.Activated.toReturn = true;
    }
}

class ReadingOperation(IValue value) : IReadingOperation
{
    internal IValue Value => value;

    public IValue Read()
    {
        return value;
    }
}

class ConstValueReadingOperation(IValue value) : IReadingOperation
{
    public IValue Read()
    {
        return value;
    }

    internal static IReadingOperation For(IValue value) => new ConstValueReadingOperation(value);
}

class ArrayReadingOperation(IReadingOperation[] readings) : IReadingOperation
{
    internal IReadingOperation[] Readings => readings ?? throw new ArgumentNullException();
    public IValue Read()
    {
        return new Instance(ClassDefSpan.ExpArrayDef, readings.Select(r => r.Read()).ToArray());
    }
}

class ConstArrayReadingOperation(ConstValueReadingOperation[] readings) : ConstValueReadingOperation(new Instance(ClassDefSpan.ExpArrayDef, readings.Select(r => r.Read()).ToArray()));

class LenofReadingOperation(IReadingOperation arrayReading, LenofWordSpan word) : IReadingOperation
{
    internal IReadingOperation ArrayReading => arrayReading ?? throw new ArgumentNullException();
    public IValue Read()
    {
        var array = ArrayReading.Read().Inst;
        if (array != null)
        {
            if (array.IsArray == true)
                return new NumberValue(array.ArrayValues.Length);
            else
                Interpreter.Activated.ThrowRuntime($"lenof operation failed: {Extensions.GetExpTypeName(array, false)} is not an array.", RuntimeException.INVALID_OPERATION, word);
        }
        else
            Interpreter.Activated.ThrowRuntime($"lenof operation failed: the given value was null.", RuntimeException.INVALID_OPERATION, word);
        throw null;
    }
}

class NotOperation(IReadingOperation readingOperation) : IReadingOperation
{
    internal IReadingOperation ReadingOperation => readingOperation ?? throw new ArgumentNullException();
    public IValue Read()
    {
        return new BoolValue(!ReadingOperation.Read().Bool);
    }
}

class InitOperation : IReadingOperation
{
    internal ClassDefSpan Def;
    internal IReadingOperation[] Args;
    internal ConstructorDefSpan Constructor { get; }
    internal InitOperation(ClassDefSpan def, IReadingOperation[] args)
    {
        this.Def = def ?? throw new ArgumentNullException(nameof(def));
        this.Args = args ?? throw new ArgumentNullException(nameof(args));

        // find the constructor with the given num of arguments
        var ctors = def.Funcs.Where(f => !f.Static && f.Args.Length == args.Length && f is ConstructorDefSpan);
        if (ctors.Count() == 0 && def.Funcs.OfType<ConstructorDefSpan>().Any())
            Interpreter.Activated.Error($"{((IDefination)def).FullName} does not contain a constructor taking {args.Length} parameters.");
        if (ctors.Count() >= 2)
            throw new Exception($"More than 1 constructor taking {args.Length} parameters found.");

        Constructor = ctors.FirstOrNull() as ConstructorDefSpan; // first or null if the class has no constructors at all
    }

    public IValue Read()
    {
        Instance inst;
        if (Constructor?.DefinedAt == ClassDefSpan.ExpArrayDef && Constructor.Args.Length == 1) // new Array(len)
        {
            int len = (int)(Args[0].Read()?.Number ?? Interpreter.Activated.ThrowRuntime<int>("Argument value was null.", RuntimeException.INVALID_ARGUMENT)); // null check is critical because the arguments null check wasn't happening yet!
            if (len < 0)
                Interpreter.Activated.ThrowRuntime("Array length cannot have a negative size.", RuntimeException.INVALID_ARGUMENT);
            inst = new Instance(Def, new IValue[len]);
        }
        else
            inst = new Instance(Def);

        if (Constructor != null)
            Interpreter.Activated.FuncCall(inst, Constructor, null, out bool _, Args.Select(a => a.Read()));

        return inst;
    }
}

class ExternTypeInitOperation : IReadingOperation
{
    internal ExternClassDefSpan Extern { get; }
    internal IReadingOperation[] Args { get; }

    internal ExternTypeInitOperation(ExternClassDefSpan extrn, IReadingOperation[] args)
    {
        ArgumentNullException.ThrowIfNull(extrn);
        ArgumentNullException.ThrowIfNull(args);

        this.Extern = extrn;
        this.Args = args;

        if (!Extern.Type.GetConstructors(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).Any(ctor => ctor.GetParameters().Length == Args.Length))
            Interpreter.Activated.Error($"Extern class '{extrn.GetExpTypeName(false)}' does not contain a constructor taking {Args.Length} parameters.");
    }

    public IValue Read()
    {
        return Interpreter.NewExtern(Extern.Type, [.. Args.Map(a => a.Read())]);
    }
}

class ExternInvocationOperation(ExternClassDefSpan extrn, object inst, string method, IReadingOperation[] args) : IReadingOperation
{
    internal ExternClassDefSpan Extern => extrn ?? throw new ArgumentNullException();
    internal IReadingOperation[] Args => args ?? throw new ArgumentNullException();
    internal string Method => method ?? throw new ArgumentNullException();
    public IValue Read()
    {
        bool statc = false ? false : throw new NotImplementedException();
        return Interpreter.InvokeExtern(Extern.Type, statc, inst, method, [.. args.Select(a => a.Read())]);
    }
}

class ExternFuncInvocationOperation(FuncDefSpan invoker, ExternFunc externFunc) : IReadingOperation
{
    public IValue Read() => externFunc.Func.Invoke(invoker.Parent as Instance, invoker.ParamVariables.Map(v => v.Value).ToArray());
}

class TryStatement(TryWordSpan ctx, IOperation[] body, CatchStatement catc, FinallyStatement finaly) : IOperationWithInnerSource
{
    public IContext Context => ctx;
    public bool IsRunning { get; set; }
    public IOperation[] InnerOperations => body;

    public void Run()
    {
        try
        {
            foreach (var op in InnerOperations)
                op.Make();
        }
        catch (RuntimeException ex) //when (catc != null && (catc.When == null || (catc.When.Read() ?? OnWhenReturnsNull()).Bool)) // when returns null handling is not working this way
        {
            if (catc == null)
                throw;
            if (catc.When != null && !(catc.When.Read() ?? OnWhenReturnsNull()).Bool)
                throw;

            catc.Word.Vars[0].Value = ex.ex;
            ((IOperation)catc).Make();
        }
        finally
        {
            ((IOperation)finaly)?.Make();
        }

        IValue OnWhenReturnsNull()
        {
            Interpreter.Activated.ThrowRuntime("The 'when' condition of a catch statement returned null, which is not allowed.", RuntimeException.INVALID_OPERATION, catc.Word.When.Condition.FirstOrDefault());
            throw null;
        }
    }
}

class CatchStatement(IOperation[] body, IReadingOperation when, CatchWordSpan word) : IOperationWithInnerSource
{
    public IContext Context => word;
    public bool IsRunning { get; set; }
    public IOperation[] InnerOperations => body;
    internal IReadingOperation When => when;
    internal CatchWordSpan Word => word;
    public void Run()
    {
        foreach (var op in InnerOperations)
            op.Make();
    }
}

class FinallyStatement(IOperation[] innerOps, FinallyWordSpan ctx) : IOperationWithInnerSource
{
    public IContext Context => ctx;
    public bool IsRunning { get; set; }
    public IOperation[] InnerOperations => innerOps;
    public void Run()
    {
        foreach (var op in InnerOperations) op.Make();
    }
}

class Throwing(IReadingOperation exread) : IOperation, IReadingOperation
{
    public void Make()
    {
        Read();
    }

    public IValue Read()
    {
        var ex = exread.Read()?.Inst;

        if (ex == null)
            Interpreter.Activated.ThrowRuntime("The value to throw was null.", RuntimeException.NULL_REFERENCE);
        if (ex.def != ClassDefSpan.ExpExceptionDef)
            Interpreter.Activated.ThrowRuntime($"Only instances of type {ClassDefSpan.ExpExceptionDef.GetExpTypeName(false)} can be thrown, but the given value was of type {ex.def.GetExpTypeName(false)}.", RuntimeException.INVALID_ARGUMENT);

        Interpreter.Activated.ThrowRuntime(ex);
        throw null;
    }
}

class PointingOrFuncCall(bool isOp, string name, IEnumerable<IReadingOperation[]> argLists, int? paramsCounter, IVarSystem vs, Span span, bool readValue, bool first) : IReadingOperation
{
    internal IVarSystem VS { get => vs; set => vs = value; }
    internal PointingOrFuncCall Next { get; set; }
    internal bool NextOrNull { get; set; }
    internal Variable KnownPointer { get; set; }
    internal FuncDefSpan KnownFunc { get; set; }
    internal INamedValue Known
    {
        get => KnownPointer ?? (INamedValue)KnownFunc;
        set
        {
            if (value is Variable p)
                KnownPointer = p;
            else if (value is FuncDefSpan f)
                KnownFunc = f;
            else if (value == null)
            {
                KnownPointer = null;
                KnownFunc = null;
            }
            else
                throw new ArgumentException($"Unknown named value type: {value.GetType().Name}");
        }
    }
    internal string Name => name;
    internal IEnumerable<IReadingOperation[]> ArgLists => argLists;
    public IValue Read()
    {
        //ArgumentNullException.ThrowIfNull(VS);
        IValue value;

        // 'this' keyword
        if (first && Known == null && name == ThisWordSpan.Keyword)
        {
            Instance thiss = Interpreter.FindParentVarSystem<Instance>(vs);

            if (thiss != null)
            {
                if (Next == null)
                    return thiss;

                Next.VS = thiss;
                return Next.Read();
            }

            Interpreter.Activated.ThrowRuntime($"Keyword 'this' is not valid in a static content.", RuntimeException.INVALID_OPERATION, span);
        }

        // get the current item
        int numOfParams = paramsCounter.HasValue ? paramsCounter.Value : (argLists.FirstOrDefault()?.Length ?? -1);
        INamedValue? item = Known ?? Interpreter.Activated.GetNamedValueItem(VS, name, span, first, numOfParams);
        if (item == null)
        {
            Interpreter.Activated.ThrowRuntime(numOfParams >= 0 ? $"Unknown function '{(VS as Instance)?.def.GetExpTypeName(false) ?? "(?)"}.{name}(..{numOfParams})'." : $"Unknown item '{(VS as Instance)?.def.GetExpTypeName(false) ?? "(?)"}.{name}'.", RuntimeException.INVALID_SYNTAX, span);
            throw null!;
        }

        // if no more dots, return pointer or call the func and return its value
        if (Next == null)
        {
            if (argLists.Any())
            {
                if (Name == "pause")
                    _ = 1;
                FuncDefSpan? fn = item as FuncDefSpan;
                Instance? inst = VS as Instance;
                if (fn == null && item is Variable v && v.Value?.IsFunc == true) // it's not a function, but it IS a function POINTER
                {
                    fn = v.Value.FuncPntr.Func;
                    inst = v.Value.FuncPntr.Instance;
                }
                return RunFunc(fn, inst);
            }
            else
            {
                if (readValue)
                {
                    if (item.IsVar)
                        return item.Value;
                }

                if (item is FuncDefSpan fn)
                    return new FuncPntr(fn, VS as Instance);

                if (item is IValue v)
                    return v;

                return SpecialValue.From(item);
            }
        }

        // if there are more dots, go to next pointing
        Instance? funcPntrInst = null;
        if (item is Variable pntr)
        {
            if (pntr.Value == null)
            {
                if (NextOrNull)
                    return readValue ? null : SpecialValue.From<INamedValue>(Variable.Futile);
                ThrowNullRef();
            }
            else if (pntr.Value.IsInst)
            {
                var inst = pntr.Value.Inst;
                if (inst.def != ClassDefSpan.ExternTypeValueDef || Next.ArgLists?.Any() == false)
                {
                    Next.VS = inst;
                    value = Next.Read();
                    goto Return;
                }
                else
                {
                    // invoke extern method
                    var csInst = (inst as ExternTypeInstance)?.ExternInstance;
                    var args = Next.ArgLists.FirstOrDefault();
                    value = Interpreter.Activated.FuncCall(null, FuncDefSpan.ExternInvoker, null, out bool _, [SpecialValue.From(csInst.GetType()), false.ToExp(), SpecialValue.From(Next.Name), SpecialValue.From(csInst), SpecialValue.From(Next.ArgLists.FirstOrDefault()) /*.Select(.Read()) is done in the builtin func code*/]);
                    goto Return;
                }
            }
            else if (pntr.Value is FuncPntr funcpntr)
            {
                funcPntrInst = funcpntr.Instance;
                item = funcpntr.Func; // so it's like if it was a direct function
            }
            else
            {
                ThrowPremitiveRef();
                throw null;
            }
        }
        if (item is FuncDefSpan func)
        {
            IValue fresult = RunFunc(func, funcPntrInst); // funcPntrInst is null unless "item" was a Variable which is value was a FuncPntr. in this case, "func" is the value of this FuncPntr "Func" property
            if (fresult == null)
            {
                if (NextOrNull)
                    return readValue ? null : SpecialValue.From<INamedValue>(Variable.Futile);
                ThrowNullRef();
            }
            if (fresult.IsInst)
            {
                Next.VS = fresult.Inst;
                value = Next.Read();
            }
            else
            {
                ThrowPremitiveRef();
                throw null;
            }
        }
        else if (item == null)
        {
            Interpreter.Activated.ThrowRuntime($"Unknown item '{name}'.", RuntimeException.INVALID_SYNTAX, span);
            throw null;
        }
        else
        {
            throw new Exception($"Something went wrong (unexpected INamedValue type: {item} [{item.GetType()}]).");
        }

    Return:
        return value;

        IValue RunFunc(FuncDefSpan? func, Instance? instance)
        {
            if (func == null)
                Interpreter.Activated.ThrowRuntime($"An argument list was read, but the given value was not a function but {Extensions.GetExpTypeName(item, true)} ({name}).", RuntimeException.INVALID_SYNTAX, span);

            if (!argLists.Any())
                Interpreter.Activated.ThrowRuntime("Missing argument list.", RuntimeException.INVALID_SYNTAX, span);

            if (isOp && Next == null && func.ReadOnly)
                Interpreter.Activated.ThrowRuntime($"The value returned by {func.GetExpTypeName(false)} must be used.", RuntimeException.INVALID_OPERATION, span);

            IValue val = null;
            foreach (var ls in argLists)
            {
                instance ??= func == FuncDefSpan.ExternInvoker || func == FuncDefSpan.ExternPropGetSet ? null : (first ? Interpreter.FindParentVarSystem<Instance>(vs) : VS as Instance);
                val = Interpreter.Activated.FuncCall(instance, func, null, out bool _, ls.Map(a => a?.Read()));
                func = val as FuncDefSpan;
            }
            return val;
        }

        void ThrowNullRef() => Interpreter.Activated.ThrowRuntime("Object reference not set to an instance of an object.", RuntimeException.NULL_REFERENCE, span);
        void ThrowPremitiveRef() => Interpreter.Activated.ThrowRuntime($"The value of {item.Name} was a premitive type and it cannot be followed by a dot.", RuntimeException.INVALID_OPERATION, span);
    }
}

class CustomReadingOperation<T>(Func<T> func) : IReadingOperation where T : IValue
{
    public IValue Read() => func();
}