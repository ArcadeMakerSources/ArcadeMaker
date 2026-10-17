using ArcadeMaker.Core.Math.Shapes;
using ArcadeMaker.Core.Resources;
using Exp;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace ArcadeMaker.Core.Models;

public class RoomModel(string name, string caption, int w, int h, Color backgroundColor, RoomInitMap initMap) : IModel, ISetsID
{
    public int ID { get; } = Core.ID.Generate();
    internal Rect Bounds { get; } = new() { X = 0, Y = 0, Width = w, Height = h, Angle = 0, OriginX = 0, OriginY = 0 };
    public string Name => name;
    public string Caption => caption;
    public RoomInitMap InitMap => initMap;
    public int Width => w;
    public int Height => h;
    public Color BackgroundColor => backgroundColor;
    public Background Background { get; init; }

    /// <summary>
    /// These views are only models to be copied by the room instance when the room is created.
    /// </summary>
    public List<RoomView> Views { get; } = [];

    /// <summary>
    /// These backgrounds are only models to be copied by the room instance when the room is created.
    /// </summary>
    internal List<RoomBackground> Backgrounds { get; } = [];
}

public class RoomInitMap(RoomInitMap.Item[] items)
{
    public class Item
    {
        public double X { get; }
        public double Y { get; }
        public int ImageIndex { get; }
        public ObjectModel Object { get; }
        public string? CreationCode { get; }
        public InstanceScriptDocument? CreationCodeDoc { get; }

        public Item(double x, double y, int imageIndex, ObjectModel @object, string? creationCode, string roomName)
        {
            this.X = x;
            this.Y = y;
            this.ImageIndex = imageIndex;
            this.Object = @object;

            if (!string.IsNullOrWhiteSpace(creationCode))
            {
                this.CreationCode = creationCode;

                // create the InstanceScriptDocument for the creation code
                string docName = $"{roomName}.Instances({Object.Name}, X: {(int)X}, Y: {(int)Y}).CreationCode";
                CreationCodeDoc = ExpSrc.ExpSrc.CreateInstanceScriptDocument(docName, Object.Class, CreationCode);

                if (!CreationCodeDoc.ContainsCode)
                    CreationCodeDoc = null;
            }
        }
    }
    public Item[] Items => items;

    public int NumberOfInstancesWithCreationCode { get; } = items.Count(i => i.CreationCodeDoc != null);
}

public record RoomView // originally it was a normal class, but we've changed it to "record" to be able to use "with" on this
{
    public bool Visible { get; set; }
    public double X { get; private set; }
    public double Y { get; private set; }
    public double Width { get; private set; }
    public double Height { get; private set; }
    public int PortX { get; private set; }
    public int PortY { get; private set; }
    public int PortWidth { get; private set; }
    public int PortHeight { get; private set; }
    public ObjectModel? Following { get; set; }
    public Runtime.Instance? SpecificInstanceToFollow { get; set; }
    public double Follow_HBorder { get; set; }
    public double Follow_VBorder { get; set; }
    public double Follow_HSpeed { get; set; }
    public double Follow_VSpeed { get; set; }

    /// <summary>
    /// An action to invoke when one or more of the values of the view / port rectangles were modified.
    /// When the <c>bool</c> argument is <c>true</c>, it means that only view x / y were modified.
    /// </summary>
    public Action<bool>? Modified;

    public RoomView(double x, double y, double width, double height, int portX, int portY, int portWidth, int portHeight)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
        PortX = portX;
        PortY = portY;
        PortWidth = portWidth;
        PortHeight = portHeight;
    }

    public void SetPosition(double x, double y)
    {
        this.X = x;
        this.Y = y;
        Modified?.Invoke(true);
    }

    public void SetSize(double width, double height)
    {
        this.Width = width;
        this.Height = height;
        Modified?.Invoke(false);
    }

    public void SetPositionAndSize(double x, double y, double w, double h)
    {
        this.X = x;
        this.Y = y;
        this.Width = w;
        this.Height = h;
        Modified?.Invoke(false);
    }

    public void SetPortPosition(int x, int y)
    {
        PortX = x;
        PortY = y;
        Modified?.Invoke(false);
    }

    public void SetPortSize(int width, int height)
    {
        PortWidth = width;
        PortHeight = height;
        Modified?.Invoke(false);
    }

    public void SetPortPositionAndSize(int x, int y, int w, int h)
    {
        PortX = x;
        PortY = y;
        PortWidth = w;
        PortHeight = h;
        Modified?.Invoke(false);
    }
}

public record RoomBackground(Background Background, bool Visible, bool TileHor, bool TileVer, bool Stretch, double HorSpeed, double VerSpeed)
{
    public double X { get; set; }
    public double Y { get; set; }
}