using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace ArcadeMaker.IDE
{
    public partial class SpansTextBox : UserControl
    {
        public readonly List<Exp.Spans.TextSpan> Spans = new List<Exp.Spans.TextSpan>();
        public float ScrollX, ScrollY;
        public int SelectionStart = 5, SelectionLength = 4;
        public Color SelectionColor = Color.Black;
        public Color SelectionBackColor = Color.Violet;
        public new Font Font = new Font("Consolas", 9.5F);

        private Point TextLocation = new Point(5, 5);
        private float LineSpacing = 15;
        private const char KeyBackspace = (char)8;

        private readonly Timer CaretTimer = new() { Interval = 1000 };
        private bool CaretFrame = false;

        public new string Text
        {
            get
            {
                string text = "";
                foreach (Exp.Spans.TextSpan span in Spans)
                    text += span.text;
                return text;
            }
        }

        public SpansTextBox()
        {
            InitializeComponent();
        }

        private void SpansTextBox_MouseClick(object sender, MouseEventArgs e)
        {

        }

        private int GetCharIndexFromLocation(Point location)
        {
            // locate the char index
            string text = Text;
            float cx = 0, cy = 0;
            using (Graphics g = CreateGraphics()) // to calculate char size
            {
                // for each char in the text, check if location touches it
                for (int c = 0; c < text.Length; c++)
                {
                    SizeF charSize = g.MeasureString(text[c].ToString(), Font);
                    float x = TextLocation.X + cx - ScrollX, y = TextLocation.Y + cy - ScrollY;
                    if (x <= location.X && x + charSize.Width > location.X &&
                        y <= location.Y && y + charSize.Height > location.Y)
                    {
                        return c + ((location.X >= x + charSize.Width / 2 && c < text.Length) ? 1 : 0);
                    }

                    if (text[c] == '\n')
                    {
                        // new line
                        cx = 0;
                        cy += LineSpacing;
                    }
                    else
                    {
                        // normal span
                        cx += charSize.Width - (text[c] == ' ' ? 0 : 4);
                    }
                }

                int lineNumber = (int)Math.Floor((location.Y + TextLocation.Y - ScrollY) / LineSpacing) - 1;

                // location's y is higher than last line's y, if y < 0 then char index is 0
                if (lineNumber < 0)
                    return 0;

                // char not found, find last char in location line
                for (int c = 0, curLine = 0; c < text.Length; c++)
                {
                    if (text[c] == '\n')
                    {
                        if (c > 0 && curLine == lineNumber)
                        {
                            return c - (c < text.Length - 1 ? 1 : 0);
                        }
                        curLine++;
                    }
                }

                // if y > last line's y then char index is last char's index
                return text.Length - 1;
            }
        }

        private void SpansTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (InterceptKeyDown)
            {
                InterceptKeyDown = false;
                return;
            }

            // insert key char at the span and char index relating to SelectionStart
            string text = Text;
            int spanIndex = 0, spanCharInd = 0;
            for (int i = 0; i < text.Length; i++)
            {
                if (spanCharInd >= Spans[spanIndex].text.Length)
                {
                    spanCharInd = 0;
                    spanIndex++;
                }
                if (i == SelectionStart)
                {
                    if (e.KeyChar != KeyBackspace)
                    {
                        Spans[spanIndex].text = Spans[spanIndex].text.Insert(spanCharInd, e.KeyChar.ToString());
                        var newSpans = Global.GetScriptBoxSpans(Text, splitMultiCommentsLines: true);
                        Spans.Clear();
                        Spans.AddRange(newSpans);
                        SelectionStart++;
                    }
                    else if (SelectionStart > 0)
                    {
                        while (spanCharInd <= 0)
                        {
                            spanIndex--;
                            spanCharInd = Spans[spanIndex].text.Length;
                        }
                        Spans[spanIndex].text = Spans[spanIndex].text.Remove(spanCharInd - 1, 1);
                        SelectionStart--;
                    }
                    break;
                }
                spanCharInd++;
                SetScrollPos();
            }

            // draw
            Invalidate();
        }

        private readonly Keys[] ControlKeys = { Keys.Tab, Keys.Right, Keys.Left, Keys.Up, Keys.Down };
        private bool InterceptKeyDown = false;
        private void SpansTextBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            // prevent lose of focus on Tab and side keys press
            if (ControlKeys.Contains(e.KeyCode))
                e.IsInputKey = true;
            else if (e.Control && e.KeyCode == Keys.C)
            {
                InterceptKeyDown = true;
                Copy();
            }
            else if (e.Control && e.KeyCode == Keys.V)
            {
                InterceptKeyDown = true;
                Paste();
            }
        }

        public void Copy()
        {
            if (SelectionLength > 0)
                Clipboard.SetText(Text.Substring(SelectionStart, SelectionLength));
        }

        public void Paste()
        {
            // find the span at SelectionStart, remove all the selected text and insert clipboard text at the right location of the span
            int spanIndex = 0, spanCharInd = 0, pastePos = -1;
            Exp.Spans.TextSpan pasteSpan = null;
            for (int i = 0; i <= SelectionStart + SelectionLength; i++)
            {
                if (spanCharInd >= Spans[spanIndex].text.Length)
                {
                    spanCharInd = 0;
                    spanIndex++;
                }
                if (i >= SelectionStart)
                {
                    if (i == SelectionStart)
                    {
                        pastePos = spanCharInd;
                        pasteSpan = Spans[spanIndex];
                    }
                    else
                    {
                        Spans[spanIndex].text = Spans[spanIndex].text.Remove(spanCharInd, 1);
                        if (Spans[spanIndex] != pasteSpan && Spans[spanIndex].text.Length == 0)
                        {
                            Spans.Remove(Spans[spanIndex]);
                            spanIndex--;
                        }
                    }
                }
                spanCharInd++;
            }
            if (pasteSpan != null)
                pasteSpan.text = pasteSpan.text.Insert(pastePos, Clipboard.GetText());
        }

        private void SpansTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Right)
                SelectionStart++;
            else if (e.KeyCode == Keys.Left)
                SelectionStart--;
        }

        private void SpansTextBox_Load(object sender, EventArgs e)
        {
            hScrollBar.Location = new Point(0, DisplayRectangle.Height - hScrollBar.Size.Height);
            vScrollBar.Location = new Point(DisplayRectangle.Width - vScrollBar.Size.Width, 0);

            Form owner = FindForm();
            if (owner != null)
                owner.FormClosed += (s, ea) => Dispose();

            CaretTimer.Tick += CaretTimer_Tick;
        }

        private void CaretTimer_Tick(object sender, EventArgs e)
        {
            CaretFrame = !CaretFrame;
            Invalidate();
        }

        private void SpansTextBox_Enter(object sender, EventArgs e)
        {
            CaretTimer.Start();
        }

        private void SpansTextBox_Leave(object sender, EventArgs e)
        {
            CaretTimer.Stop();
        }

        private int MouseLeftDown = -1;
        private void SpansTextBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                SelectionStart = GetCharIndexFromLocation(e.Location);
                SelectionLength = 0;
                MouseLeftDown = SelectionStart;
            }
        }

        private void SpansTextBox_MouseMove(object sender, MouseEventArgs e)
        {
            int lastSelectionStart = SelectionStart, lastSelectionLength = SelectionLength;
            if (MouseLeftDown >= 0)
            {
                int charIndex = GetCharIndexFromLocation(e.Location);
                if (charIndex < MouseLeftDown)
                {
                    SelectionLength = MouseLeftDown - charIndex;
                    SelectionStart = charIndex;
                    Debug.WriteLine("selection start: " + SelectionStart + "    selection length: " + SelectionLength);
                }
                else
                {
                    SelectionLength = charIndex - SelectionStart;
                }
                if (lastSelectionStart != SelectionStart || lastSelectionLength != SelectionLength)
                {
                    Invalidate();
                }
            }
        }

        private void SpansTextBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                MouseLeftDown = -1;
        }

        private void SetScrollPos()
        {
            string text = Text;
            float x = 0, y = 0;
            using (Graphics g = CreateGraphics())
            {
                for (int i = 0; i < SelectionStart; i++)
                {
                    if (text[i] == '\n')
                    {
                        x = 0;
                        y += LineSpacing;
                    }
                    else
                    {
                        x += g.MeasureString(text[i].ToString(), Font).Width - (text[i] == ' ' ? 0 : 4) - 1;
                    }
                }
            }

            if (x - ScrollX >= Size.Width)
                ScrollX = x;
            if (y - ScrollY >= Size.Height)
                ScrollY = y;
        }

        private void vScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            ScrollY = e.NewValue;
        }

        private void hScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            ScrollX = e.NewValue;
        }

        public float textWidth { get; private set; } = 0;
        public float textHeight { get; private set; } = 0;

        private void SpansTextBox_Paint(object sender, PaintEventArgs e)
        {
            textWidth = 0;
            textHeight = 0;

            if (this.Spans.Count == 0)
                return;

            // set origin point to scroll point
            e.Graphics.TranslateTransform(-ScrollX, -ScrollY);

            // draw
            float spanX = 0, spanY = 0;

            int charInd = 0;
            bool drewCaret = false;
            foreach (Exp.Spans.TextSpan span in Spans)
            {
                if (span.text.Length == 0)
                    continue;

                int firstCharInd = charInd;
                charInd += span.text.Length;

                SizeF spanSize = e.Graphics.MeasureString(span.text, Font);

                string unselectedText = span.text;
                string selectedText = null;
                int spanSelectStart = -1, spanSelectLength = -1;
                int SelectionEnd = SelectionStart + SelectionLength;
                if (SelectionLength > 0 && charInd >= SelectionStart && firstCharInd <= SelectionEnd)
                {
                    if (span.text.Length == 1)
                    {
                        spanSelectStart = 0;
                        spanSelectLength = 1;
                    }
                    else
                    {
                        spanSelectStart = firstCharInd >= SelectionStart ? 0 : SelectionStart - firstCharInd;
                        spanSelectLength = SelectionEnd >= charInd ? span.text.Length - spanSelectStart : span.text.Length - (charInd - SelectionEnd);
                        if (spanSelectLength < 0)
                            spanSelectLength = 0;
                        else while (spanSelectStart + spanSelectLength > span.text.Length)
                            spanSelectLength--;
                    }

                    selectedText = span.text.Substring(spanSelectStart, spanSelectLength);

                    using (Pen spanColorPen = new Pen(span.color))
                    {
                        // draw the text that is before selection
                        string beforeSelection = span.text.Substring(0, spanSelectStart);
                        SizeF beforeSelection_size = new SizeF(0, 0);
                        if (beforeSelection.Length > 0)
                        {
                            e.Graphics.DrawString(beforeSelection, Font, new Pen(span.color).Brush, TextLocation.X + spanX, TextLocation.Y + spanY);
                            beforeSelection_size = e.Graphics.MeasureString(beforeSelection, Font);
                        }

                        // highlight selected text
                        int space = spanSelectStart == 0 ? 0 : 4;
                        SizeF selection_size = e.Graphics.MeasureString(selectedText, Font);
                        float selectionX = TextLocation.X + spanX + beforeSelection_size.Width - space;
                        using (Pen selectionBackPen = new Pen(SelectionBackColor))
                            e.Graphics.FillRectangle(selectionBackPen.Brush, selectionX, TextLocation.Y + spanY, selection_size.Width, selection_size.Height);

                        // draw the selected text
                        using (Pen selectionTextPen = new Pen(SelectionColor))
                            e.Graphics.DrawString(selectedText, Font, selectionTextPen.Brush, selectionX, TextLocation.Y + spanY);

                        // draw the text that is after selection
                        string afterSelection = span.text.Substring(spanSelectStart + spanSelectLength);
                        float afterSelectionX = TextLocation.X + spanX + beforeSelection_size.Width + selection_size.Width - 4;
                        if (afterSelection.Length > 0)
                            e.Graphics.DrawString(afterSelection, Font, spanColorPen.Brush, afterSelectionX, TextLocation.Y + spanY);
                        SizeF afterSelection_size = e.Graphics.MeasureString(afterSelection, Font);

                        // set text width
                        if (afterSelection_size.Width + afterSelectionX > textWidth)
                            textWidth = afterSelection_size.Width + afterSelectionX;

                        // set text height
                        SizeF[] allTextSizes = { beforeSelection_size, selection_size, afterSelection_size };
                        foreach (SizeF size in allTextSizes)
                        {
                            if (size.Height + spanY > textHeight)
                            {
                                textHeight = size.Height + spanY;
                            }
                        }
                    }
                }
                else
                {
                    // draw text without selection
                    using (Pen pen = new Pen(span.color))
                        e.Graphics.DrawString(span.text, Font, pen.Brush, TextLocation.X + spanX, TextLocation.Y + spanY);

                    // set text width
                    if (spanSize.Width + spanX > textWidth)
                        textWidth = spanSize.Width + spanX;

                    // set text height
                    if (spanSize.Height + spanY > textHeight)
                        textHeight = spanSize.Height + spanY;
                }

                if (span.text.Contains("\n"))
                {
                    // new line
                    spanX = 0;
                    spanY += LineSpacing;
                }
                else
                {
                    // normal span
                    spanX += spanSize.Width - (span.text == " " ? 0 : 4);
                }

                if (CaretFrame && !drewCaret && charInd > SelectionStart)
                {
                    drewCaret = true;
                    float caretX = TextLocation.X + spanX - e.Graphics.MeasureString(span.text.Substring(span.text.Length - (charInd - SelectionStart - 1)), Font).Width, caretY = TextLocation.Y + spanY;
                    e.Graphics.DrawLine(Pens.Black, caretX, caretY, caretX, caretY + LineSpacing);
                }
            }

            SetScrollBars();
        }

        private void SetScrollBars()
        {
            // set scroll bars
            hScrollBar.Visible = textWidth + TextLocation.X > Size.Width;
            vScrollBar.Visible = textHeight + TextLocation.Y > Size.Height;

            hScrollBar.Maximum = (int)textWidth;
            vScrollBar.Maximum = (int)textHeight;
        }

        public void Dispose()
        {
            CaretTimer.Tick -= CaretTimer_Tick;
        }
    }
}