using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ArcadeMaker.IDE;
using ArcadeMaker.IDE.Editors.Object.ObjectProperties;
using ArcadeMaker.IDE.Items;
using Exp;
using ArcadeMaker.Core.Resources.Serializeables;
using ArcadeMaker.Core.Models;
using ArcadeMaker.IDE.Properties;
using ArcadeMaker.Core.Common;

namespace ArcadeMaker.IDE
{
    public partial class ObjectEditor : Form
    {
        private GameObject obj = null;
        public GameSprite sprite
        {
            get
            {
                return obj.sprite;
            }
        }

        public ObjectEditor(GameObject obj)
        {
            InitializeComponent();

            this.obj = obj;
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            // update name box when object is renamed
            obj.NameChanged += (s, e) =>
            {
                if (!renaming)
                    nameBox.Text = e.newName;
            };

            nameBox.Text = obj.name;
        }

        private bool renaming = false;

        private void nameBox_TextChanged(object sender, EventArgs e)
        {
            renaming = true;
            try
            {
                obj.name = nameBox.Text;
                if (nameBox.BackColor == Color.Red)
                    nameBox.BackColor = Color.White;
            }
            catch
            {
                nameBox.BackColor = Color.Red;
            }
            renaming = false;
        }

        private void okBtn_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ObjectEditor_Load(object sender, EventArgs e)
        {
            PropertiesModifier.Init(obj);
            foreach (var ev in obj.Events)
            {
                eventsListView.Items.Add(ev);
            }
            solidBox.Checked = obj.solid;
            depthBox.Value = obj.depth;
            Environment.project.items.CollectionChanged += (s, ea) =>
            {
                LoadSpriteBox();
            };
            SelectEventDialog.EventSelected += (s, ea) =>
            {
                // if an event with the same type [and param] already exists, select it instead of adding a new one
                ObjectEvent? existing = obj.Events.FirstOrDefault(e => e.Type == ea.Type && e.GetParam(out var existingParam) == ea.GetParam(out var newParam) && object.Equals(existingParam, newParam));
                if (existing == null)
                {
                    existing = ea;
                    obj.Events.Add(ea);
                    eventsListView.Items.Add(ea);
                }
                eventsListView.SelectedItem = existing;
            };
            LoadSpriteBox();
            parentBox.Resource = obj.parent;
            parentBox.DefaultItemTitle = "<No Parent>";
        }

        private void LoadSpriteBox()
        {
            spriteBox.DefaultItemTitle = "<No Sprite>";
            if (sprite != null)
                spriteBox.Resource = sprite;
        }

        private SelectEventDialog SelectEventDialog = new SelectEventDialog();
        private void addEventBtn_Click(object sender, EventArgs e)
        {
            SelectEventDialog.ShowDialog();
        }

        private void solidBox_CheckedChanged(object sender, EventArgs e)
        {
            obj.solid = solidBox.Checked;
        }

        bool firstEdit = true;
        //private void fullCodeBtn_Click(object sender, EventArgs e)
        //{
        //    /*
        //    Form editForm = new Form {  };
        //    RichTextBox scriptBox = new RichTextBox { Text = obj.part2script.Replace("classname", obj.name), AcceptsTab = true,
        //        Size = new Size(editForm.Size.Width, editForm.Size.Height - 20),
        //        Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom };
        //    Button okEditBtn = new Button { Text = "OK", Location = new Point(5, scriptBox.Size.Height + 5), Anchor = AnchorStyles.Bottom | AnchorStyles.Left };
        //    okEditBtn.Click += (s, ea) =>
        //    {
        //        editForm.Close();
        //    };
        //    editForm.FormClosed += (s, ea) =>
        //    {
        //        obj.part2script = scriptBox.Text;
        //    };
        //    editForm.Controls.Add(scriptBox);
        //    editForm.Controls.Add(okEditBtn);
        //    editForm.ShowDialog();
        //    */
        //    if (eventsListView.SelectedItem == null)
        //    {
        //        MessageBox.Show("Select an event to edit its script.");
        //        return;
        //    }
        //    if (firstEdit)
        //    {
        //        firstEdit = false;
        //        //obj.Script = obj.Script;
        //    }
        //    ScriptEditor editor = new(eventsListView.SelectedItem as EventScript, classname: obj.name);
        //    editor.Owner = this;
        //    editor.OKClicked += (s, script) =>
        //    {
        //        if (eventsListView.SelectedItem?.ToString() == ObjectEvent.Create.ToString())
        //            obj.createEventScripts.Script = script;
        //        else if (eventsListView.SelectedItem?.ToString() == ObjectEvent.Step.ToString())
        //            obj.stepEventScripts.Script = script;
        //        else if (eventsListView.SelectedItem?.ToString() == ObjectEvent.Draw.ToString())
        //            obj.drawEventScripts.Script = script;
        //        else
        //            throw new NotImplementedException();
        //    };
        //    editor.ShowDialog();
        //}

        private void depthBox_ValueChanged(object sender, EventArgs e)
        {
            obj.depth = (int)depthBox.Value;
        }

        private void deleteEventBtn_Click(object sender, EventArgs e)
        {
            if (eventsListView.SelectedItems.Count == 0) return;

            try
            {
                var item = eventsListView.SelectedItems[0];
                ObjectEvent ev = (ObjectEvent)item!;
                obj.Events.Remove(ev);
                eventsListView.Items.Remove(item!);
            }
            catch (Exception ex)
            {
                string err = "Cannot delete or change event";
#if DEBUG
                err += "\n\nException:\n" + ex;
#endif
                MessageBox.Show(err, "Error");
            }
        }

        private void changeEventBtn_Click(object sender, EventArgs e)
        {
            deleteEventBtn_Click(null, null);
            addEventBtn_Click(null, null);
        }

        private void editSpriteBtn_Click(object sender, EventArgs e)
        {
            if (sprite != null)
            {
                try
                {
                    sprite.editor.MdiParent = Global.form1;
                    sprite.editor.Show();
                }
                catch { }
            }
            else
            {
                MessageBox.Show("Select sprite to edit");
            }
        }

        private void newSpriteBtn_Click(object sender, EventArgs e)
        {
            // create sprite by simulating click on the 'create sprite' button on form1
            var args = new CreateGameItemEventArgs(null);
            Global.form1.createSpriteBtn_Click(this, args);
            obj.sprite = args.GameItem as GameSprite;

            try
            {
                spriteBox.Resource = obj.sprite;
            }
            catch { }
        }

        private void parentBox_SelectionChanged(object sender, GameObject e)
        {
            if (e != obj)
                obj.parent = e;
            else
            {
                parentBox.Resource = obj.parent;
                MessageBox.Show("This will create a loop in parents.");
            }
        }

        private void spriteBox_SelectionChanged(object sender, GameSprite e)
        {
            obj.sprite = spriteBox.Resource;
        }

        private void addScriptBtn_Click(object sender, EventArgs e)
        {
            if (eventsListView.SelectedItem is not ObjectEvent ev)
            {
                MessageBox.Show("Select an event to add script to.");
                return;
            }

            Wrapper<string> script = "";
            ev.Scripts.Add(script);
            scriptsListView.Items.Add(new EventScript(ev, script));
        }

        private void eventsListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadScriptsListView();
        }

        private void LoadScriptsListView()
        {
            scriptsListView.Items.Clear(); // clear scripts view

            // if no event is selected, we're done here
            if (eventsListView.SelectedItem is not ObjectEvent ev)
                return;

            // load the scripts of the event to the scripts view
            int i = 0;
            foreach (var script in ev.Scripts)
            {
                scriptsListView.Items.Add(new EventScript(ev, script));
            }
        }

        private void scriptsListView_DoubleClick(object sender, EventArgs e)
        {
            ObjectEvent ev = eventsListView.SelectedItem as ObjectEvent ?? throw new Exception("No event is selected.");
            int scriptIndex = scriptsListView.SelectedIndex;

            if (scriptsListView.SelectedItem is not EventScript script)
                return;

            ScriptEditor editor = new(script) { Owner = this };

            editor.OKClicked += (s, e) =>
            {
                script.Script = e; // updates in the event itself
                scriptsListView.Refresh(); // to update script desc
            };

            editor.ShowDialog();
        }

        private void deleteScriptBtn_Click(object sender, EventArgs e)
        {
            if (scriptsListView.SelectedItem is not EventScript script)
                return;

            script.Event.Scripts.Remove(script.Pointer);
            scriptsListView.Items.Remove(script);
        }

        private void scriptsListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            scriptsListView.Refresh();
        }

        private void scriptsListView_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            Font font = e.Font!;

            // draw rectangle
            //if (scriptsListView.SelectedIndex == e.Index)
            //    e.Graphics.FillRectangle(Brushes.RoyalBlue, e.Bounds.X, e.Bounds.Y, e.Bounds.Width, font.Height);
            e.DrawBackground();

            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;

            // draw row index
            e.Graphics.DrawString((e.Index + 1).ToString(), font, Brushes.Red, new PointF(0, e.Bounds.Y));

            // get the item
            object? item = scriptsListView.Items[e.Index];

            // draw the item text
            e.Graphics.DrawString(item?.ToString(), font, selected ? Brushes.White : Brushes.Black, new PointF(30, e.Bounds.Y));
        }

        private void eventsListView_DrawItem(object sender, DrawItemEventArgs e)
        {
            // Draw the background highlights (selection color)
            e.DrawBackground();

            // Get the current item object
            var item = (ObjectEvent)eventsListView.Items[e.Index];

            // Get the icon
            Image? icon = null;
            if (item is CollisionEvent colEv)
                icon = Environment.project.GetItem<GameObject>(colEv.Param)?.CollisionIcon;
            icon ??= GetIcon(item.Type);

            // Calculate vertical alignment positions
            int iconX = e.Bounds.Left + 4;
            int iconY = e.Bounds.Top + (e.Bounds.Height - (icon?.Height ?? 0)) / 2;

            // 1. Draw the icons
            if (icon != null)
                e.Graphics.DrawImage(icon, iconX, iconY, icon.Width, icon.Height);

            // 2. Calculate text boundaries (shifting right to prevent overlapping the icon)
            int textX = iconX + (icon?.Width ?? 24) + 6;
            Rectangle textRect = new(textX, e.Bounds.Top, e.Bounds.Width - textX, e.Bounds.Height);

            // 3. Draw the item text natively
            TextRenderer.DrawText(e.Graphics, item?.ToString() ?? "null", e.Font, textRect, e.ForeColor,
                                  TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

            // Draw the focus boundary lines if active
            e.DrawFocusRectangle();
        }

        internal static Image? GetIcon(ObjectEvent.EventType ev)
        {
            return Resources.ResourceManager.GetObject("evicon24_" + ev) as Image;
        }

        private void eventsListView_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            // Retrieve the string text of the current item
            string text = eventsListView.Items[e.Index]?.ToString() ?? "null";

            // Measure the exact text size using the control's target font
            // Adding padding (e.g., + 4) prevents descenders (g, j, p, q, y) from clipping
            Size textSize = TextRenderer.MeasureText(text, eventsListView.Font);

            e.ItemHeight = textSize.Height + 4;
        }

        private void moveScriptUpBtn_Click(object sender, EventArgs e)
        {
            if (scriptsListView.SelectedIndex < 1 || scriptsListView.SelectedItem is not EventScript script) return;

            var upperPointer = (EventScript)scriptsListView.Items[scriptsListView.SelectedIndex - 1];
            string upperScript = upperPointer.Script;
            upperPointer.Script = script.Script;
            script.Script = upperScript;

            scriptsListView.SelectedIndex--;
            scriptsListView.Invalidate();
        }

        private void moveScriptDownBtn_Click(object sender, EventArgs e)
        {
            if (scriptsListView.SelectedIndex < 0 ||
                scriptsListView.SelectedIndex >= scriptsListView.Items.Count - 1 ||
                scriptsListView.SelectedItem is not EventScript script)
                return;

            var downerPointer = (EventScript)scriptsListView.Items[scriptsListView.SelectedIndex + 1];
            string downerScript = downerPointer.Script;
            downerPointer.Script = script.Script;
            script.Script = downerScript;

            scriptsListView.SelectedIndex++;
            scriptsListView.Invalidate();
        }
    }
}