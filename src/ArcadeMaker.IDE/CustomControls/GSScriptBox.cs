using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ArcadeMaker.IDE.CustomControls;

namespace ArcadeMaker.IDE
{
    public partial class GSScriptBox : UserControl
    {
        private string text = "";
        FontDialog fontDialog = new FontDialog();
        private int caret = 0;
        private int lineCount = 0;
        private RichTextBox textHolder = null;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectionStart
        {
            get
            {
                return textBox.SelectionStart;
            }
            set
            {
                textBox.SelectionStart = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectionLength
        {
            get
            {
                return textBox.SelectionLength;
            }
            set
            {
                if (value > 0 && !textBox.Focused)
                    textBox.Focus();
                textBox.SelectionLength = value;
            }
        }

        public GSScriptBox()
        {
            InitializeComponent();

            textBox.ShowLineNumbers = true;
            textHolder = new RichTextBox { Font = textBox.Font };
            //textBox.Font = fontDialog.Font;
            //textHolder.Font = fontDialog.Font;
        }

        private void GSScriptBox_Load(object sender, EventArgs e)
        {

        }

        Exp.Spans.TextSpan[] spans = null;

        private void ColorTextBox(int startSpan = 0, int endSpan = -1, RichTextBox textBox = null)
        {
            if (textBox == null)
                textBox = this.textBox;

            skipRefill = true;
            // preventVScroll = true; -------------------------------------------------------------------------------------------------------------------------

            if (textBox.Text != text)
            {
                textBox.Text = text;
            }

            Exp.Spans.TextSpan[] newSpans = Global.GetScriptBoxSpans(text);

            if (endSpan < 0)
            {
                endSpan = newSpans.Length - 1;
            }
            int selectionStart = 0;
            for (int i = 0; i < startSpan; i++)
                selectionStart += newSpans[i].text.Length;
            textBox.SelectionStart = selectionStart;
            textBox.SelectionLength = 0;

            bool spanAdded = spans == null ? false : (newSpans.Length != spans.Length);

            for (int i = startSpan; i <= endSpan; i++)
            {
                if (i < 0 || i >= newSpans.Length)
                    continue;

                Exp.Spans.TextSpan span = newSpans[i];

                if (!(spans != null && i < spans.Length && span.text == spans[i].text && span.color == spans[i].color))
                {
                    // move caret to the span
                    textBox.SelectionStart = selectionStart;

                    // set selection length equal to span length
                    textBox.SelectionLength = span.text.Length;

                    // set selection color
                    textBox.SelectionColor = span.color;
                }

                // save caret position
                selectionStart += span.text.Length;
            }
            textBox.SelectionLength = 0;
            textBox.SelectionStart = caret;

            spans = newSpans;

            preventVScroll = false;
            skipRefill = false;
        }

        

        bool skipText = false;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Text
        {
            get
            {
                return text;
            }
            set
            {
                text = value;

                /*if (spans != null) {
                    int startSpan = 0;
                    int endSpan = 0;

                    int index = 0;
                    for (int i = 0; i < spans.Length; i++)
                    {
                        Span span = spans[i];
                        index += span.text.Length;
                        if (index >= textBox.SelectionStart)
                        {
                            startSpan = i;
                            endSpan = i + 1;
                            if (i > 0)
                                startSpan--;
                            if (i >= spans.Length - 1)
                                endSpan--;
                            break;
                        }
                    }
                    //MessageBox.Show("start: " + startSpan + " end: " + endSpan);
                    ColorTextBox(startSpan, endSpan);
                }
                else*/
                if (!skipText)
                {
                    TextChanged?.Invoke(this, new EventArgs());
                    skipText = true;

                    bool scrollPosFound = false;
                    Point scroll = Point.Empty;
                    try
                    {
                        
                        scroll = textBox.GetScrollPoint();
                        scrollPosFound = true;
                    }
                    catch (Exception ex)
                    {
#if DEBUG
                        MessageBox.Show("Err finding scroll position\n\n" + ex);
#endif
                    }

                    textHolder.Rtf = textBox.Rtf;
                    int caret = textBox.SelectionStart;
                    ColorTextBox(textBox: textHolder);
                    textBox.Rtf = textHolder.Rtf;
                    textBox.SelectionStart = caret;

                    if (scrollPosFound)
                    {
                        try
                        {
                            if (scroll.Y != 0 || scroll.X != 0) textBox.ScrollToPoint(scroll);
                        }
                        catch (Exception ex)
                        {
#if DEBUG
                            MessageBox.Show("Err scrolling back to position\n\n" + ex);
#endif
                        }
                    }
                    skipText = false;
                }
            }
        }

        private void fontBtn_Click(object sender, EventArgs e)
        {
            if (fontDialog.ShowDialog() == DialogResult.OK)
            {
                fontDialog.Font = fontDialog.Font;
                textBox.Font = fontDialog.Font;
            }
        }

        private bool skipRefill = false;
        private bool preventVScroll = false;

        public new event EventHandler<EventArgs> TextChanged;

        private void textBox_TextChanged(object sender, EventArgs e)
        {
            if (!skipRefill)
            {
                caret = textBox.SelectionStart;
                Text = textBox.Text;
                int lc = text.CountOf('\n');
                if (lc != lineCount)
                {
                    lineCount = lc;
                }

                /*
                // IntelliSense
                List<string> methodsComplete = new List<string>();
                var nss = IntelliSense.Translator.GetNamespaces(text);
                foreach (var ns in nss)
                {
                    foreach (var cl in ns.DescendantNodes().OfType<ClassDeclarationSyntax>())
                    {
                        foreach (var m in cl.DescendantNodes().OfType<MethodDeclarationSyntax>())
                        {
                            methodsComplete.Add(m.Identifier.Text);
                        }
                    }
                }
                string ms = "";
                methodsComplete.ForEach(s => ms += s);
                MessageBox.Show(ms);
                */
            }
        }

        private void textBox_VScroll(object sender, EventArgs e)
        {
            return;
            int lineNum = 0;
            for (int i = 0; i < textBox.SelectionStart; i++)
            {
                if (textBox.Text[i] == '\n')
                    lineNum++;
            }
        }

        private void textBox_KeyUp(object sender, KeyEventArgs e)
        {
            // insert tab
            if (e.KeyCode == Keys.Enter)
            {
                // get depth
                int caretLoc = textBox.SelectionStart;
                int depth = 0;
                for (int i = 0; i < caretLoc; i++)
                {
                    char c = textBox.Text[i];
                    if (c == '{')
                        depth++;
                    else if (c == '}')
                        depth--;
                }

                // insert <depth> tabs
                string tabs = "";
                for (int t = 0; t < depth; t++)
                    tabs += "\t";
                spans = null; // to draw all text again
                textBox.Text = textBox.Text.Insert(caretLoc, tabs);
                textBox.SelectionStart = caretLoc + depth;
            }
            // close brackets
            else if (e.KeyCode == Keys.OemCloseBrackets)
            {
                /*
                if (textBox.SelectionStart >= 2 && text[textBox.SelectionStart - 2] == '\t')
                {
                    int rtfPos = 0;
                    int curCount = 0;
                    int count = text.CountOf('}', startIndex: 0, endIndex: textBox.SelectionStart);
                    foreach (char c in text)
                    {
                        if (c == '}')
                        {
                            curCount++;
                            if (curCount >= count)
                                break;
                        }
                        rtfPos++;
                    }
                    textBox.Rtf = textBox.Rtf.Remove(rtfPos - 5, 4);
                }
                */
            }
        }

        private void rtfPrintBtn_Click(object sender, EventArgs e)
        {
            MessageBox.Show(textBox.Rtf);
        }

        private SpansTextBox2SearchForm searchForm = null;
        private void searchBtn_Click(object sender, EventArgs e)
        {
            //if (searchForm == null || searchForm.IsDisposed)
            //    searchForm = new GSScriptBoxSearchForm(this);
            //searchForm.Show();
        }

        /*
        private void GSScriptBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Right)
            {
                if (caret < text.Length - 1)
                    caret++;
            }
            else if (e.KeyCode == Keys.Left)
            {
                if (caret > 0)
                    caret--;
            }
        }
        */
    }

    static class RichTextBoxExtensions
    {
        public static void AppendText(this RichTextBox box, string text, Color color)
        {
            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;

            box.SelectionColor = color;
            box.AppendText(text);
            box.SelectionColor = box.ForeColor;
        }
    }

    class TextEditor : RichTextBoxEx
    {
        public event EventHandler<PaintEventArgs> Paint;
        private const int WM_PAINT = 15;
        private const int WM_VSCROLL = 0x115;
        private const int WM_MOUSEWHEEL = 0x20A;
        private const int WM_USER = 0x400;
        private const int SB_VERT = 1;
        private const int SB_HORZ = 0x0;
        private const int EM_GETSCROLLPOS = WM_USER + 221;

        [DllImport("user32.dll")]
        private static extern bool GetScrollRange(IntPtr hWnd, int nBar, out int lpMinPos, out int lpMaxPos);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, Int32 wMsg, Int32 wParam, ref Point lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int nBar, int nPos, bool bRedraw);

        public Point GetScrollPoint()
        {
            int minScroll;
            int maxScroll;
            GetScrollRange(this.Handle, SB_VERT, out minScroll, out maxScroll);
            Point rtfPoint = Point.Empty;
            SendMessage(this.Handle, EM_GETSCROLLPOS, 0, ref rtfPoint);

            return rtfPoint;
        }

        public void ScrollToPoint(Point point)
        {
            /*
            SendMessage(this.Handle, WM_VSCROLL, point.Y, false);
            SendMessage(this.Handle, WM_HSCROLL, point.X, true);
            */
        }

        /*
        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (IsAtMaxScroll())
                OnScrolledToBottom(EventArgs.Empty);

            base.OnKeyUp(e);
        }
        */

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_PAINT)
            {
                this.Invalidate();
                base.WndProc(ref m);
                using (Graphics g = Graphics.FromHwnd(this.Handle))
                {
                    if (Paint != null)
                    {
                        Paint(this, new PaintEventArgs(g, DisplayRectangle));
                    }
                }
            }
            else
            {
                base.WndProc(ref m);
            }
        }
    }

    class TextEditorScrolledEventArgs : EventArgs
    {
        public Point Point;
    }
}
