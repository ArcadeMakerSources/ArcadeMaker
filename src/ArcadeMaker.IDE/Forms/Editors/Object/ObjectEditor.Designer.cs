
using ArcadeMaker.IDE.Editors.Object.ObjectProperties;

namespace ArcadeMaker.IDE
{
    partial class ObjectEditor
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
            nameBox = new TextBox();
            label1 = new Label();
            okBtn = new Button();
            label2 = new Label();
            addEventBtn = new Button();
            changeEventBtn = new Button();
            solidBox = new CheckBox();
            label5 = new Label();
            depthBox = new NumericUpDown();
            deleteEventBtn = new Button();
            newSpriteBtn = new Button();
            editSpriteBtn = new Button();
            label4 = new Label();
            eventsListView = new ListBox();
            parentBox = new GameObjectPickerBox();
            spriteBox = new GameSpritePickerBox();
            PropertiesModifier = new ObjectPropertiesModifier();
            scriptsListView = new ListBox();
            addScriptBtn = new Button();
            deleteScriptBtn = new Button();
            moveScriptUpBtn = new Button();
            moveScriptDownBtn = new Button();
            ((System.ComponentModel.ISupportInitialize)depthBox).BeginInit();
            SuspendLayout();
            // 
            // nameBox
            // 
            nameBox.Location = new Point(97, 7);
            nameBox.Margin = new Padding(4, 3, 4, 3);
            nameBox.Name = "nameBox";
            nameBox.Size = new Size(171, 23);
            nameBox.TabIndex = 3;
            nameBox.TextChanged += nameBox_TextChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(9, 10);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(77, 15);
            label1.TabIndex = 2;
            label1.Text = "Object Name";
            // 
            // okBtn
            // 
            okBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            okBtn.Location = new Point(13, 362);
            okBtn.Margin = new Padding(4, 3, 4, 3);
            okBtn.Name = "okBtn";
            okBtn.Size = new Size(88, 27);
            okBtn.TabIndex = 4;
            okBtn.Text = "OK";
            okBtn.UseVisualStyleBackColor = true;
            okBtn.Click += okBtn_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(9, 58);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(37, 15);
            label2.TabIndex = 5;
            label2.Text = "Sprite";
            // 
            // addEventBtn
            // 
            addEventBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            addEventBtn.Location = new Point(275, 329);
            addEventBtn.Margin = new Padding(4, 3, 4, 3);
            addEventBtn.Name = "addEventBtn";
            addEventBtn.Size = new Size(211, 27);
            addEventBtn.TabIndex = 8;
            addEventBtn.Text = "Add Event";
            addEventBtn.UseVisualStyleBackColor = true;
            addEventBtn.Click += addEventBtn_Click;
            // 
            // changeEventBtn
            // 
            changeEventBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            changeEventBtn.Location = new Point(386, 362);
            changeEventBtn.Margin = new Padding(4, 3, 4, 3);
            changeEventBtn.Name = "changeEventBtn";
            changeEventBtn.Size = new Size(100, 27);
            changeEventBtn.TabIndex = 9;
            changeEventBtn.Text = "Change";
            changeEventBtn.UseVisualStyleBackColor = true;
            changeEventBtn.Click += changeEventBtn_Click;
            // 
            // solidBox
            // 
            solidBox.AutoSize = true;
            solidBox.Location = new Point(97, 151);
            solidBox.Margin = new Padding(4, 3, 4, 3);
            solidBox.Name = "solidBox";
            solidBox.Size = new Size(52, 19);
            solidBox.TabIndex = 11;
            solidBox.Text = "Solid";
            solidBox.UseVisualStyleBackColor = true;
            solidBox.CheckedChanged += solidBox_CheckedChanged;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(8, 190);
            label5.Margin = new Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new Size(39, 15);
            label5.TabIndex = 16;
            label5.Text = "Depth";
            // 
            // depthBox
            // 
            depthBox.Location = new Point(97, 188);
            depthBox.Margin = new Padding(4, 3, 4, 3);
            depthBox.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            depthBox.Minimum = new decimal(new int[] { 100000, 0, 0, int.MinValue });
            depthBox.Name = "depthBox";
            depthBox.Size = new Size(172, 23);
            depthBox.TabIndex = 17;
            depthBox.ValueChanged += depthBox_ValueChanged;
            // 
            // deleteEventBtn
            // 
            deleteEventBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            deleteEventBtn.Location = new Point(274, 362);
            deleteEventBtn.Margin = new Padding(4, 3, 4, 3);
            deleteEventBtn.Name = "deleteEventBtn";
            deleteEventBtn.Size = new Size(104, 27);
            deleteEventBtn.TabIndex = 18;
            deleteEventBtn.Text = "Delete";
            deleteEventBtn.UseVisualStyleBackColor = true;
            deleteEventBtn.Click += deleteEventBtn_Click;
            // 
            // newSpriteBtn
            // 
            newSpriteBtn.Location = new Point(89, 103);
            newSpriteBtn.Margin = new Padding(4, 3, 4, 3);
            newSpriteBtn.Name = "newSpriteBtn";
            newSpriteBtn.Size = new Size(63, 24);
            newSpriteBtn.TabIndex = 19;
            newSpriteBtn.Text = "New";
            newSpriteBtn.UseVisualStyleBackColor = true;
            newSpriteBtn.Click += newSpriteBtn_Click;
            // 
            // editSpriteBtn
            // 
            editSpriteBtn.Location = new Point(205, 103);
            editSpriteBtn.Margin = new Padding(4, 3, 4, 3);
            editSpriteBtn.Name = "editSpriteBtn";
            editSpriteBtn.Size = new Size(63, 24);
            editSpriteBtn.TabIndex = 20;
            editSpriteBtn.Text = "Edit";
            editSpriteBtn.UseVisualStyleBackColor = true;
            editSpriteBtn.Click += editSpriteBtn_Click;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(10, 267);
            label4.Margin = new Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new Size(41, 15);
            label4.TabIndex = 22;
            label4.Text = "Parent";
            // 
            // eventsListView
            // 
            eventsListView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            eventsListView.DrawMode = DrawMode.OwnerDrawVariable;
            eventsListView.Font = new Font("Microsoft Sans Serif", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 177);
            eventsListView.FormattingEnabled = true;
            eventsListView.Location = new Point(276, 7);
            eventsListView.Margin = new Padding(4, 3, 4, 3);
            eventsListView.Name = "eventsListView";
            eventsListView.Size = new Size(210, 308);
            eventsListView.TabIndex = 25;
            eventsListView.DrawItem += eventsListView_DrawItem;
            eventsListView.MeasureItem += eventsListView_MeasureItem;
            eventsListView.SelectedIndexChanged += eventsListView_SelectedIndexChanged;
            // 
            // parentBox
            // 
            parentBox.Location = new Point(96, 256);
            parentBox.Margin = new Padding(5, 3, 5, 3);
            parentBox.Name = "parentBox";
            parentBox.Size = new Size(173, 38);
            parentBox.TabIndex = 23;
            parentBox.SelectionChanged += parentBox_SelectionChanged;
            // 
            // spriteBox
            // 
            spriteBox.Location = new Point(89, 58);
            spriteBox.Margin = new Padding(5, 3, 5, 3);
            spriteBox.Name = "spriteBox";
            spriteBox.Size = new Size(180, 38);
            spriteBox.TabIndex = 24;
            spriteBox.SelectionChanged += spriteBox_SelectionChanged;
            // 
            // PropertiesModifier
            // 
            PropertiesModifier.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            PropertiesModifier.Location = new Point(664, 7);
            PropertiesModifier.Name = "PropertiesModifier";
            PropertiesModifier.Size = new Size(384, 380);
            PropertiesModifier.TabIndex = 26;
            // 
            // scriptsListView
            // 
            scriptsListView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            scriptsListView.DrawMode = DrawMode.OwnerDrawFixed;
            scriptsListView.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 177);
            scriptsListView.FormattingEnabled = true;
            scriptsListView.Location = new Point(494, 7);
            scriptsListView.Margin = new Padding(4, 3, 4, 3);
            scriptsListView.Name = "scriptsListView";
            scriptsListView.Size = new Size(163, 308);
            scriptsListView.TabIndex = 27;
            scriptsListView.DrawItem += scriptsListView_DrawItem;
            scriptsListView.SelectedIndexChanged += scriptsListView_SelectedIndexChanged;
            scriptsListView.DoubleClick += scriptsListView_DoubleClick;
            // 
            // addScriptBtn
            // 
            addScriptBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            addScriptBtn.Location = new Point(494, 329);
            addScriptBtn.Name = "addScriptBtn";
            addScriptBtn.Size = new Size(163, 27);
            addScriptBtn.TabIndex = 28;
            addScriptBtn.Text = "Add Script";
            addScriptBtn.UseVisualStyleBackColor = true;
            addScriptBtn.Click += addScriptBtn_Click;
            // 
            // deleteScriptBtn
            // 
            deleteScriptBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            deleteScriptBtn.Location = new Point(493, 362);
            deleteScriptBtn.Name = "deleteScriptBtn";
            deleteScriptBtn.Size = new Size(91, 27);
            deleteScriptBtn.TabIndex = 29;
            deleteScriptBtn.Text = "Delete Script";
            deleteScriptBtn.UseVisualStyleBackColor = true;
            deleteScriptBtn.Click += deleteScriptBtn_Click;
            // 
            // moveScriptUpBtn
            // 
            moveScriptUpBtn.Location = new Point(590, 362);
            moveScriptUpBtn.Name = "moveScriptUpBtn";
            moveScriptUpBtn.Size = new Size(30, 27);
            moveScriptUpBtn.TabIndex = 30;
            moveScriptUpBtn.Text = "^";
            moveScriptUpBtn.UseVisualStyleBackColor = true;
            moveScriptUpBtn.Click += moveScriptUpBtn_Click;
            // 
            // moveScriptDownBtn
            // 
            moveScriptDownBtn.Location = new Point(626, 362);
            moveScriptDownBtn.Name = "moveScriptDownBtn";
            moveScriptDownBtn.Size = new Size(30, 27);
            moveScriptDownBtn.TabIndex = 31;
            moveScriptDownBtn.Text = "v";
            moveScriptDownBtn.UseVisualStyleBackColor = true;
            moveScriptDownBtn.Click += moveScriptDownBtn_Click;
            // 
            // ObjectEditor
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1060, 399);
            Controls.Add(moveScriptDownBtn);
            Controls.Add(moveScriptUpBtn);
            Controls.Add(deleteScriptBtn);
            Controls.Add(addScriptBtn);
            Controls.Add(scriptsListView);
            Controls.Add(PropertiesModifier);
            Controls.Add(eventsListView);
            Controls.Add(label4);
            Controls.Add(parentBox);
            Controls.Add(editSpriteBtn);
            Controls.Add(newSpriteBtn);
            Controls.Add(deleteEventBtn);
            Controls.Add(depthBox);
            Controls.Add(label5);
            Controls.Add(solidBox);
            Controls.Add(changeEventBtn);
            Controls.Add(addEventBtn);
            Controls.Add(label2);
            Controls.Add(okBtn);
            Controls.Add(nameBox);
            Controls.Add(label1);
            Controls.Add(spriteBox);
            Margin = new Padding(4, 3, 4, 3);
            Name = "ObjectEditor";
            ShowInTaskbar = false;
            Text = "Object Editor";
            Load += ObjectEditor_Load;
            ((System.ComponentModel.ISupportInitialize)depthBox).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox nameBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button okBtn;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button addEventBtn;
        private System.Windows.Forms.Button changeEventBtn;
        private System.Windows.Forms.CheckBox solidBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown depthBox;
        private System.Windows.Forms.Button deleteEventBtn;
        private System.Windows.Forms.Button newSpriteBtn;
        private System.Windows.Forms.Button editSpriteBtn;
        private GameObjectPickerBox parentBox;
        private System.Windows.Forms.Label label4;
        private GameSpritePickerBox spriteBox;
        private System.Windows.Forms.ListBox eventsListView;
        private ObjectPropertiesModifier PropertiesModifier;
        private ListBox scriptsListView;
        private Button addScriptBtn;
        private Button deleteScriptBtn;
        private Button moveScriptUpBtn;
        private Button moveScriptDownBtn;
    }
}