using Exp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ArcadeMaker.Engines.MonoGame.Platforms.WindowsDX
{
    public partial class ExceptionViewerDialog : Form
    {
        public ExceptionViewerDialog(Exception ex)
        {
            InitializeComponent();

            this.textBox.Text = $"Engine bug:\n\n{ex}";
        }

        public ExceptionViewerDialog(RuntimeException ex)
        {
            InitializeComponent();

            this.textBox.Text = $"Exception thrown from {ex.source}({ex.line}, {ex.col}):\n\n{ex.Message}";
        }

        private void abortBtn_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Abort;
            Close();
        }

        private void ignoreBtn_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Ignore;
            Close();
        }

        private void ExceptionViewerDialog_Load(object sender, EventArgs e)
        {

        }
    }
}
