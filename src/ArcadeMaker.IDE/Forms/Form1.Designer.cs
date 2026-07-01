
using System.Windows.Forms;

namespace ArcadeMaker.IDE
{
    partial class Form1
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
            TreeNode treeNode1 = new TreeNode("Sprites");
            TreeNode treeNode2 = new TreeNode("Sounds");
            TreeNode treeNode3 = new TreeNode("Backgrounds");
            TreeNode treeNode4 = new TreeNode("Paths");
            TreeNode treeNode5 = new TreeNode("Scripts");
            TreeNode treeNode6 = new TreeNode("Fonts");
            TreeNode treeNode7 = new TreeNode("Objects");
            TreeNode treeNode8 = new TreeNode("Rooms");
            TreeNode treeNode9 = new TreeNode("Assembly Manager");
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            projectTree = new TreeView();
            toolStrip1 = new ToolStrip();
            createSpriteBtn = new ToolStripButton();
            createBackgroundBtn = new ToolStripButton();
            createMusicBtn = new ToolStripButton();
            createPathBtn = new ToolStripButton();
            createScriptBtn = new ToolStripButton();
            createFontBtn = new ToolStripButton();
            createObjectBtn = new ToolStripButton();
            createRoomBtn = new ToolStripButton();
            toolStripSeparator2 = new ToolStripSeparator();
            debugBtn = new ToolStripButton();
            toolStripSeparator1 = new ToolStripSeparator();
            saveGameBtn = new ToolStripButton();
            mainMenuStrip = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            newProjectBtn = new ToolStripMenuItem();
            openProjectBtn = new ToolStripMenuItem();
            saveProjectBtn = new ToolStripMenuItem();
            saveProjectAsBtn = new ToolStripMenuItem();
            recentProjectsMenu = new ToolStripMenuItem();
            clearRecentProjectsBtn = new ToolStripMenuItem();
            editToolStripMenuItem = new ToolStripMenuItem();
            findResBtn = new ToolStripMenuItem();
            toolsToolStripMenuItem = new ToolStripMenuItem();
            assemblyManagerBtn = new ToolStripMenuItem();
            splitter1 = new Splitter();
            errorsPanel = new Panel();
            label1 = new Label();
            errorsBox = new ListView();
            errorsBox_InCol = new ColumnHeader();
            errorsBox_DescCol = new ColumnHeader();
            errorsBox_FileCol = new ColumnHeader();
            errorsBox_LineCol = new ColumnHeader();
            splitter2 = new Splitter();
            toolStrip1.SuspendLayout();
            mainMenuStrip.SuspendLayout();
            errorsPanel.SuspendLayout();
            SuspendLayout();
            // 
            // projectTree
            // 
            projectTree.AllowDrop = true;
            projectTree.BorderStyle = BorderStyle.FixedSingle;
            projectTree.Dock = DockStyle.Left;
            projectTree.LabelEdit = true;
            projectTree.Location = new Point(0, 49);
            projectTree.Margin = new Padding(4, 3, 4, 3);
            projectTree.Name = "projectTree";
            treeNode1.Name = "spritesNode";
            treeNode1.Text = "Sprites";
            treeNode2.Name = "musicsNode";
            treeNode2.Text = "Sounds";
            treeNode3.Name = "backgroundsNode";
            treeNode3.Text = "Backgrounds";
            treeNode4.Name = "pathesNode";
            treeNode4.Text = "Paths";
            treeNode5.Name = "scriptsNode";
            treeNode5.Text = "Scripts";
            treeNode6.Name = "fontsNode";
            treeNode6.Text = "Fonts";
            treeNode7.Name = "objectsNode";
            treeNode7.Text = "Objects";
            treeNode8.Name = "roomsNode";
            treeNode8.Text = "Rooms";
            treeNode9.Name = "AssemblyManagerRoot.node";
            treeNode9.Text = "Assembly Manager";
            projectTree.Nodes.AddRange(new TreeNode[] { treeNode1, treeNode2, treeNode3, treeNode4, treeNode5, treeNode6, treeNode7, treeNode8, treeNode9 });
            projectTree.Size = new Size(203, 470);
            projectTree.TabIndex = 0;
            projectTree.BeforeLabelEdit += projectTree_BeforeLabelEdit;
            projectTree.AfterLabelEdit += projectTree_AfterLabelEdit;
            projectTree.ItemDrag += projectTree_ItemDrag;
            projectTree.NodeMouseDoubleClick += projectTree_NodeMouseDoubleClick;
            projectTree.DragDrop += projectTree_DragDrop;
            projectTree.DragEnter += projectTree_DragEnter;
            projectTree.DragOver += projectTree_DragOver;
            // 
            // toolStrip1
            // 
            toolStrip1.Items.AddRange(new ToolStripItem[] { createSpriteBtn, createBackgroundBtn, createMusicBtn, createPathBtn, createScriptBtn, createFontBtn, createObjectBtn, createRoomBtn, toolStripSeparator2, debugBtn, toolStripSeparator1, saveGameBtn });
            toolStrip1.Location = new Point(0, 24);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(933, 25);
            toolStrip1.TabIndex = 1;
            toolStrip1.Text = "toolStrip1";
            // 
            // createSpriteBtn
            // 
            createSpriteBtn.DisplayStyle = ToolStripItemDisplayStyle.Image;
            createSpriteBtn.Image = Properties.Resources.sprite;
            createSpriteBtn.ImageTransparentColor = Color.Magenta;
            createSpriteBtn.Name = "createSpriteBtn";
            createSpriteBtn.Size = new Size(23, 22);
            createSpriteBtn.Text = "Create Sprite";
            createSpriteBtn.Click += createSpriteBtn_Click;
            // 
            // createBackgroundBtn
            // 
            createBackgroundBtn.DisplayStyle = ToolStripItemDisplayStyle.Image;
            createBackgroundBtn.Image = Properties.Resources.background;
            createBackgroundBtn.ImageTransparentColor = Color.Magenta;
            createBackgroundBtn.Name = "createBackgroundBtn";
            createBackgroundBtn.Size = new Size(23, 22);
            createBackgroundBtn.Text = "Create Background";
            createBackgroundBtn.Click += createBackgroundBtn_Click;
            // 
            // createMusicBtn
            // 
            createMusicBtn.DisplayStyle = ToolStripItemDisplayStyle.Image;
            createMusicBtn.Image = Properties.Resources.sound;
            createMusicBtn.ImageTransparentColor = Color.Magenta;
            createMusicBtn.Name = "createMusicBtn";
            createMusicBtn.Size = new Size(23, 22);
            createMusicBtn.Text = "Create Sound";
            createMusicBtn.Click += createMusicBtn_Click;
            // 
            // createPathBtn
            // 
            createPathBtn.DisplayStyle = ToolStripItemDisplayStyle.Image;
            createPathBtn.Image = Properties.Resources.path;
            createPathBtn.ImageTransparentColor = Color.Magenta;
            createPathBtn.Name = "createPathBtn";
            createPathBtn.Size = new Size(23, 22);
            createPathBtn.Text = "Create Path";
            createPathBtn.Click += createPathBtn_Click;
            // 
            // createScriptBtn
            // 
            createScriptBtn.DisplayStyle = ToolStripItemDisplayStyle.Image;
            createScriptBtn.Image = Properties.Resources.script;
            createScriptBtn.ImageTransparentColor = Color.Magenta;
            createScriptBtn.Name = "createScriptBtn";
            createScriptBtn.Size = new Size(23, 22);
            createScriptBtn.Text = "Create Script";
            createScriptBtn.Click += createScriptBtn_Click;
            // 
            // createFontBtn
            // 
            createFontBtn.DisplayStyle = ToolStripItemDisplayStyle.Image;
            createFontBtn.Image = Properties.Resources.font32;
            createFontBtn.ImageTransparentColor = Color.Magenta;
            createFontBtn.Name = "createFontBtn";
            createFontBtn.Size = new Size(23, 22);
            createFontBtn.Text = "Create Font";
            createFontBtn.Click += createFontBtn_Click;
            // 
            // createObjectBtn
            // 
            createObjectBtn.DisplayStyle = ToolStripItemDisplayStyle.Image;
            createObjectBtn.Image = Properties.Resources.object32;
            createObjectBtn.ImageTransparentColor = Color.Magenta;
            createObjectBtn.Name = "createObjectBtn";
            createObjectBtn.Size = new Size(23, 22);
            createObjectBtn.Text = "Create Object";
            createObjectBtn.Click += createObjectBtn_Click;
            // 
            // createRoomBtn
            // 
            createRoomBtn.DisplayStyle = ToolStripItemDisplayStyle.Image;
            createRoomBtn.Image = Properties.Resources.map;
            createRoomBtn.ImageTransparentColor = Color.Magenta;
            createRoomBtn.Name = "createRoomBtn";
            createRoomBtn.Size = new Size(23, 22);
            createRoomBtn.Text = "Create Room";
            createRoomBtn.Click += createRoomBtn_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(6, 25);
            // 
            // debugBtn
            // 
            debugBtn.DisplayStyle = ToolStripItemDisplayStyle.Text;
            debugBtn.Image = (Image)resources.GetObject("debugBtn.Image");
            debugBtn.ImageTransparentColor = Color.Magenta;
            debugBtn.Name = "debugBtn";
            debugBtn.Size = new Size(32, 22);
            debugBtn.Text = "Run";
            debugBtn.Click += saveExeBtn_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 25);
            // 
            // saveGameBtn
            // 
            saveGameBtn.Image = (Image)resources.GetObject("saveGameBtn.Image");
            saveGameBtn.ImageTransparentColor = Color.Magenta;
            saveGameBtn.Name = "saveGameBtn";
            saveGameBtn.Size = new Size(72, 22);
            saveGameBtn.Text = "&Save Exe";
            saveGameBtn.Click += saveGameBtn_Click;
            // 
            // mainMenuStrip
            // 
            mainMenuStrip.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, editToolStripMenuItem, toolsToolStripMenuItem });
            mainMenuStrip.Location = new Point(0, 0);
            mainMenuStrip.Name = "mainMenuStrip";
            mainMenuStrip.Padding = new Padding(7, 2, 0, 2);
            mainMenuStrip.Size = new Size(933, 24);
            mainMenuStrip.TabIndex = 2;
            mainMenuStrip.Text = "Main Menu";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { newProjectBtn, openProjectBtn, saveProjectBtn, saveProjectAsBtn, recentProjectsMenu });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(37, 20);
            fileToolStripMenuItem.Text = "File";
            // 
            // newProjectBtn
            // 
            newProjectBtn.Name = "newProjectBtn";
            newProjectBtn.Size = new Size(163, 22);
            newProjectBtn.Text = "New Project";
            newProjectBtn.Click += newProjectBtn_Click;
            // 
            // openProjectBtn
            // 
            openProjectBtn.Name = "openProjectBtn";
            openProjectBtn.Size = new Size(163, 22);
            openProjectBtn.Text = "Open Project";
            openProjectBtn.Click += openProjectBtn_Click;
            // 
            // saveProjectBtn
            // 
            saveProjectBtn.Name = "saveProjectBtn";
            saveProjectBtn.Size = new Size(163, 22);
            saveProjectBtn.Text = "Save Project";
            saveProjectBtn.Click += saveProjectBtn_Click;
            // 
            // saveProjectAsBtn
            // 
            saveProjectAsBtn.Name = "saveProjectAsBtn";
            saveProjectAsBtn.Size = new Size(163, 22);
            saveProjectAsBtn.Text = "Save Project As...";
            saveProjectAsBtn.Click += saveProjectAsBtn_Click;
            // 
            // recentProjectsMenu
            // 
            recentProjectsMenu.DropDownItems.AddRange(new ToolStripItem[] { clearRecentProjectsBtn });
            recentProjectsMenu.Name = "recentProjectsMenu";
            recentProjectsMenu.Size = new Size(163, 22);
            recentProjectsMenu.Text = "Recent Projects";
            // 
            // clearRecentProjectsBtn
            // 
            clearRecentProjectsBtn.Name = "clearRecentProjectsBtn";
            clearRecentProjectsBtn.Size = new Size(145, 22);
            clearRecentProjectsBtn.Text = "Clear Recents";
            clearRecentProjectsBtn.Click += clearRecentProjectsBtn_Click;
            // 
            // editToolStripMenuItem
            // 
            editToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { findResBtn });
            editToolStripMenuItem.Name = "editToolStripMenuItem";
            editToolStripMenuItem.Size = new Size(39, 20);
            editToolStripMenuItem.Text = "Edit";
            // 
            // findResBtn
            // 
            findResBtn.Name = "findResBtn";
            findResBtn.Size = new Size(148, 22);
            findResBtn.Text = "Find Resource";
            findResBtn.Click += findResBtn_Click;
            // 
            // toolsToolStripMenuItem
            // 
            toolsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { assemblyManagerBtn });
            toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            toolsToolStripMenuItem.Size = new Size(46, 20);
            toolsToolStripMenuItem.Text = "Tools";
            // 
            // assemblyManagerBtn
            // 
            assemblyManagerBtn.Name = "assemblyManagerBtn";
            assemblyManagerBtn.Size = new Size(175, 22);
            assemblyManagerBtn.Text = "Assembly Manager";
            assemblyManagerBtn.Click += assemblyManagerBtn_Click;
            // 
            // splitter1
            // 
            splitter1.Location = new Point(203, 49);
            splitter1.Margin = new Padding(4, 3, 4, 3);
            splitter1.Name = "splitter1";
            splitter1.Size = new Size(4, 470);
            splitter1.TabIndex = 4;
            splitter1.TabStop = false;
            // 
            // errorsPanel
            // 
            errorsPanel.Controls.Add(label1);
            errorsPanel.Controls.Add(errorsBox);
            errorsPanel.Dock = DockStyle.Bottom;
            errorsPanel.Location = new Point(207, 348);
            errorsPanel.Name = "errorsPanel";
            errorsPanel.Size = new Size(726, 171);
            errorsPanel.TabIndex = 6;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(7, 8);
            label1.Name = "label1";
            label1.Size = new Size(53, 15);
            label1.TabIndex = 1;
            label1.Text = "Error List";
            // 
            // errorsBox
            // 
            errorsBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            errorsBox.Columns.AddRange(new ColumnHeader[] { errorsBox_InCol, errorsBox_DescCol, errorsBox_FileCol, errorsBox_LineCol });
            errorsBox.FullRowSelect = true;
            errorsBox.Location = new Point(7, 26);
            errorsBox.MultiSelect = false;
            errorsBox.Name = "errorsBox";
            errorsBox.Size = new Size(707, 133);
            errorsBox.TabIndex = 0;
            errorsBox.UseCompatibleStateImageBehavior = false;
            errorsBox.View = View.Details;
            // 
            // errorsBox_InCol
            // 
            errorsBox_InCol.Text = "In";
            // 
            // errorsBox_DescCol
            // 
            errorsBox_DescCol.Text = "Description";
            errorsBox_DescCol.Width = 500;
            // 
            // errorsBox_FileCol
            // 
            errorsBox_FileCol.Text = "File";
            errorsBox_FileCol.Width = 100;
            // 
            // errorsBox_LineCol
            // 
            errorsBox_LineCol.Text = "Line";
            // 
            // splitter2
            // 
            splitter2.Dock = DockStyle.Bottom;
            splitter2.Location = new Point(207, 345);
            splitter2.Name = "splitter2";
            splitter2.Size = new Size(726, 3);
            splitter2.TabIndex = 7;
            splitter2.TabStop = false;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(933, 519);
            Controls.Add(splitter2);
            Controls.Add(errorsPanel);
            Controls.Add(splitter1);
            Controls.Add(projectTree);
            Controls.Add(toolStrip1);
            Controls.Add(mainMenuStrip);
            IsMdiContainer = true;
            MainMenuStrip = mainMenuStrip;
            Margin = new Padding(4, 3, 4, 3);
            Name = "Form1";
            Text = "ArcadeMaker";
            WindowState = FormWindowState.Maximized;
            FormClosing += Form1_FormClosing;
            Load += Form1_Load;
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            mainMenuStrip.ResumeLayout(false);
            mainMenuStrip.PerformLayout();
            errorsPanel.ResumeLayout(false);
            errorsPanel.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView projectTree;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton createSpriteBtn;
        private System.Windows.Forms.ToolStripButton createObjectBtn;
        private System.Windows.Forms.ToolStripButton createRoomBtn;
        private System.Windows.Forms.ToolStripButton debugBtn;
        private System.Windows.Forms.ToolStripButton saveGameBtn;
        private System.Windows.Forms.ToolStripButton createBackgroundBtn;
        private System.Windows.Forms.ToolStripButton createPathBtn;
        private System.Windows.Forms.ToolStripButton createMusicBtn;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.MenuStrip mainMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openProjectBtn;
        private System.Windows.Forms.ToolStripMenuItem saveProjectBtn;
        private System.Windows.Forms.ToolStripButton createScriptBtn;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem findResBtn;
        private System.Windows.Forms.ToolStripMenuItem newProjectBtn;
        private System.Windows.Forms.ToolStripButton createFontBtn;
        private System.Windows.Forms.ToolStripMenuItem recentProjectsMenu;
        private System.Windows.Forms.ToolStripMenuItem clearRecentProjectsBtn;
        private System.Windows.Forms.ToolStripMenuItem saveProjectAsBtn;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem assemblyManagerBtn;
        private Splitter splitter1;
        private Panel errorsPanel;
        private Splitter splitter2;
        private ColumnHeader errorsBox_InCol;
        private ColumnHeader errorsBox_DescCol;
        private ColumnHeader errorsBox_FileCol;
        private Label label1;
        internal ListView errorsBox;
        private ColumnHeader errorsBox_LineCol;
    }
}

