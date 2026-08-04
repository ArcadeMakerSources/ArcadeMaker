using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcadeMaker.IDE;
using IntelliSense.CSharp;

namespace IntelliSense
{
    public static class CodeChecker
    {
        public static CodeSpan[] GetSpans(string code)
        {
            List<CodeSpan> spans = new List<CodeSpan>();

            CodeSpan span = new CodeSpan();
            string spanText = "";
            int length = code.Length;
            bool first = false;
            int ignore = 0; // 0: do not ignore, 1: ignore next, 2: ignore this

            // run over each char in the text
            int insideFormattedStringLength = 0;
            for (int i = 0; i < code.Length; i++)
            {
                char c = code[i];
                bool nextSpan = false;
                bool nextSpanInsideFormattedString = span.insideFormattedString;
                if (span.insideFormattedString)
                    insideFormattedStringLength++;

                if (ignore < 2 && spanText.Length == 0)
                {
                    first = true;

                    if (span.type != SpanType.FormattedString)
                    {
                        // define span type
                        switch (c)
                        {
                            case ' ':
                            case '\n':
                            case '\t':
                                span.type = SpanType.Space;
                                break;
                            case '(':
                            case ')':
                            case '{':
                            case '}':
                            case '[':
                            case ']':
                                span.type = SpanType.Brace;
                                break;
                            case '@':
                            case '$':
                                if (i < length - 1 && code[i + 1] == '\"')
                                {
                                    span.type = c == '@' ? SpanType.EscapedString : SpanType.FormattedString;
                                    ignore = 1;
                                }
                                break;
                            case '\"':
                                span.type = SpanType.String;
                                break;
                            case '\'':
                                span.type = SpanType.Char;
                                break;
                            case '/':
                                if (i < length - 1)
                                {
                                    char nextChar = code[i + 1];
                                    if (nextChar == '/')
                                    {
                                        span.type = SpanType.Comment;
                                        ignore = 1;
                                    }
                                    else if (nextChar == '*')
                                    {
                                        span.type = SpanType.MultiLineComment;
                                        ignore = 1;
                                    }
                                }
                                break;
                            default:
                                if (c >= '0' && c <= '9')
                                {
                                    span.type = SpanType.Number;
                                }
                                else if (char.IsSymbol(c) && c != '_')
                                {
                                    span.type = SpanType.Symbol;
                                }
                                break;
                        }
                    }
                }
                else
                    first = false;

                spanText += c;

                if (ignore < 2 && (span.type == SpanType.FormattedString || !first))
                {
                    // check if current char is a seperator
                    bool isSep = false;
                    switch (span.type)
                    {
                        case SpanType.Space:
                            isSep = c != ' ' && c != '\n';
                            if (isSep)
                                nextSpan = true;
                            break;
                        case SpanType.Normal:
                            isSep = !char.IsLetterOrDigit(c) && c != '_';
                            if (isSep)
                                nextSpan = true;
                            break;
                        case SpanType.Number:
                            isSep = (c < '0' || c > '9') && !(c == '.' && spanText.CountOf('.') == 1 && i < length - 1 && code[i + 1] >= '0' && code[i + 1] <= '9');
                            if (isSep)
                                nextSpan = true;
                            break;
                        case SpanType.Symbol:
                            isSep = CSharpFilter.Operators.Contains(spanText + c);
                            if (isSep)
                                nextSpan = true;
                            break;
                        case SpanType.Brace:
                            isSep = true;
                            nextSpan = true;
                            break;
                        case SpanType.String:
                        case SpanType.Char:
                        case SpanType.FormattedString:
                            if (c == '\n')
                            {
                                isSep = true;
                                break;
                            }
                            if (c == ((span.type == SpanType.String || span.type == SpanType.FormattedString) ? '\"' : '\''))
                            {
                                // check if quot is escaped
                                bool esc = false;
                                int left = 1;
                                while (i >= left && code[i - left++] == '\\')
                                {
                                    esc = !esc;
                                }
                                if (!esc)
                                {
                                    isSep = true;
                                }
                            }
                            else if (span.type == SpanType.FormattedString)
                            {
                                if (code.Length > i + 1 && code[i + 1] == '{')
                                {
                                    int lefts = 0;
                                    int left = 0;
                                    while (code[i - left++] == '{')
                                        lefts++;
                                    if (code.Length > i + 2 && code[i + 2] != '{' && lefts % 2 == 0)
                                    {
                                        isSep = true;
                                        nextSpanInsideFormattedString = true;
                                    }
                                }
                                /*
                                if (text.Length > i + 1 && text[i + 1] == '{' && c != '{')
                                {
                                    // check if brace is escaped
                                    bool esc = false;
                                    int right = 2;
                                    while (text.Length > i + right && text[i + right++] == '{')
                                    {
                                        esc = !esc;
                                    }
                                    if (!esc)
                                    {
                                        isSep = true;
                                        nextSpanInsideFormattedString = true;
                                    }
                                }
                                */
                            }
                            break;
                        case SpanType.EscapedString:
                            isSep = c == '\"';
                            break;
                        case SpanType.Comment:
                            isSep = c == '\n';
                            break;
                        case SpanType.MultiLineComment:
                            isSep = c == '/' && i > 0 && code[i - 1] == '*';
                            break;
                    }
                    if (span.insideFormattedString)
                    {
                        // it will be the end of the span if the current char is '}' and there wasn't a '{' relating to this
                        if (c == '}')
                        {
                            int numberOfOpenersRequired = 1;
                            for (int left = 1; left <= insideFormattedStringLength; left++)
                            {
                                char leftC = code[i - left];
                                if (leftC == '}')
                                    numberOfOpenersRequired++;
                                else if (leftC == '{')
                                    numberOfOpenersRequired--;
                            }
                            if (numberOfOpenersRequired == 0)
                            {
                                isSep = true;
                                nextSpan = false;
                                nextSpanInsideFormattedString = false;
                                insideFormattedStringLength = 0;
                            }
                        }
                    }
                    if (isSep)
                    {
                        // prepare to next span
                        if (nextSpan)
                        {
                            spanText = spanText.Substring(0, spanText.Length - 1);
                            i--;
                        }
                        span.text = spanText;
                        spanText = "";
                        spans.Add(span);
                        bool nextSpanFormattedStringContinue = span.insideFormattedString && !nextSpanInsideFormattedString;
                        span = new CodeSpan { insideFormattedString = nextSpanInsideFormattedString };
                        if (nextSpanFormattedStringContinue)
                            span.type = SpanType.FormattedString;
                    }
                }
                if (ignore == 1)
                    ignore = 2;
                else if (ignore == 2)
                    ignore = 0;
            }

            // insert unclosed span, if exists
            if (spanText.Length > 0)
            {
                span.text = spanText;
                spans.Add(span);
            }
            spans.RemoveAll(s => s.type == SpanType.Space);
            return spans.ToArray();
        }

        public static CodeSpan[] ReadBlock(CodeSpan[] spans, int start, out List<CodeSpan> outOfBlock, bool mustHaveBlock = true)
        {
            List<CodeSpan> block = new List<CodeSpan>();
            outOfBlock = new List<CodeSpan>();
            bool firstOpenerFound = false;
            int openers = 0, closers = 0;
            for (int i = start; i < spans.Length; i++)
            {
                CodeSpan span = spans[i];
                if (!firstOpenerFound)
                {
                    if (!(span.type == SpanType.Brace && span.text == "{"))
                        outOfBlock.Add(span);
                    else
                        firstOpenerFound = true;
                }
                else
                {
                    if (span.type == SpanType.Brace && span.text == "}")
                    {
                        openers++;
                        block.Add(span);
                    }
                    else if (span.type == SpanType.Brace && span.text == "}")
                    {
                        if (closers == openers)
                            break;
                        closers++;
                        block.Add(span);
                    }
                    else
                    {
                        block.Add(span);
                    }
                }
            }
            if (!firstOpenerFound && mustHaveBlock)
                throw new OpeningCurlyBraceExpected();
            if (closers != openers)
                throw new ClosingCurlyBraceExpected();
            return block.ToArray();
        }

        public static CSItem Build(CSItem context, CodeSpan[] spans)
        {
            for (int spanIndex = 0; spanIndex < spans.Length; spanIndex++)
            {
                CodeSpan span = spans[spanIndex];

                if (context is Namespace ns)
                {
                    // set namespace name
                    if (span.type == SpanType.Normal)
                        ns.Name = span.text;
                    else
                        throw new IdentifierExpectedException();

                    // set namespace items
                    CodeSpan[] nsSpans = ReadBlock(spans, ++spanIndex, out List<CodeSpan> beforeSpans);
                    if (beforeSpans.Any())
                        throw new NameDoesNotExistsInCurrentContextException(beforeSpans.First().text);
                    spanIndex += nsSpans.Length + 2; // +2 for 2 braces
                    List<NamespaceMember> items = new List<NamespaceMember>();
                    NamespaceMember item = new NamespaceMember();

                    bool @partial = false, @static = false;

                    for (int i = 0; i < nsSpans.Length; i++)
                    {
                        if (span.type != SpanType.Normal)
                            throw new NotImplementedException();

                        if (span.text == "public")
                            item.AccessModifier = AccessModifier.Public;
                        else if (span.text == "internal")
                            item.AccessModifier = AccessModifier.Internal;
                        else if (span.text == "static")
                            @static = true;
                        else if (span.text == "partial")
                            partial = true;
                        else if (span.text == "class")
                        {
                            Class @class = item as Class;
                            @class.Static = @static;
                            @class.Partial = partial;

                            List<CodeSpan> classSpans = ReadBlock(nsSpans, i + 1, out beforeSpans).ToList();
                            classSpans.InsertRange(0, beforeSpans);
                            i += classSpans.Count + 2; // +2 for 2 braces
                            Build(@class, classSpans.ToArray());
                            items.Add(item);
                            item = new NamespaceMember();
                        }
                        else
                        {
                            throw new NameDoesNotExistsInCurrentContextException(span.text);
                        }
                    }
                }
                else if (context is Class c)
                {
                    // read generic types
                    if (c.Name != null && span.type == SpanType.Brace && span.text == "<")
                    {
                        bool splitExpected = false;
                        do
                        {
                            if (!splitExpected)
                            {
                                // read generic type name
                                span = spans[spanIndex++];
                                if (span.type != SpanType.Normal)
                                    throw new NotImplementedException();
                                c.GenericTypes.Add(new GenericType { Name = span.text });
                                splitExpected = true;
                            }
                            else
                            {
                                // make sure we got a ','
                                if (span.type == SpanType.Symbol && span.text == ",")
                                    splitExpected = false;
                                else
                                    throw new NotImplementedException();
                            }
                        }
                        while (!(span.type == SpanType.Brace && span.text == ">"));
                        if (!splitExpected)
                            throw new IdentifierExpectedException(); // looks like this: class Name<T,>
                        continue;
                    }

                    // set class name
                    else if (span.type == SpanType.Normal)
                        c.Name = span.text;
                    else
                        throw new IdentifierExpectedException();

                    // set class members (!!! WRONG (ReadBlock(...) already done) !!!)
                    CodeSpan[] cSpans = ReadBlock(spans, spanIndex, out List<CodeSpan> beforeSpans);

                    // read extends

                    spanIndex += cSpans.Length + 2; // +2 for 2 braces
                    List<ClassMember> members = new List<ClassMember>();
                    ClassMember item = new ClassMember();
                    
                    bool @readonly = false, @const = false;
                    string type = null, name = null;

                    for (int i = 0; i < cSpans.Length; i++)
                    {
                        if (span.type != SpanType.Normal)
                            throw new NotImplementedException();

                        if (span.text == "public")
                            item.AccessModifier = AccessModifier.Public;
                        else if (span.text == "internal")
                            item.AccessModifier = AccessModifier.Internal;
                        else if (span.text == "static")
                            item.Static = true;
                        else if (span.text == "readonly")
                            @readonly = true;
                        else if (span.text == "const")
                            @const = true;
                        else if (span.text == "(")
                        {
                            Method method = item as Method;
                            if (@readonly || @const)
                                throw new UnvalidModifierException(@readonly ? "readonly" : "const");

                            // read generic types
                            if (span.type == SpanType.Brace && span.text == "<")
                            {
                                bool splitExpected = false;
                                do
                                {
                                    if (!splitExpected)
                                    {
                                        // read generic type name
                                        span = spans[spanIndex++];
                                        if (span.type != SpanType.Normal)
                                            throw new NotImplementedException();
                                        method.GenericTypes.Add(new GenericType { Name = span.text });
                                        splitExpected = true;
                                    }
                                    else
                                    {
                                        // make sure we got a ','
                                        if (span.type == SpanType.Symbol && span.text == ",")
                                            splitExpected = false;
                                        else
                                            throw new NotImplementedException();
                                    }
                                }
                                while (!(span.type == SpanType.Brace && span.text == ">"));
                                if (!splitExpected)
                                    throw new IdentifierExpectedException(); // looks like this: Type Name<T,> ...
                            }

                            // read params

                            List<CodeSpan> methodSpans = ReadBlock(cSpans, i + 1, out beforeSpans).ToList();
                            methodSpans.InsertRange(0, beforeSpans);
                            i += methodSpans.Count + 2; // +2 for 2 braces
                            Build(method, methodSpans.ToArray());
                            
                            item = new ClassMember();
                        }
                        else
                        {
                            item.Type = span.text;
                        }
                    }
                }
            }

            return context;
        }
    }

    namespace CSharp
    {
        public class CSItem { }

        public enum AccessModifier
        {
            Private,
            Public,
            Protected,
            Internal
        }

        public class NamespaceMember : CSItem
        {
            public string Name;
            public AccessModifier AccessModifier;
        }

        public class ClassMember : CSItem
        {
            public AccessModifier AccessModifier = AccessModifier.Private;
            public bool Static;
            public string Name;
            public string Type;
        }

        public class Field : ClassMember
        {
            public bool Readonly, Const;
            public Value Value = null;
        }

        public class Property : ClassMember
        {
            public bool Readonly, Const, Abstract, Virtual;
            public bool Get, Set;
            public Value Value = null;
        }

        public class MethodParameter : CSItem
        {
            public string Name, Type;
            public bool Ref, Out, HasDefaultValue;
            public Value DefaultValue = null;
        }

        public class Value : CSItem
        {
            public string Type;
        }

        public class BoolValue : Value
        {
            public BoolValue()
            {
                base.Type = "bool";
            }
        }

        public class StringValue : Value
        {
            public StringValue()
            {
                base.Type = "string";
            }
        }

        public class DecimalValue : Value
        {
            public DecimalValue()
            {
                base.Type = "decimal";
            }
        }

        public class LocalVariable : CSItem
        {
            public string Name, Type;
            public Value Value = null;
        }

        public class Operation : CSItem
        {

        }

        public class LocalVariableDeclarationOperation : Operation
        {
            public LocalVariable Variable = null;
        }

        public enum CompareType
        {
            Set,
            Plus,
            Minus,
            Multiply,
            Divide
        }

        public class CompareOperation : Operation
        {
            public CompareType Operation;
            public LocalVariable Variable;
            public Value Value;
        }

        public class ActivateActionOperation : Operation
        {
            public Method Method;
            public readonly List<Value> Values = new List<Value>();
        }

        public class ReturnOperation : Operation
        {
            public Value Value = null;
        }
        
        public class BreakOperation : Operation
        {

        }

        public class ContinueOperation : Operation
        {
            
        }

        public class ControlStructure : Operation
        {
            public readonly List<Operation> Operations = new List<Operation>();
        }

        public class If : ControlStructure
        {
            public BoolValue Condition;
        }

        public class While : ControlStructure
        {
            public BoolValue Condition;
        }

        public class For : ControlStructure
        {
            public LocalVariableDeclarationOperation VariableDeclaration = null;
            public BoolValue Condition;
            public CompareOperation Operation = null;
        }

        public class ForEach : ControlStructure
        {
            public LocalVariable Variable;
            public Value Enumerator;
        }

        public class Try : ControlStructure
        {
            public Catch Catch;
            public Finally Finally;
        }

        public class Catch : ControlStructure
        {
            public LocalVariableDeclarationOperation Exception = null;
            public Catch(LocalVariableDeclarationOperation Exception = null)
            {
                if (Exception != null)
                {
                    if (!(Exception.Variable.Type == "Exception"))
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        this.Exception = Exception;
                    }
                }
            }
        }

        public class Finally : ControlStructure
        {

        }

        public class Using : ControlStructure
        {
            public LocalVariableDeclarationOperation DeclarationOperation;
        }
        
        public class Method : ClassMember
        {
            public bool Abstract, Virtual;
            public readonly List<GenericType> GenericTypes = new List<GenericType>();
            public readonly List<MethodParameter> Parameters = new List<MethodParameter>();
            public readonly List<Operation> Operations = new List<Operation>();
        }

        public class Namespace : CSItem
        {
            public string Name;
            public readonly List<NamespaceMember> Items = new List<NamespaceMember>();
        }

        public class GenericType : CSItem
        {
            public string Name;
        }

        public class Class : NamespaceMember
        {
            public bool Static, Partial;
            public readonly List<GenericType> GenericTypes = new List<GenericType>();
            public readonly List<ClassMember> Members = new List<ClassMember>();
        }
    }

    public class CodeSpan
    {
        public string text = "";
        public SpanType type = SpanType.Normal;
        public bool insideFormattedString = false;
    }

    public enum SpanType
    {
        Space,
        Normal,
        Number,
        Symbol,
        Brace,
        String,
        Char,
        EscapedString,
        FormattedString,
        Comment,
        MultiLineComment,
    }

    public class BuildException : Exception
    {
        public readonly string code = null;
        public BuildException(string code, string message = null) : base(message)
        {
            this.code = code;
        }
    }

    public class IdentifierExpectedException : BuildException
    {
        public IdentifierExpectedException() : base("CS1001", "Identifier Expected")
        {

        }
    }

    public class OpeningCurlyBraceExpected : BuildException
    {
        public OpeningCurlyBraceExpected() : base("CS1514", "{ expected")
        {

        }
    }

    public class ClosingCurlyBraceExpected : BuildException
    {
        public ClosingCurlyBraceExpected() : base("CS1513", "} expected")
        {

        }
    }

    public class NameDoesNotExistsInCurrentContextException : BuildException
    {
        public NameDoesNotExistsInCurrentContextException(string name) : base("CS1007", "The name '" + name + "' does not exist in the current context.")
        {

        }
    }

    public class UnvalidModifierException : BuildException
    {
        public UnvalidModifierException(string modifier) : base("CS0106", "The modifier '" + modifier + "' is not valid for this item")
        {

        }
    }

    public static class CSharpFilter
    {

        public static readonly string[] Operators = new string[]
        {
            "++", "--", "+=", "-=", "/=", "*=", "==", "!=", "^=", "%=", "&=", "|=", "<<=", "&&", "||"
        };

        public static string[] keywords = new string[] {
                      "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked",
                      "class", "const", "continue", "decimal", "default", "delegate", "do", "double", "else",
                      "enum", "event", "explicit", "extern", "false", "finally", "fixed", "float", "for", "foreach",
                      "goto", "if", "implicit", "in", "int", "interface", "internal", "is", "lock", "long", "namespace",
                      "new", "null", "object", "operator", "out", "override", "params", "private", "protected",
                      "public", "readonly", "ref", "return", "sbyte", "sealed", "short", "sizeof", "stackalloc", "partial",
                      "static", "string", "struct", "switch", "this", "throw", "true", "try", "typeof", "uint",
                      "ulong", "unchecked", "unsafe", "ushort", "using", "virtual", "void", "volatile", "while"
        };

    }
}
