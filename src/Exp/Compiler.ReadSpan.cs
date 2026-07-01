using Exp.Spans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Globalization;

namespace Exp;

public partial class Interpreter
{
    private bool ignoreCanSetTag = false;
    private Span lastSpan;
    internal IVarSystem ParentVs { get; private set; }
    private Span ReadSpan(bool spoiler = false, bool spaces = false, ClassDefSpan def = null, bool singleWord = false, bool @throw = false)
    {
        if (CodeSpans != null)
        {
            lastSpan = codeCursor < CodeSpans.Length ? CodeSpans[codeCursor++] : null;
            return lastSpan;
        }

        if (spansCursor >= SourceSpans.Length)
            return null;

        TextSpan textSpan = SourceSpans[spansCursor++];
        string text = textSpan.text;
        cursor += text.Length;

        // skip spaces and comments
        if (string.IsNullOrWhiteSpace(textSpan.text) || textSpan.type == SpanType.Space || textSpan.type == SpanType.Comment || textSpan.type == SpanType.MultiLineComment)
            return ReadSpan(spoiler, spaces, def, singleWord, @throw);

        bool recording = readValue_codeRecord != null;
        var record_bu = readValue_codeRecord;
        readValue_codeRecord = null;

        bool recordingOps = readOps_codeRecord != null;
        var recordOps_bu = readOps_codeRecord;

        Span span = null;
        if (textSpan.type == SpanType.Number)
        {
            bool isHex = text.StartsWith("0x", ignoreCase: true, null) && text.Length >= 3;
            if (isHex && int.TryParse(text.Substring(2), NumberStyles.HexNumber, null, out int i))
                span = new NumberSpan(i);
            else if (!isHex && double.TryParse(text, out double d))
                span = new NumberSpan(d);
            else
            {
                Error("Invalid number format.");
                span = new NumberSpan(0);
            }
        }
        else if (textSpan.type == SpanType.Count)
        {
            if (int.TryParse(text.Substring(2), out int i))
            {
                span = new CountSpan(i);
            }
            else
            {
                Error("Invalid number format.");
                span = new NumberSpan(0);
            }
        }
        else if (textSpan.type == SpanType.String)
            span = new StringSpan(text[1..^1].Replace("\\n", "\n").Replace("\\t", "\t").Replace("\\\"", "\"").Replace("\\\\", "\\"));
        else if (textSpan.type == SpanType.EscapedString)
            span = new StringSpan(text[2..^1], escaped: true);
        else if (textSpan.type == SpanType.Char)
            span = new CharSpan(text[1..^1].Replace("\\n", "\n").Replace("\\t", "\t").Replace("\\\"", "\"").Replace("\\\\", "\\")[0]);
        else if (textSpan.type == SpanType.Tag)
        {
            if (text == "@")
            {
                List<Span> code = [];

                // read attribute def name
                code.Add(Read<WordSpan>());
                string attrName = code[0].Text;
                if (Spoiler() is NamespaceSpecificationSpan)
                {
                    code.Add(ReadSpan());
                    code.Add(ReadWord());
                    attrName += NamespaceSpecificationSpan.Symbol + code[2];
                }

                // read attribute param list
                if (Spoiler() is OpeningBracketSpan)
                {
                    code.Add(ReadSpan());
                    for (int op = 1, cl = 0; op > cl;)
                    {
                        var next = ReadSpan();
                        code.Add(next);

                        if (next == null)
                            Error(") Expected.");
                        else if (next is ClosingBracketSpan)
                            cl++;
                        else if (next is OpeningBracketSpan)
                            op++;
                    }
                }

                if (ignoreCanSetTag)
                {
                    span = new TagSpan(attrName, code.ToArray());
                }
                else
                {
                    // return the span after the tag
                    var nextSpan = ReadSpan(spoiler: spoiler, def: def);
                    if (!spoiler)
                    {
                        if (nextSpan is ICanSetAttr next)
                        {
                            next.TagsCode.Add(code.ToArray());
                            return nextSpan;
                        }
                        Error(nextSpan + " cannot have tags.");
                    }
                    else
                        return nextSpan;
                }
            }
        }
        else if (textSpan.type == SpanType.Normal)
        {
            if (singleWord)
                return new WordSpan(text);

            if (text == TrueWordSpan.Keyword)
                span = new TrueWordSpan();
            else if (text == FalseWordSpan.Keyword)
                span = new FalseWordSpan();
            else if (text == IfConditionSpan.Keyword || text == WhileConditionSpan.Keyword || text == ForLoopSpan.Keyword || text == ForEachLoopSpan.Keyword || text == ElseConditionSpan.Keyword)
            {
                bool forloop = text == ForLoopSpan.Keyword, foreachloop = text == ForEachLoopSpan.Keyword, elsec = text == ElseConditionSpan.Keyword, rangeloop = false;
                Span[] condition = [], arrReadText = [], fromReadText = [], toReadText = [], initExe = [], stepExe = [], innerSource = []; // don't init with null bc in spoiler readings these wouldn't be assigned anymore and it would throw argument null
                string idAttr = null, counterAttr = null, varname = null;
                bool foreachConst = false;

                if (!spoiler)
                {
                    if (foreachloop)
                    {
                        // check const / var keyword
                        if (Spoiler() is ConstWordSpan)
                        {
                            ReadSpan();
                            foreachConst = true;
                        }
                        else if (Spoiler() is SetWordSpan)
                        {
                            ReadSpan();
                        }

                        // read var name
                        varname = ReadWord().FullText;

                        // format "foreach x [from y] to z" is range loop
                        if (Spoiler()?.FullText == "from")
                        {
                            ReadWord();
                            rangeloop = true;
                            ReadValue(out fromReadText, null, true);
                        }
                        if ((rangeloop ? ReadWord("to") : Spoiler())?.FullText == "to")
                        {
                            if (!rangeloop)
                                ReadWord();

                            rangeloop = true;
                            foreachloop = false;
                            ReadValue(out toReadText, null, true);
                        }
                        else
                        {
                            // format "foreach x in y" is foreach loop
                            ReadWord("in");
                            ReadValue(out arrReadText, null, true);
                        }
                    }
                    else
                    {
                        if (forloop)
                        {
                            Read<OpeningBracketSpan>();

                            List<Span> initCode = [];
                            while (true)
                            {
                                if (cursor >= source.Length)
                                {
                                    Error($"Endless for loop, {SemicolonSpan.Symbol} expected.");
                                    break;
                                }
                                Span ispan = ReadSpan(spaces: true);
                                if (ispan is SemicolonSpan)
                                    break;
                                initCode.Add(ispan);
                            }
                            initExe = initCode.ToArray();
                        }

                        // read condition
                        if (!elsec)
                        {
                            ReadValue<bool>(out condition, allowUnknownVars: true);
                            if (forloop)
                                Read<SemicolonSpan>();
                        }

                        if (forloop)
                        {
                            List<Span> stepCode = [];
                            while (true)
                            {
                                if (cursor >= source.Length)
                                {
                                    Error($"Endless for loop, {SemicolonSpan.Symbol} expected.");
                                    break;
                                }

                                Span ispan = ReadSpan(spaces: true);
                                if (ispan is ClosingBracketSpan)
                                    break;
                                stepCode.Add(ispan);
                            }
                            stepExe = stepCode.ToArray();
                        }
                    }

                    string[] attr = ["id", "counter"];

                    if (!elsec && Spoiler()?.FullText == ":")
                    {
                        ReadSpan();

                        while (true)
                        {
                            WordSpan word = ReadWord();
                            if (word.FullText == attr[0])
                            {
                                if (idAttr != null)
                                    Error(attr[0] + " is already defined.");
                                else
                                    idAttr = ReadWord().FullText;
                            }
                            else if (word.FullText == attr[1])
                            {
                                if (counterAttr != null)
                                    Error(attr[1] + " is already defined.");
                                else
                                    counterAttr = ReadWord().FullText;
                            }
                            else
                            {
                                Error($"Unexpected word '{word}'. Condition attributes are: {attr}");
                            }

                            Span next = ReadSpan();
                            if (next is SourceOpenerSpan)
                                break;
                            else if (next is not CommaSpan)
                                Error($"Unexpected '{next.Text}'. Only ',' and '{{' can appear here.");
                        }
                    }

                    innerSource = ReadInnerSource(idAttr == null && counterAttr == null, allowSingleCmd: true) ?? [];
                }

                // create the spans
                if (text == IfConditionSpan.Keyword)
                    span = new IfConditionSpan(condition, innerSource, CurrentVarSystem);
                else if (text == WhileConditionSpan.Keyword)
                    span = new WhileConditionSpan(condition, innerSource, CurrentVarSystem);
                else if (forloop)
                    span = new ForLoopSpan(initExe, condition, stepExe, innerSource, CurrentVarSystem);
                else if (foreachloop)
                    span = new ForEachLoopSpan(varname, foreachConst, arrReadText, innerSource, CurrentVarSystem);
                else if (rangeloop)
                    span = new RangeLoopSpan(varname, foreachConst, fromReadText, toReadText, innerSource, CurrentVarSystem);
                else if (elsec)
                    span = new ElseConditionSpan(innerSource, CurrentVarSystem);

                if (span is ILoopContext loop)
                {
                    loop.Id = idAttr;
                    loop.Counter = counterAttr;
                }
            }
            else if (text == BreakWordSpan.Keyword)
                span = new BreakWordSpan();
            else if (text == ContinueWordSpan.Keyword)
                span = new ContinueWordSpan();
            else if (text == FuncDefSpan.Keyword || text == ConstructorDefSpan.Keyword)
            {
                string name = null;
                bool ctor = text == ConstructorDefSpan.Keyword;

                if (!ctor && Spoiler() is not OpeningBracketSpan)
                    name = ReadWord().FullText;

                Read<OpeningBracketSpan>();

                var args = new List<ArgumentSpan>();
                Span argsp = ReadWord();
                if (argsp is not ClosingBracketSpan)
                {
                    while (true)
                    {
                        bool notnull = true; // removing notnull word
                        var nspoiler = Spoiler();
                        if (Spoiler() is NotNullWordSpan)
                        {
                            notnull = true;
                            ReadWord();
                        }
                        else if (nspoiler is QuestionMarkSpan)
                        {
                            notnull = false;
                            ReadWord();
                        }

                        // error if this arg name already exists
                        ValidateDefNameLegallity(null, argsp.FullText);
                        if (args.Any(a => a.Name == argsp.FullText))
                            Error($"The parameter name '{argsp.FullText}' is a duplicate.");

                        args.Add(new ArgumentSpan(argsp.FullText, notnull));
                        argsp = ReadSpan();
                        if (argsp is CommaSpan)
                            argsp = ReadWord();
                        else if (argsp is ClosingBracketSpan)
                            break;
                        else
                            Error($"Unexpected '{argsp.Text}'. Only ',' or ')' are expected here.");
                    }
                }

                // read inner source. if next is '=>' add a return span as first inner source span
                Span[] innerSource = null;
                Span first = ReadSpan();
                if (first is ReturnSymbolSpan)
                {
                    ReadValue(out Span[] innerSrc, allowUnknownVars: true);
                    var ls = new List<Span>();
                    ls.Add(new ReturnWordSpan { Document = first.Document, DocumentLocation = first.DocumentLocation });
                    ls.AddRange(innerSrc);
                    innerSource = ls.ToArray();
                }
                else if (first is SourceOpenerSpan or DoSymbolSpan)
                    innerSource = ReadInnerSource(readOpener: false, singleCmd: first is DoSymbolSpan);
                else
                {
                    Error("Expected { or => or ->.");
                }

                innerSource ??= []; // on error

                if (!spoiler)
                {
                    if (ctor)
                        span = new ConstructorDefSpan(args.ToArray(), innerSource, def, this) { Static = true }; // static will become false in reading as class member
                    else
                        span = new FuncDefSpan(name, args.ToArray(), innerSource, def) { Static = true }; // see comment above
                }
            }
            else if (text == ClassDefSpan.Keyword)
            {
                bool basearrSet = false;
                List<Property> props = [];
                List<FuncDefSpan> funcs = [];

                // read class name
                string name = ReadWord()?.FullText;

                // read properties
                Read<OpeningBracketSpan>();

                ignoreCanSetTag = true;
                Span propsp = ReadSpan();
                ignoreCanSetTag = false;
                if (propsp is not ClosingBracketSpan)
                {
                    while (true)
                    {
                        List<Span[]> tagCode = [];
                        bool prvt = false, basearr = false, cons = false;

                        // before pname
                        while (propsp is TagSpan tag)
                        {
                            tagCode.Add(tag.Code);
                            ignoreCanSetTag = true;
                            propsp = ReadWord();
                            ignoreCanSetTag = false;
                        }

                        if (propsp is ConstWordSpan)
                        {
                            cons = true;
                            propsp = ReadWord();
                        }

                        // read param name
                        string pname = propsp.FullText;

                        // after pname
                        propsp = ReadSpan();
                        if (propsp is PrivateWordSpan)
                        {
                            prvt = true;
                            propsp = ReadWord();
                        }
                        if (propsp is BaseArrayWordSpan)
                        {
                            if (basearrSet)
                                Error("Only 1 property can be marked as basearray.");
                            else
                                basearrSet = true;
                            basearr = true;
                            propsp = ReadWord();
                        }

                        Span[] val = null;
                        if (propsp is SetSymbolSpan)
                        {
                            ReadValue(out val, allowUnknownVars: true);
                            propsp = ReadWord();
                        }

                        props.Add(new Property(null, cons, pname, prvt, basearr, val, tagCode));

                        if (propsp is CommaSpan)
                            propsp = ReadWord();
                        else if (propsp is ClosingBracketSpan)
                            break;
                        else
                            Error($"Unexpected '{propsp.Text}'. Only ',' or ')' are expected here.");
                    }
                }

                // func to read static set (static name = value) (this func will be called after reading the static word)
                ClassStaticVar ReadStaticSet(Span fsp)
                {
                    bool constant = false;
                    if (fsp is ConstWordSpan)
                    {
                        constant = true;
                        fsp = ReadWord();
                    }
                    string name = fsp.FullText;
                    Span settingSpan = null;
                    Span[] initValueCode = null;
                    if (Spoiler() is SetSymbolSpan set)
                    {
                        ReadSpan();
                        ReadValue(out initValueCode, allowUnknownVars: true); // allow unknown vars - this way a static var will be able to reference another one
                        settingSpan = set;
                    }

                    return new ClassStaticVar(name, null, null, settingSpan, false, cons: constant) { InitValueCode = initValueCode };
                }

                // collect funcs and statics
                Read<SourceOpenerSpan>();

                // create the class span to use it as def parameter in ReadSpan()
                var cls = new ClassDefSpan(name, props.ToArray());

                while (true)
                {
                    if (cursor >= source.Length)
                        Error("Endless function.");

                    ignoreCanSetTag = true;
                    Span? ispan = ReadSpan(spaces: false, def: cls);
                    bool statdef = false, prvtdef = false;
                    List<Span[]> tagsCode = [];
                    while (ispan is TagSpan tag)
                    {
                        tagsCode.Add(tag.Code);
                        ispan = ReadSpan(spaces: false, def: cls);
                    }
                    ignoreCanSetTag = false;
                    if (ispan is PrivateWordSpan)
                    {
                        prvtdef = true;
                        ispan = ReadSpan(spaces: false, def: cls);
                    }
                    if (ispan is StaticWordSpan)
                    {
                        statdef = true;
                        ispan = ReadSpan(spaces: false, def: cls);
                    }

                    if (ispan is SourceCloserSpan)
                        break;
                    if (ispan is FuncDefSpan func)
                    {
                        func.Static = statdef;
                        func.Parent = statdef ? cls : null;
                        func.Private = prvtdef;
                        func.TagsCode = tagsCode;

                        // throw if a static constructor has parameters
                        if (func is ConstructorDefSpan && func.Static && func.Args.Length > 0)
                            Error("A static constructor must be parameterless.");

                        funcs.Add(func);
                    }
                    else if (statdef)
                    {
                        var stprop = ReadStaticSet(ispan);
                        stprop.Private = prvtdef;
                        stprop.TagsCode = tagsCode;
                        cls.Vars.Add(stprop);
                    }
                    else if (ispan == null)
                    {
                        Error("} Expected.");
                        break;
                    }
                    else
                        Error($"Unexpected span in a class ('{ispan.Text}').");
                }

                cls.Props.ForEach(p => p.Def = cls);
                cls.Vars.OfType<ClassStaticVar>().ForEach(v => v.Def = cls);
                cls.Funcs = funcs.ToArray();

                // validate duplicate names
                List<IClassMember> members = cls.Vars.OfType<ClassStaticVar>().ToList<IClassMember>();
                members.AddRange(cls.Props);
                members.AddRange(cls.Funcs);
                foreach (var member in members)
                {
                    if (member is ConstructorDefSpan || member == FuncDefSpan.ArrayIndexGetter || member == FuncDefSpan.ArrayIndexSetter)
                        continue;

                    if (member.Name == null)
                        Error($"Missing {(member as IExpItem)?.GetItemName() ?? "class member"} name.");
                    else
                    {
                        int args = member is FuncDefSpan f ? f.Args.Length : -1;
                        ValidateDefNameLegallity(null, member.Name, args);
                        if (members.Any(m => m != member && m.Name == member.Name && (m is FuncDefSpan fn && fn.Args.Length == args)))
                            Error(cls.GetExpTypeName(false) + $" already contains a member named '{member.Name}'.");
                    }
                }

                span = cls;
            }
            else if (text == InstInitSpan.Keyword)
            {
                span = new InstInitSpan(ReadDefName());
            }
            else if (text == EnumDefSpan.Keyword)
            {
                // read enum name
                string ename = ReadWord().FullText;

                // read content
                List<EnumValueSpan> evalues = [];
                Read<SourceOpenerSpan>();

                Span next;
                do
                {
                    if (cursor >= source.Length)
                        Error("Endless enum.");

                    ignoreCanSetTag = true;
                    next = ReadSpan();
                    List<Span[]> tagsCode = [];
                    while (next is TagSpan tag)
                    {
                        tagsCode.Add(tag.Code);
                        next = ReadWord();
                    }
                    ignoreCanSetTag = false;

                    if (next is SourceCloserSpan)
                        break;

                    string vname;
                    bool specific = false;

                    // read value name
                    vname = next.FullText;
                    next = ReadSpan();

                    // read specific value
                    List<double> specifics = [];
                    double val = 0;
                    if (next is SetSymbolSpan)
                    {
                        val = Read<NumberSpan>().Number;
                        if (specifics.Contains(val))
                            Error($"The value {val} is already specified for another name in this enum ('{ename}').");

                        specifics.Add(val);
                        specific = true;
                        next = ReadSpan();
                    }

                    // validate duplicate names
                    ValidateDefNameLegallity(null, vname);
                    if (evalues.Any(ev => ev.Name == vname))
                        Error(ename + $" already contains a value named '{vname}'.");

                    evalues.Add(new EnumValueSpan(vname, val, specific) { TagsCode = tagsCode });

                    // expect ',' or '}'
                    if (next is SourceCloserSpan)
                        break;
                    if (next is not CommaSpan)
                        Error($"Unexpected span in enum content. Only ',' or '}}' is expected here (Span received: '{next}').");
                }
                while (true);

                // set values for all the names that has no custom value
                foreach (var ev in evalues)
                {
                    if (ev.CustomValue)
                        continue;
                    else while (evalues.FirstOrDefault(ev1 => ev1 != ev && ev1.Value == ev.Value) != null)
                            ev.Value++;
                }

                span = new EnumDefSpan(ename, evalues.ToArray());
            }
            else if (text == NotNullWordSpan.Keyword)
                span = new NotNullWordSpan();
            else if (text == ReturnWordSpan.Keyword)
                span = new ReturnWordSpan();
            else if (text == ThisWordSpan.Keyword)
                span = new ThisWordSpan();
            else if (text == NullWordSpan.Keyword)
                span = new NullWordSpan();
            else if (text == LenofWordSpan.Keyword)
                span = new LenofWordSpan();
            else if (text == AttributeDefSpan.Keyword)
            {
                string defnm = ReadWord().Text;

                // read params
                Read<OpeningBracketSpan>();
                List<AttributeParamSpan> prms = [];
                if (Spoiler() is not ClosingBracketSpan)
                {
                    while (true)
                    {
                        AttributeParamSpan prm;

                        // read param type
                        WordSpan paramType = ReadWord();
                        string[] types = ["bool", "char", "number"];
                        Type[] types_ = [typeof(BoolValue), typeof(CharValue), typeof(NumberValue)];
                        int indexOf = Array.IndexOf(types, paramType.Text);

                        // if a premitive type was read, create as premitive type
                        if (indexOf >= 0)
                            prm = new AttributeParamSpan(types_[indexOf], ReadWord().Text);
                        else
                        {
                            // read class name
                            var defName = ReadDefName(paramType);

                            prm = new AttributeParamSpan(defName, ReadWord().Text);
                        }

                        prms.Add(prm);

                        // expect ',' or ')'
                        var next = ReadSpan();
                        if (next is ClosingBracketSpan)
                            break;
                        if (next is not CommaSpan)
                            Error($"Unexpected span in attribute defination ('{next.FullText}').");
                    }
                }
                else
                    ReadSpan();

                span = new AttributeDefSpan(defnm, prms.ToArray());
            }
            /*
            else if (text == TypeOfWordSpan.Keyword)
            {
                var typeofSpan = new TypeOfWordSpan();
                var defName = ReadDefName();
                void Resolve(object s, EventArgs e)
                {
                    if (defName.Class is ClassDefSpan cls)
                        typeofSpan.Value = cls.ExpType;
                    else if (defName.Extern is ExternClassDefSpan ext)
                        typeofSpan.Value = ext.Type.AsExtern();
                    else if (defName.Attr is AttributeDefSpan attr)
                        typeofSpan.Value = attr.ExpType;
                }
                if (collectedDefs)
                    Resolve(null, null);
                else
                    CollectDefsCompleted += Resolve;
                span = typeofSpan;
            }
            */
            else if (text == TryWordSpan.Keyword || text == CatchWordSpan.Keyword || text == FinallyWordSpan.Keyword || text == SectionWordSpan.Keyword)
            {
                string catchVarName = null;
                WhenWordSpan when = null;
                bool catc = text == CatchWordSpan.Keyword, _try = text == TryWordSpan.Keyword, section = text == SectionWordSpan.Keyword;

                Span next = ReadSpan();

                if (catc)
                {
                    // read varname
                    if (next is WordSpan && next is not WhenWordSpan && next is not SourceOpenerSpan)
                    {
                        catchVarName = next.FullText;
                        next = ReadSpan();
                    }

                    // read when
                    if (next is WhenWordSpan when_)
                    {
                        when = when_;
                        next = ReadSpan();
                    }
                }

                if (next is not SourceOpenerSpan)
                    Error("'{' was expected.");

                // read inner source
                Span[] innerSource = ReadInnerSource(false) ?? /*on error:*/ [];

                // create the span
                if (catc)
                    span = new CatchWordSpan(catchVarName, when, innerSource, CurrentVarSystem);
                else if (_try)
                {
                    CatchWordSpan cToAttach = null;
                    FinallyWordSpan fToAttach = null;

                    if (Spoiler() is CatchWordSpan)
                        cToAttach = Read<CatchWordSpan>();
                    if (Spoiler() is FinallyWordSpan)
                        fToAttach = Read<FinallyWordSpan>();
                    span = new TryWordSpan(innerSource, CurrentVarSystem, cToAttach, fToAttach);
                }
                else if (section)
                    span = new SectionWordSpan(innerSource, CurrentVarSystem);
                else
                    span = new FinallyWordSpan(innerSource, CurrentVarSystem);
            }
            else if (text == ThrowWordSpan.Keyword)
                span = new ThrowWordSpan();
            else if (text == NamespaceWordSpan.Keyword)
            {
                string ns = ReadWord().FullText;
                ReadWord(":");
                span = new NamespaceWordSpan(ns);
            }
            else if (text == UsingWordSpan.Keyword)
            {
                string ns = ReadWord().FullText;
                span = new UsingWordSpan(ns);
            }
            else if (text == PrintWordSpan.Keyword)
                span = new PrintWordSpan();
            else if (text == SetWordSpan.Keyword)
                span = new SetWordSpan();
            else if (text == ConstWordSpan.Keyword)
                span = new ConstWordSpan();
            else if (text == StaticWordSpan.Keyword)
                span = new StaticWordSpan();
            else if (text == PrivateWordSpan.Keyword)
                span = new PrivateWordSpan();
            else if (text == BaseArrayWordSpan.Keyword)
                span = new BaseArrayWordSpan();
            else if (text == IsWordSpan.Keyword)
                span = new IsWordSpan();
            else if (text == WhenWordSpan.Keyword)
            {
                ReadValue<bool>(out var cond, allowUnknownVars: true);
                span = new WhenWordSpan(cond);
            }
            else if (text == ExternClassDefSpan.Keyword)
            {
                // read class keyword
                Span clsKword = ReadSpan(false, false, null, singleWord: true);
                if (clsKword is not WordSpan || clsKword.FullText != ClassDefSpan.Keyword)
                    Error($"The '{ClassDefSpan.Keyword}' keyword must appear here.");

                // read ref name
                string refName = ReadWord().FullText;

                Read<SetSymbolSpan>();

                // read full type name
                string typeName = Read<StringSpan>().Text;

                // get the actual type
                Type type = typeof(object);
                try
                {
                    type = Extensions.GetTypeByName(typeName) ?? throw new Exception($"Could not find external type '{typeName}'.");
                }
                catch
                {
                    Error($"External type '{typeName}' not found.");
                }

                span = new ExternClassDefSpan(refName, type);
            }
            else
                span = new WordSpan(text);
        }
        else if (textSpan.type == SpanType.Symbol)
        {
            if (text == NotSymbolSpan.Symbol)
                span = new NotSymbolSpan();
            else if (text == SetSymbolSpan.Symbol)
                span = new SetSymbolSpan();
            else if (text == EqualsOperatorSpan.Symbol)
                span = new EqualsOperatorSpan();
            else if (text == NotEqualsOperatorSpan.Symbol)
                span = new NotEqualsOperatorSpan();
            else if (text == PlusOperatorSpan.Symbol)
                span = new PlusOperatorSpan();
            else if (text == MinusOperatorSpan.Symbol)
                span = new MinusOperatorSpan();
            else if (text == MultiplyOperatorSpan.Symbol)
                span = new MultiplyOperatorSpan();
            else if (text == DivideOperatorSpan.Symbol)
                span = new DivideOperatorSpan();
            else if (text == ModuluOperatorSpan.Symbol)
                span = new ModuluOperatorSpan();
            else if (text == GreaterThanOperatorSpan.Symbol)
                span = new GreaterThanOperatorSpan();
            else if (text == LowerThanOperatorSpan.Symbol)
            {
                DefNameSpan defName = null;
                defName = ReadDefName(null, true);
                if (defName != null && Spoiler(defName.SpecificNs == null ? 1 : 3) is GreaterThanOperatorSpan)
                {
                    defName = ReadDefName(); // on the existing one, CancelResolve is true
                    ReadSpan();
                }
                else
                    defName = null;

                if (defName == null) // x < y
                    span = new LowerThanOperatorSpan();
                else                  // <DefName>
                {
                    var typeofSpan = new TypeOfSpan();

                    void Resolve(object s, EventArgs e)
                    {
                        if (defName.Class is ClassDefSpan cls)
                            typeofSpan.Value = cls.ExpType;
                        else if (defName.Extern is ExternClassDefSpan ext)
                            typeofSpan.Value = ext.Type.AsExtern();
                        else if (defName.Attr is AttributeDefSpan attr)
                            typeofSpan.Value = attr.ExpType;
                        CollectDefsCompleted -= Resolve;
                    }
                    if (CollectedDefs)
                        Resolve(null, null);
                    else
                        CollectDefsCompleted += Resolve;
                    span = typeofSpan;
                }
            }
            else if (text == EqualsOrGreaterOperatorSpan.Symbol)
                span = new EqualsOrGreaterOperatorSpan();
            else if (text == EqualsOrLowerOperatorSpan.Symbol)
                span = new EqualsOrLowerOperatorSpan();
            else if (text == AndOperatorSpan.Symbol)
                span = new AndOperatorSpan();
            else if (text == OrOperatorSpan.Symbol)
                span = new OrOperatorSpan();
            else if (text == PlusPlusOperatorSpan.Symbol)
                span = new PlusPlusOperatorSpan();
            else if (text == MinusMinusOperatorSpan.Symbol)
                span = new MinusMinusOperatorSpan();
            else if (text == SetPlusOperatorSpan.Symbol)
                span = new SetPlusOperatorSpan();
            else if (text == SetMinusOperatorSpan.Symbol)
                span = new SetMinusOperatorSpan();
            else if (text == QuestionMarkSpan.Symbol)
                span = new QuestionMarkSpan();
            else if (text == NullCoalescingOperatorSpan.Symbol)
                span = new NullCoalescingOperatorSpan();
            else if (text == ReturnSymbolSpan.Symbol)
                span = new ReturnSymbolSpan();
            else if (text == DoSymbolSpan.Symbol)
                span = new DoSymbolSpan();
        }
        else if (textSpan.type == SpanType.DotCom)
        {
            if (text == DotSpan.Symbol)
                span = new DotSpan();
            else if (text == CommaSpan.Symbol)
                span = new CommaSpan();
            else if (text == SemicolonSpan.Symbol)
                span = new SemicolonSpan();
            else if (text == NamespaceSpecificationSpan.Symbol)
                span = new NamespaceSpecificationSpan();
        }
        else if (textSpan.type == SpanType.Brace)
        {
            if (text == OpeningBracketSpan.Symbol)
                span = new OpeningBracketSpan();
            else if (text == ClosingBracketSpan.Symbol)
                span = new ClosingBracketSpan();
            else if (text == ArrayOpenerSpan.Symbol)
                span = new ArrayOpenerSpan();
            else if (text == ArrayCloserSpan.Symbol)
                span = new ArrayCloserSpan();
            else if (text == SourceOpenerSpan.Symbol)
                span = new SourceOpenerSpan();
            else if (text == SourceCloserSpan.Symbol)
                span = new SourceCloserSpan();
        }
        else if (textSpan.type == SpanType.Comment)
            span = new CommentSpan(text[2..]);
        else if (textSpan.type == SpanType.MultiLineComment)
            span = new CommentSpan(text[2..^2], multiLine: true);
        else if (textSpan.type == SpanType.Space)
            span = new WhiteSpaceSpan(text);

        span ??= new WordSpan(text);

        span.Document = textSpan.Doc;
        span.DocumentLocation = textSpan.location;

        readValue_codeRecord = record_bu ?? readValue_codeRecord;
        if (recording && !spoiler) readValue_codeRecord?.Add(span);

        readOps_codeRecord = recordOps_bu ?? readOps_codeRecord;
        if (recordingOps && !spoiler) readOps_codeRecord?.Add(span);

        lastSpan = span;
        return span;
    }
}