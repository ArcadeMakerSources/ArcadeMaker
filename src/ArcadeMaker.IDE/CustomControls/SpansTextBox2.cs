using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections;
using ArcadeMaker.IDE.Scripting;
using ArcadeMaker.Core.ExpSrc;
using Exp;

namespace ArcadeMaker.IDE
{
    public partial class SpansTextBox2 : UserControl
    {
        private readonly List<ScriptBoxSpan> OldSpans = new List<ScriptBoxSpan>();
        public readonly List<ScriptBoxSpan> Spans = new List<ScriptBoxSpan>();
        public readonly List<char> CharAlerts = new List<char>();

        private Font _font = new Font("Consolas", 9.5F);
        private float _scrollX;
        private int _selectionStart = 0, _selectionLength = 0;
        private Color _selectionColor = Color.White;
        private Color _selectionBackColor = Color.Violet;
        private Brush _currentLineHighlightBrush = new Pen(Color.FromArgb(60, 60, 60)).Brush;
        private Color _lineNumbersColor = Color.IndianRed;

        public float LineSpacing
        {
            get => Font.Size * Font.FontFamily.GetLineSpacing(FontStyle.Regular) / Font.FontFamily.GetEmHeight(FontStyle.Regular) + 2;
        }

        public string Text
        {
            get
            {
                string text = "";
                for (int span = 0; span < Spans.Count; span++)
                    text += Spans[span].text;
                return text;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Loading
        {
            get;
            set
            {
                if (value != field)
                {
                    field = value;
                    Invalidate();
                }
            }
        } = false;

        public string TextRange(int endIndex)
        {
            return TextRange(0, endIndex);
        }

        public string TextRange(int startIndex, int endIndex)
        {
            string text = "";
            for (int span = 0, totalCharIndex = 0; span < Spans.Count; span++)
            {
                for (int c = 0; c < Spans[span].text.Length; c++, totalCharIndex++)
                {
                    if (startIndex <= totalCharIndex)
                    {
                        text += Spans[span].text[c];
                        if (totalCharIndex == endIndex)
                            return text;
                    }
                }
            }
            return text;
        }

        private string GetTextStats(out int lineCount)
        {
            string text = "";
            lineCount = 0;
            foreach (var span in Spans)
            {
                foreach (char c in span.text)
                {
                    if (c == '\n')
                        lineCount++;
                    text += c;
                }
            }
            return text;
        }

        public int Length
        {
            get
            {
                int length = 0;
                foreach (var span in Spans)
                    length += span.text.Length;
                return length;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Font Font { get => _font; set { _font = value; knownCharWidthes_chars.Clear(); knownCharWidthes_widthes.Clear(); Invalidate(); } }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float ScrollX { get => _scrollX; set { _scrollX = value; Invalidate(); } }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float ScrollY { get; set { field = value; Invalidate(); } }
        public event EventHandler<int> SelectionStartChanged;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectionStart { get => _selectionStart; set { _selectionStart = value; SelectionStartChanged?.Invoke(this, value); Invalidate(); } }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectionLength { get => _selectionLength; set { _selectionLength = value; Invalidate(); } }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color SelectionColor { get => _selectionColor; set { _selectionColor = value; Invalidate(); } }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color SelectionBackColor { get => _selectionBackColor; set { _selectionBackColor = value; Invalidate(); } }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Brush CurrentLineHighlightBrush { get => _currentLineHighlightBrush; set { _currentLineHighlightBrush = value; Invalidate(); } }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color LineNumbersColor { get => _lineNumbersColor; set { _lineNumbersColor = value; Invalidate(); } }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color CaretColor { get; set; } = Color.White;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color CompletionColor { get; set; } = Color.White;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color CompletionBackColor { get; set; } = Color.DarkSlateGray;


        public new event EventHandler<SpansTextBox2TextChangedEventArgs> TextChanged;
        public event EventHandler<SpansTextBox2CharAlertEventArgs> CharAlert;


        private System.Windows.Forms.Timer CaretTimer = new() { Interval = 700 };

        public static readonly string TabSpace = "    ";

        private readonly ToolStripMenuItem copyMenuBtn = new("Copy");
        private readonly ToolStripMenuItem cutMenuBtn = new("Cut");
        private readonly ToolStripMenuItem pasteMenuBtn = new("Paste");
        public SpansTextBox2()
        {
            InitializeComponent();

            completionBox.BackColor = CompletionBackColor;
            completionBox.ForeColor = CompletionColor;

            completionBox.SelectedValueChanged += (s, e) => ShowCompletionItemInfo();
            textStartLocX += vScrollBar.Width;
            drawTextFormat.FormatFlags = StringFormatFlags.MeasureTrailingSpaces;
            CaretTimer.Tick += CaretTimer_Tick;

            completionBox.VisibleChanged += (s, e) =>
            {
                if (!completionBox.Visible)
                {
                    toolTipText = null;
                    completionToolTip.Hide(this);
                }
            };

            ContextMenuStrip = new();
            copyMenuBtn.Click += (s, e) => Copy();
            cutMenuBtn.Click += (s, e) => Cut();
            pasteMenuBtn.Click += (s, e) => Paste();
            copyMenuBtn.ShortcutKeys = Keys.Control | Keys.C;
            cutMenuBtn.ShortcutKeys = Keys.Control | Keys.X;
            pasteMenuBtn.ShortcutKeys = Keys.Control | Keys.V;
            ContextMenuStrip.Items.AddRange([copyMenuBtn, cutMenuBtn, pasteMenuBtn]);
            ContextMenuStrip.Opened += ContextMenu_Popup;
        }

        private void ContextMenu_Popup(object sender, EventArgs e)
        {
            bool canCopy = SelectionLength > 0, canPaste = Clipboard.ContainsText();
            copyMenuBtn.Enabled = canCopy;
            cutMenuBtn.Enabled = canCopy;
            pasteMenuBtn.Enabled = canPaste;
        }

        private bool skipNextCaretTimerTick = false;
        private void CaretTimer_Tick(object sender, EventArgs e)
        {
            if (skipNextCaretTimerTick)
            {
                skipNextCaretTimerTick = false;
                return;
            }

            invalidatedByTimer = true;

            drawCaret = !drawCaret;
            Invalidate();

            invalidatedByTimer = false;
        }

        private StringFormat drawTextFormat = StringFormat.GenericTypographic;
        private const float textMinStartLocX = 15F, textMinStartLocY = 10F;
        private float textStartLocX = textMinStartLocX, textStartLocY = textMinStartLocY;
        private bool drawCaret = false;

        private new float HScroll(SizeF textSize)
        {
            return hScrollBar.Maximum > 0 ? Global.MapGPT(hScrollBar.Value, hScrollBar.Minimum, hScrollBar.Maximum, 0, (int)textSize.Width) : 0;
        }

        private new float VScroll(SizeF textSize)
        {
            return vScrollBar.Maximum > 0 ? Global.MapGPT(vScrollBar.Value, vScrollBar.Minimum, vScrollBar.Maximum, 0, (int)textSize.Height) : 0;
        }

        private new float VScroll(int lineCount = -1)
        {
            if (lineCount < 0)
                lineCount = Text.CountOf('\n') + 1;
            return vScrollBar.Maximum > 0 ? Global.MapGPT(vScrollBar.Value, vScrollBar.Minimum, vScrollBar.Maximum, 0, (int)(lineCount * LineSpacing + textStartLocY)) : 0;
        }

        private SizeF? lastTextSize = null;
        private bool invalidatedByTimer = false;

        /// <summary>
        /// The number of lines in the last drawn text (excluding first line)
        /// </summary>
        private int displayTextLineCount = -1;
        private string displayedText = null;
        private float[] displayCharWidthes = null;

        List<char> knownCharWidthes_chars = new List<char>();
        List<float> knownCharWidthes_widthes = new List<float>();
        private void SpanTextBox2_Paint(object sender, PaintEventArgs e)
        {
            string Text = GetTextStats(out int lineCount);
            SizeF textSize;
            if (lastTextSize == null || !lastTextSize.HasValue || displayedText != Text)
            {
                textSize = MeasureString(Text, e.Graphics);
                lastTextSize = textSize;
                displayedText = Text;
            }
            else
                textSize = lastTextSize.Value;
            PointF? caretLocation = null;
            bool canScrollH = hScrollBar.Maximum > 0;
            bool canScrollV = vScrollBar.Maximum > 0;
            int Length = this.Length;
            float hscroll = 0, vscroll = 0;
            displayTextLineCount = lineCount;
            if (canScrollH || canScrollV)
            {
                hscroll = HScroll(textSize);
                vscroll = VScroll(lineCount);
                e.Graphics.TranslateTransform(-hscroll, -vscroll);
            }
            using (Pen pen = new Pen(LineNumbersColor))
            {
                float x = textStartLocX, y = textStartLocY;

                // number lines (calculation part)
                float lineNumberWidth = MeasureString((lineCount + 1).ToString(), e.Graphics).Width;
                textStartLocX = vScrollBar.Width + textMinStartLocX + lineNumberWidth;

                int totalCharIndex = 0;
                char? previousChar = null;

                List<ScriptBoxSpan> Spans = this.Spans;

                // if text is empty, simulate single line text to draw caret & line highlight
                bool foundText = false;
                foreach (var span in Spans)
                {
                    if (span.text.Length > 0)
                    {
                        foundText = true;
                        break;
                    }
                }
                if (!foundText)
                {
                    Spans.Clear();
                    Spans = new List<ScriptBoxSpan>
                    {
                        new ScriptBoxSpan { text = "\n" }
                    };
                    Length = 1; // we're talking about the local parameter 'Length', not the property << int Length { get; } >>
                }

                displayCharWidthes = new float[Text.Length];
                bool outsideView_up = y + LineSpacing < vscroll, outsideView_down = y + LineSpacing > vscroll + DisplayRectangle.Height;

                for (int spanIndex = 0; spanIndex < Spans.Count; spanIndex++)
                {
                    var span = Spans[spanIndex];
                    for (int charIndex = 0; charIndex < span.text.Length; charIndex++, totalCharIndex++)
                    {
                        char c = span.text[charIndex];

                        outsideView_up = false;
                        outsideView_down = false;
                        bool outsideView = false;

                        Action CalculateOutsideView = () =>
                        {
                            outsideView_up = y + LineSpacing < vscroll;
                            outsideView_down = y + LineSpacing > vscroll + DisplayRectangle.Height;
                            outsideView = outsideView_up || outsideView_down;
                        };

                        CalculateOutsideView();

                        bool highlightLineCheck = totalCharIndex == 0;
                        bool currentCharIsNewLine = false;
                        bool drawCaretAfterLineHighlight = false;
                        if (c == '\n')
                        {
                            x = textStartLocX;

                            // if previous char was endline too, then we need to draw here the caret
                            bool lastCharInText = Length - 1 == totalCharIndex;
                            if (!outsideView && drawCaret)
                            {
                                if (totalCharIndex == SelectionStart && previousChar == '\n')
                                {
                                    pen.Color = Color.Black;
                                    e.Graphics.DrawLine(pen, x, y + 2, x, y + LineSpacing + 2);
                                }
                                else if ((lastCharInText && totalCharIndex == SelectionStart - 1) || (SelectionStart == totalCharIndex && SelectionStart == 0))
                                {
                                    // if this is the last char in the Text, draw caret at next line
                                    drawCaretAfterLineHighlight = true;
                                }
                            }

                            y += LineSpacing;
                            CalculateOutsideView();
                            highlightLineCheck = true;
                            currentCharIsNewLine = true;
                        }
                        if (!outsideView && Focused && highlightLineCheck)
                        {
                            // check if caret is in this line
                            if (totalCharIndex < SelectionStart || (totalCharIndex == SelectionStart && SelectionStart == 0))
                            {
                                bool caretAtCurrentLine = true;
                                for (int sc = charIndex + 1, lc = totalCharIndex + 1, lspan = spanIndex; lc < SelectionStart; lc++, sc++)
                                {
                                    while (sc >= Spans[lspan].text.Length)
                                    {
                                        sc = 0;
                                        if (++lspan >= Spans.Count)
                                        {
                                            goto HighlightLine;
                                        }
                                    }
                                    if (Spans[lspan].text[sc] == '\n')
                                    {
                                        caretAtCurrentLine = false;
                                        break;
                                    }
                                }

                                // highlight current line
                            HighlightLine:
                                if (caretAtCurrentLine)
                                {
                                    float highlightY = (SelectionStart == 0 ? textStartLocY : y) + 2;
                                    e.Graphics.FillRectangle(CurrentLineHighlightBrush, textStartLocX, highlightY, textSize.Width + DisplayRectangle.Width, LineSpacing);
                                }
                            }
                        }

                        // [AFTER LINE HIGHLIGHT] if there were 2 endl in a row, draw selection (if selected)
                        if (!outsideView && SelectionLength > 0 && c == '\n' && (previousChar == '\n' || totalCharIndex == Length - 1 || totalCharIndex == 0) && SelectionStart <= totalCharIndex && SelectionLength + SelectionStart >= totalCharIndex)
                        {
                            pen.Color = SelectionBackColor;
                            float drawAtY = y - LineSpacing + 2, drawHeight = LineSpacing;
                            if (totalCharIndex == Length - 1)
                            {
                                if (previousChar != '\n')
                                    drawAtY += LineSpacing;
                                else
                                    drawHeight *= 2;
                            }
                            e.Graphics.FillRectangle(pen.Brush, x, drawAtY, 4, drawHeight);
                        }

                        if (drawCaretAfterLineHighlight)
                        {
                            pen.Color = Color.Black;
                            float caretY = y + 2;
                            if (totalCharIndex == 0)
                                caretY -= LineSpacing;
                            caretLocation = new PointF(x, caretY);
                            e.Graphics.DrawLine(pen, caretLocation.Value.X, caretLocation.Value.Y, caretLocation.Value.X, caretLocation.Value.Y + LineSpacing);
                        }
                        if (!currentCharIsNewLine)
                        {
                            if (!outsideView)
                            {
                                float charWidth;
                                int knownCharsWidthes_index = knownCharWidthes_chars.IndexOf(c);
                                if (knownCharsWidthes_index < 0)
                                {
                                    SizeF charSize = MeasureString(c, e.Graphics);
                                    charWidth = charSize.Width;
                                    knownCharWidthes_chars.Add(c);
                                    knownCharWidthes_widthes.Add(charWidth);
                                }
                                else
                                {
                                    charWidth = knownCharWidthes_widthes[knownCharsWidthes_index];
                                }
                                displayCharWidthes[totalCharIndex] = charWidth;

                                // draw selection
                                if (totalCharIndex >= SelectionStart && totalCharIndex < SelectionStart + SelectionLength)
                                {
                                    pen.Color = SelectionBackColor;
                                    e.Graphics.FillRectangle(pen.Brush, x, y + 2, charWidth, LineSpacing);
                                }
                                pen.Color = span.color;
                                if (Loading)
                                    pen.Color = Color.FromArgb(pen.Color.A / 2, pen.Color.R, pen.Color.G, pen.Color.B);

                                if (span.isLink)
                                    e.Graphics.DrawLine(pen, x, y + LineSpacing, x + charWidth, y + LineSpacing);
                                e.Graphics.DrawString(c.ToString(), Font, pen.Brush, x, y, drawTextFormat);

                                bool caretInFirstChar = SelectionStart == 0;

                                if (!caretInFirstChar)
                                    x += charWidth;

                                // draw caret
                                pen.Color = this.CaretColor;
                                float caretX = x, caretY = y + 2;
                                bool drawCaretNow = false;
                                if (totalCharIndex == SelectionStart - (caretInFirstChar ? 0 : 1))
                                {
                                    drawCaretNow = true;
                                }
                                else if (previousChar == '\n' && totalCharIndex == SelectionStart)
                                {
                                    caretX -= charWidth;
                                    drawCaretNow = true;
                                }
                                else if (totalCharIndex == Length - 1 && SelectionStart == totalCharIndex + 1)
                                {
                                    caretX = textStartLocX;
                                    caretY += LineSpacing;
                                    drawCaretNow = true;
                                }
                                caretLocation = new PointF(caretX, caretY);
                                if (drawCaret && drawCaretNow)
                                {
                                    e.Graphics.DrawLine(pen, caretX, caretY, caretX, caretY + LineSpacing);
                                }

                                if (caretInFirstChar)
                                    x += charWidth;
                            }
                        }
                        previousChar = c;
                    }
                }

                // set scroll bars
                int vert = (int)((y + LineSpacing) + textStartLocY), hor = (int)(textSize.Width + textStartLocX);
                if (!invalidatedByTimer)
                {
                    if (vert > DisplayRectangle.Height)
                    {
                        vScrollBar.Height = DisplayRectangle.Height;
                        int oldMaximum = vScrollBar.Maximum;
                        if (oldMaximum != vert - DisplayRectangle.Height)
                        {
                            vScrollBar.Maximum = vert - DisplayRectangle.Height;
                        }
                        vScrollBar.Visible = true;
                    }
                    else
                    {
                        vScrollBar.Visible = false;
                        vScrollBar.Maximum = 0;
                    }
                    if (hor > DisplayRectangle.Width)
                    {
                        hScrollBar.Width = DisplayRectangle.Width - hScrollBar.Location.X;
                        int oldMaximum = hScrollBar.Maximum;
                        if (oldMaximum != hor - DisplayRectangle.Width)
                        {
                            hScrollBar.Maximum = hor - DisplayRectangle.Width;
                        }
                        hScrollBar.Visible = true;
                    }
                    else
                    {
                        hScrollBar.Visible = false;
                        hScrollBar.Maximum = 0;
                    }

                    // move scrolling to fit caret
                    if (false && invalidatedByKeyPress && caretLocation.HasValue && caretLocation.Value != null)
                    {
                        invalidatedByKeyPress = false;
                        bool scrolled = false;
                        int v_fromHigh = (int)(lineCount * LineSpacing + textStartLocY);
                        if (caretLocation.Value.Y < vscroll)
                        {
                            //vScrollBar.Value = Global.MapGPT((int)caretLocation.Value.Y, 0, v_fromHigh, vScrollBar.Minimum, vScrollBar.Maximum);
                            scrolled = true;
                        }
                        else if (caretLocation.Value.Y > vscroll + DisplayRectangle.Height /*- (hScrollBar.Visible ? hScrollBar.Height : 0)*/)
                        {
                            //vScrollBar.Value = Global.MapGPT((int)caretLocation.Value.Y - DisplayRectangle.Height, 0, v_fromHigh, vScrollBar.Minimum, vScrollBar.Maximum);
                            scrolled = true;
                        }
                        if (caretLocation.Value.X < hscroll)
                        {
                            //hScrollBar.Value = Global.MapGPT((int)hscroll, 0, hor + (int)textStartLocX + DisplayRectangle.Width, hScrollBar.Minimum, hScrollBar.Maximum);
                            scrolled = true;
                        }
                        else if (caretLocation.Value.X > hscroll + DisplayRectangle.Width)
                        {
                            //hScrollBar.Value = Global.MapGPT((int)hscroll + DisplayRectangle.Width, 0, hor + (int)textStartLocX + DisplayRectangle.Width, hScrollBar.Minimum, hScrollBar.Maximum);
                            scrolled = true;
                        }

                        if (scrolled)
                        {
                            e.Graphics.Clear(BackColor);
                            SpanTextBox2_Paint(sender, e);
                            return;
                        }
                    }
                }

                // number lines (drawing part)
                pen.Color = LineNumbersColor;
                float lineNumbersX = (vScrollBar.Visible ? vScrollBar.Width : 0) + hscroll;
                e.Graphics.FillRectangle(Brushes.DarkSlateGray, lineNumbersX, vscroll/*0*/, textStartLocX - (vScrollBar.Visible ? vScrollBar.Width : 0), DisplayRectangle.Height/*vert + DisplayRectangle.Height * 2*/);
                for (int line = (int)(vscroll / LineSpacing) + 1; line * LineSpacing <= vscroll + DisplayRectangle.Height && line <= lineCount + 1; line++)
                {
                    e.Graphics.DrawString(line.ToString(), Font, pen.Brush, lineNumbersX + 1, (line - 1) * LineSpacing + textStartLocY + 2);
                }

                // loading message
                if (Loading)
                {
                    using (Pen lpen = new Pen(Color.FromArgb(200, Color.Black)))
                    {
                        e.Graphics.ResetTransform();
                        const string text = "Loading...";
                        textSize = MeasureString(text, e.Graphics);
                        float rwidth = textSize.Width + 50, rheight = textSize.Height + 50;
                        e.Graphics.FillRectangle(pen.Brush, Width / 2 - rwidth / 2, Height / 2 - rheight / 2, rwidth, rheight);
                        /* using (*/
                        Brush textBrush = Brushes.White;
                        e.Graphics.DrawString(text, Font, textBrush, Width / 2 - textSize.Width / 2, Height / 2 - textSize.Height / 2);
                    }
                }
            }
        }

        private int mouseDownCharInd = 0;
        private async void SpanTextBox2_MouseDown(object sender, MouseEventArgs e)
        {
            if (completionBox.Visible)
                HideSuggestions();

            if (e.Button != MouseButtons.Left)
                return;

            SelectionStart = GetCharIndexByPosition(e.Location);
            SelectionLength = 0;

            mouseDownCharInd = SelectionStart;

            // if clicked on a link, open it
            if (ModifierKeys == Keys.Control)
            {
                await Task.Run(() =>
                {
                    int spanIndex = 0;
                    bool @break = false;
                    for (int totalCharIndex = 0; spanIndex < Spans.Count; spanIndex++)
                    {
                        for (int charIndex = 0; charIndex < Spans[spanIndex].text.Length; charIndex++, totalCharIndex++)
                        {
                            if (totalCharIndex == SelectionStart)
                            {
                                @break = true;
                                break;
                            }
                        }
                        if (@break) break;
                    }

                    if (Spans[spanIndex].isLink)
                    {
                        try
                        {
                            Process.Start(Spans[spanIndex].link);
                        }
                        catch (System.Exception ex)
                        {
                            string err = "Could not open link in browser.";
#if DEBUG
                            err += "\n\n[Debug Mode]\n" + ex;
#endif
                            MessageBox.Show(err, "Error");
                        }
                    }
                });
            }
        }

        public int GetCharIndexByPosition(PointF position, bool ignoreScrolling = false)
        {
            if (position.Y < textStartLocY)
                return 0;

            string Text = displayedText ?? this.Text;

            SizeF textSize;
            if (lastTextSize.HasValue && lastTextSize.Value != null)
                textSize = lastTextSize.Value;
            else
                textSize = MeasureString(Text);
            if (!ignoreScrolling)
            {
                position.X += (int)HScroll(textSize);
                position.Y += (int)VScroll(displayTextLineCount);
            }
            float x = textStartLocX, y = textStartLocY;

            for (int index = 0; index < Text.Length; index++)
            {
                bool endl = Text[index] == '\n';
                float charWidth = displayCharWidthes == null ? MeasureString(Text[index]).Width : displayCharWidthes[index];

                if (!endl && position.Y >= y && position.Y <= y + LineSpacing)
                {
                    if (position.X <= x + charWidth || (index == Text.Length - 1 || (index < Text.Length - 1 && Text[index + 1] == '\n')))
                    {
                        if (position.X >= x + charWidth / 2)
                        {
                            return index + 1;
                        }
                        else if (position.X >= x)
                        {
                            return index;
                        }
                    }
                }

                if (endl)
                {
                    x = textStartLocX;
                    if (index > 0) // else - after empty line check
                        y += LineSpacing;

                    // check empty line
                    if ((index == 0 || (index < Text.Length - 1 && Text[index + 1] == '\n')) && position.Y >= y && position.Y <= y + LineSpacing + (index == 0 ? LineSpacing : 0))
                    {
                        if (index > 0)
                            return index + 1;
                        else
                        {
                            // the second empty line would not have a check, because the first one is checking for itself,
                            // not for the next, so we need to sepcificly check it
                            if (position.Y > y + LineSpacing)
                            {
                                if (Text[index + 1] == '\n')
                                    return index + 1;
                            }
                            else
                                return index;
                        }
                    }

                    if (index == 0)
                        y += LineSpacing;
                }
                else
                    x += charWidth;
            }

            return Text.Length;
        }

        public PointF GetPositionOfCharIndex(int charIndex, bool ignoreScrolling = false)
        {
            string Text = displayedText ?? this.Text;
            float x = 0, y = 0;
            SizeF textSize = new SizeF(0, 0);
            int lineCount = 1;
            for (int c = 0; c < charIndex; c++)
            {
                if (c < Text.Length) // prevent async work bugs
                {
                    if (Text[c] == '\n')
                    {
                        if (x > textSize.Width)
                            textSize.Width = x;

                        x = 0;
                        y += LineSpacing;

                        textSize.Height = y;
                        lineCount++;
                    }
                    else
                    {
                        x += displayCharWidthes == null ? MeasureString(Text[c]).Width : displayCharWidthes[c];
                    }
                }
            }

            if (ignoreScrolling)
            {
                x -= HScroll(lastTextSize.Value);
                y -= VScroll();
            }

            return new PointF(x + textStartLocX, y + textStartLocY);
        }

        private const char KeyBackspace = (char)8;
        private const char KeyEnter = (char)13;
        private const char KeyEscape = (char)27;
        private bool interceptEnterKeyPress = false;
        private void SpanTextBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            invalidatedByKeyPress = true;

            if (e.KeyChar == KeyBackspace)
            {
                if (SelectionLength == 0)
                    RemoveChar();
                else
                    InsertText();
            }
            else if (e.KeyChar == KeyEnter)
            {
                if (!interceptEnterKeyPress)
                {
                    //// insert new line, with correct tabbing space
                    string Text = this.Text;

                    // get depth
                    int caretLoc = SelectionStart;
                    int depth = 0;
                    for (int i = 0; i < caretLoc; i++)
                    {
                        char c = Text[i];
                        if (c == '{')
                            depth++;
                        else if (c == '}')
                            depth--;
                    }

                    // insert <depth> tabs
                    string tabs = "";
                    for (int t = 0; t < depth; t++)
                        tabs += TabSpace;

                    InsertText("\n" + tabs);
                }
                else
                {
                    interceptEnterKeyPress = false;
                }
            }
            else if (e.KeyChar == '\t')
            {
                int selectionStart = SelectionStart;
                string text = Text;

                // check if selection start is the first character (excluding spaces) in the line
                bool firstCharInLine = true;
                for (int i = selectionStart - 1; i >= 0 && text[i] != '\n'; i--)
                {
                    if (text[i] != ' ')
                    {
                        firstCharInLine = false;
                        break;
                    }
                }

                // if it is, insert TAB at each line in the selected area
                if (firstCharInLine && SelectionLength > 0)
                {
                    bool newLine = true;
                    int tabsCount = 0;
                    for (int i = selectionStart; i <= selectionStart + SelectionLength && i < text.Length; i++)
                    {
                        if (text[i] == '\n')
                            newLine = true;
                        else if (newLine && text[i] != ' ')
                        {
                            newLine = false;
                            this.SelectionStart = i + tabsCount * TabSpace.Length;
                            InsertText(TabSpace, removeSelected: false, setSpans: false, invalidate: false);
                            tabsCount++;
                        }
                    }
                    this.SelectionStart = selectionStart;
                    SelectionLength += tabsCount * TabSpace.Length;
                    SetSpans();
                    TextChanged?.Invoke(this, new SpansTextBox2TextChangedEventArgs(TextChangedKind.Replace, false));
                    Invalidate();
                }
                else
                    InsertText(TabSpace);
            }
            else if (e.KeyChar != KeyEscape)
                InsertText(e.KeyChar.ToString());
        }

        private int shiftKeyPressCharIndex = 0;
        private bool invalidatedByKeyPress = false;

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Control)
            {
                SetLinkCursorAsync();
            }
            else if (keyData == (Keys.Control | Keys.C))
            {
                Copy(); return true;
            }
            else if (keyData == (Keys.Control | Keys.X))
            {
                Cut(); return true;
            }
            else if (keyData == (Keys.Control | Keys.V))
            {
                Paste(); return true;
            }
            else if (keyData == (Keys.Control | Keys.A))
            {
                SelectionStart = 0;
                SelectionLength = Text.Length;

                return true;
            }

            else if (keyData == Keys.Right)
            {
                // caret right
                if (SelectionStart < Text.Length)
                    SelectionStart++;
                return true;
            }
            else if (keyData == Keys.Left)
            {
                // caret left
                if (SelectionStart > 0)
                    SelectionStart--;
                return true;
            }
            else if (keyData == Keys.Up)
            {
                if (!completionBox.Visible)
                {
                    PointF loc = GetPositionOfCharIndex(SelectionStart);
                    loc.Y -= (LineSpacing - 1);
                    SelectionStart = GetCharIndexByPosition(loc, true);
                }
                return true;
            }
            else if (keyData == Keys.Down)
            {
                if (!completionBox.Visible)
                {
                    PointF loc = GetPositionOfCharIndex(SelectionStart);
                    loc.Y += LineSpacing + 1;
                    SelectionStart = GetCharIndexByPosition(loc, true);
                }
                return true;
            }
            else if (keyData == (Keys.Shift | Keys.Right))
            {
                if (SelectionLength == 0)
                    shiftKeyPressCharIndex = SelectionStart;

                // select right
                if (SelectionStart >= shiftKeyPressCharIndex)
                {
                    SelectionLength++;
                }
                else
                {
                    SelectionStart++;
                    SelectionLength--;
                }
                return true;
            }
            else if (keyData == (Keys.Shift | Keys.Left))
            {
                if (SelectionLength == 0)
                    shiftKeyPressCharIndex = SelectionStart;

                // select left
                if (SelectionStart + SelectionLength > shiftKeyPressCharIndex)
                {
                    SelectionLength--;
                }
                else
                {
                    SelectionStart--;
                    SelectionLength++;
                }
                return true;
            }
            else if (keyData == (Keys.Shift | Keys.Up))
            {
                if (SelectionLength == 0)
                    shiftKeyPressCharIndex = SelectionStart;

                // select up
                mouseDownCharInd = SelectionStart + SelectionLength;

                bool selectionStartAndEndAreInSameLine = true;
                string text = Text;
                for (int i = SelectionStart; i < SelectionStart + SelectionLength; i++)
                {
                    if (text[i] == '\n')
                    {
                        selectionStartAndEndAreInSameLine = false;
                        break;
                    }
                }

                // implement this operation by simulating selection with mouse move
                PointF loc = GetPositionOfCharIndex(SelectionStart + (selectionStartAndEndAreInSameLine ? SelectionLength : 0));
                SpansTextBox2_MouseMove(this, new MouseEventArgs(MouseButtons.Left, 1, (int)loc.X, (int)(loc.Y - (LineSpacing - 1)), 0));

                return true;
            }
            else if (keyData == (Keys.Shift | Keys.Down))
            {
                if (SelectionLength == 0)
                    shiftKeyPressCharIndex = SelectionStart;

                // select down
                mouseDownCharInd = SelectionStart + SelectionLength;

                bool selectionStartAndEndAreInSameLine = true;
                string text = Text;
                for (int i = SelectionStart; i < SelectionStart + SelectionLength; i++)
                {
                    if (text[i] == '\n')
                    {
                        selectionStartAndEndAreInSameLine = false;
                        break;
                    }
                }

                // implement this operation by simulating selection with mouse move
                PointF loc = GetPositionOfCharIndex(SelectionStart + (selectionStartAndEndAreInSameLine ? SelectionLength : 0));
                SpansTextBox2_MouseMove(this, new MouseEventArgs(MouseButtons.Left, 1, (int)loc.X, (int)(loc.Y + (LineSpacing + 1)), 0));

                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        public void Copy()
        {
            string text = Text.Substring(SelectionStart, SelectionLength);
            Clipboard.SetText(text);
        }

        public void Paste()
        {
            if (Clipboard.ContainsText())
                InsertText(Clipboard.GetText());
        }

        public void Cut()
        {
            Copy();
            InsertText(); // remove selected text
        }

        private void MakeCharAlert(string text)
        {
            if (CharAlerts.Count > 0 && CharAlert != null && SelectionStart > 0)
            {
                if (text == null)
                    text = Text;
                if (SelectionStart <= text.Length && CharAlerts.Contains(text[SelectionStart - 1]))
                {
                    // get span before
                    int spanIndex, totalCharIndex;
                    ScriptBoxSpan span = null;
                    for (spanIndex = 0, totalCharIndex = 0; spanIndex < Spans.Count; spanIndex++)
                    {
                        for (int charIndex = 0; charIndex < Spans[spanIndex].text.Length; charIndex++, totalCharIndex++)
                        {
                            if (totalCharIndex == SelectionStart - 2)
                            {
                                span = Spans[spanIndex];
                                goto BreakPoint;
                            }
                        }
                    }

                BreakPoint:
                    CharAlert?.Invoke(this, new SpansTextBox2CharAlertEventArgs(text[SelectionStart - 1], SelectionStart - 1, span));
                }
            }
        }

        private void InsertText(string text = "", bool removeSelected = true, bool setSpans = true, bool invalidate = true)
        {
            if (SelectionStart == Length)
            {
                string Text = null;
                if (Spans.Any())
                {
                    Spans.Last().text += text;
                    if (setSpans)
                        SetSpans();
                }
                else
                {
                    // text is empty
                    if (setSpans)
                        Spans.AddRange(GetScriptBoxSpans(text));
                    else
                        Spans.Add(new ScriptBoxSpan { text = text });
                    Text = text;
                }

                SelectionStart += text.Length;
                if (invalidate)
                {
                    TextChanged?.Invoke(this, new SpansTextBox2TextChangedEventArgs(TextChangedKind.Insert, false));
                    MakeCharAlert(Text);
                    Invalidate();
                }
                return;
            }
            int spanIndex = 0;
            int totalCharIndex = 0;
            foreach (var span in Spans)
            {
                for (int c = 0; c < span.text.Length; c++, totalCharIndex++)
                {
                    if (totalCharIndex == SelectionStart)
                    {
                        // first, remove selected text
                        bool removedSelection = SelectionLength > 0;
                        if (removeSelected)
                        {
                            for (int removeSpanCharInd = c, removeSpanInd = spanIndex, removeInd = 0; removeInd < SelectionLength; removeInd++)
                            {
                                Spans[removeSpanInd].text = Spans[removeSpanInd].text.Remove(removeSpanCharInd, 1);
                                if (removeSpanCharInd >= Spans[removeSpanInd].text.Length)
                                {
                                    if (Spans[removeSpanInd].text.Length == 0 && Spans[removeSpanInd] != span)
                                    {
                                        Spans.RemoveAt(removeSpanInd);
                                    }
                                    else
                                    {
                                        removeSpanInd++;
                                    }
                                    removeSpanCharInd = 0;
                                }
                            }

                            SelectionLength = 0;
                        }

                        // now we can add the new char
                        span.text = span.text.Insert(c, text);

                        SelectionStart += text.Length;

                        string Text = null;
                        if (setSpans)
                            Text = SetSpans();

                        if (invalidate)
                        {
                            TextChanged?.Invoke(this, new SpansTextBox2TextChangedEventArgs(removedSelection ? TextChangedKind.Replace : TextChangedKind.Insert, false));
                            MakeCharAlert(Text);
                            Invalidate();
                        }
                        return;
                    }
                }
                spanIndex++;
            }
        }

        private void RemoveChar()
        {
            if (SelectionStart == 0)
                return;
            if (SelectionStart == Length && Spans.Any())
            {
                while (Spans.Last().text.Length == 0)
                {
                    Spans.RemoveAt(Spans.Count - 1);
                    if (!Spans.Any())
                        return;
                }

                var span = Spans.Last();
                span.text = span.text.Remove(Spans.Last().text.Length - 1);

                while (span.text.Length == 0)
                {
                    Spans.Remove(span);
                    if (Spans.Any())
                        span = Spans.Last();
                    else
                        break;
                }

                SelectionStart--;
                TextChanged?.Invoke(this, new SpansTextBox2TextChangedEventArgs(TextChangedKind.Remove, false));
                MakeCharAlert(null);
                Invalidate();
                return;
            }

            int spanIndex = 0;
            int totalCharIndex = 0;
            foreach (var span in Spans)
            {
                for (int c = 0; c < span.text.Length; c++, totalCharIndex++)
                {
                    if (totalCharIndex == SelectionStart)
                    {
                        bool removed = false;
                        if (c > 0)
                        {
                            span.text = span.text.Remove(c - 1, 1);
                            removed = true;
                        }
                        else if (spanIndex > 0)
                        {
                            ScriptBoxSpan rspan = Spans[--spanIndex];
                            while (rspan.text.Length == 0)
                            {
                                Spans.Remove(rspan);
                                if (spanIndex >= 0)
                                    rspan = Spans[spanIndex];
                                else
                                {
                                    rspan = null;
                                    break;
                                }
                            }
                            if (rspan != null)
                            {
                                rspan.text = rspan.text.Remove(rspan.text.Length - 1);
                            }
                            removed = true;
                        }

                        string Text = null;
                        if (removed)
                        {
                            SelectionStart--;
                            Text = SetSpans();
                        }

                        TextChanged?.Invoke(this, new SpansTextBox2TextChangedEventArgs(TextChangedKind.Remove, false));
                        MakeCharAlert(Text);
                        Invalidate();
                        return;
                    }
                }
                spanIndex++;
            }
        }

        private string SetSpans()
        {
            //string text = "";
            //while (Spans.Any())
            //{
            //    var span = Spans.First();
            //    text += span.text;
            //    span.Dispose();
            //    Spans.RemoveAt(0);
            //}
            //Spans.AddRange(GetScriptBoxSpans(text));

            //return text;

            return SetSpansLocateChanges(out int a, out int b);
        }

        private string SetSpansLocateChanges(out int lastSpanEqualBefore, out int firstSpanEqualAfter)
        {
            var Spans = OldSpans;

            lastSpanEqualBefore = -1;
            firstSpanEqualAfter = -1;

            string Text = this.Text;
            var newSpans = GetScriptBoxSpans(Text);

            // location algorithm: iterates from start to end & from end to start, and stops a direction when meeting non-equality:  >>>they  will make  cookies<<<
            //                                                                                                                       >>>they >have made< cookies<<<
            for (int i = 0; i < newSpans.Length && i < Spans.Count && (lastSpanEqualBefore < 0 || firstSpanEqualAfter < 0); i++)
            {
                if (lastSpanEqualBefore == -1)
                {
                    if (!newSpans[i].Equals(Spans[i]))
                        lastSpanEqualBefore = i;
                    else
                    {
                        //newSpans[i].Dispose();
                        newSpans[i] = Spans[i];
                    }
                }
                if (firstSpanEqualAfter == -1)
                {
                    int i_new = newSpans.Length - i - 1, i_old = Spans.Count - i - 1;
                    if (!newSpans[i_new].Equals(Spans[i_old]))
                        firstSpanEqualAfter = i;
                    else
                    {
                        //newSpans[i_new].Dispose();
                        newSpans[i_new] = Spans[i_old];
                    }
                }
            }

            this.Spans.Clear();
            this.Spans.AddRange(newSpans);
            Spans.Clear();
            Spans.AddRange(newSpans);
            return Text;
        }

        public ScriptBoxSpan[] GetScriptBoxSpans(string text)
        {
            return Global.GetScriptBoxSpans(text, replaceTabSpaceWith: TabSpace);
        }

        private void SpansTextBox2_Load(object sender, EventArgs e)
        {
            BackColor = Color.FromArgb(255, 42, 42, 42);
            CaretTimer.Start();
            CaretTimer.Enabled = Focused;
            var form = FindForm();
            if (form != null)
            {
                form.FormClosed += (s, ea) =>
                {
                    while (Spans.Any())
                    {
                        var span = Spans.First();
                        span.Dispose();
                        Spans.RemoveAt(0);
                    }
                    try
                    {
                        CaretTimer?.Dispose();
                        toolTipFont?.Dispose();
                        toolTipTitleFont?.Dispose();
                    }
                    catch { }
                    if (!IsDisposed)
                        Dispose();
                };
            }
        }

        private void SpansTextBox2_MouseWheel(object sender, MouseEventArgs e)
        {
            // set scroll jump for 3 lines per wheel rotation
            int scrollJump = (int)(LineSpacing * 3);

            // convert to scroll bar scale
            int textHeight = (int)((displayedText ?? Text).CountOf('\n') * LineSpacing);
            scrollJump = Global.MapGPT(scrollJump, 0, textHeight, vScrollBar.Minimum, vScrollBar.Maximum);

            if (e.Delta > 0)
            {
                // wheel rotated up
                if (vScrollBar.Minimum <= vScrollBar.Value - scrollJump)
                    vScrollBar.Value -= scrollJump;
                else
                    vScrollBar.Value = vScrollBar.Minimum;

                Invalidate();
            }
            else if (e.Delta < 0)
            {
                // wheel rotated down
                if (vScrollBar.Maximum >= vScrollBar.Value + scrollJump)
                    vScrollBar.Value += scrollJump;
                else
                    vScrollBar.Value = vScrollBar.Maximum;

                Invalidate();
            }
        }

        private Point lastMouseHoverLoc = Point.Empty;
        private void SpansTextBox2_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int charIndex = GetCharIndexByPosition(e.Location);
                if (charIndex != SelectionStart)
                {
                    SelectionLength = Math.Abs(mouseDownCharInd - charIndex);
                    if (charIndex < mouseDownCharInd)
                        SelectionStart = charIndex;
                }
            }
            else if (ModifierKeys == Keys.Control)
            {
                SetLinkCursorAsync(e.Location);
            }
            else if (toolTip.Active)
            {
                if (ToolTipShowLoc.X != -1 && ToolTipShowLoc.Y != -1 && e.Location.DistanceTo(ToolTipShowLoc) > LineSpacing)
                {
                    toolTip.Hide(this);
                    ToolTipShowLoc = new Point(-1, -1);
                }
            }

            if (lastMouseHoverLoc.DistanceTo(e.Location) > LineSpacing)
            {
                ResetMouseEventArgs(); // this way mouse hover would fire more than 1 times
                lastMouseHoverLoc = e.Location;
            }
        }

        private async void SetLinkCursorAsync(Point? mouseLoc = null)
        {
            if (mouseLoc == null || !mouseLoc.HasValue)
                mouseLoc = Cursor.Position;
            await Task.Run(() =>
            {
                int charIndex = GetCharIndexByPosition(mouseLoc.Value);
                for (int span = 0, totalCharIndex = 0; span < Spans.Count; span++)
                {
                    for (int i = 0; i < Spans[span].text.Length; i++, totalCharIndex++)
                    {
                        if (totalCharIndex == charIndex)
                        {
                            if (Spans[span].isLink)
                            {
                                if (InvokeRequired)
                                {
                                    Invoke(new MethodInvoker(() => { Cursor = Cursors.Hand; }));
                                }
                                else
                                {
                                    Cursor = Cursors.Hand;
                                }
                            }
                            else if (Cursor == Cursors.Hand)
                            {
                                if (InvokeRequired)
                                {
                                    Invoke(new MethodInvoker(() => { Cursor = Cursors.IBeam; }));
                                }
                                else
                                {
                                    Cursor = Cursors.IBeam;
                                }
                            }
                            return;
                        }
                    }
                }
            });
        }

        private void SpansTextBox2_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // select the span the user clicked on

                // first, find the char index of the click
                int charIndex = GetCharIndexByPosition(e.Location);

                // find the span that this char belongs to
                int spanIndex = 0, spanStartCharIndex = 0;
                for (int i = 0, spanCharIndex = 0; i < charIndex; i++)
                {
                    if (spanCharIndex++ >= Spans[spanIndex].text.Length)
                    {
                        spanIndex++;
                        spanCharIndex = 0;
                        spanStartCharIndex = i;
                        i--;
                    }
                }

                // select the span
                SelectionStart = spanStartCharIndex;
                SelectionLength = Spans[spanIndex].text.Length;
            }
        }

        private void SpansTextBox2_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (completionBox.Visible && e.KeyCode == Keys.Up || e.KeyCode == Keys.Down || (completionBox.SelectedItem != null && e.KeyCode == Keys.Enter) || e.KeyCode == Keys.Escape)
            {
                e.IsInputKey = false;
                completionBox_PreviewKeyDown(sender, e);
            }
            if (e.KeyData == Keys.Tab)
                e.IsInputKey = true;
        }

        private void vScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            Invalidate();
        }

        private void hScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            Invalidate();
        }

        private void SpansTextBox2_GotFocus(object sender, EventArgs e)
        {
            CaretTimer.Enabled = true;
            drawCaret = true;
            Invalidate();
            skipNextCaretTimerTick = true;
        }

        private void completionBox_MouseClick(object sender, MouseEventArgs e)
        {
            SelectSuggestion();
        }

        private void SpansTextBox2_LostFocus(object sender, EventArgs e)
        {
            CaretTimer.Enabled = false;
            drawCaret = false;
            Invalidate();
        }

        private void completionBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && completionBox.SelectedItem != null && completionBox.Visible)
            {
                interceptEnterKeyPress = true;
                SelectSuggestion();
            }
            else if (e.KeyCode == Keys.Down)
            {
                if (completionBox.SelectedIndex < completionBox.Items.Count - 1)
                    completionBox.SelectedIndex++;
            }
            else if (e.KeyCode == Keys.Up)
            {
                if (completionBox.SelectedIndex > 0)
                    completionBox.SelectedIndex--;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                completionBox.Items.Clear();
                completionBox.Visible = false;
            }
            else
            {
                e.IsInputKey = false;
                Focus();
            }
        }

        private int suggestionSpanStart, suggestionSpanEnd;
        public void ShowSuggestions(SpansTextBox2Suggestion[] suggestions, int start, int end)
        {
            completionBox.Items.Clear();

            SpansTextBox2Suggestion matchSug = null;
            if (end > start)
            {
                string span = TextRange(start, end - 1);
                Array.Sort(suggestions, new SuggestionsSorter(span));
                matchSug = suggestions.Find(s => s.DisplayText.Equals(span, StringComparison.CurrentCultureIgnoreCase));
            }
            completionBox.Items.AddRange(suggestions);

            completionBox.Visible = suggestions.Length >= 1;

            PointF caretPos = GetPositionOfCharIndex(SelectionStart, ignoreScrolling: true);
            Point loc = new Point((int)caretPos.X, (int)(caretPos.Y + LineSpacing));
            if (loc.X + completionBox.Size.Width >= DisplayRectangle.Width)
                loc.X -= loc.X + completionBox.Size.Width - DisplayRectangle.Width;
            if (loc.Y + completionBox.Size.Height >= DisplayRectangle.Height)
                loc.Y -= (completionBox.Size.Height - DisplayRectangle.Height) + (int)LineSpacing;
            completionBox.Location = loc;

            if (matchSug == null)
                completionBox.SelectedIndex = suggestions.Any() ? 0 : -1;
            else
                completionBox.SelectedItem = matchSug;

            suggestionSpanStart = start;
            suggestionSpanEnd = end;
        }

        private void SelectSuggestion()
        {
            if (completionBox.SelectedItem is SpansTextBox2Suggestion sug && completionBox.Visible)
            {
                // find current span
                //bool inserted = false;
                //for (int spanIndex = 0, totalCharIndex = 0; !inserted && spanIndex < Spans.Count; spanIndex++)
                //{
                //    for (int charIndex = 0; !inserted && charIndex < Spans[spanIndex].text.Length; charIndex++, totalCharIndex++)
                //    {
                //        if (totalCharIndex == SelectionStart - 1)
                //        {
                //            string spanLastText = Spans[spanIndex].text;
                //            Spans[spanIndex].text = sug.Text;
                //            SelectionStart = totalCharIndex + sug.Text.Length - spanLastText.Length + 1;
                //            inserted = true;
                //        }
                //    }
                //}

                //if (!inserted)
                //{
                //    Spans.Add(new ScriptBoxSpan { text = sug.Text });
                //    SelectionStart += sug.Text.Length;
                //}

                string oldText = this.Text;
                Spans.Clear();
                string newText = oldText.Insert(suggestionSpanEnd, sug.Text).Remove(suggestionSpanStart, suggestionSpanEnd - suggestionSpanStart);
                Spans.Add(new ScriptBoxSpan { text = newText });
                SelectionStart += newText.Length - oldText.Length;

                SetSpans();
                TextChanged?.Invoke(this, new SpansTextBox2TextChangedEventArgs(suggestionSpanEnd > suggestionSpanStart ? TextChangedKind.Replace : TextChangedKind.Insert, true));

                //Invalidate(); [needed?]
                completionBox.Visible = false;
                Focus();
            }
        }

        public void HideSuggestions()
        {
            completionBox.Items.Clear();
            completionBox.Visible = false;
        }

        private Point ToolTipShowLoc { get; set; } = new Point(-1, -1);
        private string toolTipText = null;
        public void ShowHint(string text, Point position, string title = null)
        {
            toolTip.ToolTipTitle = title;
            toolTipText = text;
            toolTip.Show(text, this, position);
            ToolTipShowLoc = position;
        }

        public void ShowHint(string text, string title = null)
        {
            ShowHint(text, PointToClient(Cursor.Position).WithY(+(int)LineSpacing, relative: true), title);
        }

        public bool HintIsShown => ToolTipShowLoc.X != -1 && ToolTipShowLoc.Y != -1;

        public void ShowParamDesc(string title, string summary, Point loc)
        {
            paramToolTip.ToolTipTitle = title;
            toolTipText = summary;
            paramToolTip.Show(summary, this, loc);
        }

        private void toolTip_Draw(object sender, DrawToolTipEventArgs e)
        {
            using (Pen pen = new Pen(toolTip.BackColor))
            {
                e.Graphics.FillRectangle(pen.Brush, e.Bounds);
                pen.Color = toolTip.ForeColor;
                e.Graphics.DrawRectangle(pen, e.Bounds);
                pen.Color = Color.Black;

                Rectangle textLayout = new Rectangle(5, 5, DisplayRectangle.Width - (int)maxToolTipPadding.Width, DisplayRectangle.Height - (int)maxToolTipPadding.Height);
                e.Graphics.DrawString(toolTip.ToolTipTitle, toolTipTitleFont, pen.Brush, textLayout);
                textLayout.Y = 15 + (int)toolTipTitleSize.Height;
                e.Graphics.DrawString(e.ToolTipText, toolTipFont, pen.Brush, textLayout);
            }
        }
        private readonly Font toolTipFont = new Font(FontFamily.GenericMonospace, 8);
        private Font toolTipTitleFont => this.Font;
        private SizeF toolTipTitleSize = SizeF.Empty;
        private SizeF toolTipTextSize = SizeF.Empty;
        /// <summary>The maximum possible size of the tool tip is <c>DisplayRectangle.Size</c> minus this size</summary>
        private SizeF maxToolTipPadding = new Size(100, 100);
        private void toolTip_Popup(object sender, PopupEventArgs e)
        {
            using (Graphics graphics = CreateGraphics())
            {
                if (string.IsNullOrWhiteSpace(toolTip.ToolTipTitle))
                    toolTipTitleSize = new SizeF(0, 0);
                else
                    toolTipTitleSize = graphics.MeasureString(toolTip.ToolTipTitle, toolTipFont, DisplayRectangle.Size.Width - (int)maxToolTipPadding.Width);
                toolTipTextSize = graphics.MeasureString(toolTipText, toolTipTitleFont, DisplayRectangle.Size - maxToolTipPadding);
                e.ToolTipSize = new Size((int)Math.Max(toolTipTitleSize.Width, toolTipTextSize.Width) + 30, (int)(toolTipTitleSize.Height + toolTipTextSize.Height + 30));
            }
        }

        public void HideParamDesc()
        {
            if (InvokeRequired)
                Invoke(new MethodInvoker(() => paramToolTip.Hide(this)));
            else
                paramToolTip.Hide(this);
        }

        public event EventHandler<CompletionItemInfoEventArgs> OnCompletionItemShowInfo;
        private void ShowCompletionItemInfo()
        {
            toolTipText = null;
            completionToolTip.Hide(this);

            var item = completionBox.SelectedItem as SpansTextBox2Suggestion;
            var ea = new CompletionItemInfoEventArgs(item?.Description, item);
            OnCompletionItemShowInfo?.Invoke(this, ea);
            if (ea.Text != null)
            {
                toolTipText = ea.Text;
                completionToolTip.Show(ea.Text, this, completionBox.Location.WithX(completionBox.Location.X + completionBox.Size.Width));
            }
        }

        public int GetSpanIndexByCharIndex(int index, out int spanStart)
        {
            spanStart = 0;
            for (int spanIndex = 0, totalCharIndex = 0; spanIndex < Spans.Count; spanIndex++, spanStart = totalCharIndex)
            {
                for (int charIndex = 0; charIndex < Spans[spanIndex].text.Length; charIndex++, totalCharIndex++)
                {
                    if (totalCharIndex == index)
                    {
                        return spanIndex;
                    }
                }
            }
            return -1;
        }

        public ScriptBoxSpan GetSpanByCharIndex(int index) => GetSpanByCharIndex(index);
        public ScriptBoxSpan GetSpanByCharIndex(int index, out int spanStart)
        {
            int spanIndex = GetSpanIndexByCharIndex(index, out spanStart);
            if (spanIndex >= 0)
                return Spans[spanIndex];
            return null;
        }

        private readonly List<KnownCharSize> knownCharSizes = new List<KnownCharSize>();

        private class KnownCharSize
        {
            public char Char { get; }
            public SizeF Size { get; }
            public KnownCharSize(char c, SizeF size)
            {
                Char = c;
                Size = size;
            }
        }

        private SizeF GetKnownCharSize(char c)
        {
            var known = knownCharSizes.Find(kc => kc.Char == c);
            return known != null ? known.Size : new SizeF(-1, -1);
        }

        private bool CharSizeIsKnown(char c)
        {
            return GetKnownCharSize(c).Width >= 0;
        }

        private SizeF MeasureString(string str, Graphics g = null)
        {
            if (str == null)
                throw new ArgumentNullException("str");

            SizeF size;
            bool dispose = g == null;
            if (dispose)
                g = CreateGraphics();
            try
            {
                size = g.MeasureString(str, Font, int.MaxValue, drawTextFormat);
            }
            finally
            {
                if (dispose)
                    g.Dispose();
            }
            return size;
            /*
            SizeF size = str.Length > 0 ? new SizeF(0, LineSpacing) : SizeF.Empty;

            bool dispose = g == null;
            try
            {
                foreach (char c in str)
                {
                    if (c == '\n')
                    {
                        size.Height += LineSpacing;
                        continue;
                    }
                    else if (!CharSizeIsKnown(c))
                    {
                        if (g == null)
                            g = CreateGraphics();
                        knownCharSizes.Add(new KnownCharSize(c, g.MeasureString(c.ToString(), Font, int.MaxValue, drawTextFormat)));
                    }
                    size.Width += GetKnownCharSize(c).Width;
                }
            }
            finally
            {
                if (dispose)
                    g?.Dispose();
            }

            return size;
            */
        }

        private SizeF MeasureString(char str, Graphics g = null)
        {
            return MeasureString(str.ToString(), g);
        }

        private void completionBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return; // must!
            var sug = (SpansTextBox2Suggestion)completionBox.Items[e.Index];

            e.DrawBackground();

            using Pen pen = new(CompletionColor);
            using Pen typeLblPen = new(sug.TypeLabelColor);

            // draw item text
            float typeLblWidth = e.Graphics.MeasureString(sug.Type, completionBox.Font).Width;
            e.Graphics.DrawString(sug.DisplayText, completionBox.Font, pen.Brush, e.Bounds with { Width = e.Bounds.Width - (int)typeLblWidth - 5 }, new() { Trimming = StringTrimming.EllipsisCharacter });

            // draw item type label
            float lblX = e.Bounds.Width - typeLblWidth;
            e.Graphics.DrawString(sug.Type, completionBox.Font, typeLblPen.Brush, lblX, e.Bounds.Y);
        }
    }

    public enum TextChangedKind
    {
        Insert,
        Remove,
        Replace
    }

    public class SpansTextBox2TextChangedEventArgs : EventArgs
    {
        public TextChangedKind Kind { get; set; }
        public bool BySuggestion { get; set; }

        public SpansTextBox2TextChangedEventArgs(TextChangedKind kind, bool bySuggestion)
        {
            this.Kind = kind;
            this.BySuggestion = bySuggestion;
        }
    }

    public class SpansTextBox2CharAlertEventArgs : EventArgs
    {
        public char Alert { get; private set; }
        public int Index { get; private set; }
        public ScriptBoxSpan SpanBefore { get; private set; }

        public SpansTextBox2CharAlertEventArgs(char alert, int index, ScriptBoxSpan spanBefore = null)
        {
            this.Alert = alert;
            this.Index = index;
            this.SpanBefore = spanBefore;
        }
    }

    public class SpansTextBox2Suggestion : IComparable<SpansTextBox2Suggestion>
    {
        public string DisplayText { get; set; }
        public string Text { get; set; }
        public string? Description { get; set; }
        public Color TypeLabelColor { get; set; }
        public string Type { get; set; } = "";

        public SpansTextBox2Suggestion(string type, string displayText, string? text = null)
        {
            text ??= displayText;
            this.DisplayText = displayText;
            this.Text = text;
            this.Type = type;
        }

        public SpansTextBox2Suggestion(Core.ExpSrc.ExternEngineItem expItem)
        {
            DisplayText = expItem.Name;
            Text = expItem.Name;
            Description = expItem.Desc;

            if (expItem is ExternEngineFunc func)
            {
                Type = "function";
                TypeLabelColor = Color.DeepPink;

                // add parameters info
                DisplayText += $"({string.Join(", ", func.Params.Map(p => (p.Optional ? "[" : "") + p.Name + (p.Optional ? "]" : "")))})";
                if (func.Params.Length >= 1)
                    Description += "\n\nParameters:\n   " + string.Join("\n   ", func.Params.Map(p => $"{p.Name}{(p.Optional ? " [Optional]" : "")}: {p.Type} {(p.Description == null ? "" : " (" + p.Description + ")")}"));
            }
            else if (expItem is ExternEngineProperty)
            {
                Type = "property";
                TypeLabelColor = Color.IndianRed;
            }
        }

        public int CompareTo(SpansTextBox2Suggestion? other) => string.Compare(DisplayText, other?.DisplayText);

        public override string ToString()
        {
            return DisplayText;
        }
    }

    public class SuggestionsSorter : IComparer<SpansTextBox2Suggestion>
    {
        public string Span { get; set; }
        public int Compare(SpansTextBox2Suggestion a, SpansTextBox2Suggestion b)
        {
            if (Span == "" || Span == null)
                return 0;

            var ignoreCase = StringComparison.CurrentCultureIgnoreCase;
            int indexAtA = a.DisplayText.IndexOf(Span, ignoreCase), indexAtB = b.DisplayText.IndexOf(Span, ignoreCase);
            if (indexAtA == indexAtB)
                return 0;
            if (indexAtA > indexAtB)
                return -1;
            return 1;
        }

        public SuggestionsSorter(string span)
        {
            this.Span = span;
        }
    }

    public class CompletionItemInfoEventArgs : EventArgs
    {
        public CompletionItemInfoEventArgs(string text, SpansTextBox2Suggestion item)
        {
            Text = text;
            Item = item;
        }

        public string Text { get; set; }
        public SpansTextBox2Suggestion Item { get; set; }
    }
}
