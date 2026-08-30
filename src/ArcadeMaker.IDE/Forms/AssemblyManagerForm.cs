using ArcadeMaker.IDE;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace ArcadeMaker.IDE
{
    public partial class AssemblyManagerForm : Form
    {
        public AssemblyManagerForm()
        {
            InitializeComponent();
            LoadAssembliesBox();
        }

        private void LoadAssembliesBox()
        {
            foreach (AssemblyReference assembly in Environment.Project.assemblyReferences)
            {
                assembliesBox.Items.Add(assembly);
            }
        }

        private void Add()
        {
            assembliesBox.Items.Add(new AssemblyReference());
            assembliesBox.SelectedIndex = assembliesBox.Items.Count - 1;
        }

        private void Remove()
        {
            if (assembliesBox.SelectedIndex >= 0)
                assembliesBox.Items.RemoveAt(assembliesBox.SelectedIndex);
            if (assembliesBox.Items.Count >= 1)
                assembliesBox.SelectedIndex = assembliesBox.Items.Count - 1;
        }

        private void Save()
        {
            Environment.Project.assemblyReferences.Clear();
            foreach (AssemblyReference assembly in assembliesBox.Items.OfType<AssemblyReference>())
            {
                Environment.Project.assemblyReferences.Add(assembly);
            }
        }

        private void okBtn_Click(object sender, EventArgs e)
        {
            btnClose = true;
            Save();
            Close();
        }

        private void cancelBtn_Click(object sender, EventArgs e)
        {
            btnClose = true;
            Close();
        }

        private bool skipSet = false;
        private void assembliesBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            skipSet = true;

            if (assembliesBox.SelectedItem is AssemblyReference assembly)
            {
                dllPathRBtn.Checked = assembly.UseDllPath;
                assemblyNameRBtn.Checked = !assembly.UseDllPath;
                dllPathBox.Text = assembly.DllFilePath;
                assemblyNameBox.Text = assembly.AssemblyName;
                includeAsEmbeddedResBox.Checked = assembly.IncludeAsEmbeddedResource;
                docFilePathBox.Text = assembly.DocFilePath;
                groupBox1.Enabled = true;
                removeBtn.Enabled = true;
            }
            else
            {
                dllPathRBtn.Checked = false;
                assemblyNameRBtn.Checked = false;
                dllPathBox.Text = "";
                assemblyNameBox.Text = "";
                includeAsEmbeddedResBox.Checked = false;
                docFilePathBox.Text = "";
                groupBox1.Enabled = false;
                removeBtn.Enabled = false;
            }

            skipSet = false;
        }

        private AssemblyReference SelectedAssembly
        {
            get
            {
                if (assembliesBox.SelectedItem is AssemblyReference assembly)
                    return assembly;
                return null;
            }
        }

        private void dllPathRBtn_CheckedChanged(object sender, EventArgs e)
        {
            if (!skipSet)
            {
                var assembly = SelectedAssembly;
                if (assembly != null)
                    assembly.UseDllPath = dllPathRBtn.Checked;
                assembliesBox.Invalidate();
            }
        }

        private void assemblyNameRBtn_CheckedChanged(object sender, EventArgs e)
        {
            dllPathRBtn_CheckedChanged(null, null);
        }

        private void dllPathBox_TextChanged(object sender, EventArgs e)
        {
            if (!skipSet)
            {
                var assembly = SelectedAssembly;
                if (assembly != null)
                    assembly.DllFilePath = dllPathBox.Text;
                ResetAssembliesListText();
            }
        }

        private void ResetAssembliesListText()
        {
            if (assembliesBox.Items.Count >= 1 && assembliesBox.SelectedIndex >= 0)
            {
                Control focused = this.FindFocusedControl();
                int tbSelectionStart = -1;
                if (focused is TextBox tb)
                {
                    tbSelectionStart = tb.SelectionStart;
                }
                assembliesBox.Items[assembliesBox.SelectedIndex] = assembliesBox.Items[assembliesBox.SelectedIndex];
                focused?.Focus();
                if (focused is TextBox _tb)
                {
                    _tb.SelectionLength = 0;
                    _tb.SelectionStart = tbSelectionStart < 0 ? _tb.Text.Length : tbSelectionStart;
                }
            }
        }

        private void assemblyNameBox_TextChanged(object sender, EventArgs e)
        {
            if (!skipSet)
            {
                var assembly = SelectedAssembly;
                if (assembly != null)
                    assembly.AssemblyName = assemblyNameBox.Text;
                ResetAssembliesListText();
            }
        }

        private void includeAsEmbeddedResBox_CheckedChanged(object sender, EventArgs e)
        {
            if (!skipSet)
            {
                var assembly = SelectedAssembly;
                if (assembly != null)
                    assembly.IncludeAsEmbeddedResource = includeAsEmbeddedResBox.Checked;
            }
        }

        private void addBtn_Click(object sender, EventArgs e)
        {
            Add();
        }

        private void removeBtn_Click(object sender, EventArgs e)
        {
            Remove();
        }

        private void browseDllBtn_Click(object sender, EventArgs e)
        {
            using (var fileDialog = new OpenFileDialog())
            {
                fileDialog.Filter = "DLL files|*.dll";
                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    dllPathBox.Text = fileDialog.FileName;
                }
            }
        }

        private void browseDocFileBtn_Click(object sender, EventArgs e)
        {
            using (var fileDialog = new OpenFileDialog())
            {
                fileDialog.Filter = "XML files|*.xml";
                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    docFilePathBox.Text = fileDialog.FileName;
                }
            }
        }

        private void docFilePathBox_TextChanged(object sender, EventArgs e)
        {
            if (!skipSet)
            {
                var assembly = SelectedAssembly;
                if (assembly != null)
                    assembly.DocFilePath = docFilePathBox.Text;
                assembliesBox.Invalidate();
            }
        }

        private bool btnClose = false;
        private void AssemblyManagerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (btnClose)
                return;
            DialogResult result = MessageBox.Show("Do you want to save changes?", "Confirm", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                Save();
            }
            else if (result == DialogResult.Cancel)
            {
                e.Cancel = true;
            }
        }
    }

    public class AssemblyReference
    {
        public AssemblyReference(string dllFilePath, bool useDllPath, bool includeAsEmbeddedResource, Assembly UsedByIDE)
        {
            DllFilePath = dllFilePath;
            UseDllPath = useDllPath;
            IncludeAsEmbeddedResource = includeAsEmbeddedResource;
            this.UsedByIDE = UsedByIDE;
        }
        public AssemblyReference() { }

        [XmlIgnore]
        public string Title
        {
            get
            {
                if (UseDllPath)
                    return DllFilePath?.FileName();
                else
                    return AssemblyName;
            }
        }

        public string DllFilePath { get; set; }
        public string AssemblyName { get; set; }
        public bool UseDllPath { get; set; } = true;
        public bool IncludeAsEmbeddedResource { get; set; }
        public string DocFilePath { get; set; }

        [XmlIgnore]
        public Assembly UsedByIDE { get; } = null;
        public override string ToString()
        {
            return Title ?? "Unknown Assembly";
        }
    }
}
