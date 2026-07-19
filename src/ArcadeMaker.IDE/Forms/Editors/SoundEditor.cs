using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Media;
using IntelliSense.CSharp;
using Exp;
using System.Runtime.Versioning;
using ArcadeMaker.IDE.Items;

namespace ArcadeMaker.IDE
{
    public partial class SoundEditor : Form
    {
        private GameSound sound = null;
        public SoundEditor(GameSound sound)
        {
            InitializeComponent();
            this.sound = sound;

            // set file dialog format
            var crossPlatformFormats = SupportedFormats.Where(f => string.IsNullOrWhiteSpace(f.Comment)).Map(f => f.Format);
            openFileDialog.Filter = $"{Global.ProgramName} Cross-Platform Supported Audio Formats ({string.Join(' ', crossPlatformFormats.Map(f => $"(*.{f})"))})|{string.Join(';', crossPlatformFormats.Map(f => "*." + f))}";
            foreach (var (format, comment) in SupportedFormats)
            {
                openFileDialog.Filter += $"|{format} Files {comment} (*.{format})|*.{format}";
            }

            sound.NameChanged += (s, e) =>
            {
                if (!renaming)
                    nameBox.Text = e.newName;
            };

            fileNameLbl.MaximumSize = new Size(playBtn.Location.X - fileNameLbl.Location.X - 3, 15);
            //soundPlayer?.Dispose();
        }

        private static List<(string Format, string Comment)> SupportedFormats
        {
            get
            {
                List<(string, string)> formats = [];
                foreach (var format in typeof(ArcadeMaker.Core.Resources.Sound.Formats).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
                {
                    string comment = "";
                    var supportedOSAttr = format.GetCustomAttributes(typeof(SupportedOSPlatformAttribute), false) ?? [];
                    int i = 0;
                    foreach (var os in supportedOSAttr)
                    {
                        comment += (os as SupportedOSPlatformAttribute)?.PlatformName ?? "";
                        if (i++ < supportedOSAttr.Length - 1)
                            comment += ", ";
                    }
                    if (comment.Length >= 1)
                        comment = "(" + comment + " Only)";

                    formats.Add((format.Name.ToLower(), comment));
                }
                return formats;
            }
        }

        private void okBtn_Click(object sender, EventArgs e)
        {
            //soundPlayer?.Dispose();
            Close();
        }

        private void MusicEditor_Load(object sender, EventArgs e)
        {
            nameBox.Text = sound.name;
            //if (sound != null && sound.filePath != null)
            //{
            //    fileNameLbl.Text = sound.filePath.FileName();
            //}
            //else
            //{
            //    fileNameLbl.Text = "";
            //}
            fileNameLbl.Text = "";
            float originalVolume = sound.volume;
            volumeBar.Value = (int)(100 * sound.volume);

            // set pan and pitch bars
            panBar.Value = (int)Core.Math.Formulas.LinearMapping(-1d, 1d, panBar.Minimum, panBar.Maximum, sound.Pan);
            pitchBar.Value = (int)Core.Math.Formulas.LinearMapping(-1d, 1d, pitchBar.Minimum, pitchBar.Maximum, sound.Pitch);

            sound.volume = originalVolume;
            soundEffectBtn.Checked = sound.Type == Core.Resources.Sound.Types.SoundEffect;
            backgroundMusicBtn.Checked = sound.Type == Core.Resources.Sound.Types.BackgroundMusic;
        }

        //private AudioPlayer soundPlayer = null;
        private bool isPlaying = false;
        private void playBtn_Click(object sender, EventArgs e)
        {
            if (isPlaying)
                return;
            if (sound.Data != null)
            {
                try
                {
                    //soundPlayer = new WaveOut();
                    //soundPlayer.PlaybackStopped += SoundPlayer_PlaybackStopped;
                    //reader = new AudioFileReader(sound.filePath);
                    //reader.Volume = sound.volume;
                    //soundPlayer.Init(reader);
                    //soundPlayer.Play();
                    isPlaying = true;
                }
                catch (Exception ex)
                {
                    string err = "Cannot play the selected sound";
#if DEBUG
                    err += "\n\nError message:\n" + ex.Message;
#endif
                    MessageBox.Show(err);
                }
            }
            else
            {
                MessageBox.Show("Select audio file to play");
            }
        }

        private bool userStoppedAudio = false;
        private void SoundPlayer_PlaybackStopped(object sender, object e)
        {
            //Global.ShowDebugMessage("playback stopped");
            if (!userStoppedAudio)
            {
                //reader.Position = 0;
                //soundPlayer.Play();
            }
        }

        private void pauseBtn_Click(object sender, EventArgs e)
        {
            //            if (soundPlayer != null && isPlaying)
            //            {
            //                try
            //                {
            //                    userStoppedAudio = true;
            //                    soundPlayer.Stop();
            //                    soundPlayer.Dispose();
            //                    reader?.Dispose();
            //                    userStoppedAudio = false;
            //                    isPlaying = false;
            //                }
            //                catch (Exception ex)
            //                {
            //                    string err = "Error: Cannot pause";
            //#if DEBUG
            //                    err += "\n\nError message:\n" + ex.Message;
            //#endif
            //                    MessageBox.Show(err);
            //                }
            //            }
            //            else
            //            {
            //                MessageBox.Show("Nothing is being played");
            //            }
        }

        private void loadBtn_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                sound.SetSource(openFileDialog.FileName);
                fileNameLbl.Text = openFileDialog.FileName.FileName();
            }
        }

        private bool renaming = false;
        private void nameBox_TextChanged(object sender, EventArgs e)
        {
            renaming = true;
            try
            {
                sound.name = nameBox.Text;
                if (nameBox.BackColor == Color.Red)
                    nameBox.BackColor = Color.White;
            }
            catch
            {
                nameBox.BackColor = Color.Red;
            }
            renaming = false;
        }

        private void volumeBar_Scroll(object sender, EventArgs e)
        {
            sound.volume = (volumeBar.Value - volumeBar.Minimum) * 1F / (volumeBar.Maximum - volumeBar.Minimum);

        }

        private void soundEffectBtn_CheckedChanged(object sender, EventArgs e)
        {
            if (soundEffectBtn.Checked)
                sound.Type = Core.Resources.Sound.Types.SoundEffect;
        }

        private void backgroundMusicBtn_CheckedChanged(object sender, EventArgs e)
        {
            if (backgroundMusicBtn.Checked)
                sound.Type = Core.Resources.Sound.Types.BackgroundMusic;

            panBar.Enabled = !backgroundMusicBtn.Checked;
            pitchBar.Enabled = !backgroundMusicBtn.Checked;
            panBar.Value = panBar.Maximum / 2;
            pitchBar.Value = pitchBar.Maximum / 2;
        }

        private void panBar_Scroll(object sender, EventArgs e)
        {
            sound.Pan = (float)Core.Math.Formulas.LinearMapping(panBar.Minimum, panBar.Maximum, -1d, 1d, panBar.Value);
        }

        private void pitchBar_Scroll(object sender, EventArgs e)
        {
            sound.Pitch = (float)Core.Math.Formulas.LinearMapping(pitchBar.Minimum, pitchBar.Maximum, -1d, 1d, pitchBar.Value);
        }
    }

    interface ISoundPlayer : IDisposable
    {
        void Init();
        void Play();
        void Stop();
        event EventHandler PlaybackStopped;
    }
}
