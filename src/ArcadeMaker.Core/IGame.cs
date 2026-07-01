using ArcadeMaker.Core.Exceptions;
using ArcadeMaker.Core.Models;
using ArcadeMaker.Core.Resources;
using ArcadeMaker.Core.Resources.Serializeables;
using ArcadeMaker.Core.Runtime;
using ArcadeMaker.Core.ExpSrc;
using Exp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Reflection.Metadata;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Xml.Serialization;
using ArcadeMaker.Core.Math;
using ArcadeMaker.Core.Math.Shapes;
using Exp.Spans;
using System.ComponentModel.DataAnnotations;
using ArcadeMaker.Core.ExpSrc.Controls;

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
    int CurrentViewIndex { get; }

    StringWriter Debug { get; }

    void Init();
    Exp.Void DrawInstance(Runtime.Instance inst);
    void DrawLine(double x1, double y1, double x2, double y2, double thickness);
    void SetWindowsSize(int w, int h);
    void SetCaption(string caption);
    Color BackColor { get; set; }

    internal RoomInstance GetActivatedRoom() => CurrentRoom ?? throw new NoActivatedRoomException();
    private static event EventHandler? OnProjectLoadingComplete;
    public void LoadFromProject(SerializeableGameProject sproject, string filePath)
    {
        try
        {
            string projectFileLocation = filePath.Substring(0, filePath.LastIndexOf('\\'));
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
                    Sounds.Add(new(ssound.name, projectFileLocation + ssound.path, ssound.volume, ssound.pan, ssound.patch, ssound.type));
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
                    var doc = ScriptDocument.FromFile(projectFileLocation + script.path);
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
                    // load event scripts
                    //List<InstanceScriptDocument> createEv = [], stepEv = [], drawEv = [];
                    //foreach (var evscripts in sobj.events)
                    //{
                    //    //string createEvScript = File.ReadAllText($"{projectFileLocation}\\{sobj.name}.Create.cs");
                    //    //string stepEvScript = File.ReadAllText($"{projectFileLocation}\\{sobj.name}.Step.cs");
                    //    //string drawEvScript = File.ReadAllText($"{projectFileLocation}\\{sobj.name}.Draw.cs");
                    //    List<InstanceScriptDocument> list = evscripts.Type switch
                    //    {
                    //        ObjectEvent.EventType.Create => createEv,
                    //        ObjectEvent.EventType.Step => stepEv,
                    //        ObjectEvent.EventType.Draw => drawEv,
                    //        _ => throw new NotImplementedException($"Unsupported event type: {evscripts.Type}.")
                    //    };
                    //    evscripts.Scripts.ForEach(script => { if (!string.IsNullOrWhiteSpace(script.Script)) list.Add(ExpSrc.ExpSrc.CreateInstanceScriptDocument($"{evscripts.Type} event of {sobj.name}", null, script.Script, evscripts.Type == ObjectEvent.EventType.Draw ? [ExpSrc.ExpSrc.CURRENT_VIEW_INDEX_ARG_NAME] : [])); });
                    //}

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

            OnProjectLoadingComplete?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex) when (false)
        {
            throw new LoadingException("Error loading game items from project file.", ex);
        }
    }

    public void LoadFromProjectFile(string filePath)
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

        XmlSerializer serializer = new XmlSerializer(typeof(SerializeableGameProject), extraTypes: serializerExtraTypes);
        SerializeableGameProject sproject = null;
        using (Stream reader = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            try
            {
                sproject = (SerializeableGameProject)serializer.Deserialize(reader);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        LoadFromProject(sproject, filePath);
    }
}