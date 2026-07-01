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
        public string filePath { get; set; } = null;
        /// <summary>
        /// The extension of the file, without dot ("wav", "mp3" "mid"...)
        /// </summary>
        public string fileExtension
        {
            get => filePath != null && filePath.Contains('.') ? filePath.Substring(filePath.LastIndexOf('.') + 1) : null;
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

        public GameSound(string name, string filePath = null) : base(name)
        {
            this.filePath = filePath;
            base.getEditor += (s, e) =>
            {
                e = this.editor;
            };
            editor = new SoundEditor(this);
        }
    }
}
