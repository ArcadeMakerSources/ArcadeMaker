using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.ComponentModel.Design.Serialization;
using System.Xml.Linq;
using System.Diagnostics;
using System.Collections.Immutable;
using System.Reflection;
using System.Threading;
using ArcadeMaker.IDE.Scripting;
using System.Xml;
using System.Xml.Serialization;
using Exp;
using ArcadeMaker.IDE.Debugging;
using ArcadeMaker.IDE.Items;
using ArcadeMaker.Core.Resources.Serializeables;

namespace ArcadeMaker.IDE
{
    public partial class ScriptEditor : Form
    {
        /// <summary>
        /// <inheritdoc cref="string.IndexOf(char, int)" path="/param[@name='startIndex']"/> 
        /// Ruuning on a <see cref="Task">Tasking</see> thread <seealso href="http://www.www">link</seealso><paramref name="param">pref</paramref> <c>for (int i = 0;;)</c>
        /// <list type="table">
        /// <listheader><description>hello <see langword="false"/></description></listheader><item><term>hello</term><description>world</description></item>
        /// </list>
        /// </summary> 
        /// <typeparam name="Hello"></typeparam>
        /// <typeparam name="World"></typeparam>
        /// <param name="param"></param>
        /// <inheritdoc cref="string.IndexOf(char)" path="/param"/>
        /// <returns>a task</returns>
        /// <exception cref="NotImplementedException"></exception>
        public static async Task TestG<Hello, World>(int param, char value) where World : unmanaged
        {
            bool @bool = false;
            if (@bool) await TestG<EventArgs, int>(7, 'a');
            throw new NotImplementedException();
        }

        private string classname = null;
        private readonly IContainsScript obj = null;

        private List<ScriptBoxSpan> Spans => scriptBox.Spans;

        public ScriptEditor(IContainsScript obj, string defaultText = "", string classname = null)
        {
            this.obj = obj;
            InitializeComponent();
            ShowInTaskbar = false;
            if (defaultText != null)
                scriptBox.Spans.AddRange(Global.GetScriptBoxSpans(defaultText, SpansTextBox2.TabSpace));
            this.classname = classname;
            //checkTimer.Interval = 200;

            scriptBox.SelectionStartChanged += async (s, e) => await Task.Run(() => ScriptBox_SelectionStartChanged(s, e));
            scriptBox.MouseHover += async (s, e) => await Task.Run(() => ScriptBox_MouseHover(s, e));

            expSuggestions.Sort();
            //scriptBox.OnCompletionItemShowInfo += ScriptBox_OnCompletionItemInfo;
            //scriptBox.CharAlerts.Add('.');
            //scriptBox.CharAlert += ScriptBox_CharAlert;
        }

        private void ScriptBox_MouseHover(object sender, EventArgs e)
        {
            
        }

        private string ReadInheritDoc(InheritDoc doc, Func<Relation, string> ReadRelation)
        {
            return null;
        }

        private void ScriptBox_SelectionStartChanged(object sender, int e)
        {
            
        }

        private async void ScriptEditor_Load(object sender, EventArgs e)
        {
            scriptBox.Loading = true;
            scriptBox.Enabled = false;

            string script = obj.Script;
            if (obj is RoomObject ro)
                script = ro.ScriptOrDefaultCreationCode;
            if (string.IsNullOrEmpty(scriptBox.Text) && !string.IsNullOrEmpty(script))
                scriptBox.Spans.AddRange(Global.GetScriptBoxSpans(script, SpansTextBox2.TabSpace));
            //await Task.Run(InitCompilation);

            // copy columns from main form's error box
            ColumnHeader[] columns = new ColumnHeader[Global.form1.errorsBox.Columns.Count];
            for (int i = 0; i < columns.Length; i++)
                columns[i] = (ColumnHeader)Global.form1.errorsBox.Columns[i].Clone();
            errorsBox.Columns.AddRange(columns);
            Debugging.Debug.OnDebugBuild += (s, errors) => errorsBox.FillErrors(errors);
            errorsBox.AttachMenu();

            scriptBox.Enabled = true;
            scriptBox.Loading = false;
            loaded = true;
        }

        public event EventHandler<string> OKClicked;
        private bool okClose = false;
        private void okBtn_Click(object sender, EventArgs e)
        {
            var handler = OKClicked;
            handler?.Invoke(this, scriptBox.Text);
            okClose = true;
            Close();
            okClose = false;
        }

        /*private bool skipTab = false;
        private void scriptBoxTab()
        {
            if (!skipTab)
            {
                int index = scriptBox.Text.GetLineColPosition(scriptBox.Caret.Line, scriptBox.Caret.Col);
                int depth = 0;
                for (int i = 0; i < index; i++)
                {
                    char c = scriptBox.Text[i];
                    if (c == '{')
                        depth++;
                    else if (c == '}')
                        depth--;
                    if (c == '\n')
                    {
                        while (scriptBox.Text[i + 1] == ' ' || scriptBox.Text[i + 1] == '\t')
                        {
                            i++;
                        }
                        if (scriptBox.Text[i + 1] == '}')
                        {
                            depth--;
                            i += 2;
                        }
                        if (i >= index)
                        {
                            string tabs = "";
                            for (int t = 0; t < depth; t++)
                                tabs += "    ";
                            skipTab = true;
                            scriptBox.Text = scriptBox.Text.Insert(index - 2, tabs);
                            skipTab = false;
                            for (int t = 0; t < depth * 4; t++)
                                scriptBox.Caret.MoveRight();
                            return;
                        }
                    }
                }
            }
        }
        */

        private void findBtn_Click(object sender, EventArgs e)
        {
            var findDialog = new SpansTextBox2SearchForm(scriptBox);
            findDialog.Owner = this;
            findDialog.Show();
        }

        private bool changes = false;
        private void scriptBox_Load(object sender, EventArgs e)
        {
            textChangedTimer.Tick += textChangedTimer_Tick;
        }

        private System.Windows.Forms.Timer textChangedTimer = new() { Interval = 5000 };
        private void scriptBox_TextChanged(object sender, SpansTextBox2TextChangedEventArgs e)
        {
            textChangedTimer.Stop();
            textChangedTimer.Start();

            LoadAndShowSuggestions();
        }


        // all engine funcs and properties:
        private static readonly List<SpansTextBox2Suggestion> expSuggestions =
        [..
            Core.ExpSrc.ExpSrc.AllExternFuncsAndProperties
            .Map(extrn => new SpansTextBox2Suggestion(extrn))
            .AppendRange(Exp.Spans.Filter.Keywords.Map(kw => new SpansTextBox2Suggestion("keyword", kw) { TypeLabelColor = Color.Yellow }))
        ];

        private void LoadAndShowSuggestions()
        {
            // get the word the caret is currently on
            var textSpan = scriptBox.GetSpanByCharIndex(scriptBox.SelectionStart - 1, out int spanStart);
            if (textSpan == null)
                return;

            string? word = textSpan.type == SpanType.Normal ? textSpan.text : null;

            // if it's a word and not another kind of span, show the suggestions which contains this word
            if (word != null)
            {
                var suggestions = expSuggestions.Where(sug => sug.Text.Contains(word, StringComparison.CurrentCultureIgnoreCase));
                scriptBox.ShowSuggestions([.. suggestions], spanStart, spanStart + word.Length);
            }
        }

        private async void textChangedTimer_Tick(object? sender, EventArgs e)
        {
            textChangedTimer.Stop();

            var scriptBu = obj.Script;
            obj.Script = scriptBox.Text;

            // check errors
            HashSet<ExpError>? errors = null;
            await Task.Run(() =>
            {
                Debugging.Debug.TryBuild();
            });

            obj.Script = scriptBu;
        }

        private CancellationTokenSource getCompletion_cancellationToken = new CancellationTokenSource();
        private bool getCompletion_isRunning = false;
        private SpansTextBox2Suggestion[] lastSugList = null;


        private static bool checkErrorsThreadFree = true;

        

        string gameSettings = null;
        string gameResources = null;

        private void ColorScriptEditor()
        {
            throw new NotImplementedException();
        }

        private int FindSpan(int searchFrom, char startsWith)
        {
            for (int span = searchFrom; span >= 0 && span < Spans.Count; span++)
            {
                if (Spans[span].text.Length > 0 && Spans[span].text[0] == startsWith)
                    return span;
            }

            return -1;
        }

        private int FindSpan(int charIndex)
        {
            for (int spanIndex = 0, totalCharIndex = 0; spanIndex < Spans.Count; spanIndex++)
            {
                var span = Spans[spanIndex];
                for (int i = 0; span != null && i < span.text.Length; i++, totalCharIndex++)
                {
                    if (totalCharIndex == charIndex)
                        return spanIndex;
                }
            }

            return -1;
        }

        private bool loaded = false;
        private void ScriptEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (loaded && /*changes &&*/ !okClose)
            {
                DialogResult result = MessageBox.Show("Do you want to save changes?", "Close Code Editor", MessageBoxButtons.YesNoCancel);
                if (result == DialogResult.Yes)
                {
                    var handler = OKClicked;
                    handler?.Invoke(okBtn, scriptBox.Text);
                }
                else if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }

        private void ScriptEditor_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.FormClosing -= ScriptEditor_FormClosing; // if not, causes a bug of warning box appear on closing object editor or IDE
            
            Dispose();
        }
    }
}