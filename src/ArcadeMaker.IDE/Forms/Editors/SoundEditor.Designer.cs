
namespace ArcadeMaker.IDE
{
    partial class SoundEditor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SoundEditor));
            label1 = new Label();
            nameBox = new TextBox();
            loadBtn = new Button();
            playBtn = new Button();
            pauseBtn = new Button();
            okBtn = new Button();
            openFileDialog = new OpenFileDialog();
            fileNameLbl = new Label();
            volumeBar = new TrackBar();
            label2 = new Label();
            groupBox1 = new GroupBox();
            backgroundMusicBtn = new RadioButton();
            soundEffectBtn = new RadioButton();
            BackgroundMusicTip = new ToolTip(components);
            SoundEffectTip = new ToolTip(components);
            label3 = new Label();
            panBar = new TrackBar();
            label4 = new Label();
            pitchBar = new TrackBar();
            ((System.ComponentModel.ISupportInitialize)volumeBar).BeginInit();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)panBar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pitchBar).BeginInit();
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
            nameBox.Location = new Point(53, 6);
            nameBox.Name = "nameBox";
            nameBox.Size = new Size(108, 23);
            nameBox.TabIndex = 1;
            nameBox.TextChanged += nameBox_TextChanged;
            // 
            // loadBtn
            // 
            loadBtn.Location = new Point(18, 61);
            loadBtn.Name = "loadBtn";
            loadBtn.Size = new Size(143, 23);
            loadBtn.TabIndex = 2;
            loadBtn.Text = "Import";
            loadBtn.UseVisualStyleBackColor = true;
            loadBtn.Click += loadBtn_Click;
            // 
            // playBtn
            // 
            playBtn.Location = new Point(53, 32);
            playBtn.Name = "playBtn";
            playBtn.Size = new Size(29, 23);
            playBtn.TabIndex = 3;
            playBtn.Text = ">";
            playBtn.UseVisualStyleBackColor = true;
            playBtn.Click += playBtn_Click;
            // 
            // pauseBtn
            // 
            pauseBtn.Location = new Point(88, 32);
            pauseBtn.Name = "pauseBtn";
            pauseBtn.Size = new Size(29, 23);
            pauseBtn.TabIndex = 4;
            pauseBtn.Text = "| |";
            pauseBtn.UseVisualStyleBackColor = true;
            pauseBtn.Click += pauseBtn_Click;
            // 
            // okBtn
            // 
            okBtn.Location = new Point(53, 338);
            okBtn.Name = "okBtn";
            okBtn.Size = new Size(75, 23);
            okBtn.TabIndex = 5;
            okBtn.Text = "OK";
            okBtn.UseVisualStyleBackColor = true;
            okBtn.Click += okBtn_Click;
            // 
            // openFileDialog
            // 
            openFileDialog.Filter = "All Supported Sound Files|*.wav;*.mp3;*.aif;*.aiff|wav files|*.wav|mp3 files|*.mp3|aif files|*.aif;*.aiff";
            // 
            // fileNameLbl
            // 
            fileNameLbl.AutoSize = true;
            fileNameLbl.Location = new Point(53, 93);
            fileNameLbl.Name = "fileNameLbl";
            fileNameLbl.Size = new Size(84, 15);
            fileNameLbl.TabIndex = 6;
            fileNameLbl.Text = "File Name.wav";
            // 
            // volumeBar
            // 
            volumeBar.Location = new Point(59, 210);
            volumeBar.Maximum = 100;
            volumeBar.Name = "volumeBar";
            volumeBar.Size = new Size(108, 45);
            volumeBar.TabIndex = 7;
            volumeBar.TickStyle = TickStyle.None;
            volumeBar.Value = 100;
            volumeBar.Scroll += volumeBar_Scroll;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(10, 210);
            label2.Name = "label2";
            label2.Size = new Size(47, 15);
            label2.TabIndex = 8;
            label2.Text = "Volume";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(backgroundMusicBtn);
            groupBox1.Controls.Add(soundEffectBtn);
            groupBox1.Location = new Point(12, 121);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(149, 83);
            groupBox1.TabIndex = 9;
            groupBox1.TabStop = false;
            groupBox1.Text = "Kind";
            // 
            // backgroundMusicBtn
            // 
            backgroundMusicBtn.AutoSize = true;
            backgroundMusicBtn.Location = new Point(6, 47);
            backgroundMusicBtn.Name = "backgroundMusicBtn";
            backgroundMusicBtn.Size = new Size(124, 19);
            backgroundMusicBtn.TabIndex = 1;
            backgroundMusicBtn.TabStop = true;
            backgroundMusicBtn.Text = "Background Music";
            BackgroundMusicTip.SetToolTip(backgroundMusicBtn, resources.GetString("backgroundMusicBtn.ToolTip"));
            backgroundMusicBtn.UseVisualStyleBackColor = true;
            backgroundMusicBtn.CheckedChanged += backgroundMusicBtn_CheckedChanged;
            // 
            // soundEffectBtn
            // 
            soundEffectBtn.AutoSize = true;
            soundEffectBtn.Location = new Point(6, 22);
            soundEffectBtn.Name = "soundEffectBtn";
            soundEffectBtn.Size = new Size(92, 19);
            soundEffectBtn.TabIndex = 0;
            soundEffectBtn.TabStop = true;
            soundEffectBtn.Text = "Sound Effect";
            SoundEffectTip.SetToolTip(soundEffectBtn, resources.GetString("soundEffectBtn.ToolTip"));
            soundEffectBtn.UseVisualStyleBackColor = true;
            soundEffectBtn.CheckedChanged += soundEffectBtn_CheckedChanged;
            // 
            // BackgroundMusicTip
            // 
            BackgroundMusicTip.AutoPopDelay = 50000;
            BackgroundMusicTip.InitialDelay = 500;
            BackgroundMusicTip.ReshowDelay = 100;
            BackgroundMusicTip.ToolTipTitle = "Only one song can be played at a time";
            // 
            // SoundEffectTip
            // 
            SoundEffectTip.AutoPopDelay = 50000;
            SoundEffectTip.InitialDelay = 500;
            SoundEffectTip.ReshowDelay = 100;
            SoundEffectTip.ToolTipTitle = "Can play multiple instances simultaneously";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(10, 237);
            label3.Name = "label3";
            label3.Size = new Size(27, 15);
            label3.TabIndex = 11;
            label3.Text = "Pan";
            // 
            // panBar
            // 
            panBar.Location = new Point(59, 237);
            panBar.Maximum = 100;
            panBar.Name = "panBar";
            panBar.Size = new Size(108, 45);
            panBar.TabIndex = 10;
            panBar.TickStyle = TickStyle.None;
            panBar.Value = 100;
            panBar.Scroll += panBar_Scroll;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(10, 271);
            label4.Name = "label4";
            label4.Size = new Size(34, 15);
            label4.TabIndex = 13;
            label4.Text = "Pitch";
            // 
            // pitchBar
            // 
            pitchBar.Location = new Point(59, 271);
            pitchBar.Maximum = 100;
            pitchBar.Name = "pitchBar";
            pitchBar.Size = new Size(108, 45);
            pitchBar.TabIndex = 12;
            pitchBar.TickStyle = TickStyle.None;
            pitchBar.Value = 100;
            pitchBar.Scroll += pitchBar_Scroll;
            // 
            // SoundEditor
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(179, 373);
            Controls.Add(label4);
            Controls.Add(pitchBar);
            Controls.Add(label3);
            Controls.Add(panBar);
            Controls.Add(groupBox1);
            Controls.Add(label2);
            Controls.Add(volumeBar);
            Controls.Add(fileNameLbl);
            Controls.Add(okBtn);
            Controls.Add(pauseBtn);
            Controls.Add(playBtn);
            Controls.Add(loadBtn);
            Controls.Add(nameBox);
            Controls.Add(label1);
            Name = "SoundEditor";
            Text = "Sound Editor";
            FormClosed += SoundEditor_FormClosed;
            Load += MusicEditor_Load;
            ((System.ComponentModel.ISupportInitialize)volumeBar).EndInit();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)panBar).EndInit();
            ((System.ComponentModel.ISupportInitialize)pitchBar).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox nameBox;
        private System.Windows.Forms.Button loadBtn;
        private System.Windows.Forms.Button playBtn;
        private System.Windows.Forms.Button pauseBtn;
        private System.Windows.Forms.Button okBtn;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.Label fileNameLbl;
        private System.Windows.Forms.TrackBar volumeBar;
        private System.Windows.Forms.Label label2;
        private GroupBox groupBox1;
        private RadioButton backgroundMusicBtn;
        private RadioButton soundEffectBtn;
        private ToolTip SoundEffectTip;
        private ToolTip BackgroundMusicTip;
        private Label label3;
        private TrackBar panBar;
        private Label label4;
        private TrackBar pitchBar;
    }
}