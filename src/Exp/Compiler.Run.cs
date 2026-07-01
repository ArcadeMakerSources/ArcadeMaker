using System;
using System.Linq;
using System.Collections.Generic;
using Exp.Spans;
using System.Reflection;
using System.Runtime.CompilerServices;
using Exp.Operations;
using System.Threading.Tasks;
//using Exp.Compiler;
using System.Threading;

namespace Exp;

public partial class Interpreter
{
    IContext currentContext => stack?.context;
    readonly IVarSystem _currentVarSystem_; // set to this at constructor
    IVarSystem CurrentVarSystem
    {
        get => currentContext ?? _currentVarSystem_;
    }
    string currNs;

    public void Run()
    {
        try
        {
            var currDocUsings = currUsings.ToArray();
            var currDocNs = currNs;

            // first run static vars init-code, then constructors
            foreach (var clas in definations.OfType<ClassDefSpan>())
            {
                currNs = clas.Namespace;
                currUsings.Clear();
                clas.Document.Usings?.ForEach(use => currUsings.Add(use));

                //foreach (var sv in clas.Vars.OfType<ClassStaticVar>())
                //    if (sv.InitValueCode != null) sv.Value = Run<object>(sv.InitValueCode);
                foreach (var ctor in clas.Funcs.Where(f => f is ConstructorDefSpan { Static: true }))
                    Run(ctor.InnerSource, ctor);
            }

            currUsings.Clear();
            currDocUsings.ForEach(use => currUsings.Add(use));
            currNs = currDocNs;

            IContext context = null;

            Run(context);
        }
        catch (RuntimeException ex) when (ex.ByExpThrowStmt)
        {
            ThrowRuntime(ex.ex.Vars[0].Value?.ToString(), ex.ex.Vars[1].Value?.ToString());
        }
    }

    private class StackIndex
    {
        internal readonly IContext context;
        internal readonly StackIndex parent;

        internal StackIndex(IContext context, StackIndex parent)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));
            this.context = context;
            this.parent = parent;
        }
    }
    private class VsStackIndex
    {
        internal readonly IVarSystem vs;
        internal readonly VsStackIndex parent;
        private static IVarSystem def;
        internal static IVarSystem Default { get => def ?? Interpreter.Activated; set => def = value; }

        internal VsStackIndex(IVarSystem vs, VsStackIndex parent)
        {
            ArgumentNullException.ThrowIfNull(vs, nameof(vs));
            this.vs = vs;
            this.parent = parent;
        }
    }

    private StackIndex stack;
    private VsStackIndex vsStack;
    internal void Run(IContext context = null, bool single = false, bool neutral = false, bool clearVars = true)
    {
        throw new NotImplementedException();


        //Variable[] contextVarsBefore = null;
        //if (context != null)
        //{
        //    stack = new StackIndex(context, stack);
        //    vsStack = new VsStackIndex(context, vsStack);

        //    if (clearVars)
        //    {
        //        contextVarsBefore = context.Vars.ToArray();
        //        context.Vars.Clear();
        //    }
        //}
        //else
        //{
        //    stack = null;
        //    vsStack = new VsStackIndex(this, null);
        //}

        ////if (BuiltinFuncs(context))
        ////    return;

        //WordSpan cmd = null, prevCmd = null;
        //while (CodeSpans == null ? spansCursor < SourceSpans.Length : codeCursor < CodeSpans.Length)
        //{
        //    if (cmd != null && single)
        //        break;
        //    prevCmd = cmd;

        //    // break or continue if needed
        //    ILoopContext firstBroken = null;
        //    int ShouldBreak(StackIndex stack) // 0 No, 1 Continue, 2 Break
        //    {
        //        //if (throwing != null)
        //        //    return 2;
        //        while (stack != null)
        //        {
        //            var con = stack.context;
        //            if (con is ILoopContext lc)
        //            {
        //                firstBroken = lc;
        //                if (lc.Break)
        //                    return 2;
        //                if (lc.Continue)
        //                    return 1;
        //            }
        //            else if (con is FuncDefSpan func && func.Return)
        //                return 2;

        //            stack = stack.parent;
        //        }
        //        firstBroken = null;
        //        return 0;
        //    }

        //    int res = ShouldBreak(stack);

        //    if (res == 2)
        //        break;
        //    if (res == 1)
        //    {
        //        if (firstBroken == context)
        //            firstBroken.Continue = false;
        //        break;
        //    }

        //    // make next cmd
        //    Span nextSpan = ReadSpan();
        //    if (nextSpan is WordSpan _cmd)
        //        cmd = _cmd;
        //    else if (nextSpan != null)
        //        Throw($"Unexpected span '{nextSpan.FullText}.'");
        //    else
        //        break;// throw new Exception("cmd was null, meaning something went wrong as while condition should prevent that.");

        //    //if (cmd is IContext toSetCtx)
        //    //    toSetCtx.Context = context;

        //    void RunContext(IContext rctx, bool clearVars)
        //    {
        //        //var stackb = this.stack;
        //        Run(rctx.InnerSource, rctx, clearVars: clearVars);
        //        //this.stack = stackb;
        //    }

        //    if (cmd is ConditionSpan cond)
        //    {
        //        if (neutral)
        //            continue;

        //        // init counter if set
        //        bool delCounter = false;
        //        if (cond is ILoopContext counterc)
        //            delCounter = VarExists(counterc.Counter);
        //        Action TickCounter = null;
        //        if (cond is ILoopContext lc && lc.Counter != null)
        //        {
        //            Variable counter = SetVar(lc.Counter, 0d, cond, lc, createConst: true);
        //            TickCounter = () =>
        //            {
        //                if (lc.Counter != null)
        //                    //SetVar(lc.Counter, GetVar<double>(lc.Counter, false, lc) + 1, null, lc);
        //                    counter.SetSkippingConstant((double)counter.Value + 1);
        //            };
        //        }

        //        // if statement / while loop
        //        if (cond is IfConditionSpan || cond is WhileConditionSpan)
        //        {
        //            while (Run<bool>(cond.Condition))
        //            {
        //                RunContext(cond, true);
        //                cond.ConditionWasTrue = true;

        //                if (cmd is IfConditionSpan || cond is ILoopContext { Break: true } || (ShouldBreak(stack) == 2))
        //                    break;

        //                TickCounter?.Invoke();
        //            }
        //        }

        //        // for loop
        //        else if (cond is ForLoopSpan loops)
        //        {
        //            Variable[] loopVars = loops.Vars.ToArray();
        //            loops.Vars.Clear();
        //            for (Run(loops.InitExe, loops, clearVars: false); Run<bool>(cond.Condition, loops); Run(loops.StepExe, loops, clearVars: false))
        //            {
        //                RunContext(loops, false);

        //                if (loops.Break || ShouldBreak(stack) == 2)
        //                    break;

        //                TickCounter?.Invoke();
        //            }
        //            loops.Vars.Clear();
        //            loops.Vars.AddRange(loopVars);
        //        }

        //        // delete counter if set
        //        if (delCounter && cond is ILoopContext cc)
        //            DeleteVar(cc.Counter, cc);
        //    }

        //    // else statement
        //    else if (cmd is ElseConditionSpan elses)
        //    {
        //        if (neutral)
        //            continue;

        //        if (prevCmd is ConditionSpan conditionSpan && (!conditionSpan.ConditionWasTrue) && conditionSpan is not ForLoopSpan)
        //        {
        //            RunContext(elses, true);
        //        }
        //    }
        //    else if (cmd is ForEachLoopSpan fe)
        //    {
        //        if (neutral)
        //            continue;

        //        var arr = Run<Instance>(fe.ArrReadText);

        //        // base array
        //        while (arr != null && !arr.IsArray && arr.def.BaseArrayProp is Property bap)
        //            arr = (arr.Vars.FirstOrDefault(v => v.Name.Equals(bap.Name)) is Variable vv ? vv.Value as Instance : null);

        //        if (arr == null)
        //            ThrowRuntime("Array is null.", RuntimeException.INVALID_OPERATION);
        //        if (!arr.IsArray)
        //            Throw($"An array or basearray defining instance was expected (type read: {arr})");
        //        int counter = 0;
        //        try
        //        {
        //            Variable[] vars = fe.Vars.ToArray();
        //            fe.Vars.Clear();
        //            foreach (var obj in arr.ArrayValues)
        //            {
        //                if (fe.Counter != null)
        //                    SetVar(fe.Counter, counter++, fe);
        //                SetVar(fe.VarName, obj, fe);
        //                RunContext(fe, false);

        //                // a comment left weeks after implementing StackIndex:
        //                // i have no idea why i did'nt check ShouldBreak(stack) here too, but it works (and in while loop it wouldn't):
        //                /*
        //                 * while true : id loop
        //                   {
        //                     while true
        //                     {
        //                      foreach x in [1, 2, 3]
        //                      {
        //                             print x
        //                       while true
        //                       {
        //                        break loop
        //                       }
        //                      }
        //                     }
        //                    }
        //                */
        //                if (fe.Break)
        //                    break;
        //            }
        //            fe.Vars.Clear();
        //            fe.Vars.AddRange(vars);
        //        }
        //        catch (InvalidOperationException ex)
        //        {
        //            ThrowRuntime("Invalid operation while in foreach loop: " + ex.Message, RuntimeException.INVALID_OPERATION);
        //        }
        //    }
        //    else if (cmd is BreakWordSpan or ContinueWordSpan)
        //    {
        //        // if id is attached, search a loop with this id
        //        string spoiler = Spoiler()?.FullText;

        //        StackIndex search = this.stack;
        //        while (search != null && !(search.context is ILoopContext atta && atta.Id == spoiler))
        //            search = search.parent;

        //        // if id not found, relate this as id-less break / continue
        //        if (search == null)
        //        {
        //            search = stack;
        //            while (search != null && search.context is not ILoopContext)
        //                search = search.parent;
        //        }

        //        if (search?.context is ILoopContext loopc)
        //        {
        //            if (loopc.Id != null && loopc.Id == spoiler)
        //                ReadSpan();

        //            if (neutral)
        //                continue;

        //            if (cmd is BreakWordSpan)
        //                loopc.Break = true;
        //            else if (cmd is ContinueWordSpan)
        //                loopc.Continue = true;
        //            break;
        //        }
        //        else
        //            Throw("No enclosing loop out of which to break or continue.");
        //    }
        //    else if (cmd is TryWordSpan trys)
        //    {
        //        if (neutral)
        //            continue;

        //        try
        //        {
        //            RunContext(trys, true);
        //        }
        //        catch (RuntimeException ex)
        //        {
        //            if (trys.Catch != null)
        //            {
        //                // init exception var
        //                if (trys.Catch.VarName != null)
        //                    SetVar(trys.Catch.VarName, ex.ex, specificVS: trys.Catch);

        //                // check "when"
        //                bool toCatch = true;
        //                if (trys.Catch.When != null)
        //                    toCatch = Run<bool>(trys.Catch.When.Condition, trys.Catch);

        //                if (toCatch)
        //                {
        //                    RunContext(trys.Catch, true);
        //                }
        //            }
        //        }
        //        finally
        //        {
        //            if (trys.Finally != null)
        //            {
        //                RunContext(trys.Finally, true);
        //            }
        //        }
        //    }
        //    else if (cmd is ThrowWordSpan)
        //    {
        //        ReadThrowStmt(readThrowKeyword: false, neutral: neutral);
        //    }
        //    else if (cmd is SectionWordSpan section)
        //    {
        //        if (neutral)
        //            continue;

        //        try
        //        {
        //            RunContext(section, true);
        //        }
        //        catch (RuntimeException ex)
        //        {
        //            if (FindParentContext<TryWordSpan>(cmd) is TryWordSpan tryb && tryb.Catch != null)
        //            {
        //                if (tryb.Catch.VarName != null)
        //                    SetVar(tryb.Catch.VarName, ex.ex, specificVS: tryb.Catch);
        //                if (tryb.Catch.When == null || Run<bool>(tryb.Catch.When.Condition, tryb.Catch))
        //                {
        //                    RunContext(tryb.Catch, true);
        //                }
        //            }
        //        }
        //    }
        //    else if (cmd is PrintWordSpan)
        //    {
        //        object str = ReadValue(allowUnknownVars: neutral);

        //        if (neutral)
        //            continue;

        //        Print(str ?? "NULL");
        //    }
        //    else if (cmd is SetWordSpan or ConstWordSpan)
        //    {
        //        string vname = ReadWord().Text;

        //        // if this name is catched, throw an error
        //        // the bug in this code will be fixed when inner sources will be of Span instead of TextSpan
        //        /*bool setOnOuterVS = CurrentVarSystem is IContext someContext && VarExists(vname, someContext.OuterVarSystem);
        //        if (setOnOuterVS || (GetPointer(vname) is Variable exists && exists.SettingSpan != cmd))
        //        {
        //            Throw($"'{vname}': This name is already in use. Pick another name for the variable.");
        //        }*/

        //        Read<SetSymbolSpan>();
        //        object value = ReadValue(allowUnknownVars: neutral);

        //        if (neutral)
        //            continue;

        //        SetVar(vname, value, cmd, null, cmd is ConstWordSpan);
        //    }
        //    else if (cmd is ReturnWordSpan)
        //    {
        //        // find the func we are in
        //        StackIndex ctx = this.stack;
        //        ILoopContext breakFrom = null;
        //        while (ctx != null && ctx.context is not FuncDefSpan)
        //        {
        //            if (ctx.context is ILoopContext lc)
        //                breakFrom = lc;
        //            ctx = ctx.parent;
        //        }

        //        // return
        //        if (ctx?.context is FuncDefSpan func)
        //        {
        //            if (ctx.context is not ConstructorDefSpan)
        //            {
        //                func.Returns = ReadValue(allowUnknownVars: neutral);

        //                if (neutral)
        //                {
        //                    func.Returns = null;
        //                    continue;
        //                }

        //                func.Return = true;
        //            }

        //            if (breakFrom != null)
        //                breakFrom.Break = true;
        //            break;
        //        }
        //        else
        //            Throw("No enclosing function of which to return.");
        //    }
        //    else if (cmd is IDefination or NamespaceWordSpan or UsingWordSpan or ExternClassDefSpan)
        //    {
        //        if (context != null)
        //            Throw($"{cmd.Text} word is not expected in this context.");
        //        continue;
        //    }
        //    else
        //    {
        //        // set an existing variable or call a func
        //        object obj = ReadNamedValueOrPointer(out bool isArrPntr, out int arrPntrInd, cmd, allowUnknownVars: neutral, allowFuncsToNotReturn: true);

        //        Variable pntr = null;
        //        if (obj is Variable || (obj is Instance && isArrPntr) || (neutral && Spoiler() is SetSymbolSpan))
        //        {
        //            pntr = obj as Variable;
        //            // take care of operations (++, --, +=, -=, etc.)
        //            if (Spoiler() is OperatorSpan op)
        //            {
        //                ReadSpan();

        //                object opInput = op.TwoSides ? ReadValue(allowUnknownVars: neutral) : null;

        //                if (!neutral)
        //                {
        //                    if (isArrPntr)
        //                    {
        //                        var arr = (pntr?.Value ?? obj) as Instance;
        //                        arr.ArrayValues[arrPntrInd] = op.Result(arr.ArrayValues[arrPntrInd], opInput);
        //                    }
        //                    else
        //                    {
        //                        pntr.Value = op.Result(pntr.Value, opInput);
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                // this is a variable set (not init) statement
        //                Read<SetSymbolSpan>();
        //                var value = ReadValue(allowUnknownVars: neutral);

        //                if (neutral)
        //                    continue;

        //                if (isArrPntr)
        //                    ((Instance)(pntr?.Value ?? obj)).ArrayValues[arrPntrInd] = value;
        //                else
        //                    pntr.Value = value;
        //            }
        //        }
        //        // else, it was a function and it was just been called, so do nothing
        //    }
        //    if (cmd is ILoopContext loop)
        //    {
        //        loop.Break = false;
        //        loop.Continue = false;
        //    }
        //}

        //stack = stack?.parent;
        //vsStack = vsStack.parent ?? vsStack;

        //if (clearVars && context != null)
        //{
        //    context.Vars.Clear();
        //    context.Vars.AddRange(contextVarsBefore);
        //}
    }

    internal void Run(Span[] src)
    {
        //try
        //{
        IContext context = null;
        Run(src, context);
        //}
        //catch (ScriptThrowingException)
        //{
        //    Throw(ExpStringToString(throwing.Vars[0].Value as Instance));
        //}
    }

    internal void Run(Span[] src, IContext context, bool clearVars = true)
    {
        contextLoc = cursor;
        Span[] codeSpans_backup = CodeSpans;
        Span ls = lastSpan;
        var stack = this.stack;
        var vstack = this.vsStack;
        int cur = this.cursor, spcur = codeCursor;
        cursor = 0;
        codeCursor = 0;
        CodeSpans = src;

        RuntimeException toThrow = null;
        try
        {
            Run(context, clearVars: clearVars);
        }
        catch (RuntimeException ex) when (ex.ByExpThrowStmt)
        {
            toThrow = ex;
        }
        finally
        {

            //Throw(ExpStringToString(throwing.Vars[0].Value as Instance));

            this.stack = stack;
            this.vsStack = vstack;
            CodeSpans = codeSpans_backup;
            lastSpan = ls;
            cursor = cur;
            codeCursor = spcur;
            contextLoc = 0;
            if (toThrow != null)
                throw toThrow;
        }
    }

    private T Run<T>(Span[] src, IContext context, bool allowUnknownVars = false, bool oconst = false)
    {
        contextLoc = cursor;
        Span[] codeSpans_backup = CodeSpans;
        Span ls = lastSpan;
        var stack = this.stack;
        var vstack = this.vsStack;
        int cur = this.cursor, spcur = codeCursor;
        cursor = 0;
        codeCursor = 0;
        CodeSpans = src;
        this.stack = context == null ? this.stack : new StackIndex(context, this.stack);
        T v = ReadValue<T>(allowUnknownVars: allowUnknownVars, oconst: oconst);
        this.stack = stack;
        this.vsStack = vstack;
        CodeSpans = codeSpans_backup;
        lastSpan = ls;
        cursor = cur;
        codeCursor = spcur;
        contextLoc = 0;
        return v;
    }

    internal T Run<T>(Span[] src, bool allowUnknownVars = false, bool oconst = false) => Run<T>(src, null, allowUnknownVars: allowUnknownVars, oconst: oconst);

    public void Run(ScriptDocument doc)
    {
        ArgumentNullException.ThrowIfNull(doc);

        if (!doc.IsPrepared)
            doc.TryPrepare(this, out var errors);

        Run(doc.CodeSpans);
    }

    private ScriptDocument ShellDoc { get; } = ScriptDocument.FromString("", "shell.exp");
    public void Run(string src)
    {
        ShellDoc.Script = src;
        ShellDoc.SettingsErrors.Clear();
        ShellDoc.Usings.Clear();
        if (MainDoc?.Usings != null)
            ShellDoc.Usings.AddRange(MainDoc.Usings);
        ShellDoc.Namespace ??= MainDoc?.Namespace;
        source = src;
        CodeSpans = null;
        cursor = 0;
        spansCursor = 0;
        codeCursor = 0;
        SourceSpans = Spanner.GetTextSpans(src);
        SourceSpans.ForEach(span => span.Doc = ShellDoc);

        List<Span> srcCode = [];
        Span next;
        while (true)
        {
            next = ReadSpan(@throw: true);
            if (next == null)
                break;
            srcCode.Add(next);
        }

        var ops = ReadOperations(srcCode.ToArray(), this, @throw: true);
        AfterAllOperationsCreated?.Invoke(this, null);
        Errors.AddRange(ShellDoc.SettingsErrors);
        if (Errors.Count > 0)
        {
            var errors = this.Errors.ToArray(); // ToArray() so the Clear() below won't affect it
            this.Errors.Clear();
            throw new BuildFailureException(errors);
        }

        RunOpsRunning = true;
        RunOps(ops);
        RunOpsRunning = false;
    }

    private void SwitchSpans(Span[] src, Action action)
    {
        contextLoc = cursor;
        Span[] codeSpans_backup = CodeSpans;
        Span ls = lastSpan;
        int cur = this.cursor, spcur = codeCursor;
        cursor = 0;
        codeCursor = 0;
        CodeSpans = src;

        action();

        CodeSpans = codeSpans_backup;
        lastSpan = ls;
        cursor = cur;
        codeCursor = spcur;
        contextLoc = 0;
    }

    private void ReadThrowStmt(bool readThrowKeyword = true, bool neutral = false, bool thr = true)
    {
        if (readThrowKeyword)
            Read<ThrowWordSpan>();

        Instance ex = ReadValue<Instance>(allowUnknownVars: neutral);

        if (neutral)
            return;

        if (ex == null || !ex.def.Name.Equals("Exception"))
            Error("Only instances of type Exception can be thrown.");

        if (thr)
        {
            ThrowRuntime(ex, byExpThrowStmt: true);
        }
    }

    private void CollectDefs(TextSpan[] code = null)
    {
        bool saveSpans = code == null;
        code ??= SourceSpans;

        int cursor_bu = cursor, spcur_bu = spansCursor;
        TextSpan[] sourceSpans_bu = SourceSpans;
        SourceSpans = code;
        Span lastSpan_bu = lastSpan;
        bool collected = false, codeStart = false;
        string docNs = null;

        List<Span> codeSpans = saveSpans ? [] : null;

        // find all def spans and load them to the defs list
        spansCursor = 0;
        while (spansCursor < code.Length)
        {
            Span span = ReadSpan();
            codeSpans?.Add(span);

            if (span is IContext ctx)
                ctx.Parent = this;
            if (span is IDefination def)
            {
                def.Namespace = span.Document?.Namespace;
                if (def is FuncDefSpan { Name: null } namelessFunc)
                {
                    AfterAllOperationsCreated += ErrForNamelessFunc;
                    void ErrForNamelessFunc(object s, EventArgs e)
                    {
                        if (!namelessFunc.SpanItselfIsReadedAsValue)
                            Error($"Missing {FuncDefSpan.ItemName} name.", namelessFunc);
                        AfterAllOperationsCreated -= ErrForNamelessFunc;
                    }
                }
                else
                {
                    ValidateDefNameLegallity(def.Namespace, def.Name, @params: def is FuncDefSpan fn ? fn.Args.Length : -1);
                    if (def is EnumDefSpan enm)
                    {
                        var vars = new ClassStaticVar[enm.Values.Length];
                        for (int i = 0; i < vars.Length; i++)
                            vars[i] = new ClassStaticVar(enm.Values[i].Name, enm.Values[i].Value.ToExp(), null, enm, false, true) { TagsCode = enm.Values[i].TagsCode };
                        ClassDefSpan enumcls = new(enm.Name, [], []) { Namespace = currNs, TagsCode = enm.TagsCode, Document = enm.Document, DocumentLocation = enm.DocumentLocation };
                        vars.ForEach(v => v.Def = enumcls);
                        enumcls.Vars.AddRange(vars);
                        def = enumcls;
                    }

                    definations.Add(def);
                    collected = true;
                }
                codeStart = true;
            }
            else if (span is NamespaceWordSpan nss)
            {
                //if (collected || docNs != null)
                //    Error("Namespace naming can appear only once in a doc and before first defination.");
                //currNs = nss.Namespace;
                //docNs = currNs;
                //codeStart = true;

                Error("Namespace naming can appear only once in a doc and before first defination.", span);
            }
            else if (span is UsingWordSpan use)
            {
                //if (codeStart)
                //    Error("Using directives must appear before any other code.");
                //if (currUsings.Contains(use.Namespace))
                //    Error($"The namespace '{use.Namespace}' is already being used in this document.");
                //currUsings.Add(use.Namespace);

                Error("Using directives must appear before any other code.");
            }
            else if (span == null)
                break;
        }

        cursor = cursor_bu;
        spansCursor = spcur_bu;
        lastSpan = lastSpan_bu;
        SourceSpans = sourceSpans_bu;
        if (saveSpans)
            this.CodeSpans = codeSpans.ToArray();
    }

    private void CollectDefs(ScriptDocument[] docs)
    {
        foreach (var doc in docs)
        {
            CollectDefs(doc.TextSpans);
            //doc.Namespace = currNs;
            //doc.Usings.AddRange(currUsings);
            currUsings = [];
            currNs = null;
        }
    }


    internal Span[] GetCodeSpans(TextSpan[] code)
    {
        source = "";
        code.ForEach(ts => source += ts.text);
        CodeSpans = null;
        cursor = 0;
        spansCursor = 0;
        codeCursor = 0;
        SourceSpans = code;

        List<Span> srcCode = [];
        Span next;
        while (true)
        {
            next = ReadSpan(@throw: true);
            if (next == null)
                break;
            srcCode.Add(next);
        }

        return srcCode.ToArray();
    }

    private void ResolveAttributes()
    {
        // all attribute params should be now resolved
        definations.OfType<AttributeDefSpan>().ForEach(attr =>
        {
            attr.Params.ForEach(p =>
        {
            if (!p.ResolveTypeName(definations))
                Error($"Unknown type '{p.ExpTypeName}'.");
        }); LoadAttributes(attr);
        });

        // all attributes tag should now be resolved
        definations.OfType<ICanSetAttr>().ForEach(d => { if (d is not AttributeDefSpan) LoadAttributes(d); });
        definations.OfType<ClassDefSpan>().ForEach(c => { c.Props.ForEach(LoadAttributes); c.Vars.OfType<ICanSetAttr>().ForEach(LoadAttributes); c.Funcs.ForEach(LoadAttributes); });
    }

    private IValue FuncCall(Instance inst, string fname, IContext context, out bool isFuncCall, FuncDefSpan[] usedFuncs, IValue[] parameters = null)
    {
        parameters = parameters ?? ReadParamList();

        // find the function
        var func = usedFuncs.FirstOrDefault(f => fname.Equals(f.Name) && parameters.Length == f.Args.Length) ?? GetVar(fname, context ?? (IVarSystem)inst ?? this) as FuncDefSpan;
        return FuncCall(inst, func, context, out isFuncCall, parameters);
    }

    internal IValue FuncCall(Instance? inst, FuncDefSpan func, IContext? useless_delete_it, out bool isFuncCall, IEnumerable<IValue?> parameters)
    {
        ArgumentNullException.ThrowIfNull(func);

        // if its a recursion, backup current var values
        bool wasRunning = func.IsRunning;
        func.IsRunning = true;
        IVarSystem funcAsVs = func;
        IEnumerable<IValue> backup = wasRunning ? funcAsVs.BackupValues() : null;

        // insert arguments
        int i = 0;
        foreach (var param in parameters)
            func.ParamVariables[i++].Value = param;

        // on non-static funcs, set parent VS to the instance we're calling on
        var prevParent = func.Parent;
        if (func.DefinedAt != null)
            func.Parent = (IVarSystem)inst ?? func.DefinedAt;

        try
        {
            // run func (or handle built-in function)
            if (!BuiltinFuncs(func, inst))
                RunOps(func.Operations);
        }
        finally
        {
            // if its recursion, restore previous var values
            toReturn = false;
            func.Parent = prevParent;
            if (backup != null)
                funcAsVs.RestoreValues(backup);
            func.IsRunning = wasRunning;
        }

        isFuncCall = true;

        // return
        var returns = func.Return ? func.Returns?.Pass() : Void.Return;
        func.Return = false;
        func.Returns = null;
        return returns;
    }

    internal static IValue InvokeExtern(Type type, bool sttc, object inst, string method, object[] args)
    {
        // naming rule violation: funcs starts with lowercase letters
        method = method[0].ToString().ToUpper() + method[1..];

        MethodInfo methodi = null;
        string typesStr = null;
        var flags = (sttc ? BindingFlags.Static : BindingFlags.Instance) | BindingFlags.Public;

        try
        {
            methodi = type.GetMethod(method, flags, GetArgTypesForExternInvocation(args, out typesStr)) ?? throw new Exception("Method not found");
        }
        catch (Exception ex)
        {
            Activated.ThrowRuntime($"Error occurred when searching external method '{type}.{method}{typesStr}' (Error message: {ex.Message}).", RuntimeException.NOT_FOUND);
        }

        return InvokeExtern(inst, methodi, args);
    }

    internal static IValue NewExtern(Type type, object[] args)
    {
        ConstructorInfo ctor = null;
        string typesStr = null;
        try
        {
            ctor = type.GetConstructor(GetArgTypesForExternInvocation(args, out typesStr)) ?? throw new Exception("Constructor not found");
        }
        catch (Exception ex)
        {
            Activated.ThrowRuntime($"Could not find external constructor{typesStr} for type '{type}' (Error message: {ex.Message}).", RuntimeException.EXTERN_OPERATION_FAILED);
        }

        return InvokeExtern(null, ctor, args);
    }

    private static Type[] GetArgTypesForExternInvocation(object[] args, out string typesStr)
    {
        typesStr = "(";
        Type[] types = new Type[args.Length];
        for (int i = 0; i < types.Length; i++)
        {
            static void Convert(object[] args, Type[] types, int i)
            {
                types[i] = args[i]?.GetType() ?? typeof(object);
                if (args[i] is IValue expval)
                {
                    if (expval.IsInst)
                    {
                        var expinst = expval.Inst;
                        if (expinst.def == ClassDefSpan.ExpStringDef)
                        {
                            args[i] = ExpStringToString(expinst);
                            types[i] = typeof(string);
                        }
                        else if (expinst.def == ClassDefSpan.ExpArrayDef)
                        {
                            Type[] _ = new Type[expinst.ArrayValues.Length];
                            for (int j = 0; j < expinst.ArrayValues.Length; j++)
                                Convert(expinst.ArrayValues, _, j);
                            args[i] = CSBasicTypes.MinArray(expinst);
                            types[i] = args[i].GetType();
                        }
                        else if (expinst is ExternTypeInstance extrn)
                        {
                            args[i] = extrn.ExternInstance;
                            types[i] = args[i].GetType();
                        }
                    }
                    else if (expval.IsBool)
                    {
                        args[i] = expval.Bool;
                        types[i] = typeof(bool);
                    }
                    else if (expval.IsChar)
                    {
                        args[i] = expval.Char;
                        types[i] = typeof(char);
                    }
                    else if (expval.IsNumber)
                    {
                        args[i] = expval.Number;
                        types[i] = typeof(double);
                    }
                }
            }

            Convert(args, types, i);
            typesStr += types[i].ToString() + (i < args.Length - 1 ? ", " : "");
        }

        typesStr += ")";
        return types;
    }

    private static IValue InvokeExtern(object inst, MethodBase method, object[] args)
    {
        ArgumentNullException.ThrowIfNull(method);

        try
        {
            // invoke
            object result = method is ConstructorInfo ctor ? ctor.Invoke(args) : method.Invoke(inst, args);

            return CsValToExpVal(result);
        }
        catch (Exception ex)
        {
            ex = ex.InnerException ?? ex;
            Activated.ThrowRuntime(ex.GetType() + $" was thrown while invoking {method.MemberType}.{method.Name}: " + ex.Message, RuntimeException.EXTERN_OPERATION_FAILED);
            throw null;
        }
    }

    internal static IValue CsValToExpVal(object val)
    {
        IValue result;
        if (val is null)
            return null;
        else if (val is bool b)
            result = b.ToExp();
        else if (val is char c)
            result = c.ToExp();
        else if (val is Instance expinst)
            result = expinst;
        else if (val is string csstr)
            result = StringToExpString(csstr);
        else if (val is Array csarr)
        {
            var exparr = new IValue[csarr.Length];
            for (int i = 0; i < csarr.Length; i++)
                exparr[i] = CsValToExpVal(csarr.GetValue(i));
            result = new Instance(ClassDefSpan.ExpArrayDef, exparr);
        }
        else
        {
            try
            {
                result = ((double)val).ToExp();
            }
            catch (InvalidCastException)
            {
                result = val.AsExtern();
            }
        }

        return result;
    }

    internal static object ExpValToCsVal(IValue val)
    {
        object result;
        if (val is null)
            return null;

        else if (val.IsInst)
        {
            if (val.Inst.IsArray)
                result = val.Inst.ArrayValues.Map(v => ExpValToCsVal(v));
            else if (val.Inst.IsString())
                result = ExpStringToString(val.Inst);
            else if (val.Inst is ExternTypeInstance ext)
                result = ext.ExternInstance;
            else
                result = val.Object;
        }
        else
            result = val.Object;

        return result;
    }

    private object InvokeExtern(ExternFunc func, object[] args)
    {
        try
        {
            throw new NotImplementedException();
            //GetArgTypesForExternInvocation(args, out string typesStr);
            //return CsValToExpVal(func.Func(args));
        }
        catch (Exception ex)
        {
            ex = ex.InnerException ?? ex;
            ThrowRuntime(ex.Message, ex.GetType().Name);
            throw;
        }
    }

    private T FindParentContext<T>(Span span) where T : IContext
    {
        while (span != null)
        {
            if (span is T found)
                return found;
            span = span.Container;
        }

        return default;
    }

    internal static T FindParentVarSystem<T>(IVarSystem vs) where T : IVarSystem
    {
        ArgumentNullException.ThrowIfNull(vs);

        while (vs != null)
        {
            if (vs is T found)
                return found;
            vs = vs.Parent;
        }

        return default;
    }

    private List<Task> Tasks { get; } = [];
    internal async void RunAsync(Task action)
    {
        Tasks.Add(action);
        await action.WaitAsync((CancellationToken)default);
        Tasks.Remove(action);
    }

    public bool RunOpsRunning { get; internal set; } = false;
    public void RunOps()
    {
        "--- RunOps START---\n".Println();
        RunOpsRunning = true;
        RunOps(operations);
        Task.WaitAll(Tasks);
        RunOpsRunning = false;
        "\n--- RunOps END-----".Println();
    }

    internal bool toReturn;
    internal void RunOps(IOperation[] ops)
    {
        if (ops == null)
            _ = 0;

        ArgumentNullException.ThrowIfNull(ops);

        Activated = this;
        foreach (var op in ops)
        {
            RunOp(op);
            if (toReturn)
                break;
        }
    }

    public IOperation lastOp = null;
    internal void RunOp(IOperation op)
    {
        lastOp = op;
        try
        {
            op.Make();
        }
        catch (StackOverflowException)
        {
            ThrowRuntime("Operation caused a stack overflow.", RuntimeException.STACK_OVERFLOW);
        }
    }
}

// when a func returns instance of this, it means it didn't return a value
public sealed class Void : IValue
{
    public string TypeName => "void";
    public static Void Return { get; } = new();
    public object Object => Return;
    private Void() { }
    public override string ToString() => TypeName;
}