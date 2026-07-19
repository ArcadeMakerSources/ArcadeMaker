
namespace ArcadeMaker.IDE
{
    partial class FontEditor
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
            label1 = new Label();
            nameBox = new TextBox();
            label2 = new Label();
            familiesBox = new ComboBox();
            label3 = new Label();
            sizeBox = new NumericUpDown();
            boldBox = new CheckBox();
            italicBox = new CheckBox();
            previewLbl = new Label();
            okBtn = new Button();
            ((System.ComponentModel.ISupportInitialize)sizeBox).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(39, 15);
            label1.TabIndex = 0;
            label1.Text = "Name";
            // 
            // nameBox
            // 
            nameBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            nameBox.Location = new Point(53, 6);
            nameBox.Name = "nameBox";
            nameBox.Size = new Size(116, 23);
            nameBox.TabIndex = 1;
            nameBox.TextChanged += nameBox_TextChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 63);
            label2.Name = "label2";
            label2.Size = new Size(31, 15);
            label2.TabIndex = 2;
            label2.Text = "Font";
            // 
            // familiesBox
            // 
            familiesBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            familiesBox.DropDownStyle = ComboBoxStyle.DropDownList;
            familiesBox.FormattingEnabled = true;
            familiesBox.Location = new Point(53, 60);
            familiesBox.Name = "familiesBox";
            familiesBox.Size = new Size(116, 23);
            familiesBox.TabIndex = 3;
            familiesBox.SelectedIndexChanged += familiesBox_SelectedIndexChanged;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(12, 89);
            label3.Name = "label3";
            label3.Size = new Size(27, 15);
            label3.TabIndex = 4;
            label3.Text = "Size";
            // 
            // sizeBox
            // 
            sizeBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            sizeBox.Location = new Point(53, 87);
            sizeBox.Name = "sizeBox";
            sizeBox.Size = new Size(116, 23);
            sizeBox.TabIndex = 5;
            sizeBox.ValueChanged += sizeBox_ValueChanged;
            // 
            // boldBox
            // 
            boldBox.AutoSize = true;
            boldBox.Location = new Point(15, 125);
            boldBox.Name = "boldBox";
            boldBox.Size = new Size(50, 19);
            boldBox.TabIndex = 6;
            boldBox.Text = "Bold";
            boldBox.UseVisualStyleBackColor = true;
            boldBox.CheckedChanged += boldBox_CheckedChanged;
            // 
            // italicBox
            // 
            italicBox.AutoSize = true;
            italicBox.Location = new Point(91, 125);
            italicBox.Name = "italicBox";
            italicBox.Size = new Size(51, 19);
            italicBox.TabIndex = 7;
            italicBox.Text = "Italic";
            italicBox.UseVisualStyleBackColor = true;
            italicBox.CheckedChanged += italicBox_CheckedChanged;
            // 
            // previewLbl
            // 
            previewLbl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            previewLbl.BorderStyle = BorderStyle.FixedSingle;
            previewLbl.Location = new Point(12, 169);
            previewLbl.Name = "previewLbl";
            previewLbl.Size = new Size(157, 100);
            previewLbl.TabIndex = 8;
            previewLbl.Text = "AaBbCcDd";
            previewLbl.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // okBtn
            // 
            okBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            okBtn.Location = new Point(53, 281);
            okBtn.Name = "okBtn";
            okBtn.Size = new Size(75, 23);
            okBtn.TabIndex = 9;
            okBtn.Text = "OK";
            okBtn.UseVisualStyleBackColor = true;
            okBtn.Click += okBtn_Click;
            // 
            // FontEditor
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(181, 316);
            Controls.Add(okBtn);
            Controls.Add(previewLbl);
            Controls.Add(italicBox);
            Controls.Add(boldBox);
            Controls.Add(sizeBox);
            Controls.Add(label3);
            Controls.Add(familiesBox);
            Controls.Add(label2);
            Controls.Add(nameBox);
            Controls.Add(label1);
            Name = "FontEditor";
            Text = "Font Properties";
            Load += FontEditor_Load;
            ((System.ComponentModel.ISupportInitialize)sizeBox).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox nameBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox familiesBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown sizeBox;
        private System.Windows.Forms.CheckBox boldBox;
        private System.Windows.Forms.CheckBox italicBox;
        private System.Windows.Forms.Label previewLbl;
        private System.Windows.Forms.Button okBtn;
    }
}