using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ArcadeMaker.IDE.Items
{
    public class GameFont : GameItem, ISetsIcon
    {
        [XmlIgnore]
        public static Bitmap Icon => Properties.Resources.font32;

        public string family;
        public float size = 12;
        public bool bold = false, italic = false;

        [XmlIgnore]
        public new FontEditor editor
        {
            get
            {
                if (editorClosed)
                {
                    base.editor = new FontEditor(this);
                }
                return base.Editor as FontEditor;
            }
            set
            {
                base.editor = value;
            }
        }

        public GameFont(string name) : base(name)
        {
            getEditor += (s, e) =>
            {
                var activateGet = editor;
            };
            editor = new FontEditor(this);

            const string defaultFamily = "Arial";
            if (FontFamily.Families.Find(f => f.Name == defaultFamily) != null)
            {
                family = defaultFamily;
            }
            else if (FontFamily.Families.Length > 0)
            {
                family = FontFamily.Families.First().Name;
            }
        }

        // used by XML seralizition
        public GameFont() : this(Global.GenerateRandomGameItemName("Font")) { }


        // Source - https://stackoverflow.com/a/52695239
        // Posted by Moji
        // Retrieved 2026-04-12, License - CC BY-SA 4.0
        public string? GetTTF()
        {
            var fontNameToFiles = new Dictionary<string, List<string>>();

            foreach (var fontFile in Directory.GetFiles(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Fonts)))
            {
                var fc = new PrivateFontCollection();

                if (File.Exists(fontFile))
                    fc.AddFontFile(fontFile);

                if (fc.Families.Length == 0)
                    continue;

                var name = fc.Families[0].Name;

                if (!fontNameToFiles.TryGetValue(name + (bold ? " Bold" : "") + (italic ? " Italic" : ""), out var files))
                {
                    files = [];
                    fontNameToFiles[name] = files;
                }

                files.Add(fontFile);
            }

            if (!fontNameToFiles.TryGetValue(family, out var result) || result.Count == 0)
                return null;

            return result[0];
        }

    }
}
