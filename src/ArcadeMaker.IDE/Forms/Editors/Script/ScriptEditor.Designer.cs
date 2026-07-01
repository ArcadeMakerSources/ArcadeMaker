
namespace ArcadeMaker.IDE
{
    partial class ScriptEditor
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
            components = new System.ComponentModel.Container();
            okBtn = new Button();
            findBtn = new Button();
            textPosLbl = new Label();
            scriptBox = new SpansTextBox2();
            errorsBox = new ListView();
            SuspendLayout();
            // 
            // okBtn
            // 
            okBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            okBtn.Location = new Point(12, 535);
            okBtn.Name = "okBtn";
            okBtn.Size = new Size(75, 23);
            okBtn.TabIndex = 0;
            okBtn.Text = "OK";
            okBtn.UseVisualStyleBackColor = true;
            okBtn.Click += okBtn_Click;
            // 
            // findBtn
            // 
            findBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            findBtn.Location = new Point(93, 535);
            findBtn.Name = "findBtn";
            findBtn.Size = new Size(75, 23);
            findBtn.TabIndex = 1;
            findBtn.Text = "Find";
            findBtn.UseVisualStyleBackColor = true;
            findBtn.Click += findBtn_Click;
            // 
            // textPosLbl
            // 
            textPosLbl.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            textPosLbl.AutoSize = true;
            textPosLbl.Location = new Point(174, 540);
            textPosLbl.Name = "textPosLbl";
            textPosLbl.Size = new Size(74, 15);
            textPosLbl.TabIndex = 2;
            textPosLbl.Text = "Line: 1 Col: 0";
            // 
            // scriptBox
            // 
            scriptBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            scriptBox.BackColor = Color.FromArgb(42, 42, 42);
            scriptBox.Cursor = Cursors.IBeam;
            scriptBox.Location = new Point(12, 12);
            scriptBox.Margin = new Padding(4, 3, 4, 3);
            scriptBox.Name = "scriptBox";
            scriptBox.Size = new Size(852, 424);
            scriptBox.TabIndex = 4;
            scriptBox.TextChanged += scriptBox_TextChanged;
            scriptBox.Load += scriptBox_Load;
            // 
            // errorsBox
            // 
            errorsBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            errorsBox.FullRowSelect = true;
            errorsBox.Location = new Point(12, 442);
            errorsBox.Name = "errorsBox";
            errorsBox.Size = new Size(852, 87);
            errorsBox.TabIndex = 5;
            errorsBox.UseCompatibleStateImageBehavior = false;
            errorsBox.View = View.Details;
            // 
            // ScriptEditor
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(876, 570);
            Controls.Add(errorsBox);
            Controls.Add(scriptBox);
            Controls.Add(textPosLbl);
            Controls.Add(findBtn);
            Controls.Add(okBtn);
            Name = "ScriptEditor";
            Text = "Code Editor";
            FormClosing += ScriptEditor_FormClosing;
            FormClosed += ScriptEditor_FormClosed;
            Load += ScriptEditor_Load;
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button okBtn;
        private System.Windows.Forms.Button findBtn;
        private System.Windows.Forms.Label textPosLbl;
        private SpansTextBox2 scriptBox;
        private ListView errorsBox;
    }
}