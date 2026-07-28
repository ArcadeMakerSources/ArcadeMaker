using System;
using System.Collections.Generic;
using System.Security.Cryptography.Xml;
using System.Text;
using DrawingOperation = System.Action<System.Drawing.Graphics>;

namespace ArcadeMaker.IDE;

partial class SpriteDesigner
{
    abstract class DrawingTool
    {
        public abstract string Name { get; }
        public abstract bool OnTheMove { get; }
        public virtual bool OnMouseDown => false;
        public virtual bool OnMouseUp => true;
        public virtual bool DisablePreview => false;

        public abstract DrawingOperation Draw(Color col1, Color col2, Point p1, Point p2, FillTypes fillType, float thickness);

        protected static Point GetTopLeftCorner(Point p1, Point p2) => new(Math.Min(p2.X, p1.X), Math.Min(p2.Y, p1.Y));
        protected static Size GetShapeSize(Point p1, Point p2) => new(Math.Max(p2.X, p1.X) - Math.Min(p2.X, p1.X), Math.Max(p2.Y, p1.Y) - Math.Min(p2.Y, p1.Y));
    }

    class RectangleDrawer : DrawingTool
    {
        public override string Name => "Rectangle Drawer";
        public override bool OnTheMove => false;

        public override DrawingOperation Draw(Color col1, Color col2, Point p1, Point p2, FillTypes fillType, float thickness) => graphics =>
        {
            Point topLeft = GetTopLeftCorner(p1, p2);
            Size size = GetShapeSize(p1, p2);

            void DrawOutline()
            {
                using Pen pen = new(col1, thickness);
                graphics.DrawRectangle(pen, new Rectangle(topLeft, size));
            }

            if (fillType == FillTypes.Outline)
            {
                DrawOutline();
            }
            else if (fillType == FillTypes.Fill)
            {
                using Pen pen = new(col1);
                graphics.FillRectangle(pen.Brush, new Rectangle(topLeft, size));
            }
            else
            {
                using Pen pen = new(col2);
                graphics.FillRectangle(pen.Brush, new(topLeft.X + 1, topLeft.Y + 1, size.Width - 2, size.Height - 2));
                DrawOutline();
            }
        };
    }

    class EllipseDrawer : DrawingTool
    {
        public override string Name => "Ellipse Drawer";
        public override bool OnTheMove => false;

        public override DrawingOperation Draw(Color col1, Color col2, Point p1, Point p2, FillTypes fillType, float thickness) => graphics =>
        {
            Point topLeft = GetTopLeftCorner(p1, p2);
            Size size = GetShapeSize(p1, p2);

            void DrawOutline()
            {
                using Pen pen = new(col1, thickness);
                graphics.DrawEllipse(pen, new Rectangle(topLeft, size));
            }

            if (fillType == FillTypes.Outline)
            {
                DrawOutline();
            }
            else if (fillType == FillTypes.Fill)
            {
                using Pen pen = new(col1);
                graphics.FillEllipse(pen.Brush, new Rectangle(topLeft, size));
            }
            else
            {
                using Pen pen = new(col2);
                graphics.FillEllipse(pen.Brush, new(topLeft.X + 1, topLeft.Y + 1, size.Width - 2, size.Height - 2));
                DrawOutline();
            }
        };
    }

    class LineDrawer : DrawingTool
    {
        public override string Name => "Line Drawer";
        public override bool OnTheMove => false;

        public override DrawingOperation Draw(Color col1, Color col2, Point p1, Point p2, FillTypes fillType, float thickness) => graphics =>
        {
            using Pen pen = new(col1, thickness);
            graphics.DrawLine(pen, p1, p2);
        };
    }

    class PenTool : DrawingTool
    {
        public override string Name => "Pen";
        public override bool OnTheMove => true;

        public override DrawingOperation Draw(Color col1, Color col2, Point p1, Point p2, FillTypes fillType, float thickness) => graphics =>
        {
            using Pen pen = new(col1, thickness);
            graphics.FillRectangle(pen.Brush, new(p2, new(1, 1)));
        };
    }

    class BucketTool(Func<Bitmap> imageGetter) : DrawingTool
    {
        public override string Name => "Bucket";
        public override bool OnTheMove => false;
        public override bool OnMouseDown => true;
        public override bool OnMouseUp => false;
        public override bool DisablePreview => true;

        private Func<Bitmap> GetImage { get; } = imageGetter;
        private const int SENSITIVITY = 15;

        public override DrawingOperation Draw(Color col1, Color col2, Point p1, Point p2, FillTypes fillType, float thickness)
        {
            Bitmap image = GetImage();
            List<Point> finalMap = [];

            // set the target color (the one to replace with newer color) to the current pixel (mouse position)
            if (GetPixel(p1) is not { } targetCol)
                return _ => { };

            // create a stack to hold various pixels, and push the current pixel (mouse position) to the stack
            Stack<Point> pixelsToCheck = [];
            List<Point> visitedPixels = [];
            pixelsToCheck.Push(p2);

            while (pixelsToCheck.Count >= 1)
            {
                Point px = pixelsToCheck.Pop();

                if (visitedPixels.Contains(px))
                    continue;
                visitedPixels.Add(px);

                // stack - based impl(slower):
                {
                    //// validate bounds
                    //if (px.X < 0 || px.X >= image.Width || px.Y < 0 || px.Y >= image.Height)
                    //    continue;

                    //if (GetPixel(px) is { } pxCol && pxCol.GetDifference(targetCol) <= SENSITIVITY)
                    //{
                    //    finalMap.Add(px);
                    //    (int, int)[] dirs = [(1, 0), (0, 1), (-1, 0), (0, -1)];
                    //    foreach (var (xx, yy) in dirs)
                    //        pixelsToCheck.Push(new(px.X + xx, px.Y + yy));
                    //}
                }

                // scan-line fill:
                {
                    int y1 = px.Y;
                    while (y1 >= 0 && GetPixel(new(px.X, y1)) == targetCol)
                    {
                        y1--;
                    }
                    y1++;
                    bool spanLeft = false;
                    bool spanRight = false;
                    while (y1 < image.Height && GetPixel(new(px.X, y1)) == targetCol)
                    {
                        finalMap.Add(new(px.X, y1));

                        if (!spanLeft && px.X > 0 && GetPixel(new(px.X - 1, y1)) == targetCol)
                        {
                            pixelsToCheck.Push(new Point(px.X - 1, y1));
                            spanLeft = true;
                        }
                        else if (spanLeft && px.X - 1 == 0 && GetPixel(new(px.X - 1, y1)) != targetCol)
                        {
                            spanLeft = false;
                        }
                        if (!spanRight && px.X < image.Width - 1 && GetPixel(new(px.X + 1, y1)) == targetCol)
                        {
                            pixelsToCheck.Push(new(px.X + 1, y1));
                            spanRight = true;
                        }
                        else if (spanRight && px.X < image.Width - 1 && GetPixel(new(px.X + 1, y1)) != targetCol)
                        {
                            spanRight = false;
                        }
                        y1++;
                    }
                }
            }


            return graphics =>
            {
                using Pen pen = new(col1);
                foreach (Point px in finalMap)
                    graphics.FillRectangle(pen.Brush, new(px.X, px.Y, 1, 1));
            };

            Color? GetPixel(Point px)
            {
                if (px.X < 0 || px.X >= image.Width || px.Y < 0 || px.Y >= image.Height)
                    return null;

                try
                {
                    return image.GetPixel(px.X, px.Y);
                }
                catch (Exception ex)
                {
                    _ = 0;
                    return null;
                }
            }
        }
    }
}