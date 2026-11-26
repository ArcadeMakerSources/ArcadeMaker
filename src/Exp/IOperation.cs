using Exp.Spans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Runtime.CompilerServices;

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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static (Variable? variable, IValue? readingResult) GetVariable(PointingOrFuncCall pointing)
    {
        IValue? readingResult = null;
        var known = pointing.Next is null ? pointing.KnownPointer : null;
        if (known is null)
        {
            readingResult = pointing.Read();
            known = (readingResult as SpecialValue<INamedValue>)?.Value as Variable;
        }

        return (known, readingResult);
    }

    public void Make()
    {
        var (var, readingResult) = GetVariable(pointing);

        if (var == Variable.Futile)
            return;
        else if (var is not null)
            var.Value = opertor.Result(var.Value, readingOperation?.Read());
        else
            throw new Exception($"Pointing did not return a variable but '{readingResult.GetExpTypeName(true)}'.");
    }

    public class SkippingConstant(PointingOrFuncCall pointing, OperatorSpan opertor, IReadingOperation? readingOperation) : IOperation
    {
        public void Make()
        {
            var (var, readingResult) = GetVariable(pointing);

            if (var == Variable.Futile)
                return;
            else if (var is not null)
                var.SetSkippingConstant(opertor.Result(var.Value, readingOperation?.Read()));
            else
                throw new Exception($"Pointing did not return a variable but '{readingResult.GetExpTypeName(true)}'.");
        }
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

abstract class OperationWithInnerSource : IOperation
{
    internal IOperation[] InnerOperations { get; init; }
    internal IContext Context { get; init; }
    bool IsRunning { get; set; }

    public OperationWithInnerSource(IOperation[] innerSource, IContext context)
    {
        this.InnerOperations = innerSource;
        this.Context = context;
    }

    internal abstract void Run();

    protected void ExecuteInnerSource()
    {
        IContext ctx = Context;
        foreach (IOperation op in InnerOperations)
        {
            op.Make();
            if (ctx.Break)
            {
                ctx.Break = false;
                break;
            }
        }
    }

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

interface IConditionalStatement
{
    ConditionSpan ConditionSpan { get; }
    IReadingOperation ConditionReading { get; }
}

interface ILoopStatement
{
    ILoopContext LoopContext { get; }
}

class IfStatement(Interpreter interpreter, IfConditionSpan ctx, IReadingOperation cond, IOperation[] innerOps) : OperationWithInnerSource(innerOps, ctx), IConditionalStatement
{
    public ConditionSpan ConditionSpan => ctx;
    public IReadingOperation ConditionReading => cond;
    public ElseStatement? Else { get; set; }
    internal override void Run()
    {
        bool? condition = cond.Read()?.Bool;
        if (condition == true)
            ExecuteInnerSource();
        else if (condition == false)
            (Else as IOperation)?.Make();
        else
            interpreter.ThrowRuntime($"The result of an \"if\" condition was null.", RuntimeException.INVALID_OPERATION, ctx);
    }
}

class ElseStatement(Interpreter interpreter, ElseConditionSpan ctx, IOperation[] innerOps) : OperationWithInnerSource(innerOps, ctx)
{
    internal override void Run() => ExecuteInnerSource();
}

class WhileStatement(WhileConditionSpan ctx, IReadingOperation cond, IOperation[] innerOps, Variable counter) : OperationWithInnerSource(innerOps, ctx), ILoopStatement, IConditionalStatement
{
    public ConditionSpan ConditionSpan => ctx;
    public ILoopContext LoopContext => ctx;
    public IReadingOperation ConditionReading => cond;
    internal override void Run()
    {
        IValue counterNum = 0d.ToExp();
        counter?.SetSkippingConstant(counterNum);

        WhileConditionSpan LoopContext = ctx; // optimize
        IOperation[] InnerOperations = base.InnerOperations;

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

class ForStatement(ForLoopSpan ctx, IOperation init, IReadingOperation cond, IOperation step, IOperation[] innerOps, Variable counter) : OperationWithInnerSource(innerOps, ctx), ILoopStatement, IConditionalStatement
{
    public ConditionSpan ConditionSpan => ctx;
    public ILoopContext LoopContext => ctx;
    internal IOperation Init => init;
    public IReadingOperation ConditionReading => cond;
    internal IOperation Step => step;

    internal override void Run()
    {
        // set counter
        IValue counterNum = 0d.ToExp();
        counter?.SetSkippingConstant(counterNum);

        ForLoopSpan LoopContext = ctx; // optimizes

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


class ForeachStatement(ForEachLoopSpan ctx, Variable var, IReadingOperation readingOperation, IOperation[] innerOps, Variable counter) : OperationWithInnerSource(innerOps, ctx), ILoopStatement
{
    internal Variable Var => var;
    internal IReadingOperation ReadingOperation => readingOperation;
    public ILoopContext LoopContext => ctx;
    internal override void Run()
    {
        var LoopContext = ctx; // optimizes the next references to LoopContext
        var inter = Interpreter.Activated;
        int index = 0;
        IValue counterNum = 0d.ToExp();
        counter?.SetSkippingConstant(counterNum);
        var inst =
            ReadingOperation.Read()?.Inst ??
            inter.ThrowRuntime<Instance>("An attempt to iterate over null.", RuntimeException.NULL_REFERENCE, ctx);
        Instance.StartIteration(ctx);

        // optimize for arrays
        IValue?[]? arr = inst is ArrayInstance arrInst ?
            arrInst.ArrayValues :
            (inst.def.BaseArrayPropIndex is >= 0 and int pindex ? (inst.Vars[pindex].Value as ArrayInstance)?.ArrayValues : null);
        if (arr is not null)
        {
            // compiler or jit or someone will heavily optimize it
            foreach (IValue? item in arr)
            {
                Var.SetSkippingConstant(item);
                foreach (var op in InnerOperations)
                {
                    op.Make();
                    if (LoopContext.Break)
                    {
                        LoopContext.Break = false;
                        goto break_fe_arr;
                    }
                    if (LoopContext.Continue)
                    {
                        LoopContext.Continue = false;
                        goto continue_fe;
                    }
                }

            continue_fe:
                counterNum = (++index).ToExp();
                counter?.SetSkippingConstant(counterNum);
            }
        break_fe_arr:;
        }
        else foreach (var item in inst)
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
            counterNum = (++index).ToExp();
            counter?.SetSkippingConstant(counterNum);
        }
        break_fe:;
    }
}

// TODO: delete this feature
class RangeLoopStatement(RangeLoopSpan ctx, Variable var, IReadingOperation fromReadingOperation, IReadingOperation toReadingOperation, IOperation[] innerOps, Variable counter) : OperationWithInnerSource(innerOps, ctx), ILoopStatement
{
    internal Variable Var => var;
    internal IReadingOperation ToReadingOperation => toReadingOperation;
    public ILoopContext LoopContext => ctx ?? throw new ArgumentNullException();
    internal override void Run()
    {
        var from = fromReadingOperation?.Read().Number ?? 0d;
        var to = ToReadingOperation.Read().Number;
        bool toIsHigh = to > from;

        IValue counterNum = 0d.ToExp();
        counter?.SetSkippingConstant(counterNum);
        var LoopContext = ctx; // optimizes
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
            if (span is IContext ctx)
            {
                ctx.Break = true;

                if (ctx == this.Loop)
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

class ReturnStatement(FuncDefSpan func, IReadingOperation readingOperation, ReturnWordSpan? rword) : IOperation
{
    internal FuncDefSpan Func => func ?? throw new ArgumentNullException();
    public void Make()
    {
        // break any loop until getting to the function, and return
        Span? span = rword;
        while (span != null)
        {
            if (span is FuncDefSpan)
                break;
            if (span is IContext ctx)
            {
                ctx.Break = true;
            }
            span = span.Container;
        }
        func.Returns = readingOperation.Read();
        func.Return = true;
    }
}

class ReadingOperation(IValue value) : IReadingOperation
{
    internal static IReadingOperation Error => new CustomReadingOperation<IValue>(() =>
    {
        Interpreter.Activated.ThrowRuntime("Execution reached a build-time error expression.", RuntimeException.INVALID_SYNTAX);
        return null;
    });

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
        return new ArrayInstance(ClassDefSpan.ExpArrayDef, readings.Select(r => r.Read()).ToArray());
    }
}

class ConstArrayReadingOperation(ConstValueReadingOperation[] readings) : ConstValueReadingOperation(new ArrayInstance(ClassDefSpan.ExpArrayDef, readings.Select(r => r.Read()).ToArray()));

class LenofReadingOperation(IReadingOperation arrayReading, LenofWordSpan word) : IReadingOperation
{
    internal IReadingOperation ArrayReading => arrayReading ?? throw new ArgumentNullException();
    public IValue Read()
    {
        var array = ArrayReading.Read().Inst;
        if (array != null)
        {
            if (array.IsArray)
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
    internal ConstructorDefSpan? Constructor { get; }
    private Span? _newWord;
    internal InitOperation(ClassDefSpan def, IReadingOperation[] args, Span? newWord = null)
    {
        this.Def = def ?? throw new ArgumentNullException(nameof(def));
        this.Args = args ?? throw new ArgumentNullException(nameof(args));
        
        _newWord = newWord;
        
        // find the constructor with the given num of arguments
        var ctors = def.Funcs.Where(f => !f.Static && f.Args.Length == args.Length && f is ConstructorDefSpan).ToArray();
        if (ctors.Length == 0 && (def.Funcs.OfType<ConstructorDefSpan>().Any(ctor => !ctor.Static) || args.Length >= 1))
            Interpreter.Activated.Error($"{((IDefination)def).FullName} does not contain a constructor taking {args.Length} parameters.");
        if (ctors.Length >= 2)
            throw new Exception($"More than 1 constructor taking {args.Length} parameters found.");

        Constructor = ctors.FirstOrNull() as ConstructorDefSpan; // first or null if the class has no constructors at all

        if (Constructor != null)
        {
            // error if a null literal is passed as an argument to a non-nullable paramerter
            // we must do it after ResolveAttribute(), bc of the @NullableArg attribute
            var interpreter = Interpreter.Activated;
            void ValidateNonNullableParams(object? s, EventArgs e)
            {
                interpreter.BuildCompleted -= ValidateNonNullableParams;
                interpreter.ErrorIfLiteralNullPassedToNonNullableParam(Constructor!, args);
            }
            interpreter.BuildCompleted += ValidateNonNullableParams;
        }
    }

    public IValue Read()
    {
        Instance inst;
        if (Constructor?.OverridesInit == true)
        {
            if (Interpreter.Activated.FuncCall(null, Constructor, null, out bool _, Args.Select(a => a.Read()), _newWord) is not Instance init || init.def != Def)
            {
                Interpreter.Activated.ThrowRuntime($"Constructor did not return an instance of its class ({Def.GetExpTypeName(false)}).", RuntimeException.EXTERN_OPERATION_FAILED);
                throw null;
            }
            inst = init;
        }
        else
        {
            if (Constructor?.DefinedAt == ClassDefSpan.ExpArrayDef && Constructor.Args.Length == 1) // new Array(len)
            {
                int len = (int)(Args[0].Read()?.Number ?? Interpreter.Activated.ThrowRuntime<int>("Argument value was null.", RuntimeException.INVALID_ARGUMENT)); // null check is critical because the arguments null check wasn't happening yet!
                if (len < 0)
                    Interpreter.Activated.ThrowRuntime("Array length cannot have a negative size.", RuntimeException.INVALID_ARGUMENT);
                inst = new ArrayInstance(Def, new IValue[len]);
            }
            else
                inst = new Instance(Def);

            if (Constructor != null)
                Interpreter.Activated.FuncCall(inst, Constructor, null, out bool _, Args.Select(a => a.Read()), _newWord);
        }

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

    public IValue? Read()
    {
        return Interpreter.NewExtern(Extern.Type, Args);
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
        return Interpreter.InvokeExtern(Extern.Type, statc, inst, method, args);
    }
}

class ExternFuncInvocationOperation(FuncDefSpan invoker, ExternFunc externFunc) : IReadingOperation, IOperation
{
    public void Make() => Read();
    public IValue Read()
    {
        IValue? val = externFunc.Func.Invoke(invoker.Parent as Instance, invoker.ParamVariables.Select(v => v.Value).ToArray());
        invoker.Return = true; // it's ok if it's void
        invoker.Returns = val;
        return val;
    }
}

class TryStatement(TryWordSpan ctx, IOperation[] body, CatchStatement catc, FinallyStatement finaly) : OperationWithInnerSource(body, ctx)
{
    internal override void Run()
    {
        try
        {
            ExecuteInnerSource();
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

class CatchStatement : OperationWithInnerSource
{
    internal IReadingOperation When { get; }
    internal CatchWordSpan Word => (CatchWordSpan)Context;
    internal override void Run() => ExecuteInnerSource();

    public CatchStatement(IOperation[] body, IReadingOperation when, CatchWordSpan word) : base(body, word)
    {
        this.When = when;
    }
}

class FinallyStatement(IOperation[] innerOps, FinallyWordSpan ctx) : OperationWithInnerSource(innerOps, ctx)
{
    internal override void Run() => ExecuteInnerSource();
}

class Throwing(IReadingOperation exread, Span? throwWord) : IOperation, IReadingOperation
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

        Interpreter.Activated.ThrowRuntime(ex, throwWord);
        throw null;
    }
}

class PointingOrFuncCall(bool isOperationButNotReadingOperation, string name, IEnumerable<IReadingOperation[]> argLists, int? paramsCounter, IVarSystem vs, Span span, bool readValue, bool first) : IReadingOperation
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
        IValue value;

        // 'this' keyword
        if (first && Known == null && name == ThisWordSpan.Keyword)
        {
            if (GetThisOrPropertyOfThis(out IValue? result))
                return result;
            else
                Interpreter.Activated.ThrowRuntime($"Keyword '{ThisWordSpan.Keyword}' is not valid in a static content.", RuntimeException.INVALID_OPERATION, span);
        }

        // get the current item
        int numOfParams = paramsCounter ?? argLists.FirstOrDefault()?.Length ?? -1;
        INamedValue? item =
            Known ??
            Interpreter.Activated.GetNamedValueItem(VS, name, span, first, numOfParams);
        if (item == null)
        {
            Interpreter.Activated.ThrowRuntime(
                numOfParams >= 0 ? $"Unknown function '{(VS as Instance)?.def.GetExpTypeName(false) ?? "(?)"}.{name}(..{numOfParams})'." :
                $"Unknown item '{(VS as Instance)?.def.GetExpTypeName(false) ?? "(?)"}.{name}'.", RuntimeException.INVALID_SYNTAX, span
                );
            throw null!;
        }

        // if no more dots, return pointer or call the func and return its value
        if (Next == null)
        {
            if (argLists.Any())
            {
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
                {
                    FuncPntr funcPntr = new(fn, Interpreter.FindParentVarSystem<Instance>(VS));
                    return funcPntr;
                }

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
                    value = Interpreter.Activated.FuncCall(null, FuncDefSpan.ExternInvoker, null, out bool _, [SpecialValue.From(csInst.GetType()), false.ToExp(), SpecialValue.From(Next.Name), new SpecialValue(csInst), SpecialValue.From(Next.ArgLists.FirstOrDefault()) /*.Select(.Read()) is done in the builtin func code*/]);
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
                throw null!;
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

        IValue? RunFunc(FuncDefSpan? func, Instance? instance)
        {
            if (func == null)
                Interpreter.Activated.ThrowRuntime($"An argument list was read, but the given value was not a function but {Extensions.GetExpTypeName(item, true)} ({name}).", RuntimeException.INVALID_SYNTAX, span);

            if (!argLists.Any())
                Interpreter.Activated.ThrowRuntime("Missing argument list.", RuntimeException.INVALID_SYNTAX, span);

            if (isOperationButNotReadingOperation && Next == null && func.ReadOnly)
                Interpreter.Activated.ThrowRuntime($"The value returned by {func.GetExpTypeName(false)} must be used.", RuntimeException.INVALID_OPERATION, span);

            IValue? val = null;
            foreach (var ls in argLists)
            {
                instance ??= func == FuncDefSpan.ExternInvoker || func == FuncDefSpan.ExternPropGetSet ? null : (first ? Interpreter.FindParentVarSystem<Instance>(vs) : VS as Instance);
                val = Interpreter.Activated.FuncCall(instance, func!, null, out bool _, ls.Select(a => a?.Read()), span);
                func = val as FuncDefSpan;
            }
            return val;
        }

        bool GetThisOrPropertyOfThis(out IValue? value)
        {
            Instance thiss = Interpreter.FindParentVarSystem<Instance>(vs);

            if (thiss != null)
            {
                if (Next == null)
                {
                    value = thiss;
                }
                else
                {
                    Next.VS = thiss;
                    value = Next.Read();
                }
                return true;
            }

            value = null;
            return false;
        }

        void ThrowNullRef() => Interpreter.Activated.ThrowRuntime("Object reference not set to an instance of an object.", RuntimeException.NULL_REFERENCE, span);
        void ThrowPremitiveRef() => Interpreter.Activated.ThrowRuntime($"The value of {item.Name} was a premitive type and it cannot be followed by a dot.", RuntimeException.INVALID_OPERATION, span);
    }

    /// <summary>
    /// checks if a ReadingOperation is a pointing to a static const var with literal const value assigned to it
    /// </summary>
    /// <param name="interpreter">Needed because <see cref="Interpreter.staticPropsToInit"/> is required.</param>
    /// <param name="value">The literal constant value that this pointing points to, or <c>null</c> if the method returns <c>false</c>.</param>
    public bool IsPointingToLiteralConst(Interpreter interpreter, out IValue? value)
    {
        if (Next == null && KnownPointer is ClassStaticVar { Const: true } property)
        {
            if (property.Value != null) // enum values are created with a value assigned
            {
                value = property.Value;
                return true;
            }

            if (interpreter.staticPropsToInit.TryGetValue(property, out IReadingOperation? valueReadingOp))
            {
                if (valueReadingOp is ConstValueReadingOperation or ConstArrayReadingOperation)
                {
                    value = valueReadingOp.Read();
                    return true;
                }
            }
        }

        value = null;
        return false;
    }
}

class CustomReadingOperation<T>(Func<T> func) : IReadingOperation where T : IValue
{
    public IValue Read() => func();
}