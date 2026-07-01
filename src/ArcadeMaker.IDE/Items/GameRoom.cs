using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using ArcadeMaker.Core.Resources.Serializeables;

namespace ArcadeMaker.IDE.Items
{
    public class GameRoom : GameItem, ISetsIcon, IContainsScript
    {
        public static Bitmap Icon => Properties.Resources.map;

        public readonly int index;
        private static int count = 0;

        public RoomSize size = new RoomSize(640, 480);
        public int speed = 30;
        public bool persistent = false;
        public List<RoomObject> objects = new List<RoomObject>();

        public bool viewsEnabled = false;
        public static int minNumOfViews { get; private set; } = 5;
        public List<RoomView> views = new List<RoomView>();

        public string caption = "";
        public RoomBackground[] backgrounds = new RoomBackground[8];
        public bool drawBackColor = true;
        public Color backColor = Color.Silver;

        private const string defaultScript = "namespace Game\n{\n\tpublic partial class RoomName\n\t{\n\t\tprotected override void Create()\n\t\t{\n\t\t\t\n\t\t}\n\t}\n}";
        private string creationCode = null;
        public string Script
        {
            get
            {
                if (creationCode == null)
                    creationCode = defaultScript.Replace("RoomName", name);
                return creationCode;
            }
            set => creationCode = value;
        }

        public new RoomEditor editor
        {
            get
            {
                if (editorClosed)
                {
                    base.editor = new RoomEditor(this);
                }
                return base.Editor as RoomEditor;
            }
            set
            {
                base.editor = value;
            }
        }

        public GameRoom(string name, int index = -1) : base(name)
        {
            this.index = count++;
            if (index >= 0)
                this.index = index;
            getEditor += (s, e) =>
            {
                var activateGet = editor;
            };
            if (Environment.project != null)
                caption = Environment.project.name;
            else
                caption = name;
            editor = new RoomEditor(this);

            if (views.Count == 0)
            {
                for (int v = 0; v < minNumOfViews; v++)
                {
                    views.Add(new RoomView());
                }
            }

            for (int i = 0; i < backgrounds.Length; i++)
                backgrounds[i] = new RoomBackground();
        }
    }

    public class RoomSize
    {
        public int width, height;
        public RoomSize(int width, int height)
        {
            this.width = width;
            this.height = height;
        }

        public Size ToFormSize()
        {
            return new Size(width, height);
        }
    }

    public class RoomObject : IContainsScript
    {
        public readonly string id;
        public int x, y;
        public GameObject obj;

        public RoomObject(string id, int x, int y, GameObject obj)
        {
            this.id = id;
            this.x = x;
            this.y = y;
            this.obj = obj;

            string[] use = new string[] { "System", "System.Collections.Generic", "System.Linq", "System.Threading.Tasks", "ArcadeMaker", "ArcadeMaker.Models", "ArcadeMaker.Controls", "ArcadeMaker.GameItems", "ArcadeMaker.Drawing" };
            defaultCreationCode = "";
            foreach (string ns in use)
                defaultCreationCode += "using " + ns + ";\n";
            defaultCreationCode += $"\nnamespace Game\n{{\tpublic static partial class CreationCodes\n\t{{\n\t\tpublic static void {id}_Create({obj.name} instance)\n\t\t{{\n\t\t\t\n\t\t}}\n\t}}\n}}";
            //Script = defaultCreationCode;
        }

        public readonly string defaultCreationCode = null;
        public string Script { get; set; } = null;
        public bool CompiledSyntaxTree { get; set; } = false;

        public bool HasCustomCreationCode()
        {
            return defaultCreationCode != Script;
        }

        public string ScriptOrDefaultCreationCode => Script ?? defaultCreationCode;
    }

    public class RoomBackground
    {
        private static int count = 0;
        private readonly int index = 0;

        [System.Xml.Serialization.XmlIgnore]
        public GameBackground image = null;

        public string gameBackgroundName
        {
            get
            {
                if (image != null)
                    return image.name;
                return null;
            }
            set
            {
                GameBackground.Invite(this, value);
            }
        }

        public bool visible = false;
        public bool foreground = false;
        public bool tileHor = true, tileVer = true;
        public int x = 0, y = 0;
        public bool stretch = false;
        public int horSpd = 0, verSpd = 0;

        public RoomBackground()
        {
            index = count++;
        }

        public override string ToString()
        {
            return "background " + index;
        }
    }

    public class RoomView
    {
        private static int Count = 0;
        private readonly int Index = 0;

        public bool Visible = false;
        public int X = 0, Y = 0, Width = 640, Height = 480, PortX = 0, PortY = 0, PortWidth = 640, PortHeight = 480;

        public GameObject ObjFollow = null;

        public int FollowHBor = 32, FollowVBor = 32, FollowHSp = -1, FollowVSp = -1;

        public RoomView()
        {
            Index = Count++;
        }

        public override string ToString()
        {
            return "View" + Index;
        }
    }
}
