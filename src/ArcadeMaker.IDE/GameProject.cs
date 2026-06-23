using ArcadeMaker.Core.ExpSrc;
using ArcadeMaker.Core.Models;
//using ArcadeMaker.Core.Resources.Serializeables;
using ArcadeMaker.Core.Resources;
using ArcadeMaker.Core.Resources.Serializeables;
using ArcadeMaker.IDE.Editors.Object.ObjectProperties;
using ArcadeMaker.IDE.Items;
using Exp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using GameFont = ArcadeMaker.IDE.Items.GameFont;
using IDEObjectProperty = ArcadeMaker.IDE.Editors.Object.ObjectProperties.IDEObjectProperty;
using PathPoint = ArcadeMaker.IDE.Items.PathPoint;
using RoomBackground = ArcadeMaker.IDE.Items.RoomBackground;

namespace ArcadeMaker.IDE
{
    public class GameProject
    {
        public string name = "Project";
        public string projectFilePath = null;
        public ObservableCollection<GameItem> items = new ObservableCollection<GameItem>();
        public List<AssemblyReference> assemblyReferences = new List<AssemblyReference>();

        public bool saved { get; private set; } = false;

        public GameProject(string name)
        {
            this.name = name;
        }

        public T? GetItem<T>(string name) where T : GameItem => items.OfType<T>().FirstOrDefault(i => i.name == name);

        public void Save(string path, bool successMsg = true, string fileName = null)
        {
            fileName ??= name;
            path += @"\" + fileName;
            Directory.CreateDirectory(path);

            SerializeableGameProject sproject = new SerializeableGameProject { name = name };
            List<SerializeableGameItem> sitems = new List<SerializeableGameItem>();
            IEnumerable<GameSprite> sprites = items.OfType<GameSprite>();
            IEnumerable<GameBackground> backgrounds = items.OfType<GameBackground>();
            IEnumerable<GameSound> sounds = items.OfType<GameSound>();
            IEnumerable<GameScript> scripts = items.OfType<GameScript>();
            IEnumerable<GameFont> fonts = items.OfType<GameFont>();
            IEnumerable<GameObject> objects = items.OfType<GameObject>();
            IEnumerable<GameRoom> rooms = items.OfType<GameRoom>();
            IEnumerable<GamePath> gpathes = items.OfType<GamePath>();

            // save all resources
            string res = path + @"\res";
            Directory.CreateDirectory(res);

            // save texture atlases
            using var atlas = Textures.TextureAtlas.FromProjectSprites(this, out var atlasMap);
            string atlasesDir = res + @"\atlases";
            Directory.CreateDirectory(atlasesDir);
            string mainAtlasPath = atlasesDir + @"\main.png";
            atlas.Save(mainAtlasPath);
            sproject.textureAtlasMap = new() { AtlasFilePath = mainAtlasPath, Items = atlasMap.Map(rect => new TextureAtlasMap.Item { SpriteName = rect.Sprite.name, ImageIndex = rect.Index, X = (int)rect.Rect.X, Y = (int)rect.Rect.Y, W = (int)rect.Rect.Width, H = (int)rect.Rect.Height }).ToArray() };

            // save game items
            foreach (GameSprite sprite in sprites)
            {
                List<string> pathes = [];
                /*
                sprite.images.ForEach(image =>
                {
                    string p = res + @"\" + sprite.name + index++ + ".png";
                    try
                    {
                        
                        //try
                        //{
                        //    if (File.Exists(p))
                        //        File.Delete(p);
                        //}
                        //catch { }
                        
                        image?.Save(p, System.Drawing.Imaging.ImageFormat.Png);
                    }
                */
                string p = "", gspPath = "";
                try
                {
                    if (sprite.images.Count >= 2)
                    {
                        int width = sprite.image.Width, height = sprite.image.Height;
                        using (Bitmap strip = new Bitmap(width * sprite.images.Count, height))
                        {
                            using (Graphics g = Graphics.FromImage(strip))
                            {
                                int index = 0;
                                foreach (Bitmap image in sprite.images)
                                {
                                    image.SetResolution(g.DpiX, g.DpiY);
                                    g.DrawImage(image, index * width, 0);
                                    index++;
                                }
                            }
                            p = res + @"\" + sprite.name + "_strip" + sprite.images.Count + ".png";
                            gspPath = @"\res\" + sprite.name + "_strip" + sprite.images.Count + ".png";
                            if (File.Exists(p))
                                File.Delete(p);
                            strip.Save(p);
                        }
                    }
                    else if (sprite.images.Count == 1)
                    {
                        p = res + @"\" + sprite.name + ".png";
                        gspPath = @"\res\" + sprite.name + ".png";
                        if (Global.ImageFileIsSpriteStrip(p, out int w))
                            p = p.Insert(p.Length - 4, "_");
                        if (File.Exists(p))
                            File.Delete(p);
                        sprite.image?.Save(p);
                    }
                }
                catch (Exception ex)
                {
                    string err = "Error: Cannot save sprite image (" + sprite.name /*+ ", image index " + sprite.images.IndexOf(image)*/ + ")";
                    MessageBox.Show(ex.ToString(), err);
                }
                if (!string.IsNullOrWhiteSpace(gspPath))
                    pathes.Add(gspPath);
                sitems.Add(new SerializeableGameSprite { name = sprite.name, numOfImages = sprite.images.Count, pathes = pathes.ToArray(), originX = sprite.originX, originY = sprite.originY, preciseMask = sprite.preciseMask, separateMask = sprite.separateMask, maskBounding_auto = sprite.maskBounding_auto, maskBounding_fullImage = sprite.maskBounding_fullImage, maskBounding_manual = sprite.maskBounding_manual, maskAlphaTolerance = sprite.maskAlphaTolerance, maskBottom = sprite.maskBottom, maskLeft = sprite.maskLeft, maskRight = sprite.maskRight, maskTop = sprite.maskTop });
            }
            foreach (GameBackground background in backgrounds)
            {
                string? p = null, gspPath = null;
                if (background.image != null)
                {
                    p = res + @"\" + background.name + ".png";
                    gspPath = @"\res\" + background.name + ".png";
                    try
                    {

                        try
                        {
                            if (File.Exists(p))
                                File.Delete(p);
                        }
                        catch { }

                        background.image.Save(p, System.Drawing.Imaging.ImageFormat.Png);
                    }
                    catch (Exception ex)
                    {
                        System.Windows.Forms.MessageBox.Show(ex.ToString(), "Error: Cannot save background image (" + background.name + ")");
                    }
                }
                sitems.Add(new SerializeableBackground { name = background.name, path = p });
            }
            foreach (GameSound sound in sounds)
            {
                string p = "", gspPath = "";
                if (!string.IsNullOrWhiteSpace(sound.filePath))
                {
                    p = res + @"\" + sound.name + sound.filePath.Substring(sound.filePath.LastIndexOf('.'));
                    gspPath = @"\res\" + sound.name + sound.filePath.Substring(sound.filePath.LastIndexOf('.'));
                    try
                    {
                        if (sound.filePath != p)
                            File.Copy(sound.filePath, p, true);
                    }
                    catch (Exception ex)
                    {
#if DEBUG
                        System.Windows.Forms.MessageBox.Show("Could not save sound " + sound.name + ":\n\n" + ex);
#endif
                    }
                }
                sitems.Add(new SerializeableGameSound { name = sound.name, path = gspPath, volume = sound.volume, type = sound.Type });
            }
            foreach (GamePath gpath in gpathes)
            {
                sitems.Add(new SerializeableGamePath { name = gpath.name, points = gpath.points.ToArray(), close = gpath.close });
            }
            foreach (GameFont font in fonts)
            {
                var sfont = new SerializeableGameFont(font);

                // create font to get its height property
                Font _font = new Font(font.family, font.size, font.bold && font.italic ? FontStyle.Bold | FontStyle.Italic : font.bold ? FontStyle.Bold : font.italic ? FontStyle.Italic : FontStyle.Regular);
                sfont.heightInPixels = _font.GetHeight();

                if (string.IsNullOrWhiteSpace(sfont.ttf))
                    MessageBox.Show($"Couldn't find .ttf file for font '{font.name}'.");
                else
                {
                    string fontsDir = res + $@"\fonts";
                    string p = fontsDir + @$"\{font.name}.ttf";
                    Directory.CreateDirectory(fontsDir);
                    File.Copy(sfont.ttf, p, true);
                    sfont.ttf = p;
                    sitems.Add(sfont);
                }
            }

            // save all scripts
            foreach (GameScript script in scripts)
            {
                string p = path + @"\" + script.name + ".cs", gspPath = @"\" + script.name + ".cs";
                SaveTextFile(p, script.Script);
                var sscript = new SerializeableGameScript { name = script.name, path = gspPath };
                sitems.Add(sscript);
            }
            foreach (GameObject obj in objects)
            {
                // save script files
                foreach (Core.Models.ObjectEvent evscripts in obj.Events)
                {
                    int i = 0;
                    foreach (string script in evscripts.Scripts)
                    {
                        string p = path + @"\" + obj.name + "." + evscripts.Type + (evscripts.GetParam(out var param) ? "_" + param : "") + (i++) + ".cs";
                        SaveTextFile(p, script);
                    }
                }

                // create serializeable
                string spr = obj.sprite == null ? "" : obj.sprite.name;
                var sobj = new SerializeableGameObject { name = obj.name, sprite = spr, solid = obj.solid, depth = obj.depth, parent = obj.parent?.name };
                sobj.events = [..obj.Events];
                sobj.extraProperties = [.. obj.ExtraProperties.Map(ep => new ArcadeMaker.Core.Resources.Serializeables.ObjectProperty { Name = ep.Name, Constant = ep.Constant, InitValueCode = ep.InitValueCode, Nullable = ep.Nullable, Private = ep.Private, Type = ep.Type })];
                sitems.Add(sobj);
            }
            foreach (GameRoom room in rooms)
            {
                string p = path + @"\" + room.name + ".cs", gspPath = @"\" + room.name + ".cs";
                SaveTextFile(p, room.Script);
                SerializeableRoomObject[] robjs = new SerializeableRoomObject[room.objects.Count];
                for (int i = 0; i < robjs.Length; i++)
                {
                    robjs[i] = new SerializeableRoomObject { id = room.objects[i].id, obj = room.objects[i].obj.name, x = room.objects[i].x, y = room.objects[i].y };
                    if (room.objects[i].HasCustomCreationCode())
                        robjs[i].creationCode = room.objects[i].Script;
                }
                List<SerializeableRoomView> views = new List<SerializeableRoomView>();
                foreach (var view in room.views)
                {
                    views.Add(new SerializeableRoomView
                    {
                        visible = view.Visible,
                        x = view.X,
                        y = view.Y,
                        width = view.Width,
                        height = view.Height,
                        portX = view.PortX,
                        portY = view.PortY,
                        portWidth = view.PortWidth,
                        portHeight = view.PortHeight,
                        objFollow = view.ObjFollow == null ? "" : view.ObjFollow.name ?? "",
                        followHBor = view.FollowHBor,
                        followVBor = view.FollowVBor,
                        followHSp = view.FollowHSp,
                        followVSp = view.FollowVSp
                    });
                }
                List<SerializeableRoomBackground> bgs = new List<SerializeableRoomBackground>();
                foreach (RoomBackground bg in room.backgrounds)
                {
                    bgs.Add(new SerializeableRoomBackground
                    {
                        background = bg.gameBackgroundName,
                        visible = bg.visible,
                        x = bg.x,
                        y = bg.y,
                        tileHor = bg.tileHor,
                        tileVer = bg.tileVer,
                        stretch = bg.stretch,
                        foreground = bg.foreground,
                        horSpd = bg.horSpd,
                        verSpd = bg.verSpd
                    });
                }
                sitems.Add(new SerializeableGameRoom
                {
                    index = room.index,
                    name = room.name,
                    backColor = new SerializeableColor { A = room.backColor.A, R = room.backColor.R, G = room.backColor.G, B = room.backColor.B },
                    width = room.size.width,
                    height = room.size.height,
                    speed = room.speed,
                    persistent = room.persistent,
                    backgrounds = bgs.ToArray(),
                    scriptPath = gspPath,
                    objects = robjs,
                    caption = room.caption,
                    enableViews = room.viewsEnabled,
                    views = views.ToArray()
                });
            }
            sproject.items = sitems.ToArray();

            // save user assemblies
            sproject.userAssemblies = assemblyReferences.ToArray();

            // save project tree
            Func<ProjectFolderTreeStruct<GameItem>, string, SerializeableGameProjectTreeNode> TranslateStruct = null;
            TranslateStruct = (ProjectFolderTreeStruct<GameItem> folder, string type) =>
            {
                SerializeableGameProjectTreeNode sfolder = new SerializeableGameProjectTreeNode();
                sfolder.type = type;
                sfolder.name = folder.Name;
                sfolder.isFolder = true;
                sfolder.nodes = new SerializeableGameProjectTreeNode[folder.Structs.Count];
                for (int n = 0; n < sfolder.nodes.Length; n++)
                {
                    if (folder.Structs[n] is ProjectItemTreeStruct<GameItem> itemStruct)
                    {
                        sfolder.nodes[n] = new SerializeableGameProjectTreeNode();
                        sfolder.nodes[n].name = itemStruct.Item.name;
                        sfolder.nodes[n].type = type;
                        sfolder.nodes[n].isFolder = false;
                    }
                    else if (folder.Structs[n] is ProjectFolderTreeStruct<GameItem> folderStruct)
                    {
                        sfolder.nodes[n] = TranslateStruct(folderStruct, type);
                    }
                }
                return sfolder;
            };

            Type[] gameItemTypes = new Type[] { typeof(GameSprite), typeof(GameSound), typeof(GameBackground), typeof(GamePath), typeof(GameScript), typeof(GameFont), typeof(GameObject), typeof(GameRoom) };
            SerializeableGameProjectTreeNode[] mainNodes = new SerializeableGameProjectTreeNode[gameItemTypes.Length];
            for (int t = 0; t < gameItemTypes.Length; t++)
            {
                ProjectFolderTreeStruct<GameItem> projectStruct = Global.form1.GetProjectStruct<GameItem>(gameItemTypes[t]);
                mainNodes[t] = TranslateStruct(projectStruct, gameItemTypes[t].FullName);
            }

            sproject.treeMainNodes = mainNodes;

            // generate xml file describes project
            TextWriter writer = null;
            string savePath = path + @"\" + fileName + ".gsp";
            string existsText = ReadTextFile(savePath);
            try
            {
                saved = true;

                XmlSerializer serializer = new XmlSerializer(sproject.GetType(), extraTypes: serializerExtraTypes);
                
                writer = File.CreateText(savePath);
                serializer.Serialize(writer, sproject);
                
                if (successMsg)
                    MessageBox.Show("Project Successfuly saved");
            }
            catch (Exception ex)
            {
                string cancelErr = null;
                try
                {
                    if (existsText != null)
                    {
                        SaveTextFile(savePath, existsText);
                    }
                }
                catch (Exception cancelEx)
                {
                    cancelErr = cancelEx.Message;
                }

                string err = "Could not save project:\n" + ex;
                if (cancelErr != null)
                    err += "\n\nWarning: Your project might be deleted, due to the following error:\n" + cancelErr +
                           "\nFix the saving error and save the project without closing the program!";
                System.Windows.Forms.MessageBox.Show(err, "Error");
            }
            finally
            {
                writer?.Close();
            }
        }

        private static readonly Type[] serializerExtraTypes = 
        [
                    typeof(SerializeableGameProjectTreeNode),
                    typeof(GameItem),
                    typeof(SerializeableBackground),
                    typeof(SerializeableGameSprite),
                    typeof(SerializeableGameSound),
                    typeof(SerializeableGamePath),
                    typeof(SerializeableGameScript),
                    typeof(GameFont),
                    typeof(SerializeableGameFont),
                    typeof(SerializeableGameObject),
                    typeof(SerializeableGameRoom),
                    typeof(SerializeableRoomObject),
                    typeof(RoomBackground),
                    typeof(SerializeableRoomView),
                    typeof(SerializeableColor),
                    typeof(Point),
                    typeof(PathPoint),
                    typeof(ObjectEvent),
                    typeof(ParameterizedObjectEvent<Core.ExpSrc.Controls.Keys>),
                    typeof(ParameterizedObjectEvent<Core.ExpSrc.Controls.MouseButton>),
                    typeof(ParameterizedObjectEvent<Core.ExpSrc.Controls.GamepadButton>),
                    typeof(CollisionEvent),
                    typeof(AssemblyReference),
                    typeof(IDEObjectProperty),
                    typeof(ArcadeMaker.Core.Resources.Serializeables.VariableType),
                    typeof(TextureAtlasMap),
                    typeof(TextureAtlasMap.Item),
                    typeof(Sound.Types)
        ];

        public static GameProject Open(string path)
        {
            return Open(path, out object[] ignore);
        }

        public static GameProject Open(string path, out object[] projectMainFoldersStructs)
        {
            projectMainFoldersStructs = null;
            XmlSerializer serializer = new XmlSerializer(typeof(SerializeableGameProject), extraTypes: serializerExtraTypes);
            SerializeableGameProject sproject = null;
            using (Stream reader = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                try
                {
                    sproject = serializer.Deserialize(reader) as SerializeableGameProject;
                }
                catch (Exception ex)
                {
                    string err = "Cannot open project: Bad .gsp file";
#if DEBUG
                    err += "\n\n" + ex.ToString();
#endif
                    System.Windows.Forms.MessageBox.Show(err, "Error");
                    return null;
                }
            }
            GameProject project = null;
            try
            {
                project = new GameProject(sproject.name);
                project.saved = true;
                project.projectFilePath = path;
                string projectFileLocation = path.FileLocation();
                foreach (SerializeableGameItem item in sproject.items)
                {
                    if (item is SerializeableGameSprite)
                    {
                        SerializeableGameSprite ssprite = item as SerializeableGameSprite;
                        GameSprite sprite = new GameSprite(item.name)
                        {
                            originX = ssprite.originX,
                            originY = ssprite.originY,
                            preciseMask = /*ssprite.preciseMask*/false,
                            separateMask = ssprite.separateMask,
                            maskBounding_auto = ssprite.maskBounding_auto,
                            maskBounding_fullImage = ssprite.maskBounding_fullImage,
                            maskBounding_manual = ssprite.maskBounding_manual,
                            maskAlphaTolerance = ssprite.maskAlphaTolerance,
                            maskTop = ssprite.maskTop,
                            maskRight = ssprite.maskRight,
                            maskBottom = ssprite.maskBottom,
                            maskLeft = ssprite.maskLeft
                        };
                        try
                        {
                            sprite.Import(ssprite.pathes.ToArray(p => projectFileLocation + p));
                        }
                        catch (Exception ex)
                        {
                            string msg = string.Format("Error loading image for sprite \"{0}\"", sprite.name);
                            if (!Global.ShowDebugMessage(msg + "\n" + ex))
                            {
                                MessageBox.Show(msg);
                            }
                        }
                        project.items.Add(sprite);
                    }
                    else if (item is SerializeableBackground)
                    {
                        SerializeableBackground sbg = item as SerializeableBackground;
                        GameBackground bg = new GameBackground(item.name);
                        try
                        {
                            if (sbg.path != null)
                            {
                                using (Stream stream = new FileStream(projectFileLocation + sbg.path, FileMode.Open, FileAccess.Read))
                                    bg.image = (Bitmap)Bitmap.FromStream(stream);
                            }
                        }
                        catch
                        {
                            System.Windows.Forms.MessageBox.Show(string.Format("Error loading image for background \"{0}\" from location {1}", bg.name, sbg.path));
                        }
                        project.items.Add(bg);
                    }
                    else if (item is SerializeableGameSound ssound)
                    {
                        GameSound sound = new(ssound.name)
                        {
                            filePath = projectFileLocation + ssound.path,
                            volume = ssound.volume,
                            Type = ssound.type
                        };
                        project.items.Add(sound);
                    }
                    else if (item is SerializeableGamePath)
                    {
                        SerializeableGamePath spath = item as SerializeableGamePath;
                        GamePath gpath = new GamePath(spath.name);
                        gpath.points = spath.points.ToList();
                        gpath.close = spath.close;
                        project.items.Add(gpath);
                    }
                    else if (item is SerializeableGameScript)
                    {
                        var sscript = item as SerializeableGameScript;
                        GameScript script = new GameScript(sscript.name);
                        try
                        {
                            script.Script = ReadTextFile(projectFileLocation + sscript.path);
                        }
                        catch
                        {
                            System.Windows.Forms.MessageBox.Show(string.Format("Error loading script \"{0}\" from location {1}", script.name, sscript.path));
                        }
                        project.items.Add(script);
                    }
                    else if (item is SerializeableGameFont sfont)
                    {
                        project.items.Add(sfont.font);
                    }
                    else if (item is SerializeableGameObject sobj)
                    {
                        GameObject obj = new GameObject(sobj.name);
                        if (sobj.sprite != null && sobj.sprite != "")
                            obj.sprite = project.items.OfType<GameSprite>().ToList().Find(spr => spr.name == sobj.sprite);
                        obj.solid = sobj.solid;
                        obj.depth = sobj.depth;
                        if (sobj.parent != null && sobj.parent != "")
                            obj.parent = project.items.OfType<GameObject>().ToList().Find(gobj => gobj.name == sobj.parent);
                        obj.ExtraProperties.AddRange(sobj.extraProperties.Map(pro => new IDEObjectProperty(obj) { Constant = pro.Constant, InitValueCode = pro.InitValueCode, Name = pro.Name, Nullable = pro.Nullable, Private = pro.Private, Type = pro.Type }));

                        try
                        {
                            // read scripts from files
                            //foreach (EventScripts evscripts in sobj.events)
                            //{
                            //    int i = 0;
                            //    foreach (EventScript script in evscripts.Scripts)
                            //        script.Script = ReadTextFile(projectFileLocation + @"\" + obj.name + "." + evscripts.Event + (i++) + ".cs");

                            //}
                            obj.Events.AddRange(sobj.events);
                        }
                        catch
                        {
                            MessageBox.Show(string.Format("Error loading code for object \"{0}\" from location {1}", obj.name, projectFileLocation));
                        }
                        project.items.Add(obj);
                    }
                    else if (item is SerializeableGameRoom)
                    {
                        var sroom = item as SerializeableGameRoom;
                        GameRoom room;
                        if (sroom.index >= 0)
                            room = new GameRoom(sroom.name, sroom.index);
                        else
                            room = new GameRoom(sroom.name);
                        room.backColor = Color.FromArgb(sroom.backColor.A, sroom.backColor.R, sroom.backColor.G, sroom.backColor.B);
                        if (sroom.backgrounds != null)
                        {
                            room.backgrounds = sroom.backgrounds.Select(bg => new RoomBackground
                            {
                                gameBackgroundName = bg.background,
                                visible = bg.visible,
                                x = bg.x,
                                y = bg.y,
                                tileHor = bg.tileHor,
                                tileVer = bg.tileVer,
                                stretch = bg.stretch,
                                foreground = bg.foreground,
                                horSpd = bg.horSpd,
                                verSpd = bg.verSpd
                            }).ToArray();
                        }
                        room.size = new RoomSize(sroom.width, sroom.height);
                        room.speed = sroom.speed;
                        room.persistent = sroom.persistent;
                        room.caption = sroom.caption;
                        room.viewsEnabled = sroom.enableViews;

                        if (sroom.views != null)
                        {
                            room.views.Clear();
                            List<IDE.Items.RoomView> views = [];
                            foreach (SerializeableRoomView sview in sroom.views)
                            {
                                GameObject followObj = null;
                                foreach (GameObject obj in project.items.OfType<GameObject>())
                                {
                                    if (obj.name == sview.objFollow)
                                        followObj = obj;
                                }
                                views.Add(new()
                                {
                                    Visible = sview.visible,
                                    X = sview.x,
                                    Y = sview.y,
                                    Width = sview.width,
                                    Height = sview.height,
                                    PortX = sview.portX,
                                    PortY = sview.portY,
                                    PortWidth = sview.portWidth,
                                    PortHeight = sview.portHeight,
                                    ObjFollow = followObj,
                                    FollowHBor = sview.followHBor,
                                    FollowVBor = sview.followVBor,
                                    FollowHSp = sview.followHSp,
                                    FollowVSp = sview.followVSp
                                });
                            }
                            while (views.Count < GameRoom.minNumOfViews)
                                views.Add(new());
                            room.views.AddRange(views);
                        }

                        try
                        {
                            room.Script = ReadTextFile(projectFileLocation + sroom.scriptPath);
                        }
                        catch
                        {
                            System.Windows.Forms.MessageBox.Show(string.Format("Error loading code for room \"{0}\" from location {1}", room.name, sroom.scriptPath));
                        }
                        int sroomIndex = 100; // do not set this to 1000, the 100 serie is set for room objects with undefined ID
                        foreach (SerializeableRoomObject srobj in sroom.objects)
                        {
                            if (!srobj.id.StartsWith("R"))
                                srobj.id = $"R{room.index}INST{sroomIndex++}";
                            RoomObject robj = new RoomObject(srobj.id, srobj.x, srobj.y, project.items.OfType<GameObject>().ToList().Find(obj => obj.name == srobj.obj));
                            if (srobj.creationCode != null)
                                robj.Script = srobj.creationCode;
                            room.objects.Add(robj);
                        }
                        project.items.Add(room);
                    }
                }

                // load user assemblies
                if (sproject.userAssemblies != null)
                    project.assemblyReferences.AddRange(sproject.userAssemblies);

                // build project tree struct
                if (sproject.treeMainNodes != null)
                {
                    projectMainFoldersStructs = new object[sproject.treeMainNodes.Length];
                    Func<SerializeableGameProjectTreeNode, bool, object> TranslateNode = null;
                    TranslateNode = (SerializeableGameProjectTreeNode node, bool isBaseFolder) =>
                    {
                        object @struct = null;

                        try
                        {
                            GameItem item = project.items.ToList().Find(i => i.name == node.name);
                            if (!node.isFolder)
                            {
                                if (node.type == null)
                                    throw new Exception("node.type == null");
                                Type genericType = typeof(ProjectItemTreeStruct<>).MakeGenericType(Type.GetType(node.type));
                                @struct = Activator.CreateInstance(genericType, item);
                            }
                            else
                            {
                                Type genericType = typeof(ProjectFolderTreeStruct<>).MakeGenericType(Type.GetType(node.type));
                                object[] children = new object[node.nodes.Length];
                                for (int c = 0; c < children.Length; c++)
                                    children[c] = TranslateNode(node.nodes[c], false);

                                @struct = Activator.CreateInstance(genericType, node.name, isBaseFolder, children);
                            }
                        }
                        catch (Exception ex)
                        {
                            try
                            {
                                string err = "Error loading project tree, game items will not be inserted into folders";
#if DEBUG
                                err += "\n\n[Debug Mode]\n" + ex;
#endif
                                System.Windows.Forms.MessageBox.Show(err);
                            }
                            catch { }
                            return null;
                        }

                        return @struct;
                    };

                    for (int i = 0; i < projectMainFoldersStructs.Length; i++)
                    {
                        projectMainFoldersStructs[i] = TranslateNode(sproject.treeMainNodes[i], true);
                    }
                }
            }
            catch (Exception ex)
            {
                string err = "Error opening project";
#if DEBUG
                err += "\n\n" + ex.ToString();
#endif
                System.Windows.Forms.MessageBox.Show(err, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return project;
        }

        private static void SaveTextFile(string path, string file)
        {
            if (file == null)
            {
#if DEBUG
                System.Windows.Forms.MessageBox.Show("Error at SaveTextFile() method: file content is null", "[Debug Version Only]");
#endif
                return;
            }
            StreamWriter writer = File.CreateText(path);
            foreach (char c in file)
                writer.Write(c);
            writer.Close();
        }

        private static string ReadTextFile(string path)
        {
            if (!File.Exists(path))
                return null;
            using (TextReader reader = File.OpenText(path))
            {
                return reader.ReadToEnd();
            }
        }
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
        public string type { get; set; }
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
        public Core.Resources.Sound.Types type;
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
        internal float heightInPixels;

        public SerializeableGameFont() { }
        public SerializeableGameFont(GameFont font)
        {
            name = font.name;
            this.font = font;
            this.ttf = font.GetTTF();
        }
    }

    public class SerializeableGameObject : SerializeableGameItem
    {
        public string sprite;
        public bool solid;
        public int depth;
        public string parent;
        public Core.Models.ObjectEvent[] events;
        public ArcadeMaker.Core.Resources.Serializeables.ObjectProperty[] extraProperties;
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
}
