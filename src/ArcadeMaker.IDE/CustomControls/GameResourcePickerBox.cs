using ArcadeMaker.IDE.Items;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ArcadeMaker.IDE
{
    public partial class GameResourcePickerBox<T> : UserControl where T : GameItem
    {
        public readonly ContextMenuStrip Menu = new ContextMenuStrip();
        private T resource = null;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public T Resource
        {
            get 
            {
                return resource;
            }
            set
            {
                resource = value;
                if (resource != null)
                    nameBox.Text = resource.name;
                else
                    nameBox.Text = defaultItemTitle;
            }
        }
        public event EventHandler<T> SelectionChanged;
        private string defaultItemTitle;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string DefaultItemTitle
        {
            get
            {
                return defaultItemTitle;
            }
            set
            {
                defaultItemTitle = value;
                if (noResItem != null)
                    noResItem.Text = value;
                if (resource == null)
                    nameBox.Text = value;
            }
        }

        public GameResourcePickerBox(string defaultItemTitle = "<None>", T defaultRes = null)
        {
            InitializeComponent();
            nameBox.GotFocus += (s, e) => ExternMethods.HideCaret(nameBox.Handle);
            this.defaultItemTitle = defaultItemTitle;
            nameBox.Text = defaultItemTitle;
            Resource = defaultRes;
        }

        private ToolStripMenuItem noResItem = null;

        private void LoadMenu()
        {
            try
            {
                Menu.Items.Clear();

                LoadFolderMenu(Global.form1.GetProjectStruct<T>(), null);

                noResItem = new ToolStripMenuItem(defaultItemTitle);
                noResItem.Click += (s, e) =>
                {
                    SelectResource(null);
                };
                if (Menu.Items.Count >= 1)
                    Menu.Items.Add(new ToolStripSeparator());
                Menu.Items.Add(noResItem);
            }
            catch (Exception ex)
            {

            }
        }

        private void LoadFolderMenu(ProjectFolderTreeStruct<T> folder, ToolStripMenuItem menuItem)
        {
            foreach (ProjectTreeStruct<T> str in folder.Structs)
            {
                if (str is ProjectItemTreeStruct<T> resStr)
                {
                    T res = resStr.Item as T;

                    ToolStripMenuItem item = new ToolStripMenuItem(res.name);
                    res.NameChanged += (s, e) => item.Text = res.name;

                    if (typeof(T) == typeof(GameSprite) && (res as GameSprite).image != null)
                        item.Image = (res as GameSprite).image;
                    else if (typeof(T) == typeof(GameObject) && (res as GameObject).sprite?.image != null)
                        item.Image = (res as GameObject).sprite?.image;
                    if (typeof(T) == typeof(GameBackground) && (res as GameBackground).image != null)
                        item.Image = (res as GameBackground).image;

                    item.Click += (s, e) =>
                    {
                        SelectResource(res);
                    };

                    if (menuItem != null)
                        menuItem.DropDownItems.Add(item);
                    else
                        Menu.Items.Add(item);
                }
                else if (str is ProjectFolderTreeStruct<T> folderStr)
                {
                    ToolStripMenuItem newFolder = new ToolStripMenuItem(folderStr.Name);
                    newFolder.Image = Properties.Resources.folder;
                    LoadFolderMenu(folderStr, newFolder);

                    if (menuItem != null)
                        menuItem.DropDownItems.Add(newFolder);
                    else
                        Menu.Items.Add(newFolder);
                }
            }
        }

        internal void ShowMenu(Point position, Control? ctrl = null)
        {
            Menu.Show(ctrl ?? this, position);
        }

        private void nameBox_MouseClick(object sender, MouseEventArgs e)
        {
            ShowMenu(e.Location);
        }
        private void SelectResource(T res)
        {
            Resource = res;
            SelectionChanged?.Invoke(this, Resource);
            if (resource != null)
                nameBox.Text = resource.name;
            else
                nameBox.Text = defaultItemTitle;
        }

        private void menuBtn_Click(object sender, EventArgs e)
        {
            ShowMenu(toolStrip1.Location);
        }

        private void GameResourcePickerBox_Load(object sender, EventArgs e)
        {
            Menu.Items.Clear();
            LoadMenu();
            if (Environment.project != null)
            {
                Environment.project.items.CollectionChanged += (s, ea) =>
                {
                    if (Environment.project.items.Contains(Resource))
                        Resource = null;
                    LoadMenu();
                };
            }
        }
    }

    public class GameObjectPickerBox : GameResourcePickerBox<GameObject> { }
    public class GameSpritePickerBox : GameResourcePickerBox<GameSprite> { }
    public class GameBackgroundPickerBox : GameResourcePickerBox<GameBackground> { }
    static class ExternMethods
    {

        [DllImport("User32.dll")]
        internal static extern bool HideCaret(IntPtr hWnd);
    }
}
