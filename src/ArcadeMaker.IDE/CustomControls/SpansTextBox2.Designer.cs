
namespace ArcadeMaker.IDE
{
    partial class SpansTextBox2
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            try
            {
                CaretTimer?.Dispose();
            }
            catch { }
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            vScrollBar = new VScrollBar();
            hScrollBar = new HScrollBar();
            completionBox = new ListBox();
            toolTip = new ToolTip(components);
            paramToolTip = new ToolTip(components);
            completionToolTip = new ToolTip(components);
            SuspendLayout();
            // 
            // vScrollBar
            // 
            vScrollBar.Location = new Point(0, 0);
            vScrollBar.Name = "vScrollBar";
            vScrollBar.Size = new Size(14, 375);
            vScrollBar.TabIndex = 0;
            vScrollBar.Scroll += vScrollBar_Scroll;
            // 
            // hScrollBar
            // 
            hScrollBar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            hScrollBar.Location = new Point(16, 358);
            hScrollBar.Name = "hScrollBar";
            hScrollBar.Size = new Size(386, 15);
            hScrollBar.TabIndex = 1;
            hScrollBar.Scroll += hScrollBar_Scroll;
            // 
            // completionBox
            // 
            completionBox.DrawMode = DrawMode.OwnerDrawFixed;
            completionBox.FormattingEnabled = true;
            completionBox.Location = new Point(20, 230);
            completionBox.Margin = new Padding(4, 3, 4, 3);
            completionBox.Name = "completionBox";
            completionBox.Size = new Size(338, 116);
            completionBox.TabIndex = 2;
            completionBox.Visible = false;
            completionBox.MouseClick += completionBox_MouseClick;
            completionBox.DrawItem += completionBox_DrawItem;
            completionBox.PreviewKeyDown += completionBox_PreviewKeyDown;
            // 
            // toolTip
            // 
            toolTip.OwnerDraw = true;
            toolTip.Draw += toolTip_Draw;
            toolTip.Popup += toolTip_Popup;
            // 
            // paramToolTip
            // 
            paramToolTip.OwnerDraw = true;
            paramToolTip.Draw += toolTip_Draw;
            paramToolTip.Popup += toolTip_Popup;
            // 
            // completionToolTip
            // 
            completionToolTip.OwnerDraw = true;
            completionToolTip.Draw += toolTip_Draw;
            completionToolTip.Popup += toolTip_Popup;
            // 
            // SpansTextBox2
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(42, 42, 42);
            Controls.Add(completionBox);
            Controls.Add(hScrollBar);
            Controls.Add(vScrollBar);
            Cursor = Cursors.IBeam;
            DoubleBuffered = true;
            Margin = new Padding(4, 3, 4, 3);
            Name = "SpansTextBox2";
            Size = new Size(402, 375);
            Load += SpansTextBox2_Load;
            Paint += SpanTextBox2_Paint;
            GotFocus += SpansTextBox2_GotFocus;
            KeyPress += SpanTextBox2_KeyPress;
            LostFocus += SpansTextBox2_LostFocus;
            MouseDoubleClick += SpansTextBox2_MouseDoubleClick;
            MouseDown += SpanTextBox2_MouseDown;
            MouseMove += SpansTextBox2_MouseMove;
            MouseWheel += SpansTextBox2_MouseWheel;
            PreviewKeyDown += SpansTextBox2_PreviewKeyDown;
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.VScrollBar vScrollBar;
        private System.Windows.Forms.HScrollBar hScrollBar;
        private System.Windows.Forms.ListBox completionBox;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.ToolTip paramToolTip;
        private System.Windows.Forms.ToolTip completionToolTip;
    }
}
