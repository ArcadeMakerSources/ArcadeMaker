namespace ArcadeMaker.Engines.MonoGame.Platforms.WindowsDX
{
    partial class ExceptionViewerDialog
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
            textBox = new System.Windows.Forms.RichTextBox();
            abortBtn = new System.Windows.Forms.Button();
            ignoreBtn = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // textBox
            // 
            textBox.Location = new System.Drawing.Point(12, 12);
            textBox.Name = "textBox";
            textBox.ReadOnly = true;
            textBox.Size = new System.Drawing.Size(239, 295);
            textBox.TabIndex = 0;
            textBox.Text = "";
            // 
            // abortBtn
            // 
            abortBtn.Location = new System.Drawing.Point(12, 313);
            abortBtn.Name = "abortBtn";
            abortBtn.Size = new System.Drawing.Size(75, 23);
            abortBtn.TabIndex = 1;
            abortBtn.Text = "Abort";
            abortBtn.UseVisualStyleBackColor = true;
            abortBtn.Click += abortBtn_Click;
            // 
            // ignoreBtn
            // 
            ignoreBtn.Location = new System.Drawing.Point(93, 313);
            ignoreBtn.Name = "ignoreBtn";
            ignoreBtn.Size = new System.Drawing.Size(75, 23);
            ignoreBtn.TabIndex = 2;
            ignoreBtn.Text = "Ignore";
            ignoreBtn.UseVisualStyleBackColor = true;
            ignoreBtn.Click += ignoreBtn_Click;
            // 
            // ExceptionViewerDialog
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(263, 345);
            Controls.Add(ignoreBtn);
            Controls.Add(abortBtn);
            Controls.Add(textBox);
            Name = "ExceptionViewerDialog";
            Text = "Uncaught Exception";
            Load += ExceptionViewerDialog_Load;
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.RichTextBox textBox;
        private System.Windows.Forms.Button abortBtn;
        private System.Windows.Forms.Button ignoreBtn;
    }
}