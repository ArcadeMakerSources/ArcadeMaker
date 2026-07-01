using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exp.Spans;

namespace Exp.Spans;

public static class Spanner
{
    public static TextSpan[] GetTextSpans(string text, string replaceTabSpaceWith = null, bool splitMultiCommentsLines = false)
    {
        List<TextSpan> spans = new List<TextSpan>();

        TextSpan span = new TextSpan(0);
        string spanText = "";
        int length = text.Length;
        bool first = false;
        int ignore = 0; // 0: do not ignore; 1: ignore next; 2: ignore this

        // run over each char in the text
        int insideFormattedStringLength = 0;
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (c == '\t' && replaceTabSpaceWith != null)
            {
                text = text.Remove(i, 1).Insert(i, replaceTabSpaceWith);
                length += replaceTabSpaceWith.Length - 1;
                i--;
                continue;
            }
            bool nextSpan = false;
            bool nextSpanInsideFormattedString = span.insideFormattedString;
            if (span.insideFormattedString)
                insideFormattedStringLength++;

            bool isSep = false;

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
                        case '.':
                            if (i < length - 1 && text[i + 1] == '.')
                            {
                                span.type = SpanType.Count;
                            }
                            else goto case ':';
                            break;
                        case ',':
                        case ';':
                        case ':':
                            span.type = SpanType.DotCom;
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
                            if (i < length - 1 && text[i + 1] == '\"')
                            {
                                span.type = c == '@' ? SpanType.EscapedString : SpanType.FormattedString;
                                ignore = 1;
                                // as next char is ignored, there won't be a check for '{' at the first char of the formatted string,
                                // so we'll check it now:
                                // if this span starts like this: $"{
                                if (span.type == SpanType.FormattedString && i + 2 < length && text[i + 2] == '{')
                                {
                                    // if the '{' is not escaped
                                    if (i + 3 >= length || text[i + 3] != '{')
                                    {
                                        c = '\"';
                                        i++;
                                        isSep = true;
                                        spanText = "$";
                                        nextSpanInsideFormattedString = true;
                                    }
                                }
                            }
                            else
                                span.type = SpanType.Tag;
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
                                char nextChar = text[i + 1];
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
                            if (span.type == SpanType.Normal)
                                span.type = SpanType.Symbol;
                            break;
                        case '?':
                        case '!':
                            span.type = SpanType.Symbol;
                            break;
                        default:
                            bool minusNumber = c == '-' && i + 1 < text.Length && text[i + 1] >= '0' && text[i + 1] <= '9';
                            if ((c >= '0' && c <= '9') || minusNumber)
                            {
                                span.type = SpanType.Number;
                            }
                            else if ((char.IsSymbol(c) && c != '_') || c == '!' || c == '&' || c == '-' || c == '*' || c == '%')
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
                switch (span.type)
                {
                    case SpanType.Space:
                        isSep = c != ' ' && c != '\n';
                        if (isSep)
                            nextSpan = true;
                        break;
                    case SpanType.DotCom:
                        if (c == ':')
                        {
                            isSep = spanText.Length > 0;
                            nextSpan = spanText != NamespaceSpecificationSpan.Symbol;
                        }
                        else
                        {
                            isSep = true;
                            nextSpan = true;
                        }
                        break;
                    case SpanType.Normal:
                        isSep = !char.IsLetterOrDigit(c) && c != '_';
                        if (isSep)
                            nextSpan = true;
                        break;
                    case SpanType.Number:
                        bool isHex = spanText.Length >= 2 && spanText[0] == '0' && (spanText[1] == 'x' || spanText[1] == 'X');
                        if (isHex)
                            isSep = (c < '0' || c > '9') && !(spanText.Length == 2 && (c == 'x' || c == 'X')) && (c < 'a' || c > 'f') && (c < 'A' || c > 'F');
                        else
                            isSep = (c < '0' || c > '9') && !(c == '.' && spanText.CountOf('.') == 1 && i < length - 1 && text[i + 1] >= '0' && text[i + 1] <= '9');
                        if (isSep && span.type != SpanType.Count)
                        {
                            nextSpan = !(c == 'F' || c == 'f'
                                       || c == 'D' || c == 'd'
                                       || c == 'M' || c == 'm'
                                       || (!spanText.Contains('.') && (c == 'u' || c == 'U' || c == 'L' || c == 'l')));

                            // take care of case of UL suffix
                            char lastChar = spanText[spanText.Length - 2];
                            bool lastCharIsNum = lastChar >= '0' && lastChar <= '9';
                            char nextChar = i + 1 < text.Length ? text[i + 1] : (char)0;
                            bool uSuffix = c == 'U' || c == 'u', lSuffix = c == 'L' || c == 'l';
                            if (!nextSpan && (uSuffix || lSuffix) && lastCharIsNum && nextChar.EqualsIgnoreCase(uSuffix ? 'L' : 'U'))
                            {
                                isSep = false;
                            }
                        }
                        break;
                    case SpanType.Count:
                        if (spanText.Length >= 3) // after ..
                        {
                            isSep = c < '0' || c > '9';
                            nextSpan = true;
                        }
                        break;
                    case SpanType.Symbol:
                        if (c == '+')
                        {
                            isSep = spanText.Length > 0;
                            nextSpan = spanText != "++";
                        }
                        else if (c == '-')
                        {
                            isSep = spanText.Length > 0;
                            nextSpan = spanText != "--";
                        }
                        else if (c == '=')
                        {
                            isSep = spanText.Length > 0;
                            nextSpan = spanText != ">=" && spanText != "<=" && spanText != "+=" && spanText != "-=" && spanText != "!=" && spanText != "==";
                        }
                        else if (c == '?')
                        {
                            isSep = spanText.Length > 0;
                            nextSpan = spanText != "??";
                        }
                        else if (c == '>')
                        {
                            isSep = spanText.Length > 0;
                            nextSpan = spanText != "=>" && spanText != "->";
                        }
                        else
                        {
                            isSep = true;
                            nextSpan = true;
                        }


                        //isSep = Filter.Operators.FirstOrDefault(op => op.StartsWith(spanText + c)) == null;
                        //if (isSep)
                        //{
                        //    nextSpan = true;
                        //}
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
                            while (i >= left && text[i - left++] == '\\')
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
                            if (text.Length > i + 1 && text[i + 1] == '{')
                            {
                                int lefts = 0;
                                int left = 0;
                                while (i - left >= 0 && text[i - left++] == '{')
                                    lefts++;
                                if (text.Length > i + 2 && text[i + 2] != '{' && lefts % 2 == 0)
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
                    case SpanType.Tag:
                        isSep = true;
                        nextSpan = true;
                        break;
                    case SpanType.Comment:
                        isSep = c == '\n';
                        nextSpan = true;
                        break;
                    case SpanType.MultiLineComment:
                        isSep = c == '/' && i > 0 && text[i - 1] == '*';
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
                            char leftC = text[i - left];
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

                    if (span.type == SpanType.MultiLineComment && splitMultiCommentsLines)
                    {
                        string line = "";
                        for (int j = 0; j < span.text.Length; j++)
                        {
                            if (span.text[j] == '\n')
                            {
                                spans.Add(new TextSpan(i) { type = SpanType.MultiLineComment, text = line });
                                spans.Add(new TextSpan(i) { type = SpanType.MultiLineComment, text = "\n" });
                                line = "";
                            }
                            else
                            {
                                line += span.text[j];
                            }
                        }
                        span.text = line;
                    }

                    spans.Add(span);
                    bool nextSpanFormattedStringContinue = span.insideFormattedString && !nextSpanInsideFormattedString;
                    span = new TextSpan(i) { insideFormattedString = nextSpanInsideFormattedString };
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

        foreach (var sp in spans.ToArray())
        {
            bool checkLink = false;

            // set span color
            switch (sp.type)
            {
                case SpanType.Normal:
                    // check if the span is a keyword, and if it does set its color
                    if (Filter.Operators.Contains(sp.text))
                    {
                        sp.color = Color.Blue;
                        sp.isKeyword = true;
                    }
                    break;
                case SpanType.Number:
                    sp.color = Color.Purple;
                    break;
                case SpanType.Symbol:
                    sp.color = Color.OrangeRed;
                    break;
                case SpanType.String:
                case SpanType.EscapedString:
                case SpanType.FormattedString:
                case SpanType.Char:
                    sp.color = Color.Red;
                    checkLink = true; // even for char
                    break;
                case SpanType.Comment:
                case SpanType.MultiLineComment:
                    checkLink = true;
                    sp.color = Color.Green;
                    break;
            }

            if (checkLink)
            {
                int startSearch, endSearch;
                if (sp.type == SpanType.Comment)
                {
                    startSearch = 2;
                    endSearch = sp.text.Length - 1;
                }
                else if (sp.type == SpanType.MultiLineComment)
                {
                    startSearch = 2;
                    endSearch = sp.text.Length - 3;
                }
                else if (sp.type == SpanType.String || sp.type == SpanType.EscapedString || sp.type == SpanType.FormattedString)
                {
                    startSearch = sp.text.IndexOf('"') + 1;
                    endSearch = sp.text.Length - 2;
                }
                else // if (sp.type == SpanType.Char)
                {
                    startSearch = 1;
                    endSearch = sp.text.Length - 2;
                }

                bool skipLinkSearch = startSearch >= sp.text.Length || endSearch < startSearch;
                if (!skipLinkSearch)
                {
                    // detect links
                    List<int[]> links = []; // returns List<int[2] { startIndex, length }>
                    {
                        //bool beginningOfWord = true;
                        //char[] splitters = { ' ', '\n', '\t' };
                        //for (int i = 0; i < span.text.Length; i++)
                        //{
                        //    char c = span.text[i];
                        //    if (beginningOfWord)
                        //    {
                        //        // read whole word
                        //        string link = "";
                        //        int next = 1;
                        //        while (i + next < span.text.Length && !splitters.Contains(span.text[i + next]))
                        //        {
                        //            link += span.text[i + next++];
                        //        }

                        //        // check if word is a valid link
                        //        if (Uri.IsWellFormedUriString(link, UriKind.RelativeOrAbsolute))
                        //        {
                        //            links.Add(new int[2] { i, link.Length });
                        //        }

                        //        beginningOfWord = false;
                        //    }
                        //    else
                        //    {
                        //        if (splitters.Contains(c))
                        //            beginningOfWord = true;
                        //    }
                        //}
                    } // another code for link detection (not working)

                    // split span
                    List<TextSpan> splittedSpan = new List<TextSpan>();
                    if (links.Count > 0)
                    {
                        links.Add(new int[2] { sp.text.Length, -1 }); // make the loop collect the last comment part

                        for (int link = 0, charIndex = 0; link < links.Count && charIndex < sp.text.Length; link++)
                        {
                            bool invalidLink = links[link][1] <= -1;

                            var originalSpan = sp.Duplicate();
                            originalSpan.text = sp.text.Substring(charIndex, links[link][0] - charIndex);

                            var linkSpan = sp.Duplicate();
                            if (invalidLink)
                                linkSpan.Dispose();
                            else
                            {
                                linkSpan.text = sp.text.Substring(links[link][0], links[link][1]);
                                linkSpan.link = linkSpan.text;
                                linkSpan.color = Color.Blue;
                            }

                            charIndex = links[link][0] + links[link][1];

                            if (originalSpan.text.Length > 0)
                            {
                                splittedSpan.Add(originalSpan);
                            }
                            else
                                originalSpan.Dispose();
                            if (!invalidLink)
                            {
                                splittedSpan.Add(linkSpan);
                            }
                        }

                        spans.InsertRange(spans.IndexOf(sp), splittedSpan);
                        spans.Remove(sp);
                    }
                }
            }
        }
        return spans.ToArray();
    }
}

public enum SpanType
{
    Space,
    DotCom,
    Normal,
    Number,
    Count,
    Symbol,
    Brace,
    String,
    Char,
    EscapedString,
    FormattedString,
    Tag,
    Comment,
    MultiLineComment,
}

public class TextSpan : IDisposable
{
    private string _text = "";
    public string text
    {
        get => _text;
        set
        {
            if (value != _text)
            {
                _text = value;
                if (!disposed)
                    TextChanged?.Invoke(this, value);
            }
        }
    }
    public int location;
    public ScriptDocument Doc { get; set; }
    public Color color = Color.Black;
    public Color backColor = Color.Transparent;
    public SpanType type = SpanType.Normal;
    public bool insideFormattedString = false;
    public bool isKeyword = false;
    public string link = null;
    public bool isLink
    {
        get
        {
            return link != null;
        }
    }
    //public SyntaxKind SyntaxKind { get; set; } = SyntaxKind.None;

    public event EventHandler<string> TextChanged;
    public TextSpan(int location)
    {
        this.location = location;
    }
    private bool disposed = false;

    public void Dispose()
    {
        text = "";
        color = Color.Empty;
        backColor = Color.Empty;
        disposed = true;
    }

    public TextSpan Duplicate()
    {
        return new TextSpan(location) { text = text, color = color, backColor = backColor, type = type, insideFormattedString = insideFormattedString };
    }

    public override string ToString()
    {
        return text;
    }

    public override bool Equals(object obj)
    {
        if (obj is TextSpan other)
        {
            return text == other.text && type == other.type && link == other.link;
        }
        return false;
    }
}

public static class Filter
{
    public static string[] Operators { get; } = ["=", "==", ">", "<", "!=", ">=", "<=", "!", "&", "|", "+", "-", "++", "--", "+=", "-=", "/", "*", "%", "=>", "->"];
    public static string[] Keywords { get; } = ["using", "namespace", "class", "func", "constructor", "private", "static", "if", "while", "for", "foreach", "basearray", "in", "from", "to", "is", "not", "throw", "true", "false", "new", "try", "catch", "when", "finally", "var", "const", "return", "break", "continue", "bool", "char", "number", "attribute", "null", "function", "this", "extern", "lenof", "counter", "id", "object"];
}