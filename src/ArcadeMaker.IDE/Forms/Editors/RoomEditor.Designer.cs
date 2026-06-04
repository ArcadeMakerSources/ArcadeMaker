
using ArcadeMaker.IDE.Items;

namespace ArcadeMaker.IDE
{
    partial class RoomEditor
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
            boardPanel = new Panel();
            heightBox = new NumericUpDown();
            widthBox = new NumericUpDown();
            label2 = new Label();
            label3 = new Label();
            okBtn = new Button();
            label4 = new Label();
            snapxBox = new NumericUpDown();
            snapyBox = new NumericUpDown();
            label5 = new Label();
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            deleteUnderlyingBox = new CheckBox();
            objAddSelectPanel = new PictureBox();
            objAddSelectBox = new GameObjectPickerBox();
            label26 = new Label();
            label25 = new Label();
            tabPage2 = new TabPage();
            persistentBox = new CheckBox();
            label24 = new Label();
            roomSpeedBox = new NumericUpDown();
            creationCodeBtn = new Button();
            label6 = new Label();
            captionBox = new TextBox();
            tabPage3 = new TabPage();
            bgVertSpdBox = new NumericUpDown();
            bgHorSpdBox = new NumericUpDown();
            label22 = new Label();
            Label23 = new Label();
            bgYBox = new NumericUpDown();
            bgXBox = new NumericUpDown();
            label21 = new Label();
            label10 = new Label();
            bgStretchBox = new CheckBox();
            bgTileVerBox = new CheckBox();
            bgTileHorBox = new CheckBox();
            bgForegroundBox = new CheckBox();
            bgVisibleBox = new CheckBox();
            bgImageBox = new GameBackgroundPickerBox();
            bgListBox = new ListBox();
            drawBgColorBox = new CheckBox();
            backColorBox = new TextBox();
            label7 = new Label();
            tabPage4 = new TabPage();
            groupBox3 = new GroupBox();
            viewObjectFollowingBox = new GameObjectPickerBox();
            viewFollowHSpBox = new NumericUpDown();
            viewFollowVSpBox = new NumericUpDown();
            label17 = new Label();
            label18 = new Label();
            viewFollowHBorBox = new NumericUpDown();
            viewFollowVBorBox = new NumericUpDown();
            label19 = new Label();
            label20 = new Label();
            groupBox2 = new GroupBox();
            viewPortWBox = new NumericUpDown();
            viewPortHBox = new NumericUpDown();
            label13 = new Label();
            label14 = new Label();
            viewPortXBox = new NumericUpDown();
            viewPortYBox = new NumericUpDown();
            label15 = new Label();
            label16 = new Label();
            viewsList = new ListBox();
            viewVisibleBox = new CheckBox();
            useViewsBox = new CheckBox();
            groupBox1 = new GroupBox();
            viewWBox = new NumericUpDown();
            viewHBox = new NumericUpDown();
            label11 = new Label();
            label12 = new Label();
            viewXBox = new NumericUpDown();
            viewYBox = new NumericUpDown();
            label9 = new Label();
            label8 = new Label();
            colorPicker = new ColorDialog();
            objDetailsLbl = new Label();
            mouseLocLbl = new Label();
            boardParentPanel = new Panel();
            resetAllCreationCodesBtn = new Button();
            ((System.ComponentModel.ISupportInitialize)heightBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)widthBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)snapxBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)snapyBox).BeginInit();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)objAddSelectPanel).BeginInit();
            tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)roomSpeedBox).BeginInit();
            tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)bgVertSpdBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)bgHorSpdBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)bgYBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)bgXBox).BeginInit();
            tabPage4.SuspendLayout();
            groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)viewFollowHSpBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)viewFollowVSpBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)viewFollowHBorBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)viewFollowVBorBox).BeginInit();
            groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)viewPortWBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)viewPortHBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)viewPortXBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)viewPortYBox).BeginInit();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)viewWBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)viewHBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)viewXBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)viewYBox).BeginInit();
            boardParentPanel.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(3, 9);
            label1.Name = "label1";
            label1.Size = new Size(74, 15);
            label1.TabIndex = 0;
            label1.Text = "Room Name";
            // 
            // nameBox
            // 
            nameBox.Location = new Point(76, 6);
            nameBox.Name = "nameBox";
            nameBox.Size = new Size(101, 23);
            nameBox.TabIndex = 1;
            nameBox.TextChanged += nameBox_TextChanged;
            // 
            // boardPanel
            // 
            boardPanel.BackColor = Color.Silver;
            boardPanel.Location = new Point(0, 0);
            boardPanel.Name = "boardPanel";
            boardPanel.Size = new Size(637, 446);
            boardPanel.TabIndex = 3;
            boardPanel.Paint += boardPanel_Paint;
            boardPanel.MouseClick += boardPanel_MouseClick;
            boardPanel.MouseMove += boardPanel_MouseMove;
            // 
            // heightBox
            // 
            heightBox.Location = new Point(44, 122);
            heightBox.Maximum = new decimal(new int[] { int.MaxValue, 0, 0, 0 });
            heightBox.Name = "heightBox";
            heightBox.Size = new Size(133, 23);
            heightBox.TabIndex = 4;
            heightBox.ValueChanged += heightBox_ValueChanged;
            // 
            // widthBox
            // 
            widthBox.Location = new Point(44, 92);
            widthBox.Maximum = new decimal(new int[] { int.MaxValue, 0, 0, 0 });
            widthBox.Name = "widthBox";
            widthBox.Size = new Size(133, 23);
            widthBox.TabIndex = 5;
            widthBox.ValueChanged += widthBox_ValueChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(3, 94);
            label2.Name = "label2";
            label2.Size = new Size(39, 15);
            label2.TabIndex = 6;
            label2.Text = "Width";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(3, 124);
            label3.Name = "label3";
            label3.Size = new Size(43, 15);
            label3.TabIndex = 7;
            label3.Text = "Height";
            // 
            // okBtn
            // 
            okBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            okBtn.Location = new Point(16, 526);
            okBtn.Name = "okBtn";
            okBtn.Size = new Size(75, 23);
            okBtn.TabIndex = 0;
            okBtn.Text = "OK";
            okBtn.UseVisualStyleBackColor = true;
            okBtn.Click += okBtn_Click;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(213, 13);
            label4.Name = "label4";
            label4.Size = new Size(43, 15);
            label4.TabIndex = 8;
            label4.Text = "Snap X";
            // 
            // snapxBox
            // 
            snapxBox.Location = new Point(261, 11);
            snapxBox.Name = "snapxBox";
            snapxBox.Size = new Size(47, 23);
            snapxBox.TabIndex = 9;
            snapxBox.Value = new decimal(new int[] { 32, 0, 0, 0 });
            snapxBox.ValueChanged += snapxBox_ValueChanged;
            // 
            // snapyBox
            // 
            snapyBox.Location = new Point(375, 11);
            snapyBox.Name = "snapyBox";
            snapyBox.Size = new Size(47, 23);
            snapyBox.TabIndex = 11;
            snapyBox.Value = new decimal(new int[] { 32, 0, 0, 0 });
            snapyBox.ValueChanged += snapyBox_ValueChanged;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(327, 13);
            label5.Name = "label5";
            label5.Size = new Size(43, 15);
            label5.TabIndex = 10;
            label5.Text = "Snap Y";
            // 
            // tabControl1
            // 
            tabControl1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Controls.Add(tabPage3);
            tabControl1.Controls.Add(tabPage4);
            tabControl1.Location = new Point(16, 11);
            tabControl1.Multiline = true;
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(191, 505);
            tabControl1.TabIndex = 12;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(deleteUnderlyingBox);
            tabPage1.Controls.Add(objAddSelectPanel);
            tabPage1.Controls.Add(objAddSelectBox);
            tabPage1.Controls.Add(label26);
            tabPage1.Controls.Add(label25);
            tabPage1.Location = new Point(4, 44);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(183, 457);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Objects";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // deleteUnderlyingBox
            // 
            deleteUnderlyingBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            deleteUnderlyingBox.AutoSize = true;
            deleteUnderlyingBox.Checked = true;
            deleteUnderlyingBox.CheckState = CheckState.Checked;
            deleteUnderlyingBox.Location = new Point(9, 436);
            deleteUnderlyingBox.Name = "deleteUnderlyingBox";
            deleteUnderlyingBox.Size = new Size(119, 19);
            deleteUnderlyingBox.TabIndex = 4;
            deleteUnderlyingBox.Text = "Delete underlying";
            deleteUnderlyingBox.UseVisualStyleBackColor = true;
            // 
            // objAddSelectPanel
            // 
            objAddSelectPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            objAddSelectPanel.Location = new Point(3, 3);
            objAddSelectPanel.Name = "objAddSelectPanel";
            objAddSelectPanel.Size = new Size(177, 252);
            objAddSelectPanel.TabIndex = 0;
            objAddSelectPanel.TabStop = false;
            objAddSelectPanel.MouseClick += objAddSelectPanel_MouseClick;
            // 
            // objAddSelectBox
            // 
            objAddSelectBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            objAddSelectBox.Location = new Point(9, 274);
            objAddSelectBox.Margin = new Padding(4, 3, 4, 3);
            objAddSelectBox.Name = "objAddSelectBox";
            objAddSelectBox.Size = new Size(154, 33);
            objAddSelectBox.TabIndex = 2;
            objAddSelectBox.SelectionChanged += objAddSelectBox_SelectionChanged;
            // 
            // label26
            // 
            label26.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            label26.AutoSize = true;
            label26.Font = new Font("Book Antiqua", 9F, FontStyle.Italic, GraphicsUnit.Point, 0);
            label26.Location = new Point(6, 310);
            label26.Name = "label26";
            label26.Size = new Size(148, 119);
            label26.TabIndex = 3;
            label26.Text = "Left mouse button = add\r\n  + <Alt> = no snap\r\n  + <Shift> = multiple\r\n  + <Ctrl> = move\r\nRight mouse buttom = delete\r\n  + <Shift> = delete all\r\n  + <Ctrl> = popup menu";
            // 
            // label25
            // 
            label25.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            label25.AutoSize = true;
            label25.Location = new Point(6, 258);
            label25.Name = "label25";
            label25.Size = new Size(164, 15);
            label25.TabIndex = 1;
            label25.Text = "Object to add with left mouse";
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(persistentBox);
            tabPage2.Controls.Add(label24);
            tabPage2.Controls.Add(roomSpeedBox);
            tabPage2.Controls.Add(creationCodeBtn);
            tabPage2.Controls.Add(label6);
            tabPage2.Controls.Add(captionBox);
            tabPage2.Controls.Add(label3);
            tabPage2.Controls.Add(heightBox);
            tabPage2.Controls.Add(widthBox);
            tabPage2.Controls.Add(label2);
            tabPage2.Controls.Add(label1);
            tabPage2.Controls.Add(nameBox);
            tabPage2.Location = new Point(4, 44);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(183, 457);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Settings";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // persistentBox
            // 
            persistentBox.AutoSize = true;
            persistentBox.Location = new Point(6, 196);
            persistentBox.Name = "persistentBox";
            persistentBox.Size = new Size(77, 19);
            persistentBox.TabIndex = 16;
            persistentBox.Text = "Persistent";
            persistentBox.UseVisualStyleBackColor = true;
            persistentBox.CheckedChanged += persistentBox_CheckedChanged;
            // 
            // label24
            // 
            label24.AutoSize = true;
            label24.Location = new Point(3, 160);
            label24.Name = "label24";
            label24.Size = new Size(39, 15);
            label24.TabIndex = 15;
            label24.Text = "Speed";
            // 
            // roomSpeedBox
            // 
            roomSpeedBox.Location = new Point(44, 158);
            roomSpeedBox.Maximum = new decimal(new int[] { int.MaxValue, 0, 0, 0 });
            roomSpeedBox.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            roomSpeedBox.Name = "roomSpeedBox";
            roomSpeedBox.Size = new Size(87, 23);
            roomSpeedBox.TabIndex = 14;
            roomSpeedBox.Value = new decimal(new int[] { 30, 0, 0, 0 });
            roomSpeedBox.ValueChanged += roomSpeedBox_ValueChanged;
            // 
            // creationCodeBtn
            // 
            creationCodeBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            creationCodeBtn.Location = new Point(9, 432);
            creationCodeBtn.Name = "creationCodeBtn";
            creationCodeBtn.Size = new Size(171, 23);
            creationCodeBtn.TabIndex = 13;
            creationCodeBtn.Text = "Creation Code";
            creationCodeBtn.UseVisualStyleBackColor = true;
            creationCodeBtn.Click += creationCodeBtn_Click;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(3, 32);
            label6.Name = "label6";
            label6.Size = new Size(119, 15);
            label6.TabIndex = 8;
            label6.Text = "Caption for the room";
            // 
            // captionBox
            // 
            captionBox.Location = new Point(6, 49);
            captionBox.Name = "captionBox";
            captionBox.Size = new Size(171, 23);
            captionBox.TabIndex = 9;
            captionBox.TextChanged += captionBox_TextChanged;
            // 
            // tabPage3
            // 
            tabPage3.Controls.Add(bgVertSpdBox);
            tabPage3.Controls.Add(bgHorSpdBox);
            tabPage3.Controls.Add(label22);
            tabPage3.Controls.Add(Label23);
            tabPage3.Controls.Add(bgYBox);
            tabPage3.Controls.Add(bgXBox);
            tabPage3.Controls.Add(label21);
            tabPage3.Controls.Add(label10);
            tabPage3.Controls.Add(bgStretchBox);
            tabPage3.Controls.Add(bgTileVerBox);
            tabPage3.Controls.Add(bgTileHorBox);
            tabPage3.Controls.Add(bgForegroundBox);
            tabPage3.Controls.Add(bgVisibleBox);
            tabPage3.Controls.Add(bgImageBox);
            tabPage3.Controls.Add(bgListBox);
            tabPage3.Controls.Add(drawBgColorBox);
            tabPage3.Controls.Add(backColorBox);
            tabPage3.Controls.Add(label7);
            tabPage3.Location = new Point(4, 44);
            tabPage3.Name = "tabPage3";
            tabPage3.Padding = new Padding(3);
            tabPage3.Size = new Size(183, 457);
            tabPage3.TabIndex = 2;
            tabPage3.Text = "Backgrounds";
            tabPage3.UseVisualStyleBackColor = true;
            // 
            // bgVertSpdBox
            // 
            bgVertSpdBox.Location = new Point(131, 391);
            bgVertSpdBox.Maximum = new decimal(new int[] { int.MaxValue, 0, 0, 0 });
            bgVertSpdBox.Minimum = new decimal(new int[] { int.MinValue, 0, 0, int.MinValue });
            bgVertSpdBox.Name = "bgVertSpdBox";
            bgVertSpdBox.Size = new Size(43, 23);
            bgVertSpdBox.TabIndex = 19;
            bgVertSpdBox.ValueChanged += bgVertSpdBox_ValueChanged;
            // 
            // bgHorSpdBox
            // 
            bgHorSpdBox.Location = new Point(131, 365);
            bgHorSpdBox.Maximum = new decimal(new int[] { int.MaxValue, 0, 0, 0 });
            bgHorSpdBox.Minimum = new decimal(new int[] { int.MinValue, 0, 0, int.MinValue });
            bgHorSpdBox.Name = "bgHorSpdBox";
            bgHorSpdBox.Size = new Size(43, 23);
            bgHorSpdBox.TabIndex = 18;
            bgHorSpdBox.ValueChanged += bgHorSpdBox_ValueChanged;
            // 
            // label22
            // 
            label22.AutoSize = true;
            label22.Location = new Point(64, 393);
            label22.Name = "label22";
            label22.Size = new Size(65, 15);
            label22.TabIndex = 17;
            label22.Text = "Vert. Speed";
            // 
            // Label23
            // 
            Label23.AutoSize = true;
            Label23.Location = new Point(66, 367);
            Label23.Name = "Label23";
            Label23.Size = new Size(65, 15);
            Label23.TabIndex = 16;
            Label23.Text = "Hor. Speed";
            // 
            // bgYBox
            // 
            bgYBox.Location = new Point(134, 312);
            bgYBox.Maximum = new decimal(new int[] { int.MaxValue, 0, 0, 0 });
            bgYBox.Minimum = new decimal(new int[] { int.MinValue, 0, 0, int.MinValue });
            bgYBox.Name = "bgYBox";
            bgYBox.Size = new Size(43, 23);
            bgYBox.TabIndex = 15;
            bgYBox.ValueChanged += bgYBox_ValueChanged;
            // 
            // bgXBox
            // 
            bgXBox.Location = new Point(134, 286);
            bgXBox.Maximum = new decimal(new int[] { int.MaxValue, 0, 0, 0 });
            bgXBox.Minimum = new decimal(new int[] { int.MinValue, 0, 0, int.MinValue });
            bgXBox.Name = "bgXBox";
            bgXBox.Size = new Size(43, 23);
            bgXBox.TabIndex = 14;
            bgXBox.ValueChanged += bgXBox_ValueChanged;
            // 
            // label21
            // 
            label21.AutoSize = true;
            label21.Location = new Point(117, 314);
            label21.Name = "label21";
            label21.Size = new Size(14, 15);
            label21.TabIndex = 13;
            label21.Text = "Y";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(117, 288);
            label10.Name = "label10";
            label10.Size = new Size(14, 15);
            label10.TabIndex = 12;
            label10.Text = "X";
            // 
            // bgStretchBox
            // 
            bgStretchBox.AutoSize = true;
            bgStretchBox.Location = new Point(9, 333);
            bgStretchBox.Name = "bgStretchBox";
            bgStretchBox.Size = new Size(63, 19);
            bgStretchBox.TabIndex = 11;
            bgStretchBox.Text = "Stretch";
            bgStretchBox.UseVisualStyleBackColor = true;
            bgStretchBox.CheckedChanged += bgStretchBox_CheckedChanged;
            // 
            // bgTileVerBox
            // 
            bgTileVerBox.AutoSize = true;
            bgTileVerBox.Checked = true;
            bgTileVerBox.CheckState = CheckState.Checked;
            bgTileVerBox.Location = new Point(9, 310);
            bgTileVerBox.Name = "bgTileVerBox";
            bgTileVerBox.Size = new Size(70, 19);
            bgTileVerBox.TabIndex = 10;
            bgTileVerBox.Text = "Tile Vert.";
            bgTileVerBox.UseVisualStyleBackColor = true;
            bgTileVerBox.CheckedChanged += bgTileVerBox_CheckedChanged;
            // 
            // bgTileHorBox
            // 
            bgTileHorBox.AutoSize = true;
            bgTileHorBox.Checked = true;
            bgTileHorBox.CheckState = CheckState.Checked;
            bgTileHorBox.Location = new Point(9, 287);
            bgTileHorBox.Name = "bgTileHorBox";
            bgTileHorBox.Size = new Size(70, 19);
            bgTileHorBox.TabIndex = 9;
            bgTileHorBox.Text = "Tile Hor.";
            bgTileHorBox.UseVisualStyleBackColor = true;
            bgTileHorBox.CheckedChanged += bgTileHorBox_CheckedChanged;
            // 
            // bgForegroundBox
            // 
            bgForegroundBox.AutoSize = true;
            bgForegroundBox.Location = new Point(9, 223);
            bgForegroundBox.Name = "bgForegroundBox";
            bgForegroundBox.Size = new Size(124, 19);
            bgForegroundBox.TabIndex = 7;
            bgForegroundBox.Text = "Foreground image";
            bgForegroundBox.UseVisualStyleBackColor = true;
            bgForegroundBox.CheckedChanged += bgForegroundBox_CheckedChanged;
            // 
            // bgVisibleBox
            // 
            bgVisibleBox.AutoSize = true;
            bgVisibleBox.Location = new Point(9, 200);
            bgVisibleBox.Name = "bgVisibleBox";
            bgVisibleBox.Size = new Size(155, 19);
            bgVisibleBox.TabIndex = 6;
            bgVisibleBox.Text = "Visible when room starts";
            bgVisibleBox.UseVisualStyleBackColor = true;
            bgVisibleBox.CheckedChanged += bgVisibleBox_CheckedChanged;
            // 
            // bgImageBox
            // 
            bgImageBox.Location = new Point(11, 248);
            bgImageBox.Margin = new Padding(4, 3, 4, 3);
            bgImageBox.Name = "bgImageBox";
            bgImageBox.Size = new Size(168, 53);
            bgImageBox.TabIndex = 8;
            bgImageBox.SelectionChanged += bgImageBox_SelectionChanged;
            // 
            // bgListBox
            // 
            bgListBox.FormattingEnabled = true;
            bgListBox.Location = new Point(9, 60);
            bgListBox.Name = "bgListBox";
            bgListBox.Size = new Size(165, 124);
            bgListBox.TabIndex = 5;
            bgListBox.DrawItem += bgListBox_DrawItem;
            bgListBox.SelectedIndexChanged += bgListBox_SelectedIndexChanged;
            // 
            // drawBgColorBox
            // 
            drawBgColorBox.AutoSize = true;
            drawBgColorBox.Location = new Point(9, 7);
            drawBgColorBox.Name = "drawBgColorBox";
            drawBgColorBox.Size = new Size(150, 19);
            drawBgColorBox.TabIndex = 4;
            drawBgColorBox.Text = "Draw background color";
            drawBgColorBox.UseVisualStyleBackColor = true;
            drawBgColorBox.CheckedChanged += drawBgColorBox_CheckedChanged;
            // 
            // backColorBox
            // 
            backColorBox.BackColor = Color.Silver;
            backColorBox.BorderStyle = BorderStyle.FixedSingle;
            backColorBox.Location = new Point(43, 30);
            backColorBox.Name = "backColorBox";
            backColorBox.ReadOnly = true;
            backColorBox.Size = new Size(131, 23);
            backColorBox.TabIndex = 1;
            backColorBox.MouseClick += backColorBox_MouseClick;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(6, 33);
            label7.Name = "label7";
            label7.Size = new Size(36, 15);
            label7.TabIndex = 0;
            label7.Text = "Color";
            // 
            // tabPage4
            // 
            tabPage4.Controls.Add(groupBox3);
            tabPage4.Controls.Add(groupBox2);
            tabPage4.Controls.Add(viewsList);
            tabPage4.Controls.Add(viewVisibleBox);
            tabPage4.Controls.Add(useViewsBox);
            tabPage4.Controls.Add(groupBox1);
            tabPage4.Location = new Point(4, 44);
            tabPage4.Name = "tabPage4";
            tabPage4.Padding = new Padding(3);
            tabPage4.Size = new Size(183, 457);
            tabPage4.TabIndex = 3;
            tabPage4.Text = "Views";
            tabPage4.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(viewObjectFollowingBox);
            groupBox3.Controls.Add(viewFollowHSpBox);
            groupBox3.Controls.Add(viewFollowVSpBox);
            groupBox3.Controls.Add(label17);
            groupBox3.Controls.Add(label18);
            groupBox3.Controls.Add(viewFollowHBorBox);
            groupBox3.Controls.Add(viewFollowVBorBox);
            groupBox3.Controls.Add(label19);
            groupBox3.Controls.Add(label20);
            groupBox3.Location = new Point(6, 326);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(171, 108);
            groupBox3.TabIndex = 22;
            groupBox3.TabStop = false;
            groupBox3.Text = "Object following";
            // 
            // viewObjectFollowingBox
            // 
            viewObjectFollowingBox.Location = new Point(7, 20);
            viewObjectFollowingBox.Margin = new Padding(4, 3, 4, 3);
            viewObjectFollowingBox.Name = "viewObjectFollowingBox";
            viewObjectFollowingBox.Size = new Size(154, 33);
            viewObjectFollowingBox.TabIndex = 21;
            viewObjectFollowingBox.SelectionChanged += viewObjectFollowingBox_SelectionChanged;
            // 
            // viewFollowHSpBox
            // 
            viewFollowHSpBox.Location = new Point(124, 55);
            viewFollowHSpBox.Maximum = new decimal(new int[] { int.MaxValue, 0, 0, 0 });
            viewFollowHSpBox.Minimum = new decimal(new int[] { 1, 0, 0, int.MinValue });
            viewFollowHSpBox.Name = "viewFollowHSpBox";
            viewFollowHSpBox.Size = new Size(41, 23);
            viewFollowHSpBox.TabIndex = 18;
            viewFollowHSpBox.Value = new decimal(new int[] { 1, 0, 0, int.MinValue });
            viewFollowHSpBox.ValueChanged += viewFollowHSpBox_ValueChanged;
            // 
            // viewFollowVSpBox
            // 
            viewFollowVSpBox.Location = new Point(124, 81);
            viewFollowVSpBox.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            viewFollowVSpBox.Minimum = new decimal(new int[] { 1, 0, 0, int.MinValue });
            viewFollowVSpBox.Name = "viewFollowVSpBox";
            viewFollowVSpBox.Size = new Size(41, 23);
            viewFollowVSpBox.TabIndex = 17;
            viewFollowVSpBox.Value = new decimal(new int[] { 1, 0, 0, int.MinValue });
            viewFollowVSpBox.ValueChanged += viewFollowVSpBox_ValueChanged;
            // 
            // label17
            // 
            label17.AutoSize = true;
            label17.Location = new Point(97, 57);
            label17.Name = "label17";
            label17.Size = new Size(28, 15);
            label17.TabIndex = 19;
            label17.Text = "Hsp";
            // 
            // label18
            // 
            label18.AutoSize = true;
            label18.Location = new Point(97, 83);
            label18.Name = "label18";
            label18.Size = new Size(26, 15);
            label18.TabIndex = 20;
            label18.Text = "Vsp";
            // 
            // viewFollowHBorBox
            // 
            viewFollowHBorBox.Location = new Point(42, 55);
            viewFollowHBorBox.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            viewFollowHBorBox.Name = "viewFollowHBorBox";
            viewFollowHBorBox.Size = new Size(46, 23);
            viewFollowHBorBox.TabIndex = 14;
            viewFollowHBorBox.Value = new decimal(new int[] { 32, 0, 0, 0 });
            viewFollowHBorBox.ValueChanged += viewFollowHBorBox_ValueChanged;
            // 
            // viewFollowVBorBox
            // 
            viewFollowVBorBox.Location = new Point(41, 81);
            viewFollowVBorBox.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            viewFollowVBorBox.Name = "viewFollowVBorBox";
            viewFollowVBorBox.Size = new Size(46, 23);
            viewFollowVBorBox.TabIndex = 13;
            viewFollowVBorBox.Value = new decimal(new int[] { 32, 0, 0, 0 });
            viewFollowVBorBox.ValueChanged += viewFollowVBorBox_ValueChanged;
            // 
            // label19
            // 
            label19.AutoSize = true;
            label19.Location = new Point(6, 57);
            label19.Name = "label19";
            label19.Size = new Size(34, 15);
            label19.TabIndex = 15;
            label19.Text = "Hbor";
            // 
            // label20
            // 
            label20.AutoSize = true;
            label20.Location = new Point(6, 83);
            label20.Name = "label20";
            label20.Size = new Size(32, 15);
            label20.TabIndex = 16;
            label20.Text = "Vbor";
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(viewPortWBox);
            groupBox2.Controls.Add(viewPortHBox);
            groupBox2.Controls.Add(label13);
            groupBox2.Controls.Add(label14);
            groupBox2.Controls.Add(viewPortXBox);
            groupBox2.Controls.Add(viewPortYBox);
            groupBox2.Controls.Add(label15);
            groupBox2.Controls.Add(label16);
            groupBox2.Location = new Point(6, 246);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(171, 74);
            groupBox2.TabIndex = 21;
            groupBox2.TabStop = false;
            groupBox2.Text = "Port on screen";
            // 
            // viewPortWBox
            // 
            viewPortWBox.Location = new Point(124, 19);
            viewPortWBox.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            viewPortWBox.Name = "viewPortWBox";
            viewPortWBox.Size = new Size(41, 23);
            viewPortWBox.TabIndex = 18;
            viewPortWBox.Value = new decimal(new int[] { 640, 0, 0, 0 });
            viewPortWBox.ValueChanged += viewPortWBox_ValueChanged;
            // 
            // viewPortHBox
            // 
            viewPortHBox.Location = new Point(124, 45);
            viewPortHBox.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            viewPortHBox.Name = "viewPortHBox";
            viewPortHBox.Size = new Size(41, 23);
            viewPortHBox.TabIndex = 17;
            viewPortHBox.Value = new decimal(new int[] { 480, 0, 0, 0 });
            viewPortHBox.ValueChanged += viewPortHBox_ValueChanged;
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Location = new Point(104, 21);
            label13.Name = "label13";
            label13.Size = new Size(18, 15);
            label13.TabIndex = 19;
            label13.Text = "W";
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Location = new Point(104, 47);
            label14.Name = "label14";
            label14.Size = new Size(16, 15);
            label14.TabIndex = 20;
            label14.Text = "H";
            // 
            // viewPortXBox
            // 
            viewPortXBox.Location = new Point(26, 19);
            viewPortXBox.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            viewPortXBox.Name = "viewPortXBox";
            viewPortXBox.Size = new Size(46, 23);
            viewPortXBox.TabIndex = 14;
            viewPortXBox.ValueChanged += viewPortXBox_ValueChanged;
            // 
            // viewPortYBox
            // 
            viewPortYBox.Location = new Point(26, 45);
            viewPortYBox.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            viewPortYBox.Name = "viewPortYBox";
            viewPortYBox.Size = new Size(46, 23);
            viewPortYBox.TabIndex = 13;
            viewPortYBox.ValueChanged += viewPortYBox_ValueChanged;
            // 
            // label15
            // 
            label15.AutoSize = true;
            label15.Location = new Point(6, 21);
            label15.Name = "label15";
            label15.Size = new Size(14, 15);
            label15.TabIndex = 15;
            label15.Text = "X";
            // 
            // label16
            // 
            label16.AutoSize = true;
            label16.Location = new Point(6, 47);
            label16.Name = "label16";
            label16.Size = new Size(14, 15);
            label16.TabIndex = 16;
            label16.Text = "Y";
            // 
            // viewsList
            // 
            viewsList.FormattingEnabled = true;
            viewsList.Location = new Point(6, 29);
            viewsList.Name = "viewsList";
            viewsList.Size = new Size(171, 94);
            viewsList.TabIndex = 20;
            viewsList.SelectedIndexChanged += viewsList_SelectedIndexChanged;
            // 
            // viewVisibleBox
            // 
            viewVisibleBox.AutoSize = true;
            viewVisibleBox.Location = new Point(6, 143);
            viewVisibleBox.Name = "viewVisibleBox";
            viewVisibleBox.Size = new Size(155, 19);
            viewVisibleBox.TabIndex = 19;
            viewVisibleBox.Text = "Visible when room starts";
            viewVisibleBox.UseVisualStyleBackColor = true;
            viewVisibleBox.CheckedChanged += viewVisibleBox_CheckedChanged;
            // 
            // useViewsBox
            // 
            useViewsBox.AutoSize = true;
            useViewsBox.Location = new Point(6, 6);
            useViewsBox.Name = "useViewsBox";
            useViewsBox.Size = new Size(148, 19);
            useViewsBox.TabIndex = 18;
            useViewsBox.Text = "Enable the use of views";
            useViewsBox.UseVisualStyleBackColor = true;
            useViewsBox.CheckedChanged += useViewsBox_CheckedChanged;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(viewWBox);
            groupBox1.Controls.Add(viewHBox);
            groupBox1.Controls.Add(label11);
            groupBox1.Controls.Add(label12);
            groupBox1.Controls.Add(viewXBox);
            groupBox1.Controls.Add(viewYBox);
            groupBox1.Controls.Add(label9);
            groupBox1.Controls.Add(label8);
            groupBox1.Location = new Point(6, 166);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(171, 74);
            groupBox1.TabIndex = 17;
            groupBox1.TabStop = false;
            groupBox1.Text = "View in room";
            // 
            // viewWBox
            // 
            viewWBox.Location = new Point(124, 19);
            viewWBox.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            viewWBox.Name = "viewWBox";
            viewWBox.Size = new Size(41, 23);
            viewWBox.TabIndex = 18;
            viewWBox.Value = new decimal(new int[] { 640, 0, 0, 0 });
            viewWBox.ValueChanged += viewWBox_ValueChanged;
            // 
            // viewHBox
            // 
            viewHBox.Location = new Point(124, 45);
            viewHBox.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            viewHBox.Name = "viewHBox";
            viewHBox.Size = new Size(41, 23);
            viewHBox.TabIndex = 17;
            viewHBox.Value = new decimal(new int[] { 480, 0, 0, 0 });
            viewHBox.ValueChanged += viewHBox_ValueChanged;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new Point(104, 21);
            label11.Name = "label11";
            label11.Size = new Size(18, 15);
            label11.TabIndex = 19;
            label11.Text = "W";
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Location = new Point(104, 47);
            label12.Name = "label12";
            label12.Size = new Size(16, 15);
            label12.TabIndex = 20;
            label12.Text = "H";
            // 
            // viewXBox
            // 
            viewXBox.Location = new Point(26, 19);
            viewXBox.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            viewXBox.Name = "viewXBox";
            viewXBox.Size = new Size(46, 23);
            viewXBox.TabIndex = 14;
            viewXBox.ValueChanged += viewXBox_ValueChanged;
            // 
            // viewYBox
            // 
            viewYBox.Location = new Point(26, 45);
            viewYBox.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            viewYBox.Name = "viewYBox";
            viewYBox.Size = new Size(46, 23);
            viewYBox.TabIndex = 13;
            viewYBox.ValueChanged += viewYBox_ValueChanged;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(6, 21);
            label9.Name = "label9";
            label9.Size = new Size(14, 15);
            label9.TabIndex = 15;
            label9.Text = "X";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(6, 47);
            label8.Name = "label8";
            label8.Size = new Size(14, 15);
            label8.TabIndex = 16;
            label8.Text = "Y";
            // 
            // objDetailsLbl
            // 
            objDetailsLbl.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            objDetailsLbl.AutoSize = true;
            objDetailsLbl.Location = new Point(213, 503);
            objDetailsLbl.Name = "objDetailsLbl";
            objDetailsLbl.Size = new Size(48, 15);
            objDetailsLbl.TabIndex = 13;
            objDetailsLbl.Text = "Object: ";
            // 
            // mouseLocLbl
            // 
            mouseLocLbl.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            mouseLocLbl.AutoSize = true;
            mouseLocLbl.Location = new Point(347, 503);
            mouseLocLbl.Name = "mouseLocLbl";
            mouseLocLbl.Size = new Size(54, 15);
            mouseLocLbl.TabIndex = 14;
            mouseLocLbl.Text = "X: 0   Y: 0";
            // 
            // boardParentPanel
            // 
            boardParentPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            boardParentPanel.AutoScroll = true;
            boardParentPanel.Controls.Add(boardPanel);
            boardParentPanel.Location = new Point(216, 54);
            boardParentPanel.Name = "boardParentPanel";
            boardParentPanel.Size = new Size(637, 446);
            boardParentPanel.TabIndex = 15;
            // 
            // resetAllCreationCodesBtn
            // 
            resetAllCreationCodesBtn.Location = new Point(531, 8);
            resetAllCreationCodesBtn.Name = "resetAllCreationCodesBtn";
            resetAllCreationCodesBtn.Size = new Size(200, 23);
            resetAllCreationCodesBtn.TabIndex = 16;
            resetAllCreationCodesBtn.Text = "Reset All Instances Creation Code";
            resetAllCreationCodesBtn.UseVisualStyleBackColor = true;
            resetAllCreationCodesBtn.Click += resetAllCreationCodesBtn_Click;
            // 
            // RoomEditor
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(865, 561);
            Controls.Add(resetAllCreationCodesBtn);
            Controls.Add(boardParentPanel);
            Controls.Add(mouseLocLbl);
            Controls.Add(objDetailsLbl);
            Controls.Add(tabControl1);
            Controls.Add(snapyBox);
            Controls.Add(label5);
            Controls.Add(snapxBox);
            Controls.Add(label4);
            Controls.Add(okBtn);
            KeyPreview = true;
            Name = "RoomEditor";
            ShowInTaskbar = false;
            Text = "Room Editor";
            Load += RoomEditor_Load;
            PreviewKeyDown += RoomEditor_PreviewKeyDown;
            ((System.ComponentModel.ISupportInitialize)heightBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)widthBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)snapxBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)snapyBox).EndInit();
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)objAddSelectPanel).EndInit();
            tabPage2.ResumeLayout(false);
            tabPage2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)roomSpeedBox).EndInit();
            tabPage3.ResumeLayout(false);
            tabPage3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)bgVertSpdBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)bgHorSpdBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)bgYBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)bgXBox).EndInit();
            tabPage4.ResumeLayout(false);
            tabPage4.PerformLayout();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)viewFollowHSpBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)viewFollowVSpBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)viewFollowHBorBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)viewFollowVBorBox).EndInit();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)viewPortWBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)viewPortHBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)viewPortXBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)viewPortYBox).EndInit();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)viewWBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)viewHBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)viewXBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)viewYBox).EndInit();
            boardParentPanel.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox nameBox;
        private System.Windows.Forms.Panel boardPanel;
        private System.Windows.Forms.NumericUpDown heightBox;
        private System.Windows.Forms.NumericUpDown widthBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button okBtn;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown snapxBox;
        private System.Windows.Forms.NumericUpDown snapyBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Button creationCodeBtn;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox captionBox;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TextBox backColorBox;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ColorDialog colorPicker;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.NumericUpDown viewXBox;
        private System.Windows.Forms.NumericUpDown viewYBox;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.CheckBox useViewsBox;
        private System.Windows.Forms.NumericUpDown viewWBox;
        private System.Windows.Forms.NumericUpDown viewHBox;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.CheckBox viewVisibleBox;
        private System.Windows.Forms.ListBox viewsList;
        private System.Windows.Forms.Label objDetailsLbl;
        private System.Windows.Forms.Label mouseLocLbl;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.NumericUpDown viewPortWBox;
        private System.Windows.Forms.NumericUpDown viewPortHBox;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.NumericUpDown viewPortXBox;
        private System.Windows.Forms.NumericUpDown viewPortYBox;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.GroupBox groupBox3;
        private GameObjectPickerBox viewObjectFollowingBox;
        private System.Windows.Forms.NumericUpDown viewFollowHSpBox;
        private System.Windows.Forms.NumericUpDown viewFollowVSpBox;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.NumericUpDown viewFollowHBorBox;
        private System.Windows.Forms.NumericUpDown viewFollowVBorBox;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.CheckBox bgForegroundBox;
        private System.Windows.Forms.CheckBox bgVisibleBox;
        private System.Windows.Forms.ListBox bgListBox;
        private System.Windows.Forms.CheckBox drawBgColorBox;
        private ArcadeMaker.IDE.GameBackgroundPickerBox bgImageBox;
        private System.Windows.Forms.NumericUpDown bgYBox;
        private System.Windows.Forms.NumericUpDown bgXBox;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.CheckBox bgStretchBox;
        private System.Windows.Forms.CheckBox bgTileVerBox;
        private System.Windows.Forms.CheckBox bgTileHorBox;
        private System.Windows.Forms.NumericUpDown bgVertSpdBox;
        private System.Windows.Forms.NumericUpDown bgHorSpdBox;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.Label Label23;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.NumericUpDown roomSpeedBox;
        private System.Windows.Forms.CheckBox persistentBox;
        private System.Windows.Forms.CheckBox deleteUnderlyingBox;
        private System.Windows.Forms.Label label26;
        private GameObjectPickerBox objAddSelectBox;
        private System.Windows.Forms.Label label25;
        private System.Windows.Forms.PictureBox objAddSelectPanel;
        private System.Windows.Forms.Panel boardParentPanel;
        private System.Windows.Forms.Button resetAllCreationCodesBtn;
    }
}