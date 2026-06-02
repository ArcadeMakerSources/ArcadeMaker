using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using System.Xml;
using ArcadeMaker.IDE;
using ArcadeMaker.IDE.Items;

namespace ArcadeMaker.IDE
{
    public static class Global
    {
        public const string ProgramName = "ArcadeMaker";

        public static bool allowClosing = false;
        public static Form1 form1 = null;
        public static bool IsEnglishLettersAndNumbers(this string text, char[] specials = null, bool allowEmpty = true)
        {
            if (text == null)
                return false;
            if (!allowEmpty && text == "")
                return false;
            foreach (char c in text)
            {
                if ((c < 'a' || c > 'z') && (c < 'A' || c > 'Z') && (c < '0' || c > '9'))
                {
                    bool skip = false;
                    if (specials != null)
                    {
                        foreach (char sp in specials)
                        {
                            if (c == sp)
                                skip = true;
                        }
                    }
                    if (skip)
                        continue;
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Serialize an object into an XML string
        /// </summary>
        /// <typeparam name="T">Type of object to serialize.</typeparam>
        /// <param name="obj">Object to serialize.</param>
        /// <param name="enc">Encoding of the serialized output.</param>
        /// <returns>Serialized (xml) object.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        internal static string SerializeObject<T>(T obj, Encoding enc)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                XmlWriterSettings xmlWriterSettings = new System.Xml.XmlWriterSettings()
                {
                    // If set to true XmlWriter would close MemoryStream automatically and using would then do double dispose
                    // Code analysis does not understand that. That's why there is a suppress message.
                    CloseOutput = false,
                    Encoding = enc,
                    OmitXmlDeclaration = false,
                    Indent = true
                };
                using (System.Xml.XmlWriter xw = System.Xml.XmlWriter.Create(ms, xmlWriterSettings))
                {
                    XmlSerializer s = new XmlSerializer(typeof(T));
                    s.Serialize(xw, obj);
                }

                return enc.GetString(ms.ToArray());
            }
        }

        public static bool IsPossibleName(this string name, string current)
        {
            if (name.IsEnglishLettersAndNumbers(new char[] { '_' }, false) && !Exp.Spans.Filter.Keywords.Contains(name))
            {
                if (name[0] > '9' || name[0] < '0')
                {
                    if (Environment.project != null)
                    {
                        foreach (GameItem item in Environment.project.items)
                        {
                            if (item.name == name && item.name != current)
                                return false;
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        public static string GenerateRandomGameItemName(string type = "Item")
        {
            string name = null;
            int testNum = 0;

            do
            {
                name = type + testNum++;
            }
            while (!name.IsPossibleName(null));

            return name;
        }

        public static void SetDoubleBuffered(this Panel panel, bool value)
        {
            try
            {
                typeof(Panel).InvokeMember("DoubleBuffered", BindingFlags.SetProperty
                    | BindingFlags.Instance | BindingFlags.NonPublic, null,
                    panel, new object[] { value });
            }
            catch (Exception ex)
            {
                ShowDebugMessage("[Debug Mode]\nCould not double buffer boardPanel.\n\n" + ex);
            }
        }

        private static bool canceled = true;
        public static void SetCloseAsHide(this Form frm)
        {
            if (canceled)
                return;
            frm.FormClosing += (s, e) =>
            {
                if (!allowClosing)
                {
                    e.Cancel = true;
                    frm.Hide();
                }
            };
        }

        private static string part2script = null;
        public static string ObjectPart2Script
        {
            get
            {
                throw new NotImplementedException();
//                if (part2script == null)
//                {
//#if DEBUG
//                    TextReader tr = new StreamReader(@"C:\Users\איתן\Desktop\GameStudio\res\GameObjectUserPart.cs");
//                    part2script = tr.ReadToEnd();
//                    tr.Close();
//#else
//                    part2script = Properties.Resources.GameObjectUserPart;
//#endif
//                }
//                return part2script;
            }
        }

        public static int GetLineColPosition(this string input, int lineNum, int colNum)
        {
            int currentLine = 0;
            int currentCol = 0;
            int position = 0;

            foreach (char c in input)
            {
                if (currentLine == lineNum && currentCol == colNum)
                {
                    return position;
                }

                if (c == '\n')
                {
                    currentLine++;
                    currentCol = 0;
                }
                else
                {
                    currentCol++;
                }

                position++;
            }

            return -1;
        }

        public static decimal Map(this decimal value, decimal fromSource, decimal toSource, decimal fromTarget, decimal toTarget)
        {
            return (value - fromSource) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget;
        }

        public static int MapGPT(int value, int fromLow, int fromHigh, int toLow, int toHigh)
        {
            //return (value - fromLow) * (toHigh - toLow) / (fromHigh - fromLow) + toLow;
            return MapBing(value, fromLow, fromHigh, toLow, toHigh);
        }

        public static int MapBing(int value, int fromLow, int fromHigh, int toLow, int toHigh)
        {
            if (fromHigh == fromLow)
            {
                return (toHigh == toLow) ? toHigh : (value < fromLow ? toLow : toHigh);
            }
            else
            {
                return (value - fromLow) * (toHigh - toLow) / (fromHigh - fromLow) + toLow;
            }
        }

        public static System.Drawing.Image ImageFromFile(string path)
        {
            try
            {
                using (Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                    return System.Drawing.Image.FromStream(stream);
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show($"Could not find image at path \"{path}\".", "Path Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show($"Could not open image file from path \"{path}\": Access Denied by system.\n"
                              + $"Please make sure your AntiVirus is not blocking {Global.ProgramName} and try again.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                string err = $"Failed to load image from file path \"{path}\".";
                if (!ShowDebugMessage(err + "\n\n" + ex))
                    MessageBox.Show(err, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return null;
        }

        public static double GetDifference(this Color color1, Color color2)
        {
            // Calculate the difference between the red, green, and blue values
            int diffR = Math.Abs(color1.R - color2.R);
            int diffG = Math.Abs(color1.G - color2.G);
            int diffB = Math.Abs(color1.B - color2.B);

            // Calculate the total difference as the average of the differences in each channel
            double totalDiff = (diffR + diffG + diffB) / 3.0;

            // Return the total difference
            return totalDiff;
        }

        public static int CountOf(this string src, char c, int startIndex = 0, int endIndex = -1, bool ignoreCase = false)
        {
            if (src.Length == 0)
                return 0;

            if (endIndex < 0)
                endIndex = src.Length - 1;
            if (startIndex < 0 || startIndex >= src.Length || endIndex < startIndex || endIndex >= src.Length)
                throw new IndexOutOfRangeException("start or end index is out of range");
            int count = 0;
            for (int i = startIndex; i <= endIndex; i++)
            {
                char cc = src[i];
                if (ignoreCase ? cc.EqualsIgnoreCase(c) : cc == c)
                    count++;
            }
            return count;
        }

        public static string FileName(this string path)
        {
            int index = path.LastIndexOf('\\');
            if (index >= 0 && index < path.Length - 1)
            {
                return path.Substring(index + 1);
            }
            else
            {
                return path;
            }
        }

        public static string FileNameWithoutExtension(this string path)
        {
            string fn = path.FileName();
            if (fn.Contains('.'))
                fn = fn.Substring(0, fn.LastIndexOf('.'));
            return fn;
        }

        public static string FileLocation(this string path)
        {
            int index = path.LastIndexOf('\\');
            if (index >= 0)
            {
                return path.Substring(0, index);
            }
            return null;
        }

        /// <summary>
        /// Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        public static System.Drawing.Bitmap ResizeImage(this System.Drawing.Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new System.Drawing.Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }


        public static void DrawTransparentBackground(this PictureBox box)
        {
            System.Drawing.Bitmap image = new System.Drawing.Bitmap(32, 32);
            const int rectSize = 16;
            using (Graphics g = Graphics.FromImage(image))
            {
                Pen pen = new Pen(Color.White);
                g.FillRectangle(pen.Brush, 0, 0, rectSize, rectSize);
                pen.Color = Color.Gray;
                g.FillRectangle(pen.Brush, rectSize, 0, rectSize, rectSize);
                g.FillRectangle(pen.Brush, 0, rectSize, rectSize, rectSize);
                pen.Color = Color.White;
                g.FillRectangle(pen.Brush, rectSize, rectSize, rectSize, rectSize);
            }
            box.BackgroundImage = image;
        }

        public static bool ImageFileIsSpriteStrip(string path, out int count)
        {
            count = 0;
            try
            {
                string name = path.FileNameWithoutExtension();
                string widthStr = "";

                int readInd = 0;
                char c;
                do
                {
                    int where = name.Length - readInd++ - 1;
                    c = name[where];
                    if (c >= '0' && c <= '9')
                    {
                        widthStr = c + widthStr;
                    }
                    else break;
                }
                while (true);

                if (widthStr.Length > 0 && name.EndsWith("_strip" + widthStr))
                {
                    count = Convert.ToInt32(widthStr);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public static System.Drawing.Bitmap NoSpriteIcon
        {
            get
            {
                // create "no sprite" icon
                if (field == null)
                {
                    field = new System.Drawing.Bitmap(15, 15);
                    using (Graphics g = Graphics.FromImage(field))
                    {
                        Pen pen = new Pen(Color.Blue);
                        g.FillEllipse(pen.Brush, 0, 0, field.Size.Width, field.Size.Height);
                        pen.Color = Color.Black;
                        g.DrawEllipse(pen, 0, 0, field.Size.Width, field.Size.Height);
                        pen.Color = Color.Red;
                        Font font = new Font("Arial", 8);
                        g.DrawString("?", font, pen.Brush, 2, 2);
                    }
                }
                return field;
            }
        }

        public static ScriptBoxSpan[] GetScriptBoxSpans(string text, string replaceTabSpaceWith = null, bool splitMultiCommentsLines = false)
        {
            List<ScriptBoxSpan> spans = new List<ScriptBoxSpan>();

            ScriptBoxSpan span = new ScriptBoxSpan();
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
                                span.type = SpanType.Dot;
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
                                break;
                            default:
                                bool minusNumber = c == '-' && i + 1 < text.Length && text[i + 1] >= '0' && text[i + 1] <= '9';
                                if ((c >= '0' && c <= '9') || minusNumber)
                                {
                                    span.type = SpanType.Number;
                                }
                                else if ((char.IsSymbol(c) && c != '_') || c == '!' || c == '&' || c == '-')
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
                        case SpanType.Dot:
                            isSep = true;
                            nextSpan = true;
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
                            if (isSep)
                            {
                                nextSpan = !( c == 'F' || c == 'f'
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
                        case SpanType.Symbol:
                            //isSep = !CSharpFilter.Operators.Contains(spanText + c);
                            isSep = Exp.Spans.Filter.Operators.Find(op => op.StartsWith(spanText + c)) == null;
                            if (isSep)
                            {
                                nextSpan = true;
                            }
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
                                    spans.Add(new ScriptBoxSpan { type = SpanType.MultiLineComment, text = line });
                                    spans.Add(new ScriptBoxSpan { type = SpanType.MultiLineComment, text = "\n" });
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
                        span = new ScriptBoxSpan { insideFormattedString = nextSpanInsideFormattedString };
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
                        if (Exp.Spans.Filter.Keywords.Contains(sp.text))
                        {
                            sp.color = Color.Yellow;
                            sp.isKeyword = true;
                        }
                        else
                            sp.color = Color.White;
                        break;
                    case SpanType.Number:
                        sp.color = Color.Violet;
                        break;
                    case SpanType.Symbol:
                        sp.color = Color.OrangeRed;
                        break;
                    case SpanType.Brace:
                    case SpanType.Dot:
                        sp.color = Color.White;
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
                        sp.color = Color.LightGreen;
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
                        List<int[]> links = sp.text.FindLinks(startSearch, endSearch); // returns List<int[2] { startIndex, length }>
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
                        List<ScriptBoxSpan> splittedSpan = new List<ScriptBoxSpan>();
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
                            sp.Dispose();
                        }
                    }
                }
            }
            return spans.ToArray();
        }

        /// <summary>
        /// Returns a list of arrays of integer, where index 0 is link start index, and index 2 is link length
        /// </summary>
        /// <param name="text"></param>
        /// <param name="startSearch"></param>
        /// <param name="endSearch"></param>
        /// <returns></returns>
        public static List<int[]> FindLinks(this string text, int startSearch = 0, int endSearch = -1)
        {
            if (endSearch < 0)
                endSearch = text.Length - 1;

            var match = Regex.Matches(text.Substring(startSearch, (endSearch + 1) - startSearch), @"((?<=^|\s)(http|ftp|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&:/~\+#]*[\w\-\@?^=%&/~\+#])?)");
            List<int[]> links = new List<int[]>();

            for (int m = 0; m < match.Count; m++)
            {
                links.Add(new int[2] { match[m].Index + startSearch, match[m].Length });
            }

            return links;
        }

        public static bool ShowDebugMessage(string msg)
        {
#if DEBUG
            MessageBox.Show($"[Debug Mode Only]\n{msg}");
            return true;
#endif
            return false;
        }

        /// <summary>
        /// Returns the index of the given index of instance of the given char.
        /// </summary>
        /// <param name="src">The string to check</param>
        /// <param name="c">The char to search.</param>
        /// <param name="index">The index of instance of the given char.</param>
        /// <returns><inheritdoc cref="string.IndexOf(char)" path="/returns"/></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static int IndexOf(this string src, char c, int index)
        {
            if (src == null)
                throw new ArgumentNullException("src");

            int currentIndex = 0;
            for (int i = 0; i < src.Length; i++)
            {
                if (src[i] == c)
                {
                    if (index == currentIndex)
                        return i;
                    currentIndex++;
                }
            }
            return -1;
        }

        public static int IndexOf<T>(this T[] array, T item)
        {
            return Array.IndexOf(array, item);
        }

        public static T Find<T>(this T[] array, Func<T, bool> matchFunc)
        {
            foreach (T obj in array)
            {
                if (matchFunc(obj))
                    return obj;
            }
            return default;
        }

        public static bool EqualsIgnoreCase(this char c, char value)
        {
            return c.ToString().Equals(value.ToString(), StringComparison.CurrentCultureIgnoreCase);
        }

        public static string ReadLine(this string source, int lineNumber)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (lineNumber <= 0)
                throw new ArgumentOutOfRangeException("lineNumber");

            int i, line;
            for (i = 0, line = 1; i < source.Length && line < lineNumber; i++)
            {
                if (source[i] == '\n')
                    line++;
            }

            string result = null;
            while (line == lineNumber && i < source.Length && source[i] != '\n')
            {
                if (result == null)
                    result = "";
                result += source[i++];
            }

            return result;
        }

        public static string GetAssemblyLocation(string assemblyFullName)
        {
            try
            {
                // Split the assembly full name into its components
                string[] parts = assemblyFullName.Split(',');

                // Extract the assembly name
                string assemblyName = parts[0].Trim();

                // Extract the version
                Version version = null;
                foreach (string part in parts)
                {
                    if (part.Trim().StartsWith("Version="))
                    {
                        version = new Version(part.Trim().Substring("Version=".Length));
                        break;
                    }
                }

                // Extract the culture
                string culture = null;
                foreach (string part in parts)
                {
                    if (part.Trim().StartsWith("Culture="))
                    {
                        culture = part.Trim().Substring("Culture=".Length);
                        break;
                    }
                }

                // Extract the public key token
                string publicKeyToken = null;
                foreach (string part in parts)
                {
                    if (part.Trim().StartsWith("PublicKeyToken="))
                    {
                        publicKeyToken = part.Trim().Substring("PublicKeyToken=".Length);
                        break;
                    }
                }

                // Create an AssemblyName instance
                AssemblyName assemblyNameInstance = new AssemblyName();
                assemblyNameInstance.Name = assemblyName;
                assemblyNameInstance.Version = version;
                assemblyNameInstance.CultureInfo = culture == null ? null : new System.Globalization.CultureInfo(culture);
                assemblyNameInstance.SetPublicKeyToken(HexStringToBytes(publicKeyToken));

                // Return the location
                return Assembly.Load(assemblyNameInstance).Location;
            }
            catch (Exception ex)
            {
                // Handle any exceptions
                Console.WriteLine("Error: " + ex.Message);
                return null;
            }
        }

        public static byte[] HexStringToBytes(string hexString)
        {
            if (hexString == null)
                return null;

            byte[] bytes = new byte[hexString.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }
            return bytes;
        }

        public static double DistanceTo(this Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }

        public static Point WithX(this Point point, int value, bool relative = false)
        {
            return new Point((relative ? point.X : 0) + value, point.Y);
        }
        public static Point WithY(this Point point, int value, bool relative = false)
        {
            return new Point(point.X, (relative ? point.Y : 0) + value);
        }

        public static Rectangle AddSize(this Rectangle rectangle, Size size)
        {
            rectangle.Inflate(size);
            return rectangle;
        }


        public static Point ToPoint(this PointF point)
        {
            return new Point((int)point.X, (int)point.Y);
        }

        public static string GetDirectoryPath(this Assembly assembly)
        {
            string codeBase = assembly.CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }

        public static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static string InsertRange(this string src, string[] substrings, int[] indices)
        {
            if (substrings.Length != indices.Length)
                throw new ArgumentException("Length of substring[] must be equal for the length of indices[]");
            int diff = 0;
            for (int i = 0; i < indices.Length; i++)
            {
                src = src.Insert(indices[i] + diff, substrings[i]);
                diff += substrings[i].Length;
            }
            return src;
        }

        /// <summary>
        /// Converts the given <see cref="IEnumerable{A}"/> to <typeparamref name="R"/>[]
        /// </summary>
        /// <typeparam name="R">The type to convert each element to</typeparam>
        /// <typeparam name="T">The original type of the <paramref name="src"/></typeparam>
        /// <param name="src">The source <see cref="IEnumerable{T}"/> to covert</param>
        /// <param name="Convert">A converter function</param>
        /// <returns>the given <see cref="IEnumerable{A}"/> converted to <typeparamref name="R"/>[]</returns>
        public static R[] ToArray<R, T>(this IEnumerable<T> src, Func<T, R> Convert)
        {
            List<R> array = new List<R>();
            foreach (T element in src)
            {
                array.Add(Convert(element));
            }
            return array.ToArray();
        }

        public static T[] Add<T>(this T[] src, T[] add)
        {
            T[] added = new T[src.Length + add.Length];

            int i;
            for (i = 0; i < src.Length; i++)
                added[i] = src[i];
            for (; i < src.Length + add.Length; i++)
                added[i] = add[i];

            return added;
        }

        public static string ToDisplayString<T>(this IEnumerable<T> arr, string seperator = ", ")
        {
            string text = "[";
            for (int i = 0; i < arr.Count(); i++)
                text += arr.ElementAt(i) + (i >= arr.Count() - 1 ? "" : seperator);
            return text + ']';
        }

        public static void PushRecentProject(string projectPath)
        {
            if (Properties.Settings.Default.RecentProjects == null)
                Properties.Settings.Default.RecentProjects = new System.Collections.Specialized.StringCollection();
            int indexOfProjectInRecents = Properties.Settings.Default.RecentProjects.IndexOf(projectPath);
            if (indexOfProjectInRecents < 0)
                Properties.Settings.Default.RecentProjects.Insert(0, projectPath);
            else if (indexOfProjectInRecents > 0)
            {
                string oldFirst = Properties.Settings.Default.RecentProjects[0];
                Properties.Settings.Default.RecentProjects[0] = projectPath;
                Properties.Settings.Default.RecentProjects[indexOfProjectInRecents] = oldFirst;
            }
            Properties.Settings.Default.Save();
        }

        public static bool Contains(this string src, char value, bool ignoreCase)
        {
            foreach (char c in src)
            {
                //bool a_to_A() => c >= 'a' && c <= 'z' && value >= 'A' && value <= 'Z' && c + ('A' - 'a') == value;
                //bool A_to_a() => c >= 'A' && c <= 'Z' && value >= 'a' && value <= 'z' && c - ('A' - 'a') == value;
                //if (c == value || a_to_A() || A_to_a())
                //return true;
                if (ignoreCase ? c.EqualsIgnoreCase(value) : c == value)
                    return true;
            }
            return false;
        }

        public static Control FindFocusedControl(this Control control)
        {
            var container = control as IContainerControl;
            while (container != null)
            {
                control = container.ActiveControl;
                container = control as IContainerControl;
            }
            return control;
        }

        public static byte[] ToByteArray(this Assembly assembly)
        {
            var pi = assembly.GetType().GetMethod("GetRawBytes", BindingFlags.Instance | BindingFlags.NonPublic);
            return (byte[])pi.Invoke(assembly, null);
        }
    }
}