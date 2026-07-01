using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ArcadeMaker.IDE
{
    public partial class SpriteDesigner : Form
    {
        private const bool ZOOM_IN = true, ZOOM_OUT = false;

        public Bitmap image = null;
        private Pen pen1 = new Pen(Color.Black), pen2 = new Pen(Color.White);

        private float penWidth
        {
            get
            {
                return pen1.Width;
            }
            set
            {
                pen1.Width = value;
                pen2.Width = value;
            }
        }

        private Color color1
        {
            get
            {
                return pen1.Color;
            }
            set
            {
                pen1.Color = value;
            }
        }
        private Color color2
        {
            get
            {
                return pen2.Color;
            }
            set
            {
                pen2.Color = value;
            }
        }

        public SpriteDesigner(Bitmap? image = null)
        {
            InitializeComponent();

            // default image
            image ??= new(100, 100);

            this.image = image;
            zoomWidth = image.Width;
            zoomHeight = image.Height;
            imageBox.DrawTransparentBackground();
            imageBox.SizeChanged += (s, e) => imageBox.Invalidate();
            imageBox.Paint += (s, e) =>
            {
                if (showPixelBorder && imageBox.Width >= 256 && imageBox.Height >= 256)
                {
                    Graphics g = e.Graphics;
                    Pen pen = new Pen(Color.DarkBlue);
                    float rectSize = (float)imageBox.Width / image.Width;
                    for (float x = (float)poffsetBox.Value; x <= imageBox.Width; x += rectSize)
                    {
                        g.DrawLine(pen, x, 0, x, imageBox.Height);
                    }
                    for (float y = (float)poffsetBox.Value; y <= imageBox.Height; y += (float)imageBox.Height / image.Height)
                    {
                        g.DrawLine(pen, 0, y, imageBox.Width, y);
                    }
                }
            };
            toleranceNumericBox.Maximum = toleranceBox.Maximum;
            colorPicker.Color1Changed += (s, e) =>
            {
                color1 = e;
            };
            colorPicker.Color2Changed += (s, e) =>
            {
                color2 = e;
            };

            // color tool panel
            shape = _shape;
            drawStyle = _drawStyle;
        }

        private void SpriteDesigner_Load(object sender, EventArgs e)
        {
            imageBox.Size = image.Size;
            imageBox.Image = image;
        }

        private PointF mouseDownLoc = PointF.Empty;
        private PointF previewLoc = PointF.Empty;

        private void imageBox_MouseDown(object sender, MouseEventArgs e)
        {
            mouseDownLoc = ImageLocation(e.Location);
            previewLoc = e.Location;
        }

        private void imageBox_Click(object sender, EventArgs e)
        {

        }

        private Shape _shape = Shape.Pen;
        private Shape shape
        {
            get
            {
                return _shape;
            }
            set
            {
                _shape = value;

                // set group boxes visible
                widthGroupBox.Visible = value == Shape.Pen || value == Shape.Line || value == Shape.Ellipse || value == Shape.Rect;
                drawFillGroupBox.Visible = value == Shape.Ellipse || value == Shape.Rect;
                toleranceGroupBox.Visible = value == Shape.Fill;

                // color tool's button
                Button btn = null;
                switch (value)
                {
                    case Shape.Pen:
                        btn = penBtn;
                        break;
                    case Shape.Line:
                        btn = lineBtn;
                        break;
                    case Shape.ColorPicker:
                        btn = pickColBtn;
                        break;
                    case Shape.Ellipse:
                        btn = ellipseBtn;
                        break;
                    case Shape.Rect:
                        btn = rectBtn;
                        break;
                    case Shape.Fill:
                        btn = fillBtn;
                        break;
                }
                foreach (Control ctrl in toolBox.Controls)
                {
                    if (ctrl != btn)
                        ctrl.BackColor = Color.Transparent;
                    else
                        ctrl.BackColor = Color.LightBlue;
                }
            }
        }

        private DrawStyle _drawStyle = DrawStyle.Draw;
        private DrawStyle drawStyle
        {
            get
            {
                return _drawStyle;
            }
            set
            {
                _drawStyle = value;

                // color button
                Button btn = null;
                switch (value)
                {
                    case DrawStyle.Draw:
                        btn = shapeDrawBtn;
                        break;
                    case DrawStyle.Fill:
                        btn = shapeFillBtn;
                        break;
                    case DrawStyle.DrawAndFill:
                        btn = shapeDrawFillBtn;
                        break;
                }
                foreach (Control ctrl in drawFillGroupBox.Controls)
                {
                    if (ctrl != btn)
                        ctrl.BackColor = Color.Transparent;
                    else
                        ctrl.BackColor = Color.LightBlue;
                }
            }
        }

        private void imageBox_MouseUp(object sender, MouseEventArgs e)
        {
            using (Graphics graphics = Graphics.FromImage(image))
            {
                Pen pen = pen1;
                if (e.Button == MouseButtons.Right)
                    pen = pen2;

                PointF mouseUpLoc = ImageLocation(e.Location);
                if (shape == Shape.Pen)
                {
                    return;
                }
                else if (shape == Shape.ColorPicker)
                {
                    pen.Color = image.GetPixel((int)mouseUpLoc.X, (int)mouseUpLoc.Y);
                    if (pen == pen1)
                        colorPicker.Color1 = pen.Color;
                    else if (pen == pen2)
                        colorPicker.Color2 = pen.Color;
                    return;
                }
                else
                    Draw(graphics, pen, mouseDownLoc, mouseUpLoc);
            }
            imageBox.Image = DisplayImage();
            imageBox.Invalidate();
        }

        private void zoomOutBtn_Click(object sender, EventArgs e)
        {
            /*
            if (imageBox.Size.Width <= image.Size.Width || imageBox.Size.Height <= imageBox.Size.Height)
                imageBox.Size = new Size(imageBox.Size.Width - 10, imageBox.Size.Height - 10);
            else
                imageBox.Size -= image.Size;
            */
            Zoom(ZOOM_OUT);
        }

        private void zoomInBtn_Click(object sender, EventArgs e)
        {
            //imageBox.Size += image.Size;
            Zoom(ZOOM_IN);
        }

        private static readonly int zoomRatio = 32;
        private void Zoom(bool inOut)
        {
            // set the zoomed width and height
            int widthZoom = imageBox.Width * zoomRatio / 100;
            int heightZoom = imageBox.Height * zoomRatio / 100;

            // inOut = true --> zoom in
            // inOut = false --> zoom out
            if (!inOut)
            {
                widthZoom *= -1;
                heightZoom *= -1;
            }

            // add the width and height to the picture box dimensions
            zoomWidth += widthZoom;
            zoomHeight += heightZoom;

            Image prevImg = imageBox.Image;
            imageBox.Image = DisplayImage();
            if (prevImg != image)
            {
                prevImg.Dispose(); // very important
            }
            imageBox.Size = new Size(imageBox.Image.Width - 6, imageBox.Image.Height - 6);
        }

        private bool showPixelBorder = true;
        private int zoomWidth, zoomHeight;
        private Bitmap DisplayImage(Bitmap image = null)
        {
            if (image == null)
                image = this.image;
            if (image.Size.Equals(new Size(zoomWidth, zoomHeight)))
                return image;
            else
            {
                // Create a new bitmap to hold the zoomed image
                Bitmap zoomedImage = new Bitmap(zoomWidth, zoomHeight);

                // Get a Graphics object for the new bitmap
                using (Graphics g = Graphics.FromImage(zoomedImage))
                {
                    // Set the interpolation mode to HighQualityBicubic
                    g.InterpolationMode = InterpolationMode.NearestNeighbor;

                    // Draw the original image onto the new bitmap, scaled to the new size
                    g.DrawImage(image, new RectangleF(0, 0, zoomedImage.Width, zoomedImage.Height));
                }

                return zoomedImage;
            }
        }

        private void penBtn_Click(object sender, EventArgs e)
        {
            shape = Shape.Pen;
        }

        private void lineBtn_Click(object sender, EventArgs e)
        {
            shape = Shape.Line;
        }

        private PointF previewLastEndLoc = Point.Empty;
        private void imageBox_MouseMove(object sender, MouseEventArgs e)
        {
            Pen pen = null;
            if (e.Button == MouseButtons.Left)
                pen = pen1;
            else if (e.Button == MouseButtons.Right)
                pen = pen2;
            else
                return;

            // preview & pen
            PointF mouseCurrentLoc = ImageLocation(e.Location);
            if (mouseDownLoc != Point.Empty && mouseCurrentLoc != previewLastEndLoc)
            {
                previewLastEndLoc = mouseCurrentLoc;
                if (shape == Shape.Pen)
                {
                    //penPath.Add(ImageLocation(e.Location));
                    if (mouseCurrentLoc.X < this.image.Width && mouseCurrentLoc.X >= 0 && mouseCurrentLoc.Y < this.image.Height && mouseCurrentLoc.Y >= 0)
                    {
                        this.image.SetPixel((int)Math.Round(mouseCurrentLoc.X), (int)Math.Round(mouseCurrentLoc.Y), pen.Color);
                        imageBox.Image = DisplayImage();
                    }
                    return;
                }
                else if (shape == Shape.Line || shape == Shape.Rect || shape == Shape.Ellipse)
                {
                    imageBox.Image.Dispose();
                    try
                    {
                        Bitmap image = new Bitmap(this.image.Width, this.image.Height);

                        using (Graphics g = Graphics.FromImage(image))
                        {
                            g.DrawImage(this.image, 0, 0);

                            Draw(g, pen, mouseDownLoc, mouseCurrentLoc);
                        }
                        imageBox.Image = DisplayImage(image);
                    }
                    catch (Exception ex)
                    {
                        _ = ex;
                    }
                }
            }
        }

        private void Draw(Graphics g, Pen pen, PointF start, PointF end)
        {
            //start.X = 0;
            //start.Y = 0;
            //end.X -= pen.Width / 2;
            //end.Y -= pen.Width / 2;
            float startX = start.X, startY = start.Y, endX = end.X, endY = end.Y;
            if (end.X < startX)
            {
                startX = end.X;
                endX = start.X;
            }
            if (end.Y < startY)
            {
                startY = end.Y;
                endY = start.Y;
            }
            if (shape == Shape.Line)
            {
                g.DrawLine(pen, start, end);
            }
            else if (shape == Shape.Rect)
            {
                if (drawStyle == DrawStyle.Draw)
                    g.DrawRectangle(pen, startX, startY, endX - startX, endY - startY);
                else if (drawStyle == DrawStyle.Fill)
                    g.FillRectangle(pen.Brush, startX, startY, endX - startX, endY - startY);
                else if (drawStyle == DrawStyle.DrawAndFill)
                {
                    g.DrawRectangle(pen, startX, startY, endX - startX, endY - startY);
                    g.FillRectangle((pen == pen1 ? pen2 : pen1).Brush, startX + 1, startY + 1, endX - startX - 1, endY - startY - 1);
                }
            }
            else if (shape == Shape.Ellipse)
            {
                if (drawStyle == DrawStyle.Draw)
                    g.DrawEllipse(pen, startX, startY, endX - startX, endY - startY);
                else if (drawStyle == DrawStyle.Fill)
                    g.FillEllipse(pen.Brush, startX, startY, endX - startX, endY - startY);
                else if (drawStyle == DrawStyle.DrawAndFill)
                {
                    g.DrawEllipse(pen, startX, startY, endX - startX, endY - startY);
                    float dif = 0.5F;
                    g.FillEllipse((pen == pen1 ? pen2 : pen1).Brush, startX + dif, startY + dif, endX - startX - dif, endY - startY - dif);
                }
            }
            else if (shape == Shape.Fill)
            {
                Fill(pen, Point.Round(start));
            }
        }

        private readonly bool ver2 = false;
        private PointF ImageLocation(Point loc)
        {
            if (ver2)
                return loc;
            /*
            loc.X = (int)Global.Map(loc.X, 0, imageBox.Width, 0, image.Width);
            loc.Y = (int)Global.Map(loc.Y, 0, imageBox.Height, 0, image.Height);
            */
            PointF floc = new PointF((float)loc.X / imageBox.Width * image.Width, (float)loc.Y / imageBox.Height * image.Height);
            return floc;
        }

        private void fillBtn_Click(object sender, EventArgs e)
        {
            shape = Shape.Fill;
        }

        private int fillTolerance = 30;
        private void Fill(Pen pen, Point loc)
        {
            try
            {
                image = ImageSegmentation.FillSegment(image, loc.X, loc.Y, pen.Color, fillTolerance);
            }
            catch (Exception ex)
            {
                string err = "Error filling the area.";
#if DEBUG
                err += "\n\n[Debug Mode]\nError Message:\n" + ex;
#endif
                MessageBox.Show(err);
            }
        }


        private void shapeDrawBtn_Click(object sender, EventArgs e)
        {
            drawStyle = DrawStyle.Draw;
        }

        private void shapeDrawFillBtn_Click(object sender, EventArgs e)
        {
            drawStyle = DrawStyle.DrawAndFill;
        }

        private void shapeFillBtn_Click(object sender, EventArgs e)
        {
            drawStyle = DrawStyle.Fill;
        }

        private void ellipseBtn_Click(object sender, EventArgs e)
        {
            shape = Shape.Ellipse;
        }

        private void pickColBtn_Click(object sender, EventArgs e)
        {
            shape = Shape.ColorPicker;
        }

        private void width1Btn_Click(object sender, EventArgs e)
        {
            penWidth = 1;
        }

        private void width2Btn_Click(object sender, EventArgs e)
        {
            penWidth = 2;
        }

        private void width3Btn_Click(object sender, EventArgs e)
        {
            penWidth = 3;
        }

        private void width4Btn_Click(object sender, EventArgs e)
        {
            penWidth = 4;
        }

        private void width5Btn_Click(object sender, EventArgs e)
        {
            penWidth = 5;
        }

        private void width6Btn_Click(object sender, EventArgs e)
        {
            penWidth = 6;
        }

        private void toleranceBox_Scroll(object sender, EventArgs e)
        {
            skipToleranceBoxSync = true;
            fillTolerance = toleranceBox.Value;
            toleranceNumericBox.Value = toleranceBox.Value;
            skipToleranceBoxSync = false;
        }

        private bool skipToleranceBoxSync = false;
        private void toleranceNumericBox_ValueChanged(object sender, EventArgs e)
        {
            if (!skipToleranceBoxSync)
                toleranceBox.Value = (int)toleranceNumericBox.Value;
        }

        private void showGridBtn_Click(object sender, EventArgs e)
        {
            showPixelBorder = !showPixelBorder;
            imageBox.Invalidate();
        }

        private void poffsetBox_ValueChanged(object sender, EventArgs e)
        {
            imageBox.Invalidate();
        }

        public event EventHandler<EventArgs> Finished;

        private void okBtn_Click(object sender, EventArgs e)
        {
            var handler = Finished;
            handler?.Invoke(this, new EventArgs());
            Close();
        }

        private void rectBtn_Click(object sender, EventArgs e)
        {
            shape = Shape.Rect;
        }
    }

    enum Shape
    {
        Line,
        Rect,
        Pen,
        Fill,
        Ellipse,
        ColorPicker
    }

    enum DrawStyle
    {
        Draw,
        Fill,
        DrawAndFill
    }

    public static class ImageSegmentation
    {
        // d0 (color seed range) range: 0 to 441.67 (ChatGPT)
        public static Bitmap FillSegment(Bitmap image, int x, int y, Color fill_color, int d0)
        {
            int w = image.Width;
            int h = image.Height;

            // create a bool array to mark which pixels have been visited
            bool[,] visited = new bool[w, h];

            // get the segmentation of the image
            byte[,] segment = GetSegment(image, x, y, visited, d0);

            // create a new bitmap for the result
            Bitmap result = new Bitmap(w, h);

            // iterate over the pixels of the image
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    // if the pixel is part of the segment, set the color to the fill color
                    if (segment[i, j] == 1)
                    {
                        result.SetPixel(i, j, fill_color);
                    }
                    // otherwise, copy the color from the original image
                    else
                    {
                        result.SetPixel(i, j, image.GetPixel(i, j));
                    }
                }
            }

            return result;
        }

        private static byte[,] GetSegment(Bitmap image, int x, int y, bool[,] visited, int d0)
        {
            int w = image.Width;
            int h = image.Height;

            byte[,] segment = new byte[w, h];

            // get the color of the seed pixel
            Color seed_color = image.GetPixel(x, y);

            // create a queue for BFS
            Queue<Point> queue = new Queue<Point>();

            // mark the seed pixel as visited and add it to the queue
            visited[x, y] = true;
            queue.Enqueue(new Point(x, y));

            // iterate over the queue
            while (queue.Count > 0)
            {
                // get the next pixel from the queue
                Point current = queue.Dequeue();
                int current_x = current.X;
                int current_y = current.Y;

                // mark the current pixel as part of the segment
                segment[current_x, current_y] = 1;

                // iterate over the neighbors of the current pixel
                for (int i = current_x - 1; i <= current_x + 1; i++)
                {
                    for (int j = current_y - 1; j <= current_y + 1; j++)
                    {
                        // check if the neighbor is inside the image bounds
                        if (i >= 0 && i < w && j >= 0 && j < h)
                        {
                            // check if the neighbor is not visited and has the color within the specified range
                            if (!visited[i, j] && ColorWithinRange(image.GetPixel(i, j), seed_color, d0))
                            {
                                // mark the neighbor as visited and add it to the queue
                                visited[i, j] = true;
                                queue.Enqueue(new Point(i, j));
                            }
                        }
                    }
                }
            }

            return segment;
        }

        // Helper function to check if a given color is within the specified range of another color
        private static bool ColorWithinRange(Color c1, Color c2, int d0)
        {
            int r_diff = Math.Abs(c1.R - c2.R);
            int g_diff = Math.Abs(c1.G - c2.G);
            int b_diff = Math.Abs(c1.B - c2.B);
            return r_diff <= d0 && g_diff <= d0 && b_diff <= d0;
        }

        /*
        // good code start -----------------------------------------------------------------------------------------------------------------------------------------
        public static Bitmap FillSegment(Bitmap image, int x, int y, Color fill_color)
        {
            int w = image.Width;
            int h = image.Height;

            // create a bool array to mark which pixels have been visited
            bool[,] visited = new bool[w, h];

            // get the segmentation of the image
            byte[,] segment = GetSegment(image, x, y, visited);

            // create a new bitmap for the result
            Bitmap result = new Bitmap(w, h);

            // iterate over the pixels of the image
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    // if the pixel is part of the segment, set the color to the fill color
                    if (segment[i, j] == 1)
                    {
                        result.SetPixel(i, j, fill_color);
                    }
                    // otherwise, copy the color from the original image
                    else
                    {
                        result.SetPixel(i, j, image.GetPixel(i, j));
                    }
                }
            }

            return result;
        }

        private static byte[,] GetSegment(Bitmap image, int x, int y, bool[,] visited)
        {
            int w = image.Width;
            int h = image.Height;

            byte[,] segment = new byte[w, h];

            // get the color of the seed pixel
            Color seed_color = image.GetPixel(x, y);

            // create a queue for BFS
            Queue<Point> queue = new Queue<Point>();

            // mark the seed pixel as visited and add it to the queue
            visited[x, y] = true;
            queue.Enqueue(new Point(x, y));

            // iterate over the queue
            while (queue.Count > 0)
            {
                // get the next pixel from the queue
                Point current = queue.Dequeue();
                int current_x = current.X;
                int current_y = current.Y;

                // mark the current pixel as part of the segment
                segment[current_x, current_y] = 1;

                // iterate over the neighbors of the current pixel
                for (int i = current_x - 1; i <= current_x + 1; i++)
                {
                    for (int j = current_y - 1; j <= current_y + 1; j++)
                    {
                        // check if the neighbor is inside the image bounds
                        if (i >= 0 && i < w && j >= 0 && j < h)
                        {
                            // check if the neighbor is not visited and has the same color as the seed pixel
                            if (!visited[i, j] && image.GetPixel(i, j) == seed_color)
                            {
                                // mark the neighbor as visited and add it to the queue
                                visited[i, j] = true;
                                queue.Enqueue(new Point(i, j));
                            }
                        }
                    }
                }
            }

            return segment;
        }
        // good code end --------------------------------------------------------------------------------------------------------------------------------------------
        */


        /*
        public static Bitmap ImageSegment(Bitmap image, int x, int y)
        {
            int w = image.Width;
            int h = image.Height;
            BitmapData image_data = image.LockBits(
                new Rectangle(0, 0, w, h),
                ImageLockMode.ReadOnly,
                PixelFormat.Format24bppRgb);
            int bytes = image_data.Stride * image_data.Height;
            byte[] buffer = new byte[bytes];
            byte[] result = new byte[bytes];
            Marshal.Copy(image_data.Scan0, buffer, 0, bytes);
            image.UnlockBits(image_data);
            //limit the color range for segmentation
            int d0 = 30;
            int sample_position = x * 3 + y * image_data.Stride;
            for (int i = 0; i < bytes - 3; i += 3)
            {
                double euclidean = 0;
                for (int c = 0; c < 3; c++)
                {
                    euclidean += Math.Pow(buffer[i + c] - buffer[sample_position + c], 2);
                }
                euclidean = Math.Sqrt(euclidean);
                for (int c = 0; c < 3; c++)
                {
                    result[i + c] = (byte)(euclidean > d0 ? 0 : 255);
                }
            }
            Bitmap res_img = new Bitmap(w, h);
            BitmapData res_data = res_img.LockBits(
                new Rectangle(0, 0, w, h),
                ImageLockMode.WriteOnly,
                PixelFormat.Format24bppRgb);
            Marshal.Copy(result, 0, res_data.Scan0, bytes);
            res_img.UnlockBits(res_data);
            return res_img;
        }

        public static Bitmap FillImageSegment(Bitmap image, int x, int y, Color fill_color)
        {
            int w = image.Width;
            int h = image.Height;
            BitmapData image_data = image.LockBits(
                new Rectangle(0, 0, w, h),
                ImageLockMode.ReadWrite, // set the lock mode to allow writing
                PixelFormat.Format24bppRgb);
            int bytes = image_data.Stride * image_data.Height;
            byte[] buffer = new byte[bytes];
            byte[] result = new byte[bytes];
            Marshal.Copy(image_data.Scan0, buffer, 0, bytes);
            image.UnlockBits(image_data);
            //limit the color range for segmentation
            int d0 = 30;
            int sample_position = x * 3 + y * image_data.Stride;
            for (int i = 0; i < bytes - 3; i += 3)
            {
                double euclidean = 0;
                for (int c = 0; c < 3; c++)
                {
                    euclidean += Math.Pow(buffer[i + c] - buffer[sample_position + c], 2);
                }
                euclidean = Math.Sqrt(euclidean);
                for (int c = 0; c < 3; c++)
                {
                    result[i + c] = (byte)(euclidean > d0 ? 0 : 255);
                }
            }
            Bitmap res_img = new Bitmap(w, h);
            BitmapData res_data = res_img.LockBits(
                new Rectangle(0, 0, w, h),
                ImageLockMode.WriteOnly,
                PixelFormat.Format24bppRgb);
            Marshal.Copy(result, 0, res_data.Scan0, bytes);
            res_img.UnlockBits(res_data);

            // Fill the segmented region with the specified color
            for (int i = 0; i < bytes - 3; i += 3)
            {
                if (result[i] == 255 && result[i + 1] == 255 && result[i + 2] == 255)
                {
                    buffer[i] = fill_color.B;
                    buffer[i + 1] = fill_color.G;
                    buffer[i + 2] = fill_color.R;
                }
            }
            // Update the original image with the filled segmented region
            BitmapData updated_data = image.LockBits(
                new Rectangle(0, 0, w, h),
                ImageLockMode.WriteOnly,
                PixelFormat.Format24bppRgb);
            Marshal.Copy(buffer, 0, updated_data.Scan0, bytes);
            image.UnlockBits(updated_data);

            return res_img;
        }

        public static Bitmap FillImageSegmentOn(Bitmap image, int x, int y, Color fillColor)
        {
            int w = image.Width;
            int h = image.Height;
            BitmapData image_data = image.LockBits(
                new Rectangle(0, 0, w, h),
                ImageLockMode.ReadWrite,
                PixelFormat.Format24bppRgb);
            int bytes = image_data.Stride * image_data.Height;
            byte[] buffer = new byte[bytes];
            Marshal.Copy(image_data.Scan0, buffer, 0, bytes);
            image.UnlockBits(image_data);

            Queue<Point> queue = new Queue<Point>();
            queue.Enqueue(new Point(x, y));
            int sample_position = y * image_data.Stride + x * 3;
            byte[] fill_color = new byte[3] { fillColor.B, fillColor.G, fillColor.R };
            byte[] original_color = new byte[3] { buffer[sample_position + 2], buffer[sample_position + 1], buffer[sample_position] };
            while (queue.Count > 0)
            {
                Point current = queue.Dequeue();
                int position = current.Y * image_data.Stride + current.X * 3;
                if (position < 0 || position >= bytes)
                    continue;
                if (buffer[position] != original_color[2] || buffer[position + 1] != original_color[1] || buffer[position + 2] != original_color[0])
                    continue;
                buffer[position] = fill_color[2];
                buffer[position + 1] = fill_color[1];
                buffer[position + 2] = fill_color[0];

                if (current.X > 0)
                    queue.Enqueue(new Point(current.X - 1, current.Y));
                if (current.X < w - 1)
                    queue.Enqueue(new Point(current.X + 1, current.Y));
                if (current.Y > 0)
                    queue.Enqueue(new Point(current.X, current.Y - 1));
                if (current.Y < h - 1)
                    queue.Enqueue(new Point(current.X, current.Y + 1));
            }

            Bitmap filledImage = new Bitmap(w, h);
            BitmapData filledImageData = filledImage.LockBits(
                new Rectangle(0, 0, w, h),
                ImageLockMode.WriteOnly,
                PixelFormat.Format24bppRgb);
            Marshal.Copy(buffer, 0, filledImageData.Scan0, bytes);
            filledImage.UnlockBits(filledImageData);

            return filledImage;
        }

        private static bool[] IsLocSegment(Bitmap image, int x, int y)
        {
            int w = image.Width;
            int h = image.Height;
            BitmapData image_data = image.LockBits(
                new Rectangle(0, 0, w, h),
                ImageLockMode.ReadOnly,
                PixelFormat.Format24bppRgb);
            int bytes = image_data.Stride * image_data.Height;
            byte[] buffer = new byte[bytes];
            byte[] result = new byte[bytes];
            Marshal.Copy(image_data.Scan0, buffer, 0, bytes);
            image.UnlockBits(image_data);
            //limit the color range for segmentation
            int d0 = 30;
            int sample_position = x * 3 + y * image_data.Stride;
            bool[] isSegment = new bool[bytes / 3];
            for (int i = 0; i < bytes - 3; i += 3)
            {
                double euclidean = 0;
                for (int c = 0; c < 3; c++)
                {
                    euclidean += Math.Pow(buffer[i + c] - buffer[sample_position + c], 2);
                }
                euclidean = Math.Sqrt(euclidean);
                isSegment[i / 3] = euclidean <= d0;
            }
            return isSegment;
        }
        */
    }
}
