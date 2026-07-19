using ArcadeMaker.Core.Exceptions;
using ArcadeMaker.Core.ExpSrc;
using ArcadeMaker.Core.ExpSrc.Controls;
using ArcadeMaker.Core.Math;
using ArcadeMaker.Core.Math.Shapes;
using ArcadeMaker.Core.Models;
using ArcadeMaker.Core.Resources;
using ArcadeMaker.Core.Resources.Serializeables;
using ArcadeMaker.Core.Runtime;
using Exp;
using Exp.Spans;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Drawing;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Xml.Serialization;
using static System.Net.Mime.MediaTypeNames;
using System.Resources.NetStandard;

namespace ArcadeMaker.Core;

public partial interface IGame
{
    List<Sprite> Sprites { get; }
    List<ObjectModel> Objects { get; }
    List<Sound> Sounds { get; }
    List<Background> Backgrounds { get; }
    List<Resources.Path> Paths { get; }
    List<GameFont> FontsData { get; }
    List<RoomModel> Rooms { get; }
    List<ScriptDocument> Scripts { get; }
    RoomInstance? CurrentRoom { get; set; }
    TextureAtlasMap MainTextureAtlasMap { get; set; }
    string MainTextureAtlasFilePath { get; set; }
    int CurrentViewIndex { get; }

    void Init();
    Exp.Void DrawInstance(Runtime.Instance inst);
    void DrawLine(double x1, double y1, double x2, double y2, double thickness);
    void SetWindowsSize(int w, int h);
    void SetCaption(string caption);
    Color BackColor { get; set; }
    (int x, int y) MousePositionInWindow { get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal RoomInstance GetActivatedRoom() => CurrentRoom ?? throw new NoActivatedRoomException(); // TODO: skip this method...
    private static event EventHandler? OnProjectLoadingComplete;
    public bool LoadFromProject(SerializeableGameProject sproject, Stream? bundledProjectFileStream, string? filePath)
    {
        // validate that arguments are valid
        if (bundledProjectFileStream == null && filePath == null)
            throw new ArgumentNullException(message: $"Argument {nameof(bundledProjectFileStream)} OR argument {nameof(filePath)} must have a value.", null);
        
        // find directory of project file, if there's one
        string? fileDir = System.IO.Path.GetDirectoryName(filePath);

        // check file format
        string fileExt = filePath == null ? SerializeableGameProject.FileFormat_AMPB : filePath.Substring(filePath.LastIndexOf('.'));
        bool bundled = fileExt switch
        {
            SerializeableGameProject.FileFormat_AMP => false,
            SerializeableGameProject.FileFormat_AMPB => true,
            _ => throw new FormatException($"Unsupported project file format ({fileExt}).")
        };

        try
        {
            // if it's a bundled project format, create a ResourceReader for the project file
            FileStream? fileStream = null;
            bundledProjectFileStream ??= bundled ? fileStream = File.OpenRead(filePath!) : null;
            using var _ = fileStream; // dispose it if it exists
            using ResXResourceReader? resourceReader = bundled ? new(bundledProjectFileStream!) : null;

            // a method to get an absolute path from a relative one
            string AbsPath(string relativePath) => fileDir + (relativePath.StartsWith('\\') ? "" : "\\") + relativePath;

            // a method to save a resource, wether it's a bundled project file or normal
            object? LoadResource(string key)
            {
                if (bundled)
                {
                    var resourceDictionary = resourceReader!.GetEnumerator();
                    while (resourceDictionary.MoveNext())
                    {
                        if (key.Equals(resourceDictionary.Key))
                            return resourceDictionary.Value;
                    }

                    throw new KeyNotFoundException("Bundled project file did not contain key '" + key + "'.");
                }
                else
                {
                    return File.ReadAllBytes(AbsPath(key));
                }
            }

            // a method to save text, wether it's a bundled project file or normal
            string LoadText(string key)
            {
                if (bundled)
                {
                    return (string)LoadResource(key);
                }
                else
                {
                    try
                    {
                        return File.ReadAllText(AbsPath(key));
                    }
                    catch
                    {
                        throw;
                    }
                }
            }

            string projectFileLocation = fileDir;
            foreach (SerializeableGameItem item in sproject.items)
            {
                if (item is SerializeableGameSprite ssprite)
                {
                    var mask = new SpriteMask(ssprite.maskTop, ssprite.maskLeft, ssprite.maskRight, ssprite.maskBottom);
                    var sprite = new Sprite(item.name, ssprite.pathes.FirstOrDefault(), ssprite.numOfImages, ssprite.originX, ssprite.originY, mask)
                    {
                        //preciseMask = ssprite.preciseMask,
                        //separateMask = ssprite.separateMask,
                        //maskBounding_auto = ssprite.maskBounding_auto,
                        //maskBounding_fullImage = ssprite.maskBounding_fullImage,
                        //maskBounding_manual = ssprite.maskBounding_manual,
                        //maskAlphaTolerance = ssprite.maskAlphaTolerance
                    };
                    Sprites.Add(sprite);
                }
                else if (item is SerializeableBackground sbg)
                {
                    Backgrounds.Add(new(sbg.name, sbg.path));
                }
                else if (item is SerializeableGameSound ssound)
                {
                    Sounds.Add(new(ssound.name, ssound.path, ssound.volume, ssound.pan, ssound.patch, ssound.type));
                }
                else if (item is SerializeableGamePath spath)
                {
                    double startX = 0, startY = 0;
                    if (spath.points.Length >= 1)
                    {
                        startX = spath.points[0].x;
                        startY = spath.points[0].y;
                    }

                    PathPoint? prevPnt = null;
                    List<Resources.Path.Step> steps = [];
                    int i = 0;
                    foreach (var p in spath.points)
                    {
                        var point = p;

                    Beginning:
                        if (prevPnt != null)
                        {
                            steps.Add(new(point.x - prevPnt.x, prevPnt.y - point.y, prevPnt.speed));
                        }
                        prevPnt = point;

                        // close path
                        if (spath.close && ++i == spath.points.Length)
                        {
                            point = spath.points[0];
                            goto Beginning;
                        }
                    }
                    Paths.Add(new(spath.name, startX, startY, [.. steps]));
                }
                else if (item is SerializeableGameScript script)
                {
                    var doc = ScriptDocument.FromString(LoadText(script.path), script.name);
                    doc.Usings.AddRange(ExpSrc.ExpSrc.GlobalUsings);
                    doc.Namespace ??= ExpSrc.ExpSrc.GameNamespace;
                    Scripts.Add(doc);
                }
                else if (item is SerializeableGameFont sfont)
                {
                    sfont.font.Name = sfont.name;
                    sfont.font.ttf = sfont.ttf;
                    sfont.font.heightInPixels = sfont.heightInPixels;
                    FontsData.Add(sfont.font);
                }
                else if (item is SerializeableGameObject sobj)
                {
                    ObjectModel obj = new(sobj.name, Sprites.FirstOrDefault(spr => spr.Name == sobj.sprite), sobj.events, sobj.extraProperties)
                    {
                        InitValues = (Depth: sobj.depth, Visible: true, Solid: sobj.solid)
                    };

                    // set the def class of the documents (now done at ObjectModel class)
                    //createEv.ForEach(doc => doc.Def = obj.Class);
                    //stepEv.ForEach(doc => doc.Def = obj.Class);
                    //drawEv.ForEach(doc => doc.Def = obj.Class);

                    Objects.Add(obj);
                }
                else if (item is SerializeableGameRoom sroom)
                {
                    // translate to init map
                    List<RoomInitMap.Item> items = [];
                    foreach (var obj in sroom.objects)
                        items.Add(new(obj.x, obj.y, Objects.First(model => obj.obj == model.Name)));
                    RoomInitMap initMap = new([.. items]);

                    Color bcol = Color.FromArgb(sroom.backColor.A, sroom.backColor.R, sroom.backColor.G, sroom.backColor.B);
                    RoomModel room = new(sroom.name, sroom.caption, sroom.width, sroom.height, bcol, initMap);

                    // add views
                    if (sroom.enableViews)
                    {
                        foreach (var sview in sroom.views)
                        {
                            RoomView view = new(sview.x, sview.y)
                            {
                                Visible = sview.visible,
                                Width = sview.width,
                                Height = sview.height,
                                PortX = sview.portX,
                                PortY = sview.portY,
                                PortWidth = sview.portWidth,
                                PortHeight = sview.portHeight,
                                Follow_HBorder = sview.followHBor,
                                Follow_VBorder = sview.followVBor,
                                Follow_HSpeed = sview.followHSp,
                                Follow_VSpeed = sview.followVSp
                            };

                            // set following object after all objects are loaded, bc it's specified by name
                            OnProjectLoadingComplete += (s, _) =>
                            {
                                if (s == this)
                                    view.Following = Objects.FirstOrDefault(o => o.Name == sview.objFollow);
                            };

                            room.Views.Add(view);
                        }
                    }

                    // add backgrounds
                    foreach (var srbg in sroom.backgrounds)
                    {
                        if (srbg.background == null)
                            continue;

                        RoomBackground view = new(Backgrounds.First(bg => bg.Name == srbg.background), srbg.visible, srbg.tileHor, srbg.tileVer, srbg.stretch, srbg.horSpd, srbg.verSpd) { X = srbg.x, Y = srbg.y };

                        room.Backgrounds.Add(view);
                    }

                    Rooms.Add(room);
                }
            }

            // create main texture atlas
            MainTextureAtlasMap = sproject.textureAtlasMap;
            MainTextureAtlasFilePath = MainTextureAtlasMap.AtlasFilePath;

            OnProjectLoadingComplete?.Invoke(this, EventArgs.Empty);

            return bundled;
        }
        catch (Exception ex)
        {
            throw new LoadingException("Error loading game items from project file.", ex);
        }
    }

    /// <summary>
    /// Loads resources info.
    /// </summary>
    /// <param name="path">The path to the project file to run.</param>
    /// <param name="bundledProjectFileStream">The bundled project file stream.</param>
    /// <returns><c>true</c> if the given file was a bundled project file. Otherwise, <c>false</c>.</returns>
    public bool LoadFromProjectFile(Stream? bundledProjectFileStream, string? path)
    {
        Type[] serializerExtraTypes =
        [
                    typeof(SerializeableGameProjectTreeNode),
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
                    typeof(SerializeableRoomBackground),
                    typeof(SerializeableRoomView),
                    typeof(SerializeableColor),
                    typeof(Point),
                    typeof(PathPoint),
                    typeof(ObjectEvent),
                    typeof(ParameterizedObjectEvent<Keys>),
                    typeof(ParameterizedObjectEvent<MouseButton>),
                    typeof(ParameterizedObjectEvent<GamepadButton>),
                    typeof(ParameterizedObjectEvent<int>),
                    typeof(CollisionEvent),
                    typeof(AssemblyReference),
                    typeof(ObjectProperty),
                    typeof(VariableType),
                    typeof(TextureAtlasMap),
                    typeof(TextureAtlasMap.Item)
        ];

        string? fileDir = System.IO.Path.GetDirectoryName(path);

        // check file format
        string fileExt = bundledProjectFileStream != null ? SerializeableGameProject.FileFormat_AMPB : path!.Substring(path.LastIndexOf('.'));
        bool bundled = fileExt switch
        {
            SerializeableGameProject.FileFormat_AMP => false,
            SerializeableGameProject.FileFormat_AMPB => true,
            _ => throw new FormatException()
        };

        XmlSerializer serializer = new(typeof(SerializeableGameProject), extraTypes: serializerExtraTypes);
        SerializeableGameProject? sproject = null;
        using (Stream xmlReader = bundled ? (path == null ? SerializeableGameProject.OpenStream(bundledProjectFileStream!, "*", true) : SerializeableGameProject.OpenStream(path, "*", true))! : new FileStream(path!, FileMode.Open, FileAccess.Read))
        {
            try
            {
                sproject = (SerializeableGameProject)serializer.Deserialize(xmlReader);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        return LoadFromProject(sproject, bundledProjectFileStream, path);
    }
}