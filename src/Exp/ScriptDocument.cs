using System;
using System.Linq;
using System.Collections.Generic;
using Exp.Spans;
using Exp.Operations;

namespace Exp;

public class ScriptDocument
{
    public static HashSet<string> BuildConstants { get; } = ["DEBUG"];

    static ScriptDocument()
    {
        DefinePlatformBuildConstants();
    }

    private static void DefinePlatformBuildConstants()
    {
        if (OperatingSystem.IsWindows())
            BuildConstants.Add("WINDOWS");
        else if (OperatingSystem.IsMacOS())
            BuildConstants.Add("MACOS");
        else if (OperatingSystem.IsMacCatalyst())
            BuildConstants.Add("MACCATALYST");
        else if (OperatingSystem.IsLinux())
            BuildConstants.Add("LINUX");
        else if (OperatingSystem.IsAndroid())
            BuildConstants.Add("ANDROID");
        else if (OperatingSystem.IsIOS())
            BuildConstants.Add("IOS");
        else if (OperatingSystem.IsBrowser())
            BuildConstants.Add("BROWSER");

        if (OperatingSystem.IsWindows() || OperatingSystem.IsMacCatalyst() || OperatingSystem.IsMacOS() || OperatingSystem.IsLinux())
            BuildConstants.Add("DESKTOP");
        else if (OperatingSystem.IsAndroid() || OperatingSystem.IsIOS())
            BuildConstants.Add("MOBILE");
    }

    public HashSet<ExpError> SettingsErrors { get; } = [];
    public string? Description { get; set; }
    public string Name { get; set; }
    public string Script { get; internal set; }
    public TextSpan[] TextSpans { get; private set; }
    internal Span[] CodeSpans { get; private protected set; }
    public bool IsPrepared => CodeSpans != null;
    public HashSet<string> Usings { get; } = [];
    internal TextSpan[] UsingSpans { get; private set; }
    public string? Namespace { get; set; }
    internal IOperation[] Operations { get; set; }

    protected ScriptDocument(string script, string name)
    {
        ArgumentNullException.ThrowIfNull(script);

        this.Name = name;
        this.Script = script;

        TextSpans = Spanner.GetTextSpans(this.Script);
        var (updated, errors) = RemoveDisabledCode();
        TextSpans = updated;
        SettingsErrors.AddRange(errors);

        ReadDocSettings();
        foreach (var span in TextSpans)
            span.Document = this;
    }

    private void ReadDocSettings()
    {
        ReadDocSettings(Name, TextSpans, out var updatedTextSpans, out var description, out var @namespace, out var usings, out var settingsErrors, out var usingSpans);
        (this.TextSpans, this.Description, this.Namespace) = (updatedTextSpans, description, @namespace);
        this.Usings.AddRange(usings);
        this.SettingsErrors.AddRange(settingsErrors);
        this.UsingSpans = [.. usingSpans];
    }

    public static void ReadDocSettings(string Name, TextSpan[] TextSpans, out TextSpan[] updatedTextSpans, out string? Description, out string? Namespace, out HashSet<string> Usings, out HashSet<ExpError> SettingsErrors, out List<TextSpan> usingSpans)
    {
        updatedTextSpans = TextSpans;
        Description = null;
        Namespace = null;
        Usings = [];
        SettingsErrors = [];
        usingSpans = [];


        int spanIndex = 0, line = 0, col = 1;
        TextSpan? NextSpan()
        {
            if (spanIndex >= TextSpans.Length)
                return null;

            TextSpan span = TextSpans[spanIndex];

            // skip spaces
            while (span.type == SpanType.Space)
            {
                if (span.text == "\n")
                {
                    line++;
                    col = 1;
                }
                else
                {
                    col += span.text.Length;
                }

                spanIndex++;
                return NextSpan();
            }

            return TextSpans[spanIndex++];
        }

        bool anySettingsRead = false;

        // read doc description
        var next = NextSpan();
        if (next?.type == SpanType.Comment && next.text.StartsWith('/'))
        {
            Description = next.text[3..].Trim();
            anySettingsRead = true;
            next = NextSpan();
        }

        // read usings
        while (next?.text == UsingWordSpan.Keyword)
        {
            anySettingsRead = true;
            var nsNameSpan = NextSpan();
            string? use = nsNameSpan?.text;
            if (use == null)
                SettingsErrors.Add(new(Name, line, col, "Namespace name expected."));
            else if (!use.IsLiterallyValidName())
                SettingsErrors.Add(new(Name, line, col, $"Invalid namespace name '{use}'."));
            else
            {
                if (!Usings.Add(use))
                    SettingsErrors.Add(new(Name, line, col, $"Namespace '{use}' is already imported."));
                else
                    usingSpans.Add(nsNameSpan);
            }
            next = NextSpan();
        }

        // read namespace
        if (next?.text == NamespaceWordSpan.Keyword)
        {
            anySettingsRead = true;
            string? ns = NextSpan()?.text;
            if (ns == null)
                SettingsErrors.Add(new(Name, line, col, "Namespace name expected."));
            else if (!ns.IsLiterallyValidName())
                SettingsErrors.Add(new(Name, line, col, $"Invalid namespace name '{ns}'."));
            else
            {
                if (Namespace != null)
                    SettingsErrors.Add(new(Name, line, col, $"Namespace is already declared as '{Namespace}'."));
                else
                    Namespace = ns;
            }

            if (NextSpan()?.text != ":")
            {
                SettingsErrors.Add(new(Name, line, col, "':' expected after namespace declaration."));
            }
        }

        if (anySettingsRead)
            updatedTextSpans = TextSpans[(spanIndex - 1)..];
    }

    private (TextSpan[] updated, ExpError[] errors) RemoveDisabledCode()
    {
        List<TextSpan> code = [];
        List<ExpError> errors = [];
        bool enabled = true, insideElse = false;
        Stack<bool> openedConditions = [];
        int line = 1, col = 1;
        bool lineContainsVisibleText = false;
        foreach (TextSpan span in TextSpans)
        {
            if (span.type == SpanType.PreprocessorDirective)
            {
                if (lineContainsVisibleText)
                    Error("preprocessor directives must appear as the first non-whitespace character on a line", false);

                bool isInElseIfCheck = false;
                var keyword = ReadKeyword(span.text);
                if (keyword == null)
                {
                    Error($"Invalid or missing preprocessor keyword");
                }
                else
                {
                IfKeyword:
                    if (enabled && (keyword == PreprocessorKeywords.If || isInElseIfCheck))
                    {
                        PreprocessorConditionNode? condition = ReadCondition(span.text);
                        if (condition == null)
                            Error($"constant expected");
                        else
                        {
                            enabled = false;

                            while (condition != null)
                            {
                                bool contains = BuildConstants.Contains(condition.Value);
                                if (condition.Not)
                                    contains = !contains;

                                if (contains)
                                {
                                    if (condition.Operator == PreprocessorKeywords.Or || condition.Next == null)
                                    {
                                        enabled = true;
                                        break;
                                    }
                                }
                                else
                                {
                                    if (condition.Operator == PreprocessorKeywords.And || condition.Next == null)
                                    {
                                        break;
                                    }
                                }
                                condition = condition.Next;
                            }

                            if (!isInElseIfCheck)
                                openedConditions.Push(enabled);
                        }
                    }
                    else if (keyword == PreprocessorKeywords.Else || keyword == PreprocessorKeywords.ElseIf)
                    {
                        if (openedConditions.Count >= 1)
                        {
                            enabled = !enabled;
                            if (keyword == PreprocessorKeywords.ElseIf)
                            {
                                if (enabled) // note: we just had enabled = !enabled
                                {
                                    isInElseIfCheck = true;
                                    goto IfKeyword;
                                }
                            }
                        }
                        else
                            Error($"unexpected {nameof(PreprocessorKeywords.Else)}");
                    }
                    else if (keyword == PreprocessorKeywords.EndIf)
                    {
                        if (openedConditions.Count >= 1)
                        {
                            openedConditions.Pop();
                            enabled = openedConditions.Count == 0 || openedConditions.Last();
                        }
                        else
                            Error($"unexpected {nameof(PreprocessorKeywords.EndIf)}");
                    }
                }
            }
            else
            {
                if (span.type != SpanType.Space)
                    lineContainsVisibleText = true;

                if (enabled)
                {
                    code.Add(span);
                }
            }

            int lines = span.text.CountOf('\n');
            if (lines >= 1)
            {
                line += lines;
                col = span.text.Length - span.text.LastIndexOf('\n');
                if (col == 1 || string.IsNullOrWhiteSpace(span.text.Substring(span.text.LastIndexOf('\n'))))
                    lineContainsVisibleText = false;
            }
            else
                col += span.text.Length;
        }

        foreach (bool _ in openedConditions)
            Error($"missing #{PreprocessorKeywords.EndIf}");

        return ([.. code], [.. errors]);

        static PreprocessorKeywords? ReadKeyword(string directive)
        {
            if (directive.Length < 2)
                return null;

            string keyword = directive.Contains(' ') ? directive.Substring(1, directive.IndexOf(' ') - 1) : directive.Substring(1);

            string[] allKeywords = Enum.GetNames<PreprocessorKeywords>();
            int keywordIndex = allKeywords.IndexOf(keyword);
            return keywordIndex < 0 ? null : Enum.GetValues<PreprocessorKeywords>()[keywordIndex];
        }

        PreprocessorConditionNode? ReadCondition(string directive)
        {
            PreprocessorConditionNode? first = null, current = null;
            int startIndex = directive.IndexOf(' ');
            if (startIndex < 0)
                return null;

            int i = startIndex + 1;
            bool not = false, operatorExpected = false;
        ReadWord:
            string word = "";
            for (; i < directive.Length; i++)
            {
                if (directive[i] == ' ')
                {
                    if (word.Length > 0)
                        break;
                    continue;
                }

                word += directive[i];
            }

            if (word.Length == 0)
            {
                if (not)
                    Error("constant expected");
                return first;
            }
            if (operatorExpected)
            {
                if (word == nameof(PreprocessorKeywords.And))
                    current!.Operator = PreprocessorKeywords.And;
                else if (word == nameof(PreprocessorKeywords.Or))
                    current!.Operator = PreprocessorKeywords.Or;
                else
                {
                    Error($"operator keyword ({nameof(PreprocessorKeywords.And)}/{nameof(PreprocessorKeywords.Or)}) expected");
                    return first;
                }
                operatorExpected = false;
                goto ReadWord;
            }
            if (word == nameof(PreprocessorConditionNode.Not))
            {
                if (not)
                    Error($"duplicate {nameof(PreprocessorConditionNode.Not)} keyword.");
                not = true;
                goto ReadWord;
            }

            if (current == null)
                first = current = new(word, not);
            else
            {
                current.Next = new(word, not);
                current = current.Next;
            }

            operatorExpected = true;
            not = false;
            goto ReadWord;
        }

        void Error(string msg, bool extendedMsg = true)
        {
            string err = extendedMsg ? $"Invalid preprocessor directive ({msg})." : msg;
            errors.Add(new(Name, line, col, err));
        }
    }

    private record PreprocessorConditionNode(string Value, bool Not)
    {
        public PreprocessorConditionNode? Next { get; set; }
        public PreprocessorKeywords Operator { get; set; }
    }

    public virtual bool TryPrepare(Interpreter compiler, out ExpError[] errors)
    {
        ArgumentNullException.ThrowIfNull(compiler);

        var errorsBefore = compiler.Errors.ToArray();

        CodeSpans = compiler.GetCodeSpans(TextSpans);
        Operations = compiler.ReadOperations(CodeSpans, null);

        errors = compiler.Errors.ToArray().Remove(err => errorsBefore.Contains(err)).AppendRange(SettingsErrors).ToArray();
        return errors.Length == 0;
    }

    public virtual void Run(Interpreter compiler)
    {
        ArgumentNullException.ThrowIfNull(compiler);
        compiler.Run(this);
    }

    public bool ContainsCode => CodeSpans?.Any(s => s is not UsingWordSpan and not NamespaceWordSpan and not CommentSpan and not WhiteSpaceSpan) ?? true;

    public static ScriptDocument FromString(string script, string name)
    {
        return new ScriptDocument(script, name);
    }

    public static ScriptDocument FromFile(string path)
    {
        char endd = '\\';
#if ANDROID
        endd = '/';
#endif

        return new ScriptDocument(File.ReadAllText(path), path.Contains(endd) ? path[(path.LastIndexOf(endd) + 1)..] : path);
    }

    public static ScriptDocument[] FromFiles(string[] paths)
    {
        var docs = new ScriptDocument[paths.Length];
        for (int i = 0; i < paths.Length; i++)
            docs[i] = FromFile(paths[i]);
        return docs;
    }

    public override string ToString() => Name;
}

public class InstanceScriptDocument(string name, ClassDefSpan def, string script, params string[] args) : ScriptDocument(script, name)
{
    public ClassDefSpan Def { get; set; } = def;
    internal FuncDefSpan? Runner { get; private set; }
    public string[] Args => args;

    public override bool TryPrepare(Interpreter compiler, out ExpError[] errors)
    {
        ArgumentNullException.ThrowIfNull(compiler);

        var errorsBefore = compiler.Errors.ToArray();

        CodeSpans = compiler.GetCodeSpans(TextSpans);
        var argSpans = args.Map(a => new ArgumentSpan(a));
        Runner = new FuncDefSpan(Name + ".runner", [..argSpans], CodeSpans, Def) { Static = false, Document = this };
        Def.Funcs = Def.Funcs.Append(Runner).ToArray();
        Runner.Operations = compiler.ReadOperations(CodeSpans, Runner);

        errors = compiler.Errors.ToArray().Remove(err => errorsBefore.Contains(err)).ToArray();
        return errors.Length == 0;
    }

    public void Run(Interpreter compiler, Exp.Instance inst, params IValue?[] args)
    {
        if (Runner == null)
        {
            TryPrepare(compiler, out var errors);
            if (errors.Length >= 1)
                throw new BuildFailureException(errors);
        }

        compiler.RunOpsRunning = true;
        compiler.FuncCall(inst, Runner!, null, out bool _, args);
        compiler.RunOpsRunning = false;
    }
}

public interface ILocatableSourceMark
{
    ScriptDocument Document { get; }
    int DocumentLocation { get; }
}

enum PreprocessorKeywords
{
    If,
    Else,
    ElseIf,
    EndIf,

    Not,
    And,
    Or
}