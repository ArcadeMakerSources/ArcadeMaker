using ArcadeMaker.Core.Resources.Serializeables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcadeMaker.IDE.Items
{
    public class GameScript : GameItem, ISetsIcon, IContainsScript
    {
        public static Bitmap Icon => Properties.Resources.script;

        public string Script { get; set; } = "";
        public bool CompiledSyntaxTree { get; set; } = false;

        public new ScriptEditor editor
        {
            get
            {
                if (editorClosed)
                {
                    ScriptEditor _editor = new(this, Script);
                    base.editor = _editor;

                    void OKClicked(object? sender, string text)
                    {
                        Script = text;
                        _editor.OKClicked -= OKClicked;
                    }
                    _editor.OKClicked += OKClicked;
                }
                return (ScriptEditor)base.Editor;
            }
            set
            {
                value?.OKClicked += (s, e) => Script = e;
                base.editor = value;
            }
        }

        public GameScript(string name, string? code = null) : base(name)
        {
            if (code != null)
                Script = code;
            base.getEditor += (s, e) =>
            {
                e = this.editor;
            };
            this.editor = new ScriptEditor(this, Script);
        }

        public void InitDefaultCode()
        {
            string code = $"func {name}()\n{{\n\t\n}}";
            Script = code;
        }
    }
}
