using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ArcadeMaker.IDE
{
    public partial class GSColorPicker : UserControl
    {
        private ColorDialog dlg1 = new ColorDialog(), dlg2 = new ColorDialog();
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color Color1
        {
            get
            {
                return dlg1.Color;
            }
            set
            {
                dlg1.Color = value;
                display1Pnl.BackColor = value;
                if (!skipHandle)
                    Color1Changed?.Invoke(this, value);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color Color2
        {
            get
            {
                return dlg2.Color;
            }
            set
            {
                dlg2.Color = value;
                display2Pnl.BackColor = value;
                if (!skipHandle)
                    Color2Changed?.Invoke(this, value);
            }
        }
        public event EventHandler<Color> Color1Changed;
        public event EventHandler<Color> Color2Changed;
        private bool skipHandle = false;
        public GSColorPicker()
        {
            InitializeComponent();
            selectPnl.Size = new Size(baseCols.Length * rectSize + 4, (3 + lightLevels) * rectSize + 4);
            int displaySize = selectPnl.Size.Width / 2 - 4;
            display1Pnl.Size = new Size(displaySize, displaySize);
            display1Pnl.Location = new Point(selectPnl.Location.X, selectPnl.Location.Y - display1Pnl.Height - 4);
            display2Pnl.Size = display1Pnl.Size;
            display2Pnl.Location = new Point(selectPnl.Location.X + selectPnl.Size.Width - display2Pnl.Size.Width, display1Pnl.Location.Y);
            leftLbl.Location = new Point(display1Pnl.Location.X, display1Pnl.Location.Y - leftLbl.Height - 2);
            rightLbl.Location = new Point(display2Pnl.Location.X, display2Pnl.Location.Y - rightLbl.Height - 2);
            alphaTrack.Location = new Point(alphaTrack.Location.X, selectPnl.Location.Y + selectPnl.Height + 6);
        }

        private void GSColorPicker_Load(object sender, EventArgs e)
        {
            //skipHandle = true;
            Color1 = Color.Black;
            Color2 = Color.White;
            skipHandle = false;
        }

        private Color[] baseCols = new Color[] { Color.Red, Color.Yellow, greenCol, azureCol, Color.Blue, purpleCol };
        private static readonly Color greenCol = Color.FromArgb(0, 255, 0);
        private static readonly Color azureCol = Color.FromArgb(0, 255, 255);
        private static readonly Color purpleCol = Color.FromArgb(255, 0, 255);
        private int rectSize = 10;
        private int lightLevels = 6;

        private void selectPnl_Paint(object sender, PaintEventArgs e)
        {
            Pen pen = new Pen(Color.Black);
            int width = baseCols.Length, height = lightLevels + 2;
            for (int w = 0; w < width; w++)
            {
                for (int h = 0; h < height; h++)
                {
                    pen.Color = GetColor(w * rectSize, h * rectSize);
                    e.Graphics.FillRectangle(pen.Brush, w * rectSize, h * rectSize, rectSize, rectSize);
                }
            }
            for (int w = 0; w < baseCols.Length * rectSize; w++)
            {
                pen.Color = GetColor(w, height * rectSize);
                e.Graphics.FillRectangle(pen.Brush, w, height * rectSize, rectSize, rectSize);
            }
        }

        private Color GetColor(Point loc)
        {
            Color color = Color.Empty;

            // get base color
            int baseInd = loc.X / rectSize;
            if (baseInd >= baseCols.Length)
                baseInd = baseCols.Length - 1;
            Color baseCol = baseCols[baseInd];

            if (loc.Y < rectSize)
            {
                // layout 1: base colors
                color = baseCol;
            }
            else if (loc.Y < (lightLevels + 1) * rectSize)
            {
                // layout 2: light levels
                int y = loc.Y / rectSize;
                int dif = 64;
                if (y * dif <= 255)
                {
                    if (baseCol == Color.Red)
                        color = Color.FromArgb(y * dif, 0, 0);
                    else if (baseCol == Color.Yellow)
                        color = Color.FromArgb(y * dif, y * dif, 0);
                    else if (baseCol == greenCol)
                        color = Color.FromArgb(0, y * dif, 0);
                    else if (baseCol == azureCol)
                        color = Color.FromArgb(0, y * dif, y * dif);
                    else if (baseCol == Color.Blue)
                        color = Color.FromArgb(0, 0, y * dif);
                    else if (baseCol == purpleCol)
                        color = Color.FromArgb(y * dif, 0, y * dif);
                }
                else
                {
                    y = (loc.Y / rectSize) - 3;
                    if (baseCol == Color.Red)
                        color = Color.FromArgb(255, y * dif, y * dif);
                    else if (baseCol == Color.Yellow)
                        color = Color.FromArgb(255, 255, y * dif);
                    else if (baseCol == greenCol)
                        color = Color.FromArgb(y * dif, 255, y * dif);
                    else if (baseCol == azureCol)
                        color = Color.FromArgb(y * dif, 255, 255);
                    else if (baseCol == Color.Blue)
                        color = Color.FromArgb(y * dif, y * dif, 255);
                    else if (baseCol == purpleCol)
                        color = Color.FromArgb(255, y * dif, 255);
                }
            }
            else if (loc.Y < (lightLevels + 2) * rectSize)
            {
                // layout 3: white levels
                int val = (loc.X / rectSize) * 32;
                if (val == 128)
                    val = 192;
                else if (val == 160)
                    val = 255;
                color = Color.FromArgb(val, val, val);
            }
            else
            {
                // layout 4: white power
                int val = Global.MapGPT(loc.X, 0, baseCols.Length * rectSize, 0, 255);
                color = Color.FromArgb(val, val, val);
            }
            return color;
        }

        private void selectPnl_MouseClick(object sender, MouseEventArgs e)
        {
            Color color = GetColor(e.Location);
            if (e.Button == MouseButtons.Left)
            {
                Color1 = color;
            }
            else if (e.Button == MouseButtons.Right)
            {
                Color2 = color;
            }
        }

        private void display1Pnl_Click(object sender, EventArgs e)
        {
            if (dlg1.ShowDialog() == DialogResult.OK)
            {
                if (Color1 != dlg1.Color)
                    Color1 = dlg1.Color;
            }
        }

        private void display2Pnl_Click(object sender, EventArgs e)
        {
            if (dlg2.ShowDialog() == DialogResult.OK)
            {
                if (Color2 != dlg2.Color)
                    Color2 = dlg2.Color;
            }
        }

        private void alphaTrack_Scroll(object sender, EventArgs e)
        {
            Color1 = Color.FromArgb(alphaTrack.Value, Color1.R, Color1.G, Color1.B);
            Color2 = Color.FromArgb(alphaTrack.Value, Color2.R, Color2.G, Color2.B);
        }

        private Color GetColor(int x, int y)
        {
            return GetColor(new Point(x, y));
        }
    }
}
