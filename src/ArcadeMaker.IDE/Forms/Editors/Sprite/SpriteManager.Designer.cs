
namespace ArcadeMaker.IDE
{
    partial class SpriteManager
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SpriteManager));
            toolStrip1 = new ToolStrip();
            newSpriteBtn = new ToolStripButton();
            importSpriteBtn = new ToolStripButton();
            saveSpriteBtn = new ToolStripButton();
            toolStripSeparator = new ToolStripSeparator();
            cutSpriteBtn = new ToolStripButton();
            copySpriteBtn = new ToolStripButton();
            pasteSpriteBtn = new ToolStripButton();
            toolStripSeparator1 = new ToolStripSeparator();
            moveLeftBtn = new ToolStripButton();
            moveRightBtn = new ToolStripButton();
            imageListView = new ListView();
            pictureBox1 = new PictureBox();
            openFileDialog = new OpenFileDialog();
            okBtn = new Button();
            menu = new MenuStrip();
            imagesToolStripMenuItem = new ToolStripMenuItem();
            strechBtn = new ToolStripMenuItem();
            toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            menu.SuspendLayout();
            SuspendLayout();
            // 
            // toolStrip1
            // 
            toolStrip1.Items.AddRange(new ToolStripItem[] { newSpriteBtn, importSpriteBtn, saveSpriteBtn, toolStripSeparator, cutSpriteBtn, copySpriteBtn, pasteSpriteBtn, toolStripSeparator1, moveLeftBtn, moveRightBtn });
            toolStrip1.Location = new Point(0, 24);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(800, 25);
            toolStrip1.TabIndex = 0;
            toolStrip1.Text = "toolStrip1";
            // 
            // newSpriteBtn
            // 
            newSpriteBtn.DisplayStyle = ToolStripItemDisplayStyle.Image;
            newSpriteBtn.Image = (Image)resources.GetObject("newSpriteBtn.Image");
            newSpriteBtn.ImageTransparentColor = Color.Magenta;
            newSpriteBtn.Name = "newSpriteBtn";
            newSpriteBtn.Size = new Size(23, 22);
            newSpriteBtn.Text = "&New";
            newSpriteBtn.Click += newSpriteBtn_Click;
            // 
            // importSpriteBtn
            // 
            importSpriteBtn.DisplayStyle = ToolStripItemDisplayStyle.Image;
            importSpriteBtn.Image = (Image)resources.GetObject("importSpriteBtn.Image");
            importSpriteBtn.ImageTransparentColor = Color.Magenta;
            importSpriteBtn.Name = "importSpriteBtn";
            importSpriteBtn.Size = new Size(23, 22);
            importSpriteBtn.Text = "&Open";
            importSpriteBtn.Click += importSpriteBtn_Click;
            // 
            // saveSpriteBtn
            // 
            saveSpriteBtn.DisplayStyle = ToolStripItemDisplayStyle.Image;
            saveSpriteBtn.Enabled = false;
            saveSpriteBtn.Image = (Image)resources.GetObject("saveSpriteBtn.Image");
            saveSpriteBtn.ImageTransparentColor = Color.Magenta;
            saveSpriteBtn.Name = "saveSpriteBtn";
            saveSpriteBtn.Size = new Size(23, 22);
            saveSpriteBtn.Text = "&Save";
            // 
            // toolStripSeparator
            // 
            toolStripSeparator.Name = "toolStripSeparator";
            toolStripSeparator.Size = new Size(6, 25);
            // 
            // cutSpriteBtn
            // 
            cutSpriteBtn.DisplayStyle = ToolStripItemDisplayStyle.Image;
            cutSpriteBtn.Image = (Image)resources.GetObject("cutSpriteBtn.Image");
            cutSpriteBtn.ImageTransparentColor = Color.Magenta;
            cutSpriteBtn.Name = "cutSpriteBtn";
            cutSpriteBtn.Size = new Size(23, 22);
            cutSpriteBtn.Text = "C&ut";
            // 
            // copySpriteBtn
            // 
            copySpriteBtn.DisplayStyle = ToolStripItemDisplayStyle.Image;
            copySpriteBtn.Image = (Image)resources.GetObject("copySpriteBtn.Image");
            copySpriteBtn.ImageTransparentColor = Color.Magenta;
            copySpriteBtn.Name = "copySpriteBtn";
            copySpriteBtn.Size = new Size(23, 22);
            copySpriteBtn.Text = "&Copy";
            // 
            // pasteSpriteBtn
            // 
            pasteSpriteBtn.DisplayStyle = ToolStripItemDisplayStyle.Image;
            pasteSpriteBtn.Image = (Image)resources.GetObject("pasteSpriteBtn.Image");
            pasteSpriteBtn.ImageTransparentColor = Color.Magenta;
            pasteSpriteBtn.Name = "pasteSpriteBtn";
            pasteSpriteBtn.Size = new Size(23, 22);
            pasteSpriteBtn.Text = "&Paste";
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 25);
            // 
            // moveLeftBtn
            // 
            moveLeftBtn.DisplayStyle = ToolStripItemDisplayStyle.Text;
            moveLeftBtn.Enabled = false;
            moveLeftBtn.Image = (Image)resources.GetObject("moveLeftBtn.Image");
            moveLeftBtn.ImageTransparentColor = Color.Magenta;
            moveLeftBtn.Name = "moveLeftBtn";
            moveLeftBtn.Size = new Size(23, 22);
            moveLeftBtn.Text = "<";
            moveLeftBtn.Click += moveLeftBtn_Click;
            // 
            // moveRightBtn
            // 
            moveRightBtn.DisplayStyle = ToolStripItemDisplayStyle.Text;
            moveRightBtn.Enabled = false;
            moveRightBtn.Image = (Image)resources.GetObject("moveRightBtn.Image");
            moveRightBtn.ImageTransparentColor = Color.Magenta;
            moveRightBtn.Name = "moveRightBtn";
            moveRightBtn.Size = new Size(23, 22);
            moveRightBtn.Text = ">";
            moveRightBtn.Click += moveRightBtn_Click;
            // 
            // imageListView
            // 
            imageListView.Location = new Point(157, 72);
            imageListView.MultiSelect = false;
            imageListView.Name = "imageListView";
            imageListView.Size = new Size(631, 331);
            imageListView.TabIndex = 1;
            imageListView.UseCompatibleStateImageBehavior = false;
            imageListView.SelectedIndexChanged += imageListView_SelectedIndexChanged;
            imageListView.KeyUp += imageListView_KeyUp;
            imageListView.MouseDoubleClick += imageListView_MouseDoubleClick;
            // 
            // pictureBox1
            // 
            pictureBox1.Location = new Point(12, 72);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(139, 117);
            pictureBox1.TabIndex = 2;
            pictureBox1.TabStop = false;
            // 
            // openFileDialog
            // 
            openFileDialog.FileName = "openFileDialog1";
            openFileDialog.Filter = "Image Files|*.jpeg;*.jpg;*.png";
            // 
            // okBtn
            // 
            okBtn.Location = new Point(12, 385);
            okBtn.Name = "okBtn";
            okBtn.Size = new Size(75, 23);
            okBtn.TabIndex = 3;
            okBtn.Text = "OK";
            okBtn.UseVisualStyleBackColor = true;
            okBtn.Click += okBtn_Click;
            // 
            // menu
            // 
            menu.Items.AddRange(new ToolStripItem[] { imagesToolStripMenuItem });
            menu.Location = new Point(0, 0);
            menu.Name = "menu";
            menu.Size = new Size(800, 24);
            menu.TabIndex = 4;
            menu.Text = "menuStrip1";
            // 
            // imagesToolStripMenuItem
            // 
            imagesToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { strechBtn });
            imagesToolStripMenuItem.Name = "imagesToolStripMenuItem";
            imagesToolStripMenuItem.Size = new Size(57, 20);
            imagesToolStripMenuItem.Text = "Images";
            // 
            // strechBtn
            // 
            strechBtn.Name = "strechBtn";
            strechBtn.Size = new Size(180, 22);
            strechBtn.Text = "Strech...";
            strechBtn.Click += strechBtn_Click;
            // 
            // SpriteManager
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 420);
            Controls.Add(okBtn);
            Controls.Add(pictureBox1);
            Controls.Add(imageListView);
            Controls.Add(toolStrip1);
            Controls.Add(menu);
            MainMenuStrip = menu;
            Name = "SpriteManager";
            Text = "Sprite Manager";
            Load += SpriteManager_Load;
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            menu.ResumeLayout(false);
            menu.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton newSpriteBtn;
        private System.Windows.Forms.ToolStripButton importSpriteBtn;
        private System.Windows.Forms.ToolStripButton saveSpriteBtn;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator;
        private System.Windows.Forms.ToolStripButton cutSpriteBtn;
        private System.Windows.Forms.ToolStripButton copySpriteBtn;
        private System.Windows.Forms.ToolStripButton pasteSpriteBtn;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ListView imageListView;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.ToolStripButton moveLeftBtn;
        private System.Windows.Forms.ToolStripButton moveRightBtn;
        private System.Windows.Forms.Button okBtn;
        private MenuStrip menu;
        private ToolStripMenuItem imagesToolStripMenuItem;
        private ToolStripMenuItem strechBtn;
    }
}