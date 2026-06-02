using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Drawing2D;
using Ionic;

namespace ArcadeMaker.IDE.CustomControls
{
    [ToolboxBitmap(typeof(RichTextBox))]
    public class RichTextBoxEx : RichTextBox
    {
        private User32.CHARFORMAT charFormat;
        private IntPtr lParam1;

        private int _savedScrollLine;
        private int _savedSelectionStart;
        private int _savedSelectionEnd;
        private Pen _borderPen;
        private System.Drawing.StringFormat _stringDrawingFormat;
        private System.Security.Cryptography.HashAlgorithm alg;   // used for comparing text values

        public RichTextBoxEx()
        {
            charFormat = new User32.CHARFORMAT()
            {
                cbSize = Marshal.SizeOf(typeof(User32.CHARFORMAT)),
                szFaceName = new char[32]
            };

            lParam1 = Marshal.AllocCoTaskMem(charFormat.cbSize);

            // defaults
            NumberFont = new System.Drawing.Font("Consolas",
                                                9.75F,
                                                System.Drawing.FontStyle.Regular,
                                                System.Drawing.GraphicsUnit.Point, ((byte)(0)));

            NumberColor = Color.FromName("DarkGray");
            NumberLineCounting = LineCounting.CRLF;
            NumberAlignment = StringAlignment.Center;
            NumberBorder = SystemColors.ControlDark;
            NumberBorderThickness = 1;
            NumberPadding = 2;
            NumberBackground1 = SystemColors.ControlLight;
            NumberBackground2 = SystemColors.Window;
            SetStringDrawingFormat();

            alg = System.Security.Cryptography.SHA1.Create();
        }

        ~RichTextBoxEx()
        {
            // Free the allocated memory
            Marshal.FreeCoTaskMem(lParam1);
        }

        private void SetStringDrawingFormat()
        {
            _stringDrawingFormat = new System.Drawing.StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = NumberAlignment,
                Trimming = StringTrimming.None,
            };
        }


        protected override void OnTextChanged(EventArgs e)
        {
            NeedRecomputeOfLineNumbers();
            base.OnTextChanged(e);
        }

        public void BeginUpdate()
        {
            User32.SendMessage(this.Handle, (int)User32.Msgs.WM_SETREDRAW, 0, IntPtr.Zero);
        }

        public void EndUpdate()
        {
            User32.SendMessage(this.Handle, (int)User32.Msgs.WM_SETREDRAW, 1, IntPtr.Zero);
        }


        public IntPtr BeginUpdateAndSuspendEvents()
        {
            // Stop redrawing:
            User32.SendMessage(this.Handle, (int)User32.Msgs.WM_SETREDRAW, 0, IntPtr.Zero);
            // Stop sending of events:
            IntPtr eventMask = User32.SendMessage(this.Handle, User32.Msgs.EM_GETEVENTMASK, 0, IntPtr.Zero);

            return eventMask;
        }

        public void EndUpdateAndResumeEvents(IntPtr eventMask)
        {
            // turn on events
            User32.SendMessage(this.Handle, User32.Msgs.EM_SETEVENTMASK, 0, eventMask);
            // turn on redrawing
            User32.SendMessage(this.Handle, User32.Msgs.WM_SETREDRAW, 1, IntPtr.Zero);
            NeedRecomputeOfLineNumbers();
            this.Invalidate();
        }



        public void GetSelection(out int start, out int end)
        {
            User32.SendMessageRef(this.Handle, (int)User32.Msgs.EM_GETSEL, out start, out end);
        }

        public void SetSelection(int start, int end)
        {
            User32.SendMessage(this.Handle, (int)User32.Msgs.EM_SETSEL, start, end);
        }

        public void BeginUpdateAndSaveState()
        {
            User32.SendMessage(this.Handle, (int)User32.Msgs.WM_SETREDRAW, 0, IntPtr.Zero);
            // save scroll position
            _savedScrollLine = FirstVisibleDisplayLine;

            // save selection
            GetSelection(out _savedSelectionStart, out _savedSelectionEnd);
        }

        public void EndUpdateAndRestoreState()
        {
            // restore scroll position
            int Line1 = FirstVisibleDisplayLine;
            Scroll(_savedScrollLine - Line1);

            // restore the selection/caret
            SetSelection(_savedSelectionStart, _savedSelectionEnd);

            // allow redraw
            User32.SendMessage(this.Handle, (int)User32.Msgs.WM_SETREDRAW, 1, IntPtr.Zero);

            // explicitly ask for a redraw?
            Refresh();
        }

        private String _sformat;
        private int _ndigits;
        private int _lnw = -1;
        private int LineNumberWidth
        {
            get
            {
                if (_lnw > 0) return _lnw;
                if (NumberLineCounting == LineCounting.CRLF)
                {
                    _ndigits = (CharIndexForTextLine.Length == 0)
                        ? 1
                        : (int)(1 + Math.Log((double)CharIndexForTextLine.Length, 10));
                }
                else
                {
                    int n = GetDisplayLineCount();
                    _ndigits = (n == 0)
                        ? 1
                        : (int)(1 + Math.Log((double)n, 10));
                }
                var s = new String('0', _ndigits);
                var b = new Bitmap(400, 400); // in pixels
                var g = Graphics.FromImage(b);
                SizeF size = g.MeasureString(s, NumberFont);
                g.Dispose();
                _lnw = NumberPadding * 2 + 4 + (int)(size.Width + 0.5 + NumberBorderThickness);
                _sformat = "{0:D" + _ndigits + "}";
                return _lnw;
            }
        }


        public bool _lineNumbers;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowLineNumbers
        {
            get
            {
                return _lineNumbers;
            }
            set
            {
                if (value == _lineNumbers) return;
                SetLeftMargin(value ? LineNumberWidth + Margin.Left : Margin.Left);
                _lineNumbers = value;
                User32.SendMessage(this.Handle, User32.Msgs.WM_PAINT, 0, 0);
            }
        }

        private void NeedRecomputeOfLineNumbers()
        {
            //System.Console.WriteLine("Need Recompute of line numbers...");
            _CharIndexForTextLine = null;
            _Text2 = null;
            _lnw = -1;

            if (_paintingDisabled) return;

            User32.SendMessage(this.Handle, User32.Msgs.WM_PAINT, 0, 0);
        }

        private Font _NumberFont;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Font NumberFont
        {
            get { return _NumberFont; }
            set
            {
                if (_NumberFont == value) return;
                _lnw = -1;
                _NumberFont = value;
                User32.SendMessage(this.Handle, User32.Msgs.WM_PAINT, 0, 0);
            }
        }

        private LineCounting _NumberLineCounting;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public LineCounting NumberLineCounting
        {
            get { return _NumberLineCounting; }
            set
            {
                if (_NumberLineCounting == value) return;
                _lnw = -1;
                _NumberLineCounting = value;
                User32.SendMessage(this.Handle, User32.Msgs.WM_PAINT, 0, 0);
            }
        }

        private StringAlignment _NumberAlignment;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public StringAlignment NumberAlignment
        {
            get { return _NumberAlignment; }
            set
            {
                if (_NumberAlignment == value) return;
                _NumberAlignment = value;
                SetStringDrawingFormat();
                User32.SendMessage(this.Handle, User32.Msgs.WM_PAINT, 0, 0);
            }
        }

        private Color _NumberColor;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color NumberColor
        {
            get { return _NumberColor; }
            set
            {
                if (_NumberColor.ToArgb() == value.ToArgb()) return;
                _NumberColor = value;
                User32.SendMessage(this.Handle, User32.Msgs.WM_PAINT, 0, 0);
            }
        }

        private bool _NumberLeadingZeroes;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool NumberLeadingZeroes
        {
            get { return _NumberLeadingZeroes; }
            set
            {
                if (_NumberLeadingZeroes == value) return;
                _NumberLeadingZeroes = value;
                User32.SendMessage(this.Handle, User32.Msgs.WM_PAINT, 0, 0);
            }
        }

        private Color _NumberBorder;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color NumberBorder
        {
            get { return _NumberBorder; }
            set
            {
                if (_NumberBorder.ToArgb() == value.ToArgb()) return;
                _NumberBorder = value;
                NewBorderPen();
                User32.SendMessage(this.Handle, User32.Msgs.WM_PAINT, 0, 0);
            }
        }

        private int _NumberPadding;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int NumberPadding
        {
            get { return _NumberPadding; }
            set
            {
                if (_NumberPadding == value) return;
                _lnw = -1;
                _NumberPadding = value;
                User32.SendMessage(this.Handle, User32.Msgs.WM_PAINT, 0, 0);
            }
        }

        public Single _NumberBorderThickness;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Single NumberBorderThickness
        {
            get { return _NumberBorderThickness; }
            set
            {
                if (_NumberBorderThickness == value) return;
                _lnw = -1;
                _NumberBorderThickness = value;
                NewBorderPen();
                User32.SendMessage(this.Handle, User32.Msgs.WM_PAINT, 0, 0);
            }
        }

        private Color _NumberBackground1;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color NumberBackground1
        {
            get { return _NumberBackground1; }
            set
            {
                if (_NumberBackground1.ToArgb() == value.ToArgb()) return;
                _NumberBackground1 = value;
                User32.SendMessage(this.Handle, User32.Msgs.WM_PAINT, 0, 0);
            }
        }

        private Color _NumberBackground2;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color NumberBackground2
        {
            get { return _NumberBackground2; }
            set
            {
                if (_NumberBackground2.ToArgb() == value.ToArgb()) return;
                _NumberBackground2 = value;
                User32.SendMessage(this.Handle, User32.Msgs.WM_PAINT, 0, 0);
            }
        }


        private bool _paintingDisabled;
        public void SuspendLineNumberPainting()
        {
            _paintingDisabled = true;
        }
        public void ResumeLineNumberPainting()
        {
            _paintingDisabled = false;
        }


        private void NewBorderPen()
        {
            _borderPen = new Pen(NumberBorder);
            _borderPen.Width = NumberBorderThickness;
            _borderPen.SetLineCap(LineCap.Round, LineCap.Round, DashCap.Round);
        }



        private DateTime _lastMsgRecd = new DateTime(1901, 1, 1);

        protected override void WndProc(ref Message m)
        {
            bool handled = false;
            switch (m.Msg)
            {
                case (int)User32.Msgs.WM_PAINT:
                    //System.Console.WriteLine("{0}", User32.Mnemonic(m.Msg));
                    //System.Console.Write(".");
                    if (_paintingDisabled) return;
                    if (_lineNumbers)
                    {
                        base.WndProc(ref m);
                        this.PaintLineNumbers();
                        handled = true;
                    }
                    break;

                case (int)User32.Msgs.WM_CHAR:
                    // the text is being modified
                    NeedRecomputeOfLineNumbers();
                    break;

                    //                 case (int)User32.Msgs.EM_POSFROMCHAR:
                    //                 case (int)User32.Msgs.WM_GETDLGCODE:
                    //                 case (int)User32.Msgs.WM_ERASEBKGND:
                    //                 case (int)User32.Msgs.OCM_COMMAND:
                    //                 case (int)User32.Msgs.OCM_NOTIFY:
                    //                 case (int)User32.Msgs.EM_CHARFROMPOS:
                    //                 case (int)User32.Msgs.EM_LINEINDEX:
                    //                 case (int)User32.Msgs.WM_NCHITTEST:
                    //                 case (int)User32.Msgs.WM_SETCURSOR:
                    //                 case (int)User32.Msgs.WM_KEYUP:
                    //                 case (int)User32.Msgs.WM_KEYDOWN:
                    //                 case (int)User32.Msgs.WM_MOUSEMOVE:
                    //                 case (int)User32.Msgs.WM_MOUSEACTIVATE:
                    //                 case (int)User32.Msgs.WM_NCMOUSEMOVE:
                    //                 case (int)User32.Msgs.WM_NCMOUSEHOVER:
                    //                 case (int)User32.Msgs.WM_NCMOUSELEAVE:
                    //                 case (int)User32.Msgs.WM_NCLBUTTONDOWN:
                    //                     break;
                    //
                    //                   default:
                    //                       // divider
                    //                       var now = DateTime.Now;
                    //                       if ((now - _lastMsgRecd) > TimeSpan.FromMilliseconds(850))
                    //                           System.Console.WriteLine("------------ {0}", now.ToString("G"));
                    //                       _lastMsgRecd = now;
                    //
                    //                       System.Console.WriteLine("{0}", User32.Mnemonic(m.Msg));
                    //                       break;
            }

            if (!handled)
                base.WndProc(ref m);
        }


        int _lastWidth = 0;
        private void PaintLineNumbers()
        {
            //System.Console.WriteLine(">> PaintLineNumbers");
            // To reduce flicker, double-buffer the output

            if (_paintingDisabled) return;

            int w = LineNumberWidth;
            if (w != _lastWidth)
            {
                //System.Console.WriteLine("  WIDTH change {0} != {1}", _lastWidth, w);
                SetLeftMargin(w + Margin.Left);
                _lastWidth = w;
                // Don't bother painting line numbers - the margin isn't wide enough currently.
                // Ask for a new paint, and paint them next time round.
                User32.SendMessage(this.Handle, User32.Msgs.WM_PAINT, 0, 0);
                return;
            }

            Bitmap buffer = new Bitmap(w, this.Bounds.Height);
            Graphics g = Graphics.FromImage(buffer);

            Brush forebrush = new SolidBrush(NumberColor);
            var rect = new Rectangle(0, 0, w, this.Bounds.Height);

            bool wantDivider = NumberBackground1.ToArgb() == NumberBackground2.ToArgb();
            Brush backBrush = (wantDivider)
                ? (Brush)new SolidBrush(NumberBackground2)
                : SystemBrushes.Window;

            g.FillRectangle(backBrush, rect);

            int n = (NumberLineCounting == LineCounting.CRLF)
                ? NumberOfVisibleTextLines
                : NumberOfVisibleDisplayLines;

            int first = (NumberLineCounting == LineCounting.CRLF)
                ? FirstVisibleTextLine
                : FirstVisibleDisplayLine + 1;

            int py = 0;
            int w2 = w - 2 - (int)NumberBorderThickness;
            LinearGradientBrush brush;
            Pen dividerPen = new Pen(NumberColor);

            for (int i = 0; i <= n; i++)
            {
                int ix = first + i;
                int c = (NumberLineCounting == LineCounting.CRLF)
                    ? GetCharIndexForTextLine(ix)
                    : GetCharIndexForDisplayLine(ix) - 1;

                var p = GetPosFromCharIndex(c + 1);

                Rectangle r4 = Rectangle.Empty;

                if (i == n) // last line?
                {
                    if (this.Bounds.Height <= py) continue;
                    r4 = new Rectangle(1, py, w2, this.Bounds.Height - py);
                }
                else
                {
                    if (p.Y <= py) continue;
                    r4 = new Rectangle(1, py, w2, p.Y - py);
                }

                if (wantDivider)
                {
                    if (i != n)
                        g.DrawLine(dividerPen, 1, p.Y + 1, w2, p.Y + 1); // divider line
                }
                else
                {
                    // new brush each time for gradient across variable rect sizes
                    brush = new LinearGradientBrush(r4,
                                                     NumberBackground1,
                                                     NumberBackground2,
                                                     LinearGradientMode.Vertical);
                    g.FillRectangle(brush, r4);
                }

                if (NumberLineCounting == LineCounting.CRLF) ix++;

                // conditionally slide down
                if (NumberAlignment == StringAlignment.Near)
                    rect.Offset(0, 3);

                var s = (NumberLeadingZeroes) ? String.Format(_sformat, ix) : ix.ToString();
                g.DrawString(s, NumberFont, forebrush, r4, _stringDrawingFormat);
                py = p.Y;
            }

            if (NumberBorderThickness != 0.0)
            {
                int t = (int)(w - (NumberBorderThickness + 0.5) / 2) - 1;
                g.DrawLine(_borderPen, t, 0, t, this.Bounds.Height);
                //g.DrawLine(_borderPen, w-2, 0, w-2, this.Bounds.Height);
            }

            // paint that buffer to the screen
            Graphics g1 = this.CreateGraphics();
            g1.DrawImage(buffer, new Point(0, 0));
            g1.Dispose();
            g.Dispose();
        }



        private int GetCharIndexFromPos(int x, int y)
        {
            var p = new User32.POINTL { X = x, Y = y };
            int rawSize = Marshal.SizeOf(typeof(User32.POINTL));
            IntPtr lParam = Marshal.AllocHGlobal(rawSize);
            Marshal.StructureToPtr(p, lParam, false);
            int r = User32.SendMessage(this.Handle, (int)User32.Msgs.EM_CHARFROMPOS, 0, lParam);
            Marshal.FreeHGlobal(lParam);
            return r;
        }


        private Point GetPosFromCharIndex(int ix)
        {
            int rawSize = Marshal.SizeOf(typeof(User32.POINTL));
            IntPtr wParam = Marshal.AllocHGlobal(rawSize);
            int r = User32.SendMessage(this.Handle, (int)User32.Msgs.EM_POSFROMCHAR, (int)wParam, ix);

            User32.POINTL p1 = (User32.POINTL)Marshal.PtrToStructure(wParam, typeof(User32.POINTL));

            Marshal.FreeHGlobal(wParam);
            var p = new Point { X = p1.X, Y = p1.Y };
            return p;
        }


        private int GetLengthOfLineContainingChar(int charIndex)
        {
            int r = User32.SendMessage(this.Handle, (int)User32.Msgs.EM_LINELENGTH, 0, 0);
            return r;
        }

        private int GetLineFromChar(int charIndex)
        {
            return User32.SendMessage(this.Handle, (int)User32.Msgs.EM_LINEFROMCHAR, charIndex, 0);
        }

        private int GetCharIndexForDisplayLine(int line)
        {
            return User32.SendMessage(this.Handle, (int)User32.Msgs.EM_LINEINDEX, line, 0);
        }

        private int GetDisplayLineCount()
        {
            return User32.SendMessage(this.Handle, (int)User32.Msgs.EM_GETLINECOUNT, 0, 0);
        }


        /// <summary>
        ///   Sets the color of the characters in the given range.
        /// </summary>
        ///
        /// <remarks>
        /// Calling this is equivalent to calling
        /// <code>
        ///   richTextBox.Select(start, end-start);
        ///   this.richTextBox1.SelectionColor = color;
        /// </code>
        /// ...but without the error and bounds checking.
        /// </remarks>
        ///
        public void SetSelectionColor(int start, int end, System.Drawing.Color color)
        {
            User32.SendMessage(this.Handle, (int)User32.Msgs.EM_SETSEL, start, end);

            charFormat.dwMask = 0x40000000;
            charFormat.dwEffects = 0;
            charFormat.crTextColor = System.Drawing.ColorTranslator.ToWin32(color);

            Marshal.StructureToPtr(charFormat, lParam1, false);
            User32.SendMessage(this.Handle, (int)User32.Msgs.EM_SETCHARFORMAT, User32.SCF_SELECTION, lParam1);
        }


        private void SetLeftMargin(int widthInPixels)
        {
            User32.SendMessage(this.Handle, (int)User32.Msgs.EM_SETMARGINS, User32.EC_LEFTMARGIN,
                               widthInPixels);
        }

        public Tuple<int, int> GetMargins()
        {
            int r = User32.SendMessage(this.Handle, (int)User32.Msgs.EM_GETMARGINS, 0, 0);
            return Tuple.New(r & 0x0000FFFF, (int)((r >> 16) & 0x0000FFFF));
        }

        public void Scroll(int delta)
        {
            User32.SendMessage(this.Handle, (int)User32.Msgs.EM_LINESCROLL, 0, delta);
        }


        private int FirstVisibleDisplayLine
        {
            get
            {
                return User32.SendMessage(this.Handle, (int)User32.Msgs.EM_GETFIRSTVISIBLELINE, 0, 0);
            }
            set
            {
                // scroll
                int current = FirstVisibleDisplayLine;
                int delta = value - current;
                User32.SendMessage(this.Handle, (int)User32.Msgs.EM_LINESCROLL, 0, delta);
            }
        }

        private int NumberOfVisibleDisplayLines
        {
            get
            {
                int topIndex = this.GetCharIndexFromPosition(new System.Drawing.Point(1, 1));
                int bottomIndex = this.GetCharIndexFromPosition(new System.Drawing.Point(1, this.Height - 1));
                int topLine = this.GetLineFromCharIndex(topIndex);
                int bottomLine = this.GetLineFromCharIndex(bottomIndex);
                int n = bottomLine - topLine + 1;
                return n;
            }
        }

        private int FirstVisibleTextLine
        {
            get
            {
                int c = GetCharIndexFromPos(1, 1);
                for (int i = 0; i < CharIndexForTextLine.Length; i++)
                {
                    if (c < CharIndexForTextLine[i]) return i;
                }
                return CharIndexForTextLine.Length;
            }
        }

        private int LastVisibleTextLine
        {
            get
            {
                int c = GetCharIndexFromPos(1, this.Bounds.Y + this.Bounds.Height);
                for (int i = 0; i < CharIndexForTextLine.Length; i++)
                {
                    if (c < CharIndexForTextLine[i]) return i;
                }
                return CharIndexForTextLine.Length;
            }
        }

        private int NumberOfVisibleTextLines
        {
            get
            {
                return LastVisibleTextLine - FirstVisibleTextLine;
            }
        }


        public int FirstVisibleLine
        {
            get
            {
                if (this.NumberLineCounting == LineCounting.CRLF)
                    return FirstVisibleTextLine;
                else
                    return FirstVisibleDisplayLine;
            }
        }

        public int NumberOfVisibleLines
        {
            get
            {
                if (this.NumberLineCounting == LineCounting.CRLF)
                    return NumberOfVisibleTextLines;
                else
                    return NumberOfVisibleDisplayLines;
            }
        }

        private int GetCharIndexForTextLine(int ix)
        {
            if (ix >= CharIndexForTextLine.Length) return 0;
            if (ix < 0) return 0;
            return CharIndexForTextLine[ix];
        }



        // The char index is expensive to compute.

        private int[] _CharIndexForTextLine;
        private int[] CharIndexForTextLine
        {
            get
            {
                if (_CharIndexForTextLine == null)
                {
                    var list = new List<int>();
                    int ix = 0;
                    foreach (var c in Text2)
                    {
                        if (c == '\n') list.Add(ix);
                        ix++;
                    }
                    _CharIndexForTextLine = list.ToArray();
                }
                return _CharIndexForTextLine;
            }

        }


        private String _Text2;
        private String Text2
        {
            get
            {
                if (_Text2 == null)
                    _Text2 = this.Text;
                return _Text2;
            }
        }

        private bool CompareHashes(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i]) return false;
            }
            return true;  // they are equal
        }



        public enum LineCounting
        {
            CRLF,
            AsDisplayed
        }

    }



    public static class Tuple
    {
        // Allows Tuple.New(1, "2") instead of new Tuple<int, string>(1, "2")
        public static Tuple<T1, T2> New<T1, T2>(T1 v1, T2 v2)
        {
            return new Tuple<T1, T2>(v1, v2);
        }
    }

    public class Tuple<T1, T2>
    {
        public Tuple(T1 v1, T2 v2)
        {
            V1 = v1;
            V2 = v2;
        }

        public T1 V1 { get; set; }
        public T2 V2 { get; set; }
    }
}

// User32.cs
// ------------------------------------------------------------------
//
// Wrapper for User32.dll methods, etc.
//
// Time-stamp: <2010-April-23 18:49:20>
// ------------------------------------------------------------------
//
// Copyright (c) 2010 by Dino Chiesa
// All rights reserved!
//
// ------------------------------------------------------------------

namespace Ionic
{
    public static class User32
    {
        public enum Styles : uint
        {
            WS_OVERLAPPED = 0x00000000,
            WS_POPUP = 0x80000000,
            WS_CHILD = 0x40000000,
            WS_MINIMIZE = 0x20000000,
            WS_VISIBLE = 0x10000000,
            WS_DISABLED = 0x08000000,
            WS_CLIPSIBLINGS = 0x04000000,
            WS_CLIPCHILDREN = 0x02000000,
            WS_MAXIMIZE = 0x01000000,
            WS_CAPTION = 0x00C00000,
            WS_BORDER = 0x00800000,
            WS_DLGFRAME = 0x00400000,
            WS_VSCROLL = 0x00200000,
            WS_HSCROLL = 0x00100000,
            WS_SYSMENU = 0x00080000,
            WS_THICKFRAME = 0x00040000,
            WS_GROUP = 0x00020000,
            WS_TABSTOP = 0x00010000,
            GWL_STYLE = 0xFFFFFFF0,
        }

        public enum Msgs
        {
            // GetWindow
            GW_HWNDFIRST = 0,
            GW_HWNDLAST = 1,
            GW_HWNDNEXT = 2,
            GW_HWNDPREV = 3,
            GW_OWNER = 4,
            GW_CHILD = 5,

            // Window messages - WinUser.h
            WM_NULL = 0x0000,
            WM_CREATE = 0x0001,
            WM_DESTROY = 0x0002,
            WM_MOVE = 0x0003,
            WM_SIZE = 0x0005,
            WM_KILLFOCUS = 0x0008,
            WM_SETREDRAW = 0x000B,
            WM_GETTEXT = 0x000D,
            WM_GETTEXTLENGTH = 0x000E,
            WM_PAINT = 0x000F,
            WM_ERASEBKGND = 0x0014,
            WM_SHOWWINDOW = 0x0018,

            WM_FONTCHANGE = 0x001d,
            WM_SETCURSOR = 0x0020,
            WM_MOUSEACTIVATE = 0x0021,
            WM_CHILDACTIVATE = 0x0022,

            WM_DRAWITEM = 0x002B,
            WM_MEASUREITEM = 0x002C,
            WM_DELETEITEM = 0x002D,
            WM_VKEYTOITEM = 0x002E,
            WM_CHARTOITEM = 0x002F,

            WM_SETFONT = 0x0030,
            WM_COMPAREITEM = 0x0039,
            WM_WINDOWPOSCHANGING = 0x0046,
            WM_WINDOWPOSCHANGED = 0x0047,
            WM_NOTIFY = 0x004E,
            WM_NOTIFYFORMAT = 0x0055,
            WM_STYLECHANGING = 0x007C,
            WM_STYLECHANGED = 0x007D,
            WM_NCMOUSEMOVE = 0x00A0,
            WM_NCLBUTTONDOWN = 0x00A1,

            WM_NCCREATE = 0x0081,
            WM_NCDESTROY = 0x0082,
            WM_NCCALCSIZE = 0x0083,
            WM_NCHITTEST = 0x0084,
            WM_NCPAINT = 0x0085,
            WM_GETDLGCODE = 0x0087,

            // from WinUser.h and RichEdit.h
            EM_GETSEL = 0x00B0,
            EM_SETSEL = 0x00B1,
            EM_GETRECT = 0x00B2,
            EM_SETRECT = 0x00B3,
            EM_SETRECTNP = 0x00B4,
            EM_SCROLL = 0x00B5,
            EM_LINESCROLL = 0x00B6,
            //EM_SCROLLCARET       = 0x00B7,
            EM_GETMODIFY = 0x00B8,
            EM_SETMODIFY = 0x00B9,
            EM_GETLINECOUNT = 0x00BA,
            EM_LINEINDEX = 0x00BB,
            EM_SETHANDLE = 0x00BC,
            EM_GETHANDLE = 0x00BD,
            EM_GETTHUMB = 0x00BE,
            EM_LINELENGTH = 0x00C1,
            EM_LINEFROMCHAR = 0x00C9,
            EM_GETFIRSTVISIBLELINE = 0x00CE,
            EM_SETMARGINS = 0x00D3,
            EM_GETMARGINS = 0x00D4,
            EM_POSFROMCHAR = 0x00D6,
            EM_CHARFROMPOS = 0x00D7,

            WM_KEYFIRST = 0x0100,
            WM_KEYDOWN = 0x0100,
            WM_KEYUP = 0x0101,
            WM_CHAR = 0x0102,
            WM_DEADCHAR = 0x0103,
            WM_SYSKEYDOWN = 0x0104,
            WM_SYSKEYUP = 0x0105,
            WM_SYSCHAR = 0x0106,
            WM_SYSDEADCHAR = 0x0107,

            WM_COMMAND = 0x0111,
            WM_SYSCOMMAND = 0x0112,
            WM_TIMER = 0x0113,
            WM_HSCROLL = 0x0114,
            WM_VSCROLL = 0x0115,
            WM_UPDATEUISTATE = 0x0128,
            WM_QUERYUISTATE = 0x0129,
            WM_MOUSEFIRST = 0x0200,
            WM_MOUSEMOVE = 0x0200,
            WM_LBUTTONDOWN = 0x0201,
            WM_LBUTTONUP = 0x0202,
            WM_PARENTNOTIFY = 0x0210,

            WM_NEXTMENU = 0x0213,
            WM_SIZING = 0x0214,
            WM_CAPTURECHANGED = 0x0215,
            WM_MOVING = 0x0216,

            WM_IME_SETCONTEXT = 0x0281,
            WM_IME_NOTIFY = 0x0282,
            WM_IME_CONTROL = 0x0283,
            WM_IME_COMPOSITIONFULL = 0x0284,
            WM_IME_SELECT = 0x0285,
            WM_IME_CHAR = 0x0286,
            WM_IME_REQUEST = 0x0288,
            WM_IME_KEYDOWN = 0x0290,
            WM_IME_KEYUP = 0x0291,
            WM_NCMOUSEHOVER = 0x02A0,
            WM_NCMOUSELEAVE = 0x02A2,
            WM_MOUSEHOVER = 0x02A1,
            WM_MOUSELEAVE = 0x02A3,

            WM_CUT = 0x0300,
            WM_COPY = 0x0301,
            WM_PASTE = 0x0302,
            WM_CLEAR = 0x0303,
            WM_UNDO = 0x0304,
            WM_RENDERFORMAT = 0x0305,
            WM_RENDERALLFORMATS = 0x0306,
            WM_DESTROYCLIPBOARD = 0x0307,
            WM_DRAWCLIPBOARD = 0x0308,
            WM_PAINTCLIPBOARD = 0x0309,
            WM_VSCROLLCLIPBOARD = 0x030A,
            WM_SIZECLIPBOARD = 0x030B,
            WM_ASKCBFORMATNAME = 0x030C,
            WM_CHANGECBCHAIN = 0x030D,
            WM_HSCROLLCLIPBOARD = 0x030E,
            WM_QUERYNEWPALETTE = 0x030F,
            WM_PALETTEISCHANGING = 0x0310,
            WM_PALETTECHANGED = 0x0311,
            WM_HOTKEY = 0x0312,

            WM_USER = 0x0400,
            EM_SCROLLCARET = (WM_USER + 49),

            EM_CANPASTE = (WM_USER + 50),
            EM_DISPLAYBAND = (WM_USER + 51),
            EM_EXGETSEL = (WM_USER + 52),
            EM_EXLIMITTEXT = (WM_USER + 53),
            EM_EXLINEFROMCHAR = (WM_USER + 54),
            EM_EXSETSEL = (WM_USER + 55),
            EM_FINDTEXT = (WM_USER + 56),
            EM_FORMATRANGE = (WM_USER + 57),
            EM_GETCHARFORMAT = (WM_USER + 58),
            EM_GETEVENTMASK = (WM_USER + 59),
            EM_GETOLEINTERFACE = (WM_USER + 60),
            EM_GETPARAFORMAT = (WM_USER + 61),
            EM_GETSELTEXT = (WM_USER + 62),
            EM_HIDESELECTION = (WM_USER + 63),
            EM_PASTESPECIAL = (WM_USER + 64),
            EM_REQUESTRESIZE = (WM_USER + 65),
            EM_SELECTIONTYPE = (WM_USER + 66),
            EM_SETBKGNDCOLOR = (WM_USER + 67),
            EM_SETCHARFORMAT = (WM_USER + 68),
            EM_SETEVENTMASK = (WM_USER + 69),
            EM_SETOLECALLBACK = (WM_USER + 70),
            EM_SETPARAFORMAT = (WM_USER + 71),
            EM_SETTARGETDEVICE = (WM_USER + 72),
            EM_STREAMIN = (WM_USER + 73),
            EM_STREAMOUT = (WM_USER + 74),
            EM_GETTEXTRANGE = (WM_USER + 75),
            EM_FINDWORDBREAK = (WM_USER + 76),
            EM_SETOPTIONS = (WM_USER + 77),
            EM_GETOPTIONS = (WM_USER + 78),
            EM_FINDTEXTEX = (WM_USER + 79),

            // Tab Control Messages - CommCtrl.h
            TCM_DELETEITEM = 0x1308,
            TCM_INSERTITEM = 0x133E,
            TCM_GETITEMRECT = 0x130A,
            TCM_GETCURSEL = 0x130B,
            TCM_SETCURSEL = 0x130C,
            TCM_ADJUSTRECT = 0x1328,
            TCM_SETITEMSIZE = 0x1329,
            TCM_SETPADDING = 0x132B,

            // olectl.h
            OCM__BASE = (WM_USER + 0x1c00),
            OCM_COMMAND = (OCM__BASE + WM_COMMAND),
            OCM_DRAWITEM = (OCM__BASE + WM_DRAWITEM),
            OCM_MEASUREITEM = (OCM__BASE + WM_MEASUREITEM),
            OCM_DELETEITEM = (OCM__BASE + WM_DELETEITEM),
            OCM_VKEYTOITEM = (OCM__BASE + WM_VKEYTOITEM),
            OCM_CHARTOITEM = (OCM__BASE + WM_CHARTOITEM),
            OCM_COMPAREITEM = (OCM__BASE + WM_COMPAREITEM),
            OCM_HSCROLL = (OCM__BASE + WM_HSCROLL),
            OCM_VSCROLL = (OCM__BASE + WM_VSCROLL),
            OCM_PARENTNOTIFY = (OCM__BASE + WM_PARENTNOTIFY),
            OCM_NOTIFY = (OCM__BASE + WM_NOTIFY),

        }

        public const int SCF_SELECTION = 0x0001;

        /* Edit control EM_SETMARGIN parameters */
        public const int EC_LEFTMARGIN = 0x0001;
        public const int EC_RIGHTMARGIN = 0x0002;

        [Flags]
        public enum Flags
        {
            // SetWindowPos Flags - WinUser.h
            SWP_NOSIZE = 0x0001,
            SWP_NOMOVE = 0x0002,
            SWP_NOZORDER = 0x0004,
            SWP_NOREDRAW = 0x0008,
            SWP_NOACTIVATE = 0x0010,
            SWP_FRAMECHANGED = 0x0020,
            SWP_SHOWWINDOW = 0x0040,
            SWP_HIDEWINDOW = 0x0080,
            SWP_NOCOPYBITS = 0x0100,
            SWP_NOOWNERZORDER = 0x0200,
            SWP_NOSENDCHANGING = 0x0400,
        };



        private static Type tmsgs = typeof(Msgs);


        public static string Mnemonic(int z)
        {
            foreach (int ix in Enum.GetValues(tmsgs))
            {
                if (z == ix)
                    return Enum.GetName(tmsgs, ix);
            }

            return z.ToString("X4");
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWPOS
        {
            public IntPtr hwnd, hwndInsertAfter;
            public int x, y, cx, cy, flags;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct STYLESTRUCT
        {
            public int styleOld;
            public int styleNew;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct CREATESTRUCT
        {
            public IntPtr lpCreateParams;
            public IntPtr hInstance;
            public IntPtr hMenu;
            public IntPtr hwndParent;
            public int cy;
            public int cx;
            public int y;
            public int x;
            public int style;
            public string lpszName;
            public string lpszClass;
            public int dwExStyle;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CHARFORMAT
        {
            public int cbSize;
            public UInt32 dwMask;
            public UInt32 dwEffects;
            public Int32 yHeight;
            public Int32 yOffset;
            public Int32 crTextColor;
            public byte bCharSet;
            public byte bPitchAndFamily;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public char[] szFaceName;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINTL
        {
            public Int32 X;
            public Int32 Y;
        }


        public static void BeginUpdate(IntPtr hWnd)
        {
            SendMessage(hWnd, (int)Msgs.WM_SETREDRAW, 0, IntPtr.Zero);
        }

        public static void EndUpdate(IntPtr hWnd)
        {
            SendMessage(hWnd, (int)Msgs.WM_SETREDRAW, 1, IntPtr.Zero);
        }


        [DllImport("User32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("User32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd,
                                                [MarshalAs(UnmanagedType.I4)] Msgs msg,
                                                int wParam,
                                                IntPtr lParam);


        [DllImport("User32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd,
                                                [MarshalAs(UnmanagedType.I4)] Msgs msg,
                                                int wParam,
                                                int lParam);


        [DllImport("User32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hWnd, int msg, int wparam, IntPtr lparam);

        [DllImport("User32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hWnd, int msg, int wparam, int lparam);

        [DllImport("User32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
        public static extern int SendMessageRef(IntPtr hWnd, int msg, out int wparam, out int lparam);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetWindow(IntPtr hWnd, int uCmd);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern int GetClassName(IntPtr hWnd, char[] className, int maxCount);

        [DllImport("user32.dll")]
        //[return: MarshalAs(UnmanagedType.Bool)]
        public static extern int SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
                                              int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowLong(IntPtr hWnd, uint nIndex);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, uint nIndex, uint dwNewLong);
    }
}