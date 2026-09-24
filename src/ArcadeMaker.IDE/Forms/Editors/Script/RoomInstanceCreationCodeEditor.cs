using ArcadeMaker.IDE.Items;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ArcadeMaker.IDE
{
    public partial class RoomInstanceCreationCodeEditor : Form
    {
        private RoomObject obj = null;
        public RoomInstanceCreationCodeEditor(RoomObject obj)
        {
            InitializeComponent();
            this.obj = obj;
            codeBox.Text = obj.Script;
            Text = "Edit creation code for instance of " + obj.obj.name;
        }

        private void resetCodeBtn_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(codeBox.Text))
            {
                if (MessageBox.Show("Are you sure you want to reset creation code for this instance?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    codeBox.Text = "";
            }
        }

        private bool closedWithOkBtn = false;
        private void okBtn_Click(object sender, EventArgs e)
        {
            SaveChanges();
            closedWithOkBtn = true;
            Close();
        }

        private void RoomInstanceCreationCodeEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!closedWithOkBtn && !string.IsNullOrWhiteSpace(codeBox.Text))
            {
                DialogResult result = MessageBox.Show("Do you want to save changes?", "Close", MessageBoxButtons.YesNoCancel);
                if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
                else if (result == DialogResult.Yes)
                {
                    SaveChanges();
                }
            }
        }

        private void SaveChanges()
        {
            obj.Script = codeBox.Text;
        }
    }
}
