using ArcadeMaker.Core.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcadeMaker.IDE.Items
{
    public class GameSound : GameItem
    {
        public static Bitmap Icon => Properties.Resources.sound;
        public byte[]? Data { get; private set; }

        /// <summary>
        /// The extension of the file that this sound was load from, including the opening dot, e.g. ".mp3".
        /// </summary>
        public string? FileExtension
        {
            get;
            private set
            {
                // make sure the value starts with . (like ".mp3")
                if (value == null)
                    field = null;
                else
                {
                    if (!value.StartsWith('.'))
                        value = '.' + value;
                    field = value;
                }
            }
        }

        public void SetSource(string? fileExt, byte[]? row)
        {
            if (fileExt == null || row == null)
            {
                this.FileExtension = null;
                Data = null;
            }
            else
            {
                this.FileExtension = fileExt;
                Data = row;
            }
        }

        public void SetSource(string? file)
        {
            if (file == null)
                SetSource(null, null);
            else
            {
                string ext = System.IO.Path.GetExtension(file) ?? throw new ArgumentException($"The given file must have an extenion.\n(Given file: {file}).", paramName: nameof(file));
                SetSource(ext, File.ReadAllBytes(file));
            }
        }

        public float volume = 1.0F;
        public Sound.Types Type { get; set; } = Sound.Types.SoundEffect;

        public new SoundEditor editor
        {
            get
            {
                if (editorClosed)
                {
                    base.editor = new SoundEditor(this);
                }
                return base.Editor as SoundEditor;
            }
            set
            {
                base.editor = value;
            }
        }

        public float Pan { get; internal set; }
        public float Pitch { get; internal set; }

        public GameSound(string name, string? filePath = null) : base(name)
        {
            SetSource(filePath);
            base.getEditor += (s, e) =>
            {
                e = this.editor;
            };
            editor = new SoundEditor(this);
        }
    }
}
