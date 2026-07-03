using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using Exp.Operations;
using Exp.Spans;
//using Exp.Compiler;

namespace Exp
{
    internal interface ICanSetAttr
    {
        List<Span[]> TagsCode { get; set; }
        Instance[] AttrInfo { get; set; }
    }

    internal interface IClassMember : ICanSetAttr
    {
        ClassDefSpan Def { get; }
        string Name { get; }
    }

    interface INamedValue
    {
        string Name { get; }
        bool Private { get; }

        IValue Value { get; }
        bool IsVar { get; }
    }

    public class Property : IClassMember, IExpItem
    {
        public static string ItemName { get; } = "class property";
        public string Name { get; }
        internal bool Const { get; }
        internal bool Private { get; }
        internal bool BaseArray { get; }
        internal Span[] InitValueReadText;
        public Instance[] AttrInfo { get; set; }
        public List<Span[]> TagsCode { get; set; }
        public ClassDefSpan Def { get; set; }

        public Property(ClassDefSpan def, bool cons, string name, bool prvt, bool baseArr, Span[] initVal = null, List<Span[]> tagsCode = null)
        {
            this.Const = cons;
            this.Name = name;
            this.Private = prvt;
            this.BaseArray = baseArr;
            this.InitValueReadText = initVal;
            this.TagsCode = tagsCode ?? [];
            this.Def = def;
        }
    }

    /// <summary>
    /// Represents a fast-invoked extern function.
    /// </summary>
    public class ExternFunc
    {
        public ExternFunc(Func<Instance, IValue?[], IValue?> func, int[] paramsCountOptions, string? name = null, string? @namespace = null)
        {
            ArgumentNullException.ThrowIfNull(func);

            Name = name ?? func.Method.Name.StartWithLowerCase();
            ParamsCountOptions = paramsCountOptions.Length == 0 ? [0] : paramsCountOptions;
            Namespace = @namespace;
            Func = func;
        }

        public ExternFunc(Func<Instance, IValue?[], IValue?> func, int paramsCount, string? name = null, string? @namespace = null) : this(func, [paramsCount], name, @namespace)
        {
            
        }

        public string Name { get; }
        public int[] ParamsCountOptions { get; }
        public string? Namespace { get; }
        public Func<Instance, IValue?[], IValue?> Func { get; }
    }


    /// <summary>
    /// An Exp interpreter. Can run Exp scripts.
    /// </summary>
    public partial class Interpreter : IVarSystem
    {
        private bool neutral = false;

        private string source;
        private TextSpan[] _sourceSpans_;
        private TextSpan[] SourceSpans
        {
            get => _sourceSpans_;
            set
            {
                _sourceSpans_ = value;
                source = "";
                foreach (var ss in value)
                    source += ss.text;
            }
        }
        private int codeCursor = 0;
        private Span[] CodeSpans { get; set; }

        private readonly List<ScriptDocument> docs = [];
        private HashSet<string> currUsings = [];
        private string specificNS = null;
        private int cursor = 0, contextLoc = 0;
        private int spansCursor = 0, spansContextLoc = 0;

        /// <summary>
        /// Contains variables that were defined out of any scope.
        /// </summary>
        public List<Variable> Vars { get; } = [];
        public IVarSystem Parent { get; set; }

        private Dictionary<FuncDefSpan, ExternFunc> ExternInvokers { get; } = [];
        public void AddExternFunc(ExternFunc fn, ClassDefSpan? def = null, bool statc = false)
        {
            ArgumentNullException.ThrowIfNull(fn);

            foreach (int paramsCount in fn.ParamsCountOptions)
            {
                List<ArgumentSpan> prms = [];
                for (int i = 1; i <= paramsCount; i++)
                    prms.Add(new("p" + i));
                FuncDefSpan invoker = new(fn.Name, prms.ToArray(), [], def) { Namespace = fn.Namespace, Static = statc };
                invoker.Operations = [new ReturnStatement(invoker, new ExternFuncInvocationOperation(invoker, fn), null)];
                if (def == null)
                    definations.Add(invoker);
                else
                    def.Funcs = (def.Funcs ?? []).Append(invoker).ToArray();
                ExternInvokers.Add(invoker, fn);
            }
        }

        internal IEnumerable<FuncDefSpan> UsedFuncs(Span from) => UsedDefinations(from).OfType<FuncDefSpan>();
        internal IEnumerable<ClassDefSpan> UsedClasses(Span from) => UsedDefinations(from).OfType<ClassDefSpan>();
        internal IEnumerable<IDefination> UsedDefinations(Span from)
        {
            return definations.Where(d => d.Namespace == null || from.Document.Namespace == d.Namespace || from.Document.Usings?.Contains(d.Namespace) == true);
        }

        internal List<ExternClassDefSpan> externs = [];

        public readonly List<IDefination> definations = [];

        /// <summary>
        /// The currently activated <see cref="Interpreter"/> instance.
        /// </summary>
        public static Interpreter Activated { get; private set; }

        internal event EventHandler CollectDefsCompleted;
        internal bool CollectedDefs { get; private set; } = false;

        /// <summary>
        /// The document to run.
        /// </summary>
        public ScriptDocument MainDoc { get; private set; }
        internal event EventHandler AfterAllOperationsCreated;

        /// <summary>
        /// Creates and iniitializes a new interpreter.
        /// </summary>
        /// <param name="source">Script to run.</param>
        /// <param name="imports">Libraries codes.</param>
        public Interpreter()
        {
            this._currentVarSystem_ = this;
        }

        public void Build(ScriptDocument source, IEnumerable<IDefination>? defsToImport = null, params ScriptDocument[] imports)
        {
            "build started...".Println();
            Activated = this;
            MainDoc = source;
            this.source = source.Script;
            SourceSpans = source.TextSpans;

            var importsLs = new List<ScriptDocument>(imports);
            importsLs.Insert(0, ScriptDocument.FromString(Extensions.ReadLib("xml"), "xml.txt"));
            importsLs.Insert(0, ScriptDocument.FromString(Extensions.ReadLib("json"), "json.txt"));
            importsLs.Insert(0, ScriptDocument.FromString(Extensions.ReadLib("reflection"), "reflection.txt"));
            importsLs.Insert(0, ScriptDocument.FromString(Extensions.ReadLib("system"), "system.txt"));
            docs.AddRange(importsLs);
            importsLs.ForEach(doc => Errors.AddRange(doc.SettingsErrors));

            definations.Add(AttributeDefSpan.ToString);
            definations.Add(AttributeDefSpan.EqualizerAttr);
            definations.Add(AttributeDefSpan.AllowFor);
            definations.Add(AttributeDefSpan.AllowMultipleAttr);
            definations.Add(AttributeDefSpan.LimitTo1InClsAttr);
            definations.Add(AttributeDefSpan.FuncRequirementsAttr);
            definations.Add(AttributeDefSpan.ReadOnlyAttr);
            definations.AddRange(defsToImport ?? []);

            "collecting defs...".Print();
            CollectDefs(importsLs.ToArray());
            CollectDefs(); // also loads code spans
            source.Usings.AddRange(currUsings);

            " OK".Println();
            CollectedDefs = true;
            CollectDefsCompleted?.Invoke(this, null);

            ResolveAttributes();

            void OperateFunc(FuncDefSpan func)
            {
                func.Operations = ReadOperations(func.InnerSource, func);
            }
            definations.OfType<FuncDefSpan>().ForEach(OperateFunc);
            definations.OfType<ClassDefSpan>().ForEach(cls => cls.Funcs.ForEach(OperateFunc));
            operations = ReadOperations(null, this);
            AfterAllOperationsCreated?.Invoke(this, null);

            ThrowIfErr();

            void ThrowIfErr()
            {
                if (Errors.Count > 0)
                    throw new BuildFailureException(Errors);
            }
        }

        private IOperation[] operations;

        private WordSpan ReadWord(string specific = null)
        {
            Span span = ReadSpan();

            if (specific != null && span?.FullText != specific)
                Error($"Word '{specific}' was expected.");
            else if (span == null)
                return null;
            else if (span is not WordSpan word)
                Error($"A word was expected (span received: {span.GetType().Name}).");
            else
                return word;
            return null;
        }

        private T Read<T>() where T : Span
        {
            Span span = ReadSpan();
            if (span == null)
                Error($"{GetSymbolOrKeyword(typeof(T))} expected.");
            else if (span is not T)
                Error($"{GetSymbolOrKeyword(typeof(T))} was expected, but {GetSymbolOrKeyword(span.GetType())} received.", span);
            return span as T;

            static string GetSymbolOrKeyword(Type type)
            {
                string? str = null;
                // use reflection to get static property Symbol for ISymbol or Keyword for IKeyword
                if (type.GetInterfaces().Contains(typeof(ISymbol)))
                    str = type.GetProperty(nameof(ISymbol.Symbol), BindingFlags.Public | BindingFlags.Static)?.GetValue(null)?.ToString();
                else if (type.GetInterfaces().Contains(typeof(IKeyword)))
                    str = type.GetProperty(nameof(IKeyword.Keyword), BindingFlags.Public)?.GetValue(null)?.ToString();

                return str ?? type.Name;
            }
        }


        private IValue[] ReadParamList(bool arrayBrackets = false, bool openerWasAlreadyRead = false, bool allowUnknownVars = false)
        {
            if (!openerWasAlreadyRead)
            {
                if (arrayBrackets)
                    Read<ArrayOpenerSpan>();
                else
                    Read<OpeningBracketSpan>();
            }

            var spoiler = Spoiler();
            if (arrayBrackets ? spoiler is ArrayCloserSpan : spoiler is ClosingBracketSpan)
            {
                ReadSpan();
                return new IValue[0];
            }
            var args = new List<object>();
            while (true)
            {
                // parse the value
                var val = ReadValue(out var src);
                args.Add(val);

                Span symb = Spoiler();
                if (arrayBrackets ? symb is ArrayCloserSpan : symb is ClosingBracketSpan)
                {
                    ReadSpan();
                    break;
                }
                else if (symb is CommaSpan)
                {
                    ReadSpan();
                }
                else
                    Error($"Unexpected span in parameter list (Span: '{symb}').");
            }
            return new IValue[args.Count];
        }

        private IReadingOperation[] ReadParamListOps(bool arrayBrackets = false, bool openerWasAlreadyRead = false, bool allowUnknownVars = false)
        {
            if (!openerWasAlreadyRead)
            {
                if (arrayBrackets)
                    Read<ArrayOpenerSpan>();
                else
                    Read<OpeningBracketSpan>();
            }

            var spoiler = Spoiler();
            if (arrayBrackets ? spoiler is ArrayCloserSpan : spoiler is ClosingBracketSpan)
            {
                ReadSpan();
                return new IReadingOperation[0];
            }
            var readings = new List<IReadingOperation>();
            while (true)
            {
                // parse the value
                var reading = ReadReadingOperation(out var src);
                readings.Add(reading);

                Span symb = Spoiler();
                if (arrayBrackets ? symb is ArrayCloserSpan : symb is ClosingBracketSpan)
                {
                    ReadSpan();
                    break;
                }
                else if (symb is CommaSpan)
                {
                    ReadSpan();
                }
                else
                {
                    Error($"Unexpected span in parameter list (Span: '{symb}').");
                    break;
                }
            }
            return readings.ToArray();
        }

        private class FutileContext : IContext
        {
            public IVarSystem Parent { get; set; }
            public List<Variable> Vars { get; } = []; 
            public Span[] InnerSource { get; set; }
            public IOperation[] Operations { get; set;}
        }

        private Span[] ReadInnerSource(bool readOpener = true, bool allowSingleCmd = false, bool singleCmd = false)
        {
            allowSingleCmd = allowSingleCmd || singleCmd;
            if (readOpener || singleCmd)
            {
                if (allowSingleCmd && Spoiler() is not SourceOpenerSpan)
                {
                    DefNameSpan.CancelResolveForNewOnes = true;
                    var errors = this.Errors.ToArray();
                    ReadOperation(null, new FutileContext(), out var src);
                    this.Errors.Clear();
                    this.Errors.AddRange(errors);
                    DefNameSpan.CancelResolveForNewOnes = false;
                    return src;
                }

                Read<SourceOpenerSpan>();
            }

            List<Span> code = [];

            // read inner source until }
            while (true)
            {
                if (cursor >= source.Length)
                {
                    Error($"Endless context, {SourceCloserSpan.Symbol} expected.");
                    return null;
                }

                Span ispan = ReadSpan(spaces: false);

                if (ispan is SourceCloserSpan)
                    break;
                code.Add(ispan);
            }

            return code.ToArray();
        }

        private DefNameSpan ReadDefName(WordSpan firstSpan = null, bool spoiler = false)
        {
            if (spoiler && firstSpan != null)
                throw new Exception($"{nameof(spoiler)} cannot be true when {nameof(firstSpan)} is not null.");

            WordSpan name = firstSpan;
            if (name == null)
            {
                if (Spoiler() is WordSpan ws)
                    name = spoiler ? ws : ReadWord();
                else
                    return null;
            }
            string specNs = null;

            // namespace sepecification
            if (Spoiler(spoiler ? 1 : 0) is NamespaceSpecificationSpan)
            {
                specNs = name.FullText;
                if (spoiler)
                {
                    var next = Spoiler(2);
                    if (next is WordSpan ws)
                        name = ws;
                    else
                        return null;
                }
                else
                {
                    ReadSpan();
                    name = ReadWord();
                }
            }

            return new DefNameSpan(specNs, name.FullText, name.Document, name.DocumentLocation, this) { CancelResolve = spoiler };
        }

        private Span Spoiler(int skip = 0)
        {
            Span last = lastSpan;
            int cur = cursor;
            int spcur = CodeSpans == null ? spansCursor : codeCursor;
            Span span = null;
            for (int i = 0; i <= skip; i++)
                span = ReadSpan(true);
            cursor = cur;
            if (CodeSpans == null)
                spansCursor = spcur;
            else
                codeCursor = spcur;
            lastSpan = last;
            return span;
        }

        private bool KeywordSpoiler<T>() where T : IKeyword
        {
            throw new NotImplementedException("remove spaces and comments");
            return spansCursor < SourceSpans.Length && SourceSpans[spansCursor].text == T.Keyword;
        }

        private Variable GetPointer(string name, IVarSystem from, out IVarSystem foundAt)
        {
            Variable pointer = null;

            // a function to scan a single VS
            Variable Scan(IVarSystem vs)
            {
                ArgumentNullException.ThrowIfNull(vs);
                if (vs.Vars is null)
                    throw new NullReferenceException(nameof(vs.Vars));
                if (name is null)
                    return null;
                return vs.Vars.FirstOrDefault(v => name.Equals(v.Name)) ?? (vs is FuncDefSpan func && func.DefinedAt != null ? func.DefinedAt.Vars.FirstOrDefault(v => name.Equals(v.Name)) : null);
            }

            // scan the current VS inner, then in its outers, then outers' outers and so on
            // if specific VS is attached, start from it
            IVarSystem vs = from;
            while (vs != null)
            {
                pointer = Scan(vs);
                if (pointer == null)
                {
                    vs = vs.Parent;
                }
                else
                    break;
            }

            if (pointer != null)
            {
                foundAt = vs;
                //ValidateAccess(pointer, vs, from); now it's done at GetNamedValueItem(...)
            }
            else
                foundAt = null;

            return pointer;
        }

        private void LoadAttributes(ICanSetAttr taggedItem)
        {
            if (taggedItem.TagsCode == null)
            {
                taggedItem.AttrInfo = [];
                return;
            }
            taggedItem.AttrInfo = new Instance[taggedItem.TagsCode.Count];
            int counter = 0;
            foreach (var code in taggedItem.TagsCode)
            {
                // set source properties before reading spans
                Span[] sourceSpans_backup = CodeSpans;
                Span ls = lastSpan;
                int cur = this.cursor, spcur = codeCursor;
                cursor = 0;
                codeCursor = 0;
                CodeSpans = code.ToArray();

                // get attribute
                var defName = ReadDefName(); // as current method is executed after collecting defs, we can use this def name span immidiately (without defName.Resolved += ...)
                var attr = defName?.Attr;
                if (attr == null)
                {
                    Error($"Unknown attribute '{defName.Name}'.");
                    return;
                }

                // read args
                List<IValue> args = [];
                var spoiler = Spoiler();
                if (spoiler == null || spoiler is not OpeningBracketSpan)
                {
                    if (attr.Params.Length >= 1)
                        Error("Expected '('" + (spoiler != null ? $", but '{spoiler.FullText}' was read" : "") + ".");
                    else
                        goto AfterReadingArgs;
                }
                Read<OpeningBracketSpan>();
                for (int i = 0; i < attr.Params.Length; i++)
                {
                    var valop = ReadReadingOperation();
                    if (valop is not ConstValueReadingOperation or ConstArrayReadingOperation)
                        Error($"Argument {i} of attribute {((IDefination)attr).FullName} must be a constant value.");
                    var val = valop.Read();

                    // check type match
                    bool typeMismatch = false;
                    if (attr.Params[i].ExpType != null)
                        typeMismatch = val is not Instance inst || attr.Params[i].ExpType != inst.def.ExpType;
                    else
                        typeMismatch = attr.Params[i].Type != val?.GetType();

                    if (typeMismatch)
                        Error($"Argument {i} of attribute {((IDefination)attr).FullName} must be of type {attr.Params[i].ExpType?.Vars[1].Value.GetExpTypeName(true) ?? attr.Params[i].Type.GetExpTypeName()} (Type read: {Extensions.GetExpTypeName(val, true)}).");

                    args.Add(val);

                    // read , or )
                    var next = ReadSpan();
                    if (next == null)
                        Error("Missing ).");
                    if (next is ClosingBracketSpan)
                        break;
                    if (next is not CommaSpan)
                        Error($"Unexpected span '{next.FullText}'.");
                }
            AfterReadingArgs:

                // validate AllowFor
                if (taggedItem is ClassDefSpan && !attr.AllowFor_Class
                    || taggedItem is Property or ClassStaticVar && !attr.AllowFor_Property
                    || taggedItem is FuncDefSpan and not ConstructorDefSpan && !attr.AllowFor_Func
                    || taggedItem is ConstructorDefSpan && !attr.AllowFor_Constructor
                    || taggedItem is AttributeDefSpan && !attr.AllowFor_Attr)
                {
                    Error($"The {((IDefination)attr).FullName} attribute cannot be set for {(taggedItem as IExpItem)?.GetItemName()}.");
                }

                // validate AllowMultiple
                if (!attr.AllowMultiple && taggedItem.HasTag(attr))
                    Error($"An item cannot have multiple tags of attribute '{attr.GetExpTypeName(false)}'.");

                // validate OneInClass
                if (attr.LimitTo1InCls && taggedItem is IClassMember member && member.Def is ClassDefSpan cls1)
                {
                    void Validate(ICanSetAttr item)
                    {
                        if (item.HasTag(attr))
                            Error($"The {attr.GetExpTypeName(false)} attribute may only be set once in a class.");
                    }

                    cls1.Vars.OfType<ClassStaticVar>().ForEach(Validate);
                    cls1.Props.ForEach(Validate);
                    cls1.Funcs.ForEach(Validate);
                }

                // validate FuncRequirements
                if (taggedItem is FuncDefSpan fn and not ConstructorDefSpan)
                {
                    if (attr.Func_StaticRequirement == AttributeDefSpan.StaticRequirement.Static && !fn.Static)
                        Error($"Function {fn} must be marked as static because it has tag of attribute {attr.GetExpTypeName(false)}.");
                    else if (attr.Func_StaticRequirement == AttributeDefSpan.StaticRequirement.NonStatic && fn.Static)
                        Error($"Function {fn} cannot be marked as static because it has tag of attribute {attr.GetExpTypeName(false)}.");

                    if (attr.Func_ParamsCountRequirement >= 0 && fn.Args.Length != attr.Func_ParamsCountRequirement)
                        Error($"Function {fn} must take {attr.Func_ParamsCountRequirement} parameters because it has tag of attribute {attr.GetExpTypeName(false)}.");
                }

                // validate expects
                if (taggedItem is ClassDefSpan cls)
                {
                    foreach (var info in attr.GetAttrInfoOf(AttributeDefSpan.ExpectFuncAttr))
                    {
                        var infoVals = info.Vars.First(v => v.Name == "values").Value.Inst;
                        string func1 = infoVals.ArrayValues[0].Inst.ToString();
                        int paramsc = (int)infoVals.ArrayValues[1].Number;
                        bool stat = infoVals.ArrayValues[2].Bool;
                        if (!cls.Funcs.Any(f => f.Name == func1 && f.Args.Length == paramsc && !f.Private && f.Static == stat))
                            Error($"'{cls.GetExpTypeName(false)}' must contain a public {(stat ? "" : "non-")}static function named '{func1}' taking {paramsc} arguments, because it contains tag of attribute '{attr.GetExpTypeName(false)}'.");
                    }
                }

                // if it's @AllowFor set AllowFor_X properties
                if (attr == AttributeDefSpan.AllowFor)
                {
                    if (taggedItem is AttributeDefSpan setFor)
                    {
                        setFor.AllowFor_Class = args[0].Bool;
                        setFor.AllowFor_Property = args[1].Bool;
                        setFor.AllowFor_Func = args[2].Bool;
                        setFor.AllowFor_Constructor = args[3].Bool;
                        setFor.AllowFor_Attr = args[4].Bool;
                    }
                }

                // if it's @AllowMultiple set AllowMultiple property
                else if (attr == AttributeDefSpan.AllowMultipleAttr)
                {
                    if (taggedItem is AttributeDefSpan setFor)
                    {
                        setFor.AllowMultiple = true;
                    }
                }

                // if it's @OneInClass set LimitTo1InCls property
                else if (attr == AttributeDefSpan.LimitTo1InClsAttr)
                {
                    if (taggedItem is AttributeDefSpan setFor)
                    {
                        setFor.LimitTo1InCls = true;
                    }
                }

                // if it's @FuncRequirements set Func_X properties
                else if (attr == AttributeDefSpan.FuncRequirementsAttr)
                {
                    if (taggedItem is AttributeDefSpan setFor)
                    {
                        setFor.Func_StaticRequirement = (AttributeDefSpan.StaticRequirement)args[0].Number;
                        setFor.Func_ParamsCountRequirement = (int)args[1].Number;
                    }
                }

                taggedItem.AttrInfo[counter++] = CreateAttrInfo(attr, args.ToArray());

                // if it's @Translator attr, set this func as toString func
                if (attr == AttributeDefSpan.ToString)
                {
                    if (taggedItem is FuncDefSpan { Static: false, Name: not null, Args.Length: 0, DefinedAt: not null } func)
                    {
                        if (func.DefinedAt.ToString != null)
                            Error($"The {attr.Name} tag may only be set once in a class.");
                        func.DefinedAt.ToString = func;
                    }
                    else
                        Error($"Only non-static functions defined in a class and not taking parameters may have the {attr.Name} tag.");
                }

                // if it's @Equalizer attr, set this func as equalizer func
                if (attr == AttributeDefSpan.EqualizerAttr)
                {
                    if (taggedItem is FuncDefSpan { Static: false, Args.Length: 1, DefinedAt: not null } func)
                    {
                        func.DefinedAt.Equalizer = func;
                    }
                }

                // if it's @ReadOnly attr, set this func as read-only func
                if (attr == AttributeDefSpan.ReadOnlyAttr)
                {
                    if (taggedItem is FuncDefSpan func)
                    {
                        func.ReadOnly = true;
                    }
                }

                // reset source properties
                CodeSpans = sourceSpans_backup;
                lastSpan = ls;
                cursor = cur;
                codeCursor = spcur;
                contextLoc = 0;
            }
        }

        public void ValidateDefNameLegallity(string ns, string name, int @params = -1)
        {
            if (!name.IsLiterallyValidName())
                Error(name == null ? "Item name was expected." : $"'{name}' is not a valid name.");

            // validate in definations
            else if (definations.Any(d => d != null && d.Namespace == ns && d.Name == name && (d is not FuncDefSpan func || func.Args.Length == @params)))
                Error($"An item with the name '{(ns == null ? "" : ns + NamespaceSpecificationSpan.Symbol)}{name}' already exists.");
        }

        public void ValidateLocalNameLegallity(string name, IVarSystem vs, int args = -1)
        {
            ValidateDefNameLegallity(null, name);

            // validate in given VS
            if (GetPointer(name, vs) != null)
                Error($"A variable with the name '{name}' already exists in this context / item.");

            // check funcs
            else if (vs is ClassDefSpan cls)
            {
                if (cls.Funcs.Any(f => f.Name == name && args == f.Args.Length))
                    Error($"A function with the name '{name}' already exists in this context / item.");
            }
        }

        private Variable GetPointer(string name, IVarSystem from) => GetPointer(name, from, out IVarSystem _);

        private void ValidateAccess(INamedValue member, IVarSystem setAt, IVarSystem from)
        {
            ArgumentNullException.ThrowIfNull(member);

            if (!member.Private || from == setAt)
                return;

            var fromFn = FindParentVarSystem<FuncDefSpan>(from);
            var setAtInst = setAt as Instance;
            var setAtCls = setAt as ClassDefSpan;

            if (fromFn != null)
            {
                if (setAtInst != null && fromFn.DefinedAt == setAtInst.def)
                    return;
                if (setAtCls != null && fromFn.DefinedAt == setAtCls)
                    return;
            }

            Error($"Access to this class member is denied from this VS / Context (Member: '{member.Name}').");
        }

        private StackIndex FindStack(IContext ctx)
        {
            var stack = this.stack;
            if (stack?.context == ctx)
                return stack;
            while (stack != null)
            {
                if (stack.context == ctx)
                    return stack;
                stack = stack.parent;
            }
            return null;
        }

        private VsStackIndex FindVsStack(IVarSystem ctx)
        {
            var stack = this.vsStack;
            if (stack?.vs == ctx)
                return stack;
            while (stack != null)
            {
                if (stack.vs == ctx)
                    return stack;
                stack = stack.parent;
            }
            return null;
        }

        private Variable SetVar(string name, IValue value, IVarSystem at, WordSpan settingVar = null, bool createConst = false)
        {
            var v = GetPointer(name, at, out IVarSystem foundAt);
            if (v != null)
            {
                ValidateAccess(v, foundAt, at);
                v.Value = value;
            }
            else
            {
                throw new Exception($"Variable {name} not found.");
            }
            return v;
        }

        private void SetArrVar(string name, int index, object value, IVarSystem specificVS = null)
        {
            var v = GetPointer(name, specificVS);
            if (v.Value is Instance inst && inst.IsArray)
            {
                if (index < 0 || index >= inst.ArrayValues.Length)
                    ThrowRuntime($"index is out of range (index: {index}, array length: {inst.ArrayValues.Length}).", RuntimeException.INDEX_OUT_OF_RANGE);
            }
            else
                Error($"'{name}' is not an array.");
        }

        private object GetVar(string name, IVarSystem specificVS)
        {
            throw new NotImplementedException();
            Variable pointer = null; //GetPointer(name, out IVarSystem foundAt, specificVS);
            if (pointer != null)
            {
                ValidateAccess(pointer, null, CurrentVarSystem);
                return pointer.Value;
            }

            Error($"Unknown variable '{name}'.");
            throw null;
        }

        private T GetVar<T>(string name, bool allowNull = true, IVarSystem specificVS = null)
        {
            object val = GetVar(name, specificVS);
            if (val == null)
            {
                if (allowNull)
                    return default;
                ThrowRuntime($"{name} is null.", RuntimeException.NULL_REFERENCE);
            }
            if (val is T obj)
                return obj;
            Error(name + $" is {val.GetType()}, but {typeof(T)} was expected.");
            throw null;
        }

        private bool VarExists(string name, IVarSystem specificVS = null)
        {
            return GetPointer(name, specificVS) != null;
        }

        private static Instance CreateAttrInfo(AttributeDefSpan attr, IValue[] vals)
        {
            Instance info = new(ClassDefSpan.ExpAttrInfoDef);
            info.Vars[0].SetSkippingConstant(attr.ExpType);
            info.Vars[1].SetSkippingConstant(attr.Namespace?.ToExpString());
            info.Vars[2].SetSkippingConstant(attr.Name.ToExpString());
            info.Vars[3].SetSkippingConstant(vals.ToExpArray());
            return info;
        }

        private static void Print(object s)
        {
            s.Print();
        }

        private static void Println(object s)
        {
            s.Println();
        }
    }

    public class ExpError : Exception
    {
        public string Doc { get; }
        public int Line { get; }
        public int Col { get; }
        public ExpError(string sourceName, int line, int col, string msg) : base(msg)
        {
            this.Doc = sourceName;
            this.Line = line;
            this.Col = col;
        }
    }

    public class BuildFailureException(IEnumerable<ExpError> errors) : Exception
    {
        public IEnumerable<ExpError> Errors => errors;
    }

    public class RuntimeException : Exception
    {
        public const string INDEX_OUT_OF_RANGE = "INDEX_OUT_OF_RANGE";
        public const string ARGUMENT_NULL = "ARGUMENT_NULL";
        public const string NULL_REFERENCE = "NULL_REFERENCE";
        public const string INVALID_ARGUMENT = "INVALID_ARGUMENT";
        public const string INVALID_OPERATION = "INVALID_OPERATION";
        public const string NOT_FOUND = "NOT_FOUND";
        public const string DIVIDE_BY_ZERO = "DIVIDE_BY_ZERO";
        public const string EXTERN_OPERATION_FAILED = "EXTERN_OPERATION_FAILED";
        public const string STACK_OVERFLOW = "STACK_OVERFLOW";
        public const string INIT_ERR = "INIT_ERR";
        public const string INVALID_SYNTAX = "INVALID_SYNTAX";
        public string msg, type, source;
        public int line, col;
        public Instance ex;
        public bool ByExpThrowStmt { get; }

        internal RuntimeException(Instance ex, string msg, string type, string source = null, int line = 0, int col = 0, bool byExpThrowStmt = false) : base($"{source ?? "Unknown"}({line}, {col}): {msg}")
        {
            if (ex == null || ex.def != ClassDefSpan.ExpExceptionDef)
                throw new Exception($"Argument '{nameof(ex)}' was not an instance of system::Exception.");
            this.ex = ex;
            this.msg = msg;
            this.type = type;
            this.source = source;
            this.line = line;
            this.col = col;
            this.ByExpThrowStmt = byExpThrowStmt;
        }
    }
}