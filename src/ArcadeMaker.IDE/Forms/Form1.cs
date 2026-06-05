using ArcadeMaker.IDE.Items;
using ArcadeMaker.IDE.Properties;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ArcadeMaker.IDE
{
    public partial class Form1 : Form
    {
        public ImageList treeImages = new ImageList();
        private const string folderKey = "folder";

        private void DefaultInit()
        {
            Type[] types = new Type[] { typeof(GameSprite), typeof(GameSound), typeof(GameBackground), typeof(GamePath), typeof(GameScript), typeof(GameFont), typeof(GameObject), typeof(GameRoom) };

            foreach (TreeNode node in projectTree.Nodes)
            {
                if (node.Index >= types.Length)
                {
                    // clear node icon by setting its ImageIndex property to a value higher that the number
                    // of images in the ImageList
                    node.ImageIndex = Environment.project.items.Count + treeImages.Images.Count + 2;
                    node.SelectedImageIndex = node.ImageIndex;
                    continue;
                }

                SetNodeAsFolder(node, types[node.Index], types[node.Index].GetProperty("icon", BindingFlags.Static | BindingFlags.Public)?.GetValue(null) as Bitmap);
            }
            foreach (var item in Environment.project.items)
            {
                InsertItemToTree(item);
            }
        }

        private void LoadFolder<T>(ProjectFolderTreeStruct<T> @struct, TreeNode node, Bitmap typeIcon) where T : GameItem
        {
            foreach (var str in @struct.Structs)
            {
                if (str is ProjectItemTreeStruct<T> item)
                {
                    InsertItemToTree(item.Item, new CreateGameItemEventArgs(item.Item, node));
                }
                else
                {
                    if (str is ProjectFolderTreeStruct<T> folder)
                    {
                        TreeNode folderNode = new TreeNode(folder.Name);
                        folderNode.Name = folderKey;
                        SetNodeAsFolder(folderNode, typeof(T), typeIcon);
                        LoadFolder(folder, folderNode, typeIcon);
                        node.Nodes.Add(folderNode);
                    }
                }
            }
        }

        private void GlobalInit()
        {
            InitializeComponent();
            Global.form1 = this;
            SetFormTitle();

            // load project tree
            projectTree.ImageList = treeImages;
            treeImages.Images.Add(folderKey, Properties.Resources.folder);
        }

        private void SetFormTitle(GameProject forProject = null)
        {
            if (forProject == null)
                forProject = Environment.project;
            Text = forProject.name + " - " + Global.ProgramName;
        }

        public Form1()
        {
            GlobalInit();
            DefaultInit();
        }

        public Form1(object[] projectTree)
        {
            GlobalInit();

            if (projectTree == null)
            {
                DefaultInit();
                return;
            }

            Type[] types = new Type[] { typeof(GameSprite), typeof(GameSound), typeof(GameBackground), typeof(GamePath), typeof(GameScript), typeof(GameFont), typeof(GameObject), typeof(GameRoom) };
            for (int nodeIndex = 0; nodeIndex < this.projectTree.Nodes.Count; nodeIndex++)
            {
                var node = this.projectTree.Nodes[nodeIndex];
                if (nodeIndex >= types.Length)
                {
                    // clear node icon by setting its ImageIndex property to a value higher that the number
                    // of images in the ImageList
                    node.ImageIndex = Environment.project.items.Count + treeImages.Images.Count + 2;
                    node.SelectedImageIndex = node.ImageIndex;
                    continue;
                }
                Bitmap icon = types[node.Index].GetProperty("icon", BindingFlags.Static | BindingFlags.Public)?.GetValue(null) as Bitmap;
                SetNodeAsFolder(node, types[node.Index], icon);

                foreach (object str in projectTree)
                {
                    if (str != null && str.GetType().GetGenericTypeDefinition() == typeof(ProjectFolderTreeStruct<>) && str.GetType().GetGenericArguments()[0] == types[node.Index])
                    {
                        if (str is ProjectFolderTreeStruct<GameSprite> strFolder)
                        {
                            LoadFolder(strFolder, node, icon);
                        }
                        else if (str is ProjectFolderTreeStruct<GameSound> strFolder1)
                        {
                            LoadFolder(strFolder1, node, icon);
                        }
                        else if (str is ProjectFolderTreeStruct<GameBackground> strFolder2)
                        {
                            LoadFolder(strFolder2, node, icon);
                        }
                        else if (str is ProjectFolderTreeStruct<GamePath> strFolder3)
                        {
                            LoadFolder(strFolder3, node, icon);
                        }
                        else if (str is ProjectFolderTreeStruct<GameScript> strFolder4)
                        {
                            LoadFolder(strFolder4, node, icon);
                        }
                        else if (str is ProjectFolderTreeStruct<GameFont> strFolder5)
                        {
                            LoadFolder(strFolder5, node, icon);
                        }
                        else if (str is ProjectFolderTreeStruct<GameObject> strFolder6)
                        {
                            LoadFolder(strFolder6, node, icon);
                        }
                        else if (str is ProjectFolderTreeStruct<GameRoom> strFolder7)
                        {
                            LoadFolder(strFolder7, node, icon);
                        }
                    }
                }
            }
        }

        private void SetNodeAsFolder(TreeNode node, Type type, Bitmap typeIcon)
        {
            node.ImageKey = folderKey;
            var createFolderBtn = new ToolStripMenuItem("Create Folder");
            createFolderBtn.Image = Resources.folder;

            string typeName = null;
            Action<object, EventArgs> createAction = null;
            {
                if (type == typeof(GameSprite))
                {
                    typeName = "Sprite";
                    createAction = createSpriteBtn_Click;
                }
                else if (type == typeof(GameBackground))
                {
                    typeName = "Background";
                    createAction = createBackgroundBtn_Click;
                }
                else if (type == typeof(GameSound))
                {
                    typeName = "Sound";
                    createAction = createMusicBtn_Click;
                }
                else if (type == typeof(GamePath))
                {
                    typeName = "Path";
                    createAction = createPathBtn_Click;
                }
                else if (type == typeof(GameScript))
                {
                    typeName = "Script";
                    createAction = createScriptBtn_Click;
                }
                else if (type == typeof(GameFont))
                {
                    typeName = "Font";
                    createAction = createFontBtn_Click;
                }
                else if (type == typeof(GameObject))
                {
                    typeName = "Object";
                    createAction = createObjectBtn_Click;
                }
                else if (type == typeof(GameRoom))
                {
                    typeName = "Room";
                    createAction = createRoomBtn_Click;
                }
            }
            var createItemBtn = new ToolStripMenuItem("Create " + typeName);
            if (typeIcon != null)
                createItemBtn.Image = typeIcon;

            createItemBtn.Click += (s, e) =>
            {
                createAction(this, new CreateGameItemEventArgs(folder: node));
            };

            createFolderBtn.Click += (s, e) =>
            {
                string text = Microsoft.VisualBasic.Interaction.InputBox("Folder Name:");
                if (string.IsNullOrWhiteSpace(text))
                {
                    text = null;
                }
                if (text != null)
                {
                    TreeNode newNode = new TreeNode(text);
                    newNode.Name = folderKey;
                    SetNodeAsFolder(newNode, type, typeIcon);
                    node.Nodes.Add(newNode);
                    node.Expand();
                }
            };

            ToolStripMenuItem renameFolderBtn = new ToolStripMenuItem("Rename") { Enabled = node.Name == folderKey };
            renameFolderBtn.Click += (s, e) =>
            {
                renameSentByButton = true;
                node.BeginEdit();
            };

            ToolStripMenuItem deleteFolderBtn = new ToolStripMenuItem("Delete") { Enabled = node.Name == folderKey };
            deleteFolderBtn.Click += (s, e) =>
            {
                string confirmText = $"You are about to delete folder \"{node.Text}\" with all items it contains. This will be permanent. Continue?";
                var result = MessageBox.Show(confirmText, "Confirm", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    Action<TreeNode> RemoveFolderItems = null;
                    RemoveFolderItems = (folderNode) =>
                    {
                        foreach (TreeNode child in folderNode.Nodes)
                        {
                            if (child == null)
                                return;
                            if (child.Tag is GameItem item)
                            {
                                Environment.project.items.Remove(item);
                            }
                            else
                            {
                                RemoveFolderItems(child);
                            }
                            child.Remove();
                        }
                    };
                    RemoveFolderItems(node);
                    node.Remove();
                }
            };

            node.ContextMenuStrip = new ContextMenuStrip();
            node.ContextMenuStrip.Items.Add(createItemBtn);
            node.ContextMenuStrip.Items.Add(createFolderBtn);
            node.ContextMenuStrip.Items.Add(renameFolderBtn);
            node.ContextMenuStrip.Items.Add(deleteFolderBtn);
        }

        public void createSpriteBtn_Click(object sender, EventArgs e)
        {
            GameSprite sprite = new GameSprite("s");
            InsertItemToTree(sprite, e);
            CreateItem(sprite, "Sprite");

            if (e is CreateGameItemEventArgs)
            {
                (e as CreateGameItemEventArgs).GameItem = sprite;
            }
        }

        private void createBackgroundBtn_Click(object sender, EventArgs e)
        {
            GameBackground background = new GameBackground("b");
            InsertItemToTree(background, e);
            CreateItem(background, "Background");
        }

        private void createPathBtn_Click(object sender, EventArgs e)
        {
            GamePath path = new GamePath("p");
            InsertItemToTree(path, e);
            CreateItem(path, "Path");
        }

        public void RefreshTreeView()
        {
            projectTree.Invalidate();
        }

        private void createMusicBtn_Click(object sender, EventArgs e)
        {
            GameSound sound = new GameSound("m");
            InsertItemToTree(sound, e);
            CreateItem(sound, "Sound");
        }
        private void createScriptBtn_Click(object sender, EventArgs e)
        {
            GameScript script = new GameScript(GenerateItemName("Script"));
            script.InitDefaultCode();
            InsertItemToTree(script, e);
            CreateItem(script, null, show: false);
            
            script.editor.MdiParent = this;
            script.editor.Show();
        }

        private void createFontBtn_Click(object sender, EventArgs e)
        {
            GameFont font = new GameFont("f");
            InsertItemToTree(font, e);
            CreateItem(font, "Font");
        }

        private void createObjectBtn_Click(object sender, EventArgs e)
        {
            GameObject obj = new GameObject("o");
            InsertItemToTree(obj, e);
            CreateItem(obj, "Object");
        }
        private void createRoomBtn_Click(object sender, EventArgs e)
        {
            GameRoom room = new GameRoom("r");
            InsertItemToTree(room, e);
            CreateItem(room, "Room");
        }

        private string GenerateItemName(string baseName)
        {
            int index = 0;

            restart:
            foreach (GameItem pitem in Environment.project.items)
            {
                if (pitem.name == baseName + index)
                {
                    index++;
                    goto restart;
                }
            }

            return baseName + index;
        }

        private string CreateItem(GameItem item, string name, bool show = true)
        {
            if (name != null)
                item.name = GenerateItemName(name);
            Environment.project.items.Add(item);
            if (show)
            {
                item.editor.MdiParent = this;
                item.editor.Show();
            }

            return item.name;
        }

        private TreeNode InsertItemToTree(GameItem item, EventArgs e = null)
        {
            Bitmap icon = null;
            int tree = 0;
            if (item is GameSprite)
            {
                tree = 0;
                icon = (item as GameSprite).image;
            }
            else if (item is GameSound)
            {
                tree = 1;
                icon = GameSound.icon;
            }
            else if (item is GameBackground)
            {
                tree = 2;
                icon = (item as GameBackground).image;
            }
            else if (item is GamePath)
            {
                tree = 3;
                icon = GamePath.icon;
            }
            else if (item is GameScript)
            {
                tree = 4;
                icon = GameScript.icon;
            }
            else if (item is GameFont)
            {
                tree = 5;
                icon = GameFont.icon;
            }
            else if (item is GameObject)
            {
                tree = 6;
                GameObject obj = item as GameObject;
                if (obj.sprite != null)
                    icon = obj.sprite.image;
            }
            else if (item is GameRoom)
            {
                tree = 7;
                icon = GameRoom.icon;
            }

            if (icon == null)
                icon = new Bitmap(1, 1);

            TreeNode node = null;
            if (e != null && e is CreateGameItemEventArgs)
            {
                node = (e as CreateGameItemEventArgs).Folder;
            }
            if (node == null)
                node = projectTree.Nodes[tree];

            TreeNode itemNode = new TreeNode(item.name);
            itemNode.Tag = item;
            var amNode = AssemblyManagerNode;
            treeImages.Images.Add(icon);
            if (amNode != null)
            {
                amNode.ImageIndex = Environment.project.items.Count + treeImages.Images.Count + 3;
                amNode.SelectedImageIndex = amNode.ImageIndex;
            }
            itemNode.ImageIndex = treeImages.Images.Count - 1;
            itemNode.SelectedImageIndex = itemNode.ImageIndex;
            itemNode.ContextMenuStrip = new ContextMenuStrip();
            ToolStripMenuItem renameBtn = new ToolStripMenuItem("Rename");
            renameBtn.Click += (s, ea) =>
            {
                renameSentByButton = true;
                itemNode.BeginEdit();
            };
            itemNode.ContextMenuStrip.Items.Add(renameBtn);
            item.treeImageIndex = itemNode.ImageIndex;
            item.treeNode = itemNode;
            item.NameChanged += (s, ea) =>
            {
                itemNode.Text = ea.newName;
            };
            ToolStripMenuItem deleteBtn = new ToolStripMenuItem("Delete");
            deleteBtn.Click += (s, ea) =>
            {
                string validateMsg = $"You are about to delete {item.name}. This will be permanent. Continue?";
                if (MessageBox.Show(validateMsg, "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    Environment.project.items.Remove(item);
                    try
                    {
                        projectTree.Nodes.Remove(itemNode);
                        if (item.editor != null && !item.editor.IsDisposed)
                            item.editor.Close();
                    }
                    catch { }
                }
            };
            itemNode.ContextMenuStrip.Items.Add(deleteBtn);
            node.Nodes.Add(itemNode);
            node.Expand();

            return itemNode;
        }

        private void projectTree_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Tag is GameItem)
            {
                GameItem item = e.Node.Tag as GameItem;
                if (item != null)
                {
                    item.editor.MdiParent = this;
                    item.editor.Show();
                }
            }
            else if (e.Node == AssemblyManagerNode)
            {
                OpenAssemblyManager();
            }
        }

        private TreeNode AssemblyManagerNode
        {
            get
            {
                foreach (TreeNode node in projectTree.Nodes)
                {
                    if (node.Name == "AssemblyManagerRoot.node")
                        return node;
                }
                return null;
            }
        }

        public void Form1_Load(object sender, EventArgs e)
        {
#if DEBUG
            bool show = false;
            string debugText =
                "public class Program {\n" +
                "   /* A C# program to print a string in the console\n" +
                "      and close the console */\n" +
                "   public static int Main(object[] args)\n   {\n" +
                "      Console.WriteLine(\"Closing program...\");\n" +
                "      this.Close(); // close the program https://www.google.co.il naviagte http://www.sport5.co.il\n      " +
                "\n      return 0;\n   }\n" +
                "}";
            
            if (show)
            {
                Form debug = new Form { Size = new Size(500, 500) };
                SpansTextBox2 box = new SpansTextBox2 { /*Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom*/ };
                box.Spans.AddRange(Global.GetScriptBoxSpans(debugText, splitMultiCommentsLines: true));
                box.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
                debug.Controls.Add(box);
                debug.ShowDialog();
            }
#endif
            LoadRecentProjectsMenu();
        }

        private void LoadRecentProjectsMenu()
        {
            Settings.Default.RecentProjects ??= [];
            recentProjectsMenu.DropDownItems.Clear();
            if (Settings.Default.RecentProjects != null)
            {
                foreach (string project in Properties.Settings.Default.RecentProjects)
                {
                    ToolStripMenuItem openBtn = new ToolStripMenuItem(project.FileName());
                    openBtn.Click += (s, ea) =>
                    {
                        string fileNotFoundError = "Could not find the project at " + project;
                        try
                        {
                            Environment.project = new GameProject("escape_equal_names");
                            GameProject gameProject = GameProject.Open(project);
                            Global.PushRecentProject(project);
                            OpenProject(gameProject);
                        }
                        catch (FileNotFoundException)
                        {
                            MessageBox.Show(fileNotFoundError);
                        }
                        catch (DirectoryNotFoundException)
                        {
                            MessageBox.Show(fileNotFoundError);
                        }
                        catch
                        {
                            MessageBox.Show("Error: Could not open the selected project");
                        }
                    };
                    recentProjectsMenu.DropDownItems.Add(openBtn);
                }
            }
            recentProjectsMenu.DropDownItems.Add(new ToolStripSeparator());
            recentProjectsMenu.DropDownItems.Add(clearRecentProjectsBtn);
        }

        private void clearRecentProjectsBtn_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.RecentProjects.Clear();
            Properties.Settings.Default.Save();

            recentProjectsMenu.DropDownItems.Clear();
            recentProjectsMenu.DropDownItems.Add(new ToolStripSeparator());
            recentProjectsMenu.DropDownItems.Add(clearRecentProjectsBtn);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Global.allowClosing = true;
            if (e.Cancel)
                e.Cancel = false;
        }

        private async void saveExeBtn_Click(object sender, EventArgs e)
        {
            ProgressForm frm = new ProgressForm();
            frm.Show();
            await Task.Run(() => { Environment.GenerateExe(run: true); frm.Close(); });
        }

        private async void saveGameBtn_Click(object sender, EventArgs e)
        {
            MessageBox.Show("This option is not available yet.");
            return;

            // old code
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Executeable File|*.exe";
                if (Environment.project.projectFilePath != null)
                    saveFileDialog.FileName = Environment.project.name + ".exe";
                else
                    saveFileDialog.FileName = "Game " + DateTime.Now.ToString("dd-MM-yy") + ".exe";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    await Task.Run(() =>
                    {
                        Environment.GenerateExe(savePath: saveFileDialog.FileName, run: false, console: false);
                        MessageBox.Show("Game saved.", ".exe File Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    });
                }
            }
        }

        private void saveProjectBtn_Click(object sender, EventArgs e)
        {
            SaveProject();
        }

        private static void SaveProject(bool saveAs = false)
        {
            string rootFolder; // the folder to which we want to save the full project folder, e.g. Desktop
            string projectName = Environment.project.name;

            saveAs = saveAs || Environment.project.projectFilePath == null;
            if (saveAs)
            {
                using SaveFileDialog dialog = new();
                dialog.Filter = $"{Global.ProgramName} Project|*.gsp";
                if (Environment.project.projectFilePath != null)
                    dialog.FileName = Environment.project.name;
                else
                {
                    string name = DateTime.Now.ToString("dd-MM-yy");
                    dialog.FileName = name;
                }
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string loc = dialog.FileName;
                    if (!string.IsNullOrWhiteSpace(loc))
                    {
                        Environment.project.name = loc.FileNameWithoutExtension();
                        rootFolder = loc.FileLocation();
                    }
                    else
                    {
                        var res = MessageBox.Show("Something went wrong. Would you like to insert path directly?", "Error", MessageBoxButtons.YesNo);
                        if (res == DialogResult.Yes)
                        {
                            loc = Microsoft.VisualBasic.Interaction.InputBox("Location to save the project to:");
                            Environment.project.name = loc.FileNameWithoutExtension();
                            rootFolder = loc.FileLocation();
                        }
                        else return;
                    }
                }
                else return;
            }
            else
                rootFolder = Environment.project.projectFilePath!.FileLocation().FileLocation(); // when dialog appears, the selected location is used to save the root folder containing the .gsp file.
                                                                                                 // so given the path of the .gsp, first FileLocation() would return the root folder,
                                                                                                 // and second would return the folder that was selected in
                                                                                                 // the dialog when the project was first saved.
            
            if (Environment.project.projectFilePath != null && string.IsNullOrWhiteSpace(Environment.project.name))
                Environment.project.name = Environment.project.projectFilePath.FileNameWithoutExtension();
            Environment.project.Save(rootFolder);
        }

        // make the toolstrip enabled with 1 click when form is not focused
        protected override void WndProc(ref Message m)
        {
            int WM_PARENTNOTIFY = 0x0210;
            if (!this.Focused && m.Msg == WM_PARENTNOTIFY)
            {
                // Make this form auto-grab the focus when menu/controls are clicked
                this.Activate();
            }
            base.WndProc(ref m);
        }

        private void openProjectBtn_Click(object sender, EventArgs e)
        {
            Environment.project = new GameProject("escape_equal_names");

            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "GameStudio Projects|*.gsp";
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                OpenProject(GameProject.Open(fileDialog.FileName, out object[] pTree), pTree);
                Global.PushRecentProject(fileDialog.FileName);
            }
        }

        private void OpenProject(GameProject project, object[] pTree = null)
        {
            Environment.project = project;

            // reset form
            Form1 newForm;
            if (pTree != null)
                newForm = new Form1(pTree);
            else
                newForm = new Form1();
            newForm.FormClosed += (s, ea) =>
            {
                Application.Exit(); // this.Close() will not work if no editor has been opened
            };
            newForm.Show();

            foreach (Control control in Controls)
            {
                Controls.Remove(control);
                control.Dispose();
            }
            Dispose(false);
            Hide();
        }

        private void findResBtn_Click(object sender, EventArgs e)
        {
            string name = Microsoft.VisualBasic.Interaction.InputBox("Resource Name");
            if (!string.IsNullOrWhiteSpace(name))
            {
                foreach (GameItem item in Environment.project.items)
                {
                    if (item.name == name)
                    {
                        try
                        {
                            item.editor.MdiParent = this;
                            item.editor.Show();
                            return;
                        }
                        catch { }
                    }
                }
                MessageBox.Show("Cannot find resource " + name);
            }
        }

        private void newProjectBtn_Click(object sender, EventArgs e)
        {
            if ((!Environment.project.saved) && Environment.project.items.Any())
            {
                var result = MessageBox.Show("Do you want to save project first?", "Save Project", MessageBoxButtons.YesNoCancel);
                if (result == DialogResult.Yes)
                {
                    saveProjectBtn_Click(saveProjectBtn, new EventArgs());
                }
                else if (result == DialogResult.Cancel)
                {
                    return;
                }
            }

            OpenProject(new GameProject($"{Global.ProgramName} Project"));
        }

        private void projectTree_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (e.Node.Tag is GameItem && e.Label != null)
            {
                if (!e.Label.IsPossibleName((e.Node.Tag as GameItem).name))
                {
                    e.CancelEdit = true;
                    MessageBox.Show("This is an illegal name");
                }
                else
                {
                    (e.Node.Tag as GameItem).name = e.Label;
                }
            }
            else if (string.IsNullOrWhiteSpace(e.Label))
            {
                e.CancelEdit = true;
            }
        }

        private bool renameSentByButton = false;
        private void projectTree_BeforeLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (e.Node.Parent == null || !renameSentByButton)
                e.CancelEdit = true;
            else
                renameSentByButton = false;
        }

        public ProjectFolderTreeStruct<T> GetProjectStruct<T>(Type? argType = null) where T : GameItem
        {
            if (argType == null)
                argType = typeof(T);
            ProjectFolderTreeStruct<T> mainStr = null;
            int tree = -1;
            {
                if (argType == typeof(GameSprite))
                {
                    tree = 0;
                }
                else if (argType == typeof(GameSound))
                {
                    tree = 1;
                }
                else if (argType == typeof(GameBackground))
                {
                    tree = 2;
                }
                else if (argType == typeof(GamePath))
                {
                    tree = 3;
                }
                else if (argType == typeof(GameScript))
                {
                    tree = 4;
                }
                else if (argType == typeof(GameFont))
                {
                    tree = 5;
                }
                else if (argType == typeof(GameObject))
                {
                    tree = 6;
                }
                else if (argType == typeof(GameRoom))
                {
                    tree = 7;
                }
                if (tree <= -1)
                {
                    throw new NotImplementedException();
                }
            }
            TreeNode node = projectTree.Nodes[tree];
            mainStr = new ProjectFolderTreeStruct<T>(node.Text, true);
            TranslateFolder<T>(mainStr, node);
            return mainStr;
        }

        private void TranslateFolder<T>(ProjectFolderTreeStruct<T> str, TreeNode node) where T : GameItem
        {
            foreach (TreeNode subnode in node.Nodes)
            {
                if (subnode.Tag is GameItem item)
                {
                    str.Structs.Add(new ProjectItemTreeStruct<T>(item));
                }
                else
                {
                    ProjectFolderTreeStruct<T> folder = new ProjectFolderTreeStruct<T>(subnode.Text, false);
                    str.Structs.Add(folder);
                    TranslateFolder<T>(folder, subnode);
                }
            }
        }

        private void projectTree_DragDrop(object sender, DragEventArgs e)
        {
            // retrieve the client coordinates of the drop location
            Point targetPoint = projectTree.PointToClient(new Point(e.X, e.Y));

            // retrieve the node at the drop location
            TreeNode targetNode = projectTree.GetNodeAt(targetPoint);

            // retrieve the node that was dragged
            TreeNode draggedNode = (TreeNode)e.Data.GetData(typeof(TreeNode));

            if (targetNode == null || draggedNode == null || !CanDragTo(draggedNode, targetNode))
                return;

            // confirm that the node at the drop location is not 
            // the dragged node and that the base folder of both nodes is the same
            // and that the target node is a folder
            if (!draggedNode.Equals(targetNode))
            {
                int targetIndex = 0;

                if (targetNode.Tag is GameItem)
                {
                    targetIndex = targetNode.Index;
                }

                // remove the node from its current location and add it to the node at the drop location
                draggedNode.Remove();
                (targetNode.Tag is GameItem ? targetNode.Parent.Nodes : targetNode.Nodes).Insert(targetIndex, draggedNode);

                // expand the node at the location to show the dropped node
                targetNode.Expand();
            }
        }

        private void projectTree_ItemDrag(object sender, ItemDragEventArgs e)
        {
            DoDragDrop(e.Item, DragDropEffects.Move);
        }

        private void projectTree_DragEnter(object sender, DragEventArgs e)
        {
            projectTree_DragOver(sender, e);
        }

        private bool CanDragTo(TreeNode draggedNode, TreeNode targetNode)
        {
            if (draggedNode == null || targetNode == null || draggedNode == targetNode)
                return false;

            // find base folders of both dragged node & target node
            TreeNode targetBaseFolder = targetNode;
            while (targetBaseFolder.Parent != null)
            {
                targetBaseFolder = targetBaseFolder.Parent;
            }
            TreeNode draggedBaseFolder = draggedNode;
            while (draggedBaseFolder.Parent != null)
            {
                draggedBaseFolder = draggedBaseFolder.Parent;
            }

            return draggedBaseFolder == targetBaseFolder;
        }

        private void projectTree_DragOver(object sender, DragEventArgs e)
        {
            // retrieve the client coordinates of the drop location
            Point targetPoint = projectTree.PointToClient(new Point(e.X, e.Y));

            // retrieve the node at the drop location
            TreeNode targetNode = projectTree.GetNodeAt(targetPoint);

            e.Effect = CanDragTo((TreeNode)e.Data.GetData(typeof(TreeNode)), targetNode) ? DragDropEffects.Move : DragDropEffects.None;
        }

        private void saveProjectAsBtn_Click(object sender, EventArgs e)
        {
            SaveProject(saveAs: true);
        }

        private void assemblyManagerBtn_Click(object sender, EventArgs e)
        {
            OpenAssemblyManager();
        }

        private void OpenAssemblyManager()
        {
            (new AssemblyManagerForm()).ShowDialog();
        }
    }

    public class CreateGameItemEventArgs : EventArgs
    {
        public GameItem GameItem = null;
        public TreeNode Folder = null;
        public CreateGameItemEventArgs(GameItem item = null, TreeNode folder = null)
        {
            GameItem = item;
            this.Folder = folder;
        }
    }
}
