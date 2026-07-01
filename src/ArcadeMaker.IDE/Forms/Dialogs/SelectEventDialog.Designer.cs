
namespace ArcadeMaker.IDE
{
    partial class SelectEventDialog
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
            keyDownBtn = new Button();
            keyUpBtn = new Button();
            createBtn = new Button();
            stepBtn = new Button();
            mouseMenu = new ContextMenuStrip(components);
            mouseDownBtn = new ToolStripMenuItem();
            mousePressBtn = new ToolStripMenuItem();
            mouseUpBtn = new ToolStripMenuItem();
            mouseWheelBtn = new ToolStripMenuItem();
            drawBtn = new Button();
            keyPressBtn = new Button();
            cancelBtn = new Button();
            alarmBtn = new Button();
            mouseMenuBtn = new Button();
            collisionBtn = new Button();
            destroyBtn = new Button();
            mouseMenu.SuspendLayout();
            SuspendLayout();
            // 
            // keyDownBtn
            // 
            keyDownBtn.Image = Properties.Resources.evicon24_KeyDown;
            keyDownBtn.ImageAlign = ContentAlignment.MiddleLeft;
            keyDownBtn.Location = new Point(178, 14);
            keyDownBtn.Margin = new Padding(4, 3, 4, 3);
            keyDownBtn.Name = "keyDownBtn";
            keyDownBtn.Size = new Size(157, 32);
            keyDownBtn.TabIndex = 0;
            keyDownBtn.Text = "Key Down";
            keyDownBtn.UseVisualStyleBackColor = true;
            keyDownBtn.Click += keyDownBtn_Click;
            // 
            // keyUpBtn
            // 
            keyUpBtn.Image = Properties.Resources.evicon24_KeyRelease;
            keyUpBtn.ImageAlign = ContentAlignment.MiddleLeft;
            keyUpBtn.Location = new Point(13, 166);
            keyUpBtn.Margin = new Padding(4, 3, 4, 3);
            keyUpBtn.Name = "keyUpBtn";
            keyUpBtn.Size = new Size(157, 32);
            keyUpBtn.TabIndex = 1;
            keyUpBtn.Text = "Key Release";
            keyUpBtn.UseVisualStyleBackColor = true;
            keyUpBtn.Click += keyUpBtn_Click;
            // 
            // createBtn
            // 
            createBtn.BackgroundImageLayout = ImageLayout.Zoom;
            createBtn.Image = Properties.Resources.evicon24_Create;
            createBtn.ImageAlign = ContentAlignment.MiddleLeft;
            createBtn.Location = new Point(13, 14);
            createBtn.Margin = new Padding(4, 3, 4, 3);
            createBtn.Name = "createBtn";
            createBtn.Size = new Size(158, 32);
            createBtn.TabIndex = 2;
            createBtn.Text = "Create";
            createBtn.UseVisualStyleBackColor = true;
            createBtn.Click += createBtn_Click;
            // 
            // stepBtn
            // 
            stepBtn.Image = Properties.Resources.evicon24_Step;
            stepBtn.ImageAlign = ContentAlignment.MiddleLeft;
            stepBtn.Location = new Point(13, 52);
            stepBtn.Margin = new Padding(4, 3, 4, 3);
            stepBtn.Name = "stepBtn";
            stepBtn.Size = new Size(157, 32);
            stepBtn.TabIndex = 3;
            stepBtn.Text = "Step";
            stepBtn.UseVisualStyleBackColor = true;
            stepBtn.Click += stepBtn_Click;
            // 
            // mouseMenu
            // 
            mouseMenu.Items.AddRange(new ToolStripItem[] { mouseDownBtn, mousePressBtn, mouseUpBtn, mouseWheelBtn });
            mouseMenu.Name = "mouseMenu";
            mouseMenu.Size = new Size(192, 92);
            // 
            // mouseDownBtn
            // 
            mouseDownBtn.Name = "mouseDownBtn";
            mouseDownBtn.Size = new Size(191, 22);
            mouseDownBtn.Text = "Mouse Button Down";
            mouseDownBtn.Click += mouseDownBtn_Click;
            // 
            // mousePressBtn
            // 
            mousePressBtn.Name = "mousePressBtn";
            mousePressBtn.Size = new Size(191, 22);
            mousePressBtn.Text = "Mouse Button Press";
            mousePressBtn.Click += mousePressBtn_Click;
            // 
            // mouseUpBtn
            // 
            mouseUpBtn.Name = "mouseUpBtn";
            mouseUpBtn.Size = new Size(191, 22);
            mouseUpBtn.Text = "Mouse Button Release";
            mouseUpBtn.Click += mouseUpBtn_Click;
            // 
            // mouseWheelBtn
            // 
            mouseWheelBtn.Name = "mouseWheelBtn";
            mouseWheelBtn.Size = new Size(191, 22);
            mouseWheelBtn.Text = "Mouse Wheel";
            mouseWheelBtn.Click += mouseWheelBtn_Click;
            // 
            // drawBtn
            // 
            drawBtn.Image = Properties.Resources.evicon24_Draw;
            drawBtn.ImageAlign = ContentAlignment.MiddleLeft;
            drawBtn.Location = new Point(178, 90);
            drawBtn.Margin = new Padding(4, 3, 4, 3);
            drawBtn.Name = "drawBtn";
            drawBtn.Size = new Size(157, 32);
            drawBtn.TabIndex = 5;
            drawBtn.Text = "Draw";
            drawBtn.UseVisualStyleBackColor = true;
            drawBtn.Click += drawBtn_Click;
            // 
            // keyPressBtn
            // 
            keyPressBtn.Image = Properties.Resources.evicon24_KeyPress;
            keyPressBtn.ImageAlign = ContentAlignment.MiddleLeft;
            keyPressBtn.Location = new Point(13, 128);
            keyPressBtn.Margin = new Padding(4, 3, 4, 3);
            keyPressBtn.Name = "keyPressBtn";
            keyPressBtn.Size = new Size(157, 32);
            keyPressBtn.TabIndex = 6;
            keyPressBtn.Text = "Key Press";
            keyPressBtn.UseVisualStyleBackColor = true;
            keyPressBtn.Click += keyPressBtn_Click;
            // 
            // cancelBtn
            // 
            cancelBtn.Location = new Point(64, 240);
            cancelBtn.Margin = new Padding(4, 3, 4, 3);
            cancelBtn.Name = "cancelBtn";
            cancelBtn.Size = new Size(216, 27);
            cancelBtn.TabIndex = 8;
            cancelBtn.Text = "Cancel";
            cancelBtn.UseVisualStyleBackColor = true;
            cancelBtn.Click += cancelBtn_Click;
            // 
            // alarmBtn
            // 
            alarmBtn.Image = Properties.Resources.evicon24_Alarm;
            alarmBtn.ImageAlign = ContentAlignment.MiddleLeft;
            alarmBtn.Location = new Point(178, 128);
            alarmBtn.Margin = new Padding(4, 3, 4, 3);
            alarmBtn.Name = "alarmBtn";
            alarmBtn.Size = new Size(157, 32);
            alarmBtn.TabIndex = 9;
            alarmBtn.Text = "Alarm";
            alarmBtn.UseVisualStyleBackColor = true;
            alarmBtn.Click += alarmBtn_Click;
            // 
            // mouseMenuBtn
            // 
            mouseMenuBtn.ContextMenuStrip = mouseMenu;
            mouseMenuBtn.Image = Properties.Resources.evicon24_MouseDown;
            mouseMenuBtn.ImageAlign = ContentAlignment.MiddleLeft;
            mouseMenuBtn.Location = new Point(178, 52);
            mouseMenuBtn.Margin = new Padding(4, 3, 4, 3);
            mouseMenuBtn.Name = "mouseMenuBtn";
            mouseMenuBtn.Size = new Size(157, 32);
            mouseMenuBtn.TabIndex = 4;
            mouseMenuBtn.Text = "Mouse";
            mouseMenuBtn.UseVisualStyleBackColor = true;
            mouseMenuBtn.MouseClick += mouseMenuBtn_MouseClick;
            // 
            // collisionBtn
            // 
            collisionBtn.Image = Properties.Resources.evicon24_Collision;
            collisionBtn.ImageAlign = ContentAlignment.MiddleLeft;
            collisionBtn.Location = new Point(13, 90);
            collisionBtn.Margin = new Padding(4, 3, 4, 3);
            collisionBtn.Name = "collisionBtn";
            collisionBtn.Size = new Size(157, 32);
            collisionBtn.TabIndex = 10;
            collisionBtn.Text = "Collision";
            collisionBtn.UseVisualStyleBackColor = true;
            collisionBtn.Click += collisionBtn_Click;
            // 
            // destroyBtn
            // 
            destroyBtn.Image = Properties.Resources.evicon24_Destroy;
            destroyBtn.ImageAlign = ContentAlignment.MiddleLeft;
            destroyBtn.Location = new Point(178, 166);
            destroyBtn.Margin = new Padding(4, 3, 4, 3);
            destroyBtn.Name = "destroyBtn";
            destroyBtn.Size = new Size(157, 32);
            destroyBtn.TabIndex = 11;
            destroyBtn.Text = "Destroy";
            destroyBtn.UseVisualStyleBackColor = true;
            destroyBtn.Click += destroyBtn_Click;
            // 
            // SelectEventDialog
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(353, 279);
            Controls.Add(destroyBtn);
            Controls.Add(collisionBtn);
            Controls.Add(alarmBtn);
            Controls.Add(cancelBtn);
            Controls.Add(keyPressBtn);
            Controls.Add(drawBtn);
            Controls.Add(mouseMenuBtn);
            Controls.Add(stepBtn);
            Controls.Add(createBtn);
            Controls.Add(keyUpBtn);
            Controls.Add(keyDownBtn);
            Margin = new Padding(4, 3, 4, 3);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "SelectEventDialog";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Select Event";
            Load += SelectEventDialog_Load;
            mouseMenu.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button keyDownBtn;
        private System.Windows.Forms.Button keyUpBtn;
        private System.Windows.Forms.Button createBtn;
        private System.Windows.Forms.Button stepBtn;
        private System.Windows.Forms.Button drawBtn;
        private System.Windows.Forms.Button keyPressBtn;
        private System.Windows.Forms.Button cancelBtn;
        private System.Windows.Forms.Button alarmBtn;
        private System.Windows.Forms.ContextMenuStrip mouseMenu;
        private System.Windows.Forms.ToolStripMenuItem mouseDownBtn;
        private System.Windows.Forms.ToolStripMenuItem mousePressBtn;
        private System.Windows.Forms.ToolStripMenuItem mouseUpBtn;
        private System.Windows.Forms.ToolStripMenuItem mouseWheelBtn;
        private System.Windows.Forms.Button mouseMenuBtn;
        private Button collisionBtn;
        private Button destroyBtn;
    }
}