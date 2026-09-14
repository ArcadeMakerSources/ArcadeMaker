using ArcadeMaker.IDE.Items;
using System;
using System.Collections.Generic;

namespace ArcadeMaker.IDE
{
    public partial class GameScriptEditor : Form
    {
        public readonly GameScript script;
        public GameScriptEditor(GameScript script)
        {
            InitializeComponent();
            this.script = script;

            script.NameChanged += (s, e) =>
            {
                if (!renaming)
                    nameBox.Text = e.newName;
            };
        }

        private void GameScriptEditor_Load(object sender, EventArgs e)
        {
            nameBox.Text = script.name;
            scriptBox.Text = script.Script;
        }

        private bool renaming = false;
        private void nameBox_TextChanged(object sender, EventArgs e)
        {
            renaming = true;
            try
            {
                script.name = nameBox.Text;
                if (nameBox.BackColor == Color.Red)
                    nameBox.BackColor = Color.White;
            }
            catch
            {
                nameBox.BackColor = Color.Red;
            }
            renaming = false;
        }

        private void scriptBox_TextChanged(object sender, EventArgs e)
        {
            script.Script = scriptBox.Text;
        }

        private void okBtn_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void GameScriptEditor_FormClosed(object sender, FormClosedEventArgs e)
        {
            
        }
    }
}
