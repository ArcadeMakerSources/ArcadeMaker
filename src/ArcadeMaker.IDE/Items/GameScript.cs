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
                    base.editor = new ScriptEditor(this, Script);
                    (base.editor as ScriptEditor).OKClicked += (s, e) => Script = e;
                }
                return base.Editor as ScriptEditor;
            }
            set
            {
                value?.OKClicked += (s, e) => Script = e;
                base.editor = value;
            }
        }

        public GameScript(string name, string code = null) : base(name)
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
