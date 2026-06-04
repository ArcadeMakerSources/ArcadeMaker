using Exp;
using Exp.Spans;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;
using System.Xml.Serialization;

namespace ArcadeMaker.Core.Resources.Serializeables;

public interface IContainsScript
{
    string Script { get; set; }
}

public class SerializeableGameProject
{
    public string name;
    public SerializeableGameItem[] items;
    public AssemblyReference[] userAssemblies;
    public SerializeableGameProjectTreeNode[] treeMainNodes;
    public TextureAtlasMap textureAtlasMap;
}

public class SerializeableGameProjectTreeNode
{
    public string type;
    public string name;
    public SerializeableGameProjectTreeNode[] nodes;
    public bool isFolder;
}

public class SerializeableGameItem
{
    public string name;
}

public class SerializeableGameSprite : SerializeableGameItem
{
    public string[] pathes;
    public int numOfImages;
    public int originX, originY;
    public bool preciseMask, separateMask;
    public bool maskBounding_auto, maskBounding_fullImage, maskBounding_manual;
    public int maskTop, maskRight, maskBottom, maskLeft;
    public int maskAlphaTolerance;
}

public class SerializeableBackground : SerializeableGameItem
{
    public string path;
}

public class SerializeableGameSound : SerializeableGameItem
{
    public string path;
    public float volume, pan, patch;
    public Sound.Types type;
}

public class SerializeableGamePath : SerializeableGameItem
{
    public PathPoint[] points;
    public bool close;
}

public class SerializeableGameFont : SerializeableGameItem
{
    public GameFont font;
    public string ttf;
    public float heightInPixels;
    public SerializeableGameFont() { }
    public SerializeableGameFont(GameFont font)
    {
        this.font = font;
    }
}

public class SerializeableGameObject : SerializeableGameItem
{
    public string sprite;
    public bool solid;
    public int depth;
    public string parent;
    public EventScripts[] events;
    public ObjectProperty[] extraProperties;
}



public class EventScript : IContainsScript
{
    public string Script
    {
        get;
        set
        {
            field = value;

            // update description
            ScriptDocument.ReadDocSettings("", Spanner.GetTextSpans(value), out var _, out string? description, out var _, out var _, out var _);
            if (string.IsNullOrWhiteSpace(description))
                description = "<No Description>";
            this.Description = description;
        }
    }
    public string Description { get; set; }

    public EventScript(string script)
    {
        this.Script = script;
    }

    public EventScript() : this("") { } // for the serializer
    public override string ToString() => Description;
}

public class EventScripts(ObjectEvent ev, params IEnumerable<string> scripts)
{
    public List<EventScript> Scripts = [.. scripts.Map(s => new EventScript(s))];
    public ObjectEvent Event { get => ev; set => ev = value; }
    public EventScripts() : this(ObjectEvent.Create) { }
}

public class SerializeableGameRoom : SerializeableGameItem
{
    public int index = -1;
    public SerializeableColor backColor;
    public SerializeableRoomBackground[] backgrounds;
    public string scriptPath;
    public int width, height;
    public int speed = 30;
    public bool persistent;
    public string caption;
    public SerializeableRoomObject[] objects;
    public bool enableViews;
    public SerializeableRoomView[] views;
}

public class SerializeableGameScript : SerializeableGameItem
{
    public string path;
}

public class SerializeableRoomObject
{
    public string id = "Undefined";
    public int x, y;
    public string obj;
    public string creationCode = null;
}

public class SerializeableRoomView
{
    public bool visible = false;
    public int x = 0, y = 0, width = 640, height = 480, portX = 0, portY = 0, portWidth = 640, portHeight = 480;

    public string objFollow;
    public int followHBor = 32, followVBor = 32, followHSp = -1, followVSp = -1;
}

public class SerializeableColor
{
    public int A, R, G, B;
}

public class GameFont : ISetsID
{
    private static int idCounter = 0;
    public int ID { get; set; } = idCounter++;
    public string Name { get; set; }

    public string family;
    public float size = 12;
    public bool bold = false, italic = false;
    public string ttf;
    public float heightInPixels;
}

public enum ObjectEvent
{
    KeyDown,
    KeyUp,
    KeyPress,
    Create,
    Step,
    MouseDown,
    MousePress,
    MouseUp,
    //MouseMove,
    MouseWheel,
    Draw,
    Alarm
}

public class PathPoint
{
    public int x = 0, y = 0, speed = 100;
}

public class SerializeableRoomBackground
{
    private static int count = 0;
    private readonly int index = 0;

    public string background;
    public bool visible = false;
    public bool foreground = false;
    public bool tileHor = true, tileVer = true;
    public int x = 0, y = 0;
    public bool stretch = false;
    public int horSpd = 0, verSpd = 0;

    public SerializeableRoomBackground()
    {
        index = count++;
    }
}

public class ObjectProperty
{
    public string Name { get; set; }
    public VariableType Type { get; set; }
    public string InitValueCode { get; set; }
    public bool Constant { get; set; }
    public bool Private { get; set; }
    public bool Nullable { get; set; }
}

public enum VariableType
{
    Bool,
    Char,
    Number,
    Array,
    String,
    Any
}