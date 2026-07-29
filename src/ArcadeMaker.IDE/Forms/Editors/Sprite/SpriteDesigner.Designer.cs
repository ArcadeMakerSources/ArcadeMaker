
namespace ArcadeMaker.IDE
{
    partial class SpriteDesigner
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
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SpriteDesigner));
            okBtn = new Button();
            penBtn = new Button();
            lineBtn = new Button();
            fillBtn = new Button();
            rectBtn = new Button();
            imageBox = new PictureBox();
            imagePanel = new Panel();
            toolStrip1 = new ToolStrip();
            undoBtn = new ToolStripButton();
            redoBtn = new ToolStripButton();
            toolStripSeparator1 = new ToolStripSeparator();
            zoomInBtn = new ToolStripButton();
            zoomOutBtn = new ToolStripButton();
            shapeDrawBtn = new Button();
            shapeDrawFillBtn = new Button();
            shapeFillBtn = new Button();
            ellipseBtn = new Button();
            pickColBtn = new Button();
            drawFillGroupBox = new GroupBox();
            widthGroupBox = new GroupBox();
            width6Btn = new Button();
            width5Btn = new Button();
            width4Btn = new Button();
            width3Btn = new Button();
            width2Btn = new Button();
            width1Btn = new Button();
            toleranceBox = new TrackBar();
            toleranceGroupBox = new GroupBox();
            toleranceNumericBox = new NumericUpDown();
            toolBox = new GroupBox();
            colorPicker = new GSColorPicker();
            ((System.ComponentModel.ISupportInitialize)imageBox).BeginInit();
            imagePanel.SuspendLayout();
            toolStrip1.SuspendLayout();
            drawFillGroupBox.SuspendLayout();
            widthGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)toleranceBox).BeginInit();
            toleranceGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)toleranceNumericBox).BeginInit();
            toolBox.SuspendLayout();
            SuspendLayout();
            // 
            // okBtn
            // 
            okBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            okBtn.Location = new Point(12, 463);
            okBtn.Name = "okBtn";
            okBtn.Size = new Size(75, 23);
            okBtn.TabIndex = 2;
            okBtn.Text = "OK";
            okBtn.UseVisualStyleBackColor = true;
            okBtn.Click += okBtn_Click;
            // 
            // penBtn
            // 
            penBtn.Location = new Point(12, 19);
            penBtn.Name = "penBtn";
            penBtn.Size = new Size(26, 23);
            penBtn.TabIndex = 5;
            penBtn.Text = "✎";
            penBtn.UseVisualStyleBackColor = true;
            // 
            // lineBtn
            // 
            lineBtn.Location = new Point(12, 48);
            lineBtn.Name = "lineBtn";
            lineBtn.Size = new Size(26, 23);
            lineBtn.TabIndex = 6;
            lineBtn.Text = "\\";
            lineBtn.UseVisualStyleBackColor = true;
            // 
            // fillBtn
            // 
            fillBtn.Location = new Point(12, 77);
            fillBtn.Name = "fillBtn";
            fillBtn.Size = new Size(26, 23);
            fillBtn.TabIndex = 7;
            fillBtn.Text = "+";
            fillBtn.UseVisualStyleBackColor = true;
            // 
            // rectBtn
            // 
            rectBtn.Location = new Point(44, 48);
            rectBtn.Name = "rectBtn";
            rectBtn.Size = new Size(26, 23);
            rectBtn.TabIndex = 8;
            rectBtn.Text = "▯";
            rectBtn.UseVisualStyleBackColor = true;
            // 
            // imageBox
            // 
            imageBox.BackColor = Color.Transparent;
            imageBox.Location = new Point(3, 27);
            imageBox.Name = "imageBox";
            imageBox.Size = new Size(143, 123);
            imageBox.SizeMode = PictureBoxSizeMode.Zoom;
            imageBox.TabIndex = 0;
            imageBox.TabStop = false;
            imageBox.Paint += imageBox_Paint;
            imageBox.MouseDown += imageBox_MouseDown;
            imageBox.MouseMove += imageBox_MouseMove;
            imageBox.MouseUp += imageBox_MouseUp;
            // 
            // imagePanel
            // 
            imagePanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            imagePanel.AutoScroll = true;
            imagePanel.BorderStyle = BorderStyle.Fixed3D;
            imagePanel.Controls.Add(toolStrip1);
            imagePanel.Controls.Add(imageBox);
            imagePanel.Location = new Point(134, 26);
            imagePanel.Name = "imagePanel";
            imagePanel.Size = new Size(445, 460);
            imagePanel.TabIndex = 10;
            // 
            // toolStrip1
            // 
            toolStrip1.Items.AddRange(new ToolStripItem[] { undoBtn, redoBtn, toolStripSeparator1, zoomInBtn, zoomOutBtn });
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(441, 25);
            toolStrip1.TabIndex = 1;
            toolStrip1.Text = "toolStrip1";
            // 
            // undoBtn
            // 
            undoBtn.DisplayStyle = ToolStripItemDisplayStyle.Text;
            undoBtn.Enabled = false;
            undoBtn.Image = (Image)resources.GetObject("undoBtn.Image");
            undoBtn.ImageTransparentColor = Color.Magenta;
            undoBtn.Name = "undoBtn";
            undoBtn.Size = new Size(40, 22);
            undoBtn.Text = "Undo";
            undoBtn.Click += undoBtn_Click;
            // 
            // redoBtn
            // 
            redoBtn.DisplayStyle = ToolStripItemDisplayStyle.Text;
            redoBtn.Enabled = false;
            redoBtn.Image = (Image)resources.GetObject("redoBtn.Image");
            redoBtn.ImageTransparentColor = Color.Magenta;
            redoBtn.Name = "redoBtn";
            redoBtn.Size = new Size(38, 22);
            redoBtn.Text = "Redo";
            redoBtn.Click += redoBtn_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 25);
            // 
            // zoomInBtn
            // 
            zoomInBtn.DisplayStyle = ToolStripItemDisplayStyle.Text;
            zoomInBtn.Image = (Image)resources.GetObject("zoomInBtn.Image");
            zoomInBtn.ImageTransparentColor = Color.Magenta;
            zoomInBtn.Name = "zoomInBtn";
            zoomInBtn.Size = new Size(23, 22);
            zoomInBtn.Text = "+";
            zoomInBtn.Click += zoomInBtn_Click;
            // 
            // zoomOutBtn
            // 
            zoomOutBtn.DisplayStyle = ToolStripItemDisplayStyle.Text;
            zoomOutBtn.Image = (Image)resources.GetObject("zoomOutBtn.Image");
            zoomOutBtn.ImageTransparentColor = Color.Magenta;
            zoomOutBtn.Name = "zoomOutBtn";
            zoomOutBtn.Size = new Size(23, 22);
            zoomOutBtn.Text = "-";
            zoomOutBtn.Click += zoomOutBtn_Click;
            // 
            // shapeDrawBtn
            // 
            shapeDrawBtn.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 177);
            shapeDrawBtn.Location = new Point(6, 19);
            shapeDrawBtn.Name = "shapeDrawBtn";
            shapeDrawBtn.Size = new Size(69, 23);
            shapeDrawBtn.TabIndex = 11;
            shapeDrawBtn.Text = "Border";
            shapeDrawBtn.UseVisualStyleBackColor = true;
            shapeDrawBtn.Click += shapeDrawBtn_Click;
            // 
            // shapeDrawFillBtn
            // 
            shapeDrawFillBtn.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 177);
            shapeDrawFillBtn.Location = new Point(6, 48);
            shapeDrawFillBtn.Name = "shapeDrawFillBtn";
            shapeDrawFillBtn.Size = new Size(69, 23);
            shapeDrawFillBtn.TabIndex = 12;
            shapeDrawFillBtn.Text = "Border  Fill";
            shapeDrawFillBtn.UseVisualStyleBackColor = true;
            shapeDrawFillBtn.Click += shapeDrawFillBtn_Click;
            // 
            // shapeFillBtn
            // 
            shapeFillBtn.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 177);
            shapeFillBtn.Location = new Point(6, 77);
            shapeFillBtn.Name = "shapeFillBtn";
            shapeFillBtn.Size = new Size(69, 23);
            shapeFillBtn.TabIndex = 13;
            shapeFillBtn.Text = "Fill";
            shapeFillBtn.UseVisualStyleBackColor = true;
            shapeFillBtn.Click += shapeFillBtn_Click;
            // 
            // ellipseBtn
            // 
            ellipseBtn.Location = new Point(44, 77);
            ellipseBtn.Name = "ellipseBtn";
            ellipseBtn.Size = new Size(26, 23);
            ellipseBtn.TabIndex = 14;
            ellipseBtn.Text = "◯";
            ellipseBtn.UseVisualStyleBackColor = true;
            // 
            // pickColBtn
            // 
            pickColBtn.Location = new Point(44, 19);
            pickColBtn.Name = "pickColBtn";
            pickColBtn.Size = new Size(26, 23);
            pickColBtn.TabIndex = 15;
            pickColBtn.Text = "P";
            pickColBtn.UseVisualStyleBackColor = true;
            // 
            // drawFillGroupBox
            // 
            drawFillGroupBox.Controls.Add(shapeDrawBtn);
            drawFillGroupBox.Controls.Add(shapeDrawFillBtn);
            drawFillGroupBox.Controls.Add(shapeFillBtn);
            drawFillGroupBox.Location = new Point(12, 237);
            drawFillGroupBox.Name = "drawFillGroupBox";
            drawFillGroupBox.Size = new Size(116, 114);
            drawFillGroupBox.TabIndex = 16;
            drawFillGroupBox.TabStop = false;
            drawFillGroupBox.Text = "Shape";
            // 
            // widthGroupBox
            // 
            widthGroupBox.Controls.Add(width6Btn);
            widthGroupBox.Controls.Add(width5Btn);
            widthGroupBox.Controls.Add(width4Btn);
            widthGroupBox.Controls.Add(width3Btn);
            widthGroupBox.Controls.Add(width2Btn);
            widthGroupBox.Controls.Add(width1Btn);
            widthGroupBox.Location = new Point(12, 131);
            widthGroupBox.Name = "widthGroupBox";
            widthGroupBox.Size = new Size(116, 100);
            widthGroupBox.TabIndex = 17;
            widthGroupBox.TabStop = false;
            widthGroupBox.Text = "Width";
            // 
            // width6Btn
            // 
            width6Btn.Font = new Font("Microsoft Sans Serif", 15.75F, FontStyle.Regular, GraphicsUnit.Point, 177);
            width6Btn.Location = new Point(76, 64);
            width6Btn.Name = "width6Btn";
            width6Btn.Size = new Size(29, 30);
            width6Btn.TabIndex = 5;
            width6Btn.Text = "⬤";
            width6Btn.UseVisualStyleBackColor = true;
            width6Btn.Click += width6Btn_Click;
            // 
            // width5Btn
            // 
            width5Btn.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 177);
            width5Btn.Location = new Point(41, 64);
            width5Btn.Name = "width5Btn";
            width5Btn.Size = new Size(29, 30);
            width5Btn.TabIndex = 4;
            width5Btn.Text = "⬤";
            width5Btn.TextAlign = ContentAlignment.TopCenter;
            width5Btn.UseVisualStyleBackColor = true;
            width5Btn.Click += width5Btn_Click;
            // 
            // width4Btn
            // 
            width4Btn.Font = new Font("Microsoft Sans Serif", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 177);
            width4Btn.Location = new Point(6, 64);
            width4Btn.Name = "width4Btn";
            width4Btn.Size = new Size(29, 30);
            width4Btn.TabIndex = 3;
            width4Btn.Text = "⬤";
            width4Btn.UseVisualStyleBackColor = true;
            width4Btn.Click += width4Btn_Click;
            // 
            // width3Btn
            // 
            width3Btn.Location = new Point(76, 19);
            width3Btn.Name = "width3Btn";
            width3Btn.Size = new Size(29, 28);
            width3Btn.TabIndex = 2;
            width3Btn.Text = "⬤";
            width3Btn.UseVisualStyleBackColor = true;
            width3Btn.Click += width3Btn_Click;
            // 
            // width2Btn
            // 
            width2Btn.Font = new Font("Microsoft Sans Serif", 6F, FontStyle.Regular, GraphicsUnit.Point, 177);
            width2Btn.Location = new Point(41, 19);
            width2Btn.Name = "width2Btn";
            width2Btn.Size = new Size(29, 28);
            width2Btn.TabIndex = 1;
            width2Btn.Text = "⬤";
            width2Btn.UseVisualStyleBackColor = true;
            width2Btn.Click += width2Btn_Click;
            // 
            // width1Btn
            // 
            width1Btn.Font = new Font("Microsoft Sans Serif", 3F, FontStyle.Regular, GraphicsUnit.Point, 177);
            width1Btn.Location = new Point(6, 19);
            width1Btn.Name = "width1Btn";
            width1Btn.Size = new Size(29, 28);
            width1Btn.TabIndex = 0;
            width1Btn.Text = "⬤";
            width1Btn.UseVisualStyleBackColor = true;
            width1Btn.Click += width1Btn_Click;
            // 
            // toleranceBox
            // 
            toleranceBox.Location = new Point(6, 49);
            toleranceBox.Maximum = 441;
            toleranceBox.Name = "toleranceBox";
            toleranceBox.Size = new Size(104, 45);
            toleranceBox.TabIndex = 18;
            // 
            // toleranceGroupBox
            // 
            toleranceGroupBox.Controls.Add(toleranceNumericBox);
            toleranceGroupBox.Controls.Add(toleranceBox);
            toleranceGroupBox.Location = new Point(12, 357);
            toleranceGroupBox.Name = "toleranceGroupBox";
            toleranceGroupBox.Size = new Size(116, 100);
            toleranceGroupBox.TabIndex = 19;
            toleranceGroupBox.TabStop = false;
            toleranceGroupBox.Text = "Tolerance";
            toleranceGroupBox.Visible = false;
            // 
            // toleranceNumericBox
            // 
            toleranceNumericBox.Location = new Point(13, 23);
            toleranceNumericBox.Maximum = new decimal(new int[] { 441, 0, 0, 0 });
            toleranceNumericBox.Name = "toleranceNumericBox";
            toleranceNumericBox.Size = new Size(97, 23);
            toleranceNumericBox.TabIndex = 19;
            // 
            // toolBox
            // 
            toolBox.Controls.Add(pickColBtn);
            toolBox.Controls.Add(penBtn);
            toolBox.Controls.Add(lineBtn);
            toolBox.Controls.Add(fillBtn);
            toolBox.Controls.Add(rectBtn);
            toolBox.Controls.Add(ellipseBtn);
            toolBox.Location = new Point(15, 10);
            toolBox.Name = "toolBox";
            toolBox.Size = new Size(113, 115);
            toolBox.TabIndex = 20;
            toolBox.TabStop = false;
            toolBox.Text = "Tools";
            // 
            // colorPicker
            // 
            colorPicker.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            colorPicker.Location = new Point(585, 26);
            colorPicker.Margin = new Padding(4, 3, 4, 3);
            colorPicker.Name = "colorPicker";
            colorPicker.Size = new Size(91, 363);
            colorPicker.TabIndex = 9;
            // 
            // SpriteDesigner
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            ClientSize = new Size(682, 496);
            Controls.Add(toolBox);
            Controls.Add(toleranceGroupBox);
            Controls.Add(widthGroupBox);
            Controls.Add(drawFillGroupBox);
            Controls.Add(imagePanel);
            Controls.Add(colorPicker);
            Controls.Add(okBtn);
            Name = "SpriteDesigner";
            Text = "Sprite Designer";
            ((System.ComponentModel.ISupportInitialize)imageBox).EndInit();
            imagePanel.ResumeLayout(false);
            imagePanel.PerformLayout();
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            drawFillGroupBox.ResumeLayout(false);
            widthGroupBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)toleranceBox).EndInit();
            toleranceGroupBox.ResumeLayout(false);
            toleranceGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)toleranceNumericBox).EndInit();
            toolBox.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox imageBox;
        private System.Windows.Forms.Button okBtn;
        private System.Windows.Forms.Button penBtn;
        private System.Windows.Forms.Button lineBtn;
        private System.Windows.Forms.Button fillBtn;
        private System.Windows.Forms.Button rectBtn;
        private GSColorPicker colorPicker;
        private System.Windows.Forms.Panel imagePanel;
        private System.Windows.Forms.Button shapeDrawBtn;
        private System.Windows.Forms.Button shapeDrawFillBtn;
        private System.Windows.Forms.Button shapeFillBtn;
        private System.Windows.Forms.Button ellipseBtn;
        private System.Windows.Forms.Button pickColBtn;
        private System.Windows.Forms.GroupBox drawFillGroupBox;
        private System.Windows.Forms.GroupBox widthGroupBox;
        private System.Windows.Forms.Button width6Btn;
        private System.Windows.Forms.Button width5Btn;
        private System.Windows.Forms.Button width4Btn;
        private System.Windows.Forms.Button width3Btn;
        private System.Windows.Forms.Button width2Btn;
        private System.Windows.Forms.Button width1Btn;
        private System.Windows.Forms.TrackBar toleranceBox;
        private System.Windows.Forms.GroupBox toleranceGroupBox;
        private System.Windows.Forms.NumericUpDown toleranceNumericBox;
        private System.Windows.Forms.GroupBox toolBox;
        private ToolStrip toolStrip1;
        private ToolStripButton undoBtn;
        private ToolStripButton redoBtn;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripButton zoomInBtn;
        private ToolStripButton zoomOutBtn;
    }
}