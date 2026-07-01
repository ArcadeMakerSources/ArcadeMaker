using Exp.Operations;
using Exp.Spans;
using System;
using System.Collections.Generic;
using System.Text;

namespace Exp;

public partial class Interpreter
{
    private Dictionary<IOperation, Span> OperationsSpanPair = [];
    private List<Span> readOps_codeRecord = null;
    
    internal IOperation[] ReadOperations(Span[] spans, IVarSystem vs, out Span[] src, bool @throw = false, bool breakAfter1 = false)
    {
        bool deleteRecord = readOps_codeRecord == null;
        readOps_codeRecord ??= [];

        IOperation prevOperation = null;
        List<IOperation> operations = [];

        void action()
        {
            while (ReadWord() is WordSpan word)
            {
                lastSpan = word;
                IOperation operation;
                if (word is IfConditionSpan @if)
                {
                    // it's an if condition
                    var stmt = new IfStatement(@if, ReadReadingOperation(@if.Condition), ReadOperations(@if.InnerSource, @if));

                    // check else statement
                    if (Spoiler() is ElseConditionSpan)
                    {
                        var els = Read<ElseConditionSpan>();
                        stmt.Else = new ElseStatement(els, ReadOperations(els.InnerSource, els));
                    }

                    operation = stmt;
                }
                else if (word is ElseConditionSpan @else)
                {
                    Error("Else statement must follow an if statement.");
                    throw null;
                }
                else if (word is ILoopContext loop)
                {
                    // it's a loop
                    Variable counter = null;
                    if (loop.Counter != null)
                        loop.Vars.Add(counter = new(loop.Counter, 0d.ToExp(), loop as Span, cons: true));

                    if (loop is WhileConditionSpan @while)
                        operation = new WhileStatement(@while, ReadReadingOperation(@while.Condition), ReadOperations(@while.InnerSource, @while), counter);
                    else if (loop is ForLoopSpan @for)
                    {
                        IOperation[] init = ReadOperations(@for.InitExe, @for), step = ReadOperations(@for.StepExe, @for);
                        if (step.Length == 1 && step[0] is VariablesDeclaration)
                            Error($"Variable declaration as the iteration statement of a {ForLoopSpan.ItemName} is forbidden.");
                        if (init.Length >= 2)
                            Error("Init code of a for loop can only contain 1 operation.");
                        if (step.Length >= 2)
                            Error("Step code of a for loop can only contain 1 operation.");

                        operation = new ForStatement(@for, init.FirstOrNull(), ReadReadingOperation(@for.Condition), step.FirstOrNull(), ReadOperations(@for.InnerSource, @for), counter);
                    }
                    else if (loop is ForEachLoopSpan fe)
                    {
                        var var = new Variable(fe.VarName, null, fe, cons: fe.ConstVar);
                        fe.Vars.Add(var);
                        operation = new ForeachStatement(fe, var, ReadReadingOperation(fe.ArrReadText), ReadOperations(fe.InnerSource, fe), counter);
                    }
                    else if (loop is RangeLoopSpan range)
                    {
                        var var = new Variable(range.VarName, null, range, cons: range.ConstVar);
                        range.Vars.Add(var);
                        var fromOp = range.FromReadText == null ? null : ReadReadingOperation(range.FromReadText);
                        operation = new RangeLoopStatement(range, var, fromOp, ReadReadingOperation(range.ToReadText), ReadOperations(range.InnerSource, range), counter);
                    }
                    else
                        throw new Exception("Unexpected loop type.");
                }
                else if (word is BreakWordSpan or ContinueWordSpan)
                {
                    // check if the next word is a loop's ID. find the right loop anyway
                    var next = Spoiler();
                    ILoopContext chosenLoop = null;
                    var span = word.Container;

                    var id = next?.FullText;

                    while (span is Span container)
                    {
                        if (container is ILoopContext loop1)
                        {
                            if (loop1.Id == id)
                            {
                                ReadSpan(); // bc the ID reading was with Spoiler()
                                chosenLoop = loop1;
                                break;
                            }
                            chosenLoop ??= loop1;
                        }
                        span = span.Container;
                    }


                    if (chosenLoop == null)
                        Error("No enclosing loop out of which to break or continue (or given loop's ID was not found).");

                    // create the operation
                    if (word is BreakWordSpan bword)
                        operation = new BreakStatement(bword, chosenLoop);
                    else
                        operation = new ContinueStatement(word as ContinueWordSpan, chosenLoop);
                }
                else if (word is ReturnWordSpan ret)
                {
                    // find the func
                    FuncDefSpan func = null;
                    Span span = word.Container;
                    while (true)
                    {
                        if (span is FuncDefSpan fn)
                        {
                            func = fn;
                            break;
                        }
                        if (span == null)
                        {
                            Error("No enclosing function out of which to return.");
                            break;
                        }
                        span = span.Container;
                    }

                    operation = new ReturnStatement(func, ReadReadingOperation(), ret);
                }
                else if (word is TryWordSpan trw)
                {
                    var tryBody = ReadOperations(trw.InnerSource, trw);
                    CatchStatement catc = null;
                    FinallyStatement finaly = null;

                    CatchWordSpan cw = trw.Catch;
                    FinallyWordSpan fw = trw.Finally;

                    if (cw?.VarName != null)
                        cw.Vars.Add(new Variable(cw.VarName, null, null));

                    if (cw != null)
                    {
                        IReadingOperation readWhen = null;
                        if (cw.When?.Condition != null)
                            readWhen = ReadReadingOperation(cw.When.Condition);
                        catc = new CatchStatement(ReadOperations(cw.InnerSource, cw), readWhen, cw);
                    }
                    if (fw != null)
                    {
                        finaly = new FinallyStatement(ReadOperations(fw.InnerSource, fw), fw);
                    }

                    operation = new TryStatement(trw, tryBody, catc, finaly);
                }
                else if (word is ThrowWordSpan)
                {
                    operation = new Throwing(ReadReadingOperation());
                }
                else if (word is SetWordSpan or ConstWordSpan)
                {
                    // it's a variable declaration
                    Dictionary<string, IReadingOperation> decs = [];
                    while (true)
                    {
                        string name = ReadWord().Text;
                        ValidateLocalNameLegallity(name, vs);
                        var next = Spoiler();
                        IReadingOperation val = null;
                        if (next is SetSymbolSpan)
                        {
                            ReadSpan();
                            val = ReadReadingOperation();
                            next = Spoiler();
                        }

                        if (!decs.TryAdd(name, val))
                            Error($"Duplicate variable declaration '{name}'.");

                        if (next is CommaSpan)
                        {
                            ReadSpan();
                            continue;
                        }
                        break;
                    }

                    Dictionary<Variable, IReadingOperation> vars = [];
                    foreach (var var in decs)
                    {
                        var v = new Variable(var.Key, null, word, cons: word is ConstWordSpan);
                        vs.Vars.Add(v);
                        vars.Add(v, var.Value);
                    }

                    operation = new VariablesDeclaration(vars);
                }
                else if (word is PrintWordSpan)
                {
                    var val = ReadReadingOperation();
                    operation = Operation.Custom(() => Print(val?.Read()?.ToString() ?? "NULL"));
                }
                else if (word is FuncDefSpan func && vs is IContext)
                {
                    // if a function is declared inside a context, create it as function pointer
                    func.Operations = ReadOperations(func.InnerSource, func);
                    ValidateLocalNameLegallity(func.Name, vs, func.Args.Length);
                    vs.Vars.Add(new Variable(func.Name, new FuncPntr(func, null), func, false, true));
                    // it is readonly: func.Name = "localfunc." + func.Name;
                    continue; // bc it's not an operation
                }
                //else if (word.GetType().Name != "WordSpan") { operation = Operation.Custom(() => { }); }
                else if (word is IDefination or NamespaceWordSpan or UsingWordSpan or ExternClassDefSpan)
                    continue;
                else
                {
                    // first word is a named item, so it'll be an assignment or function call
                    var pointingOrFuncCall = ReadPointingOrFuncCall(out bool isCall, isOp: true, word);

                    if (pointingOrFuncCall == null) // error was occoured and is already logged
                        operation = Operation.Error;
                    else
                    {
                        if (!isCall)
                        {
                            var spoiler = Spoiler();
                            if (spoiler is OperatorSpan { Action: true } opertor) // then it's an assignment
                            {
                                // check if the variable is a known constant, so we can throw an error already at build time
                                if (pointingOrFuncCall.Next == null && pointingOrFuncCall.Known is Variable { Const: true } var)
                                    Error($"This variable ('{pointingOrFuncCall.Name}') is marked as constant and cannot be reset.", spoiler);

                                // create the operation
                                ReadSpan();
                                var right = opertor.TwoSides ? ReadReadingOperation() : null;
                                operation = new Assignment(pointingOrFuncCall, opertor, right);
                            }
                            else
                            {
                                Error($"An action operator was expected, but {spoiler} was read.");
                                operation = Operation.Error;
                            }
                        }
                        else
                        {
                            // it's a function call
                            if (pointingOrFuncCall.Next == null && pointingOrFuncCall.KnownFunc?.ReadOnly == true)
                                Error($"The value returned by {pointingOrFuncCall.KnownFunc?.GetExpTypeName(false)} must be used because it's marked as read-only.");

                            operation = Operation.Custom(() => pointingOrFuncCall.Read());
                        }
                    }
                }

                OperationsSpanPair.Add(operation, word);

                operations.Add(operation);
                prevOperation = operation;

                if (breakAfter1)
                    break;
            }
        }


        if (spans != null)
            SwitchSpans(spans, action);
        else
            action();

        src = [..readOps_codeRecord];
        if (deleteRecord)
            readOps_codeRecord = null;

        return operations.ToArray();
    }

    internal IOperation[] ReadOperations(Span[] spans, IVarSystem vs, bool @throw = false) => ReadOperations(spans, vs, out var _, @throw);

    private IOperation ReadOperation(Span[] spans, IVarSystem vs, out Span[] src)
    {
        var ops = ReadOperations(spans, vs, out src, breakAfter1: true);

        if (ops == null || ops.Length == 0 || ops.Length >= 2)
        {
            Error($"1 Operation was expected, but {ops?.Length ?? 0} read.");
            return new Throwing(ConstValueReadingOperation.For("Execution reached a syntax error.".ToExpString()));
        }
        return ops[0];
    }
}
