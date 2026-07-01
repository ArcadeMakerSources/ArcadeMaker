
namespace ArcadeMaker.IDE
{
    partial class GameResourcePickerBox<T>
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.nameBox = new System.Windows.Forms.TextBox();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.menuBtn = new System.Windows.Forms.ToolStripButton();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // nameBox
            // 
            this.nameBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nameBox.BackColor = System.Drawing.Color.White;
            this.nameBox.Cursor = System.Windows.Forms.Cursors.Default;
            this.nameBox.Location = new System.Drawing.Point(3, 3);
            this.nameBox.Name = "nameBox";
            this.nameBox.ReadOnly = true;
            this.nameBox.Size = new System.Drawing.Size(126, 20);
            this.nameBox.TabIndex = 0;
            this.nameBox.MouseClick += new System.Windows.Forms.MouseEventHandler(this.nameBox_MouseClick);
            // 
            // toolStrip1
            // 
            this.toolStrip1.AllowMerge = false;
            this.toolStrip1.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuBtn});
            this.toolStrip1.GripStyle = ToolStripGripStyle.Hidden;
            this.toolStrip1.BackColor = System.Drawing.Color.Transparent;
            this.toolStrip1.ForeColor = System.Drawing.Color.Transparent;
            this.toolStrip1.Location = new System.Drawing.Point(119, 3);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(35, 25);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // menuBtn
            // 
            this.menuBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.menuBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.menuBtn.Name = "menuBtn";
            this.menuBtn.Size = new System.Drawing.Size(23, 22);
            this.menuBtn.Text = "Show Menu";
            this.menuBtn.Image = ArcadeMaker.IDE.Properties.Resources.menu;
            this.menuBtn.Click += new System.EventHandler(this.menuBtn_Click);
            // 
            // GameResourcePickerBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.nameBox);
            this.Name = "GameResourcePickerBox";
            this.Size = new System.Drawing.Size(154, 33);
            this.Load += new System.EventHandler(this.GameResourcePickerBox_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox nameBox;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton menuBtn;
    }
}
