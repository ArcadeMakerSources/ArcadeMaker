using ArcadeMaker.IDE;
using ArcadeMaker.IDE.Items;
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
    public partial class PathEditor : Form
    {
        private GamePath path = null;
        private GameRoom roomView = null;
        private int scrollX = 0, scrollY = 0;
        public PathEditor(GamePath path)
        {
            InitializeComponent();
            panel.SetDoubleBuffered(true);
            this.path = path;
            path.NameChanged += (s, e) =>
            {
                if (!renaming)
                    nameBox.Text = e.newName;
            };
        }

        private int titleBarHeight = 20;

        private void PathEditor_Load(object sender, EventArgs e)
        {
            nameBox.Text = path.name;
            roomBox.Items.Add("None");
            roomBox.Items.AddRange(Environment.project.items.OfType<GameRoom>().ToArray());
            roomBox.SelectedIndex = 0;
            closeCheckBox.Checked = path.close;

            for (int i = 0; i < path.points.Count; i++)
            {
                AddPointToPanel(path.points[i], i);
            }

            Rectangle screenRectangle = RectangleToScreen(this.ClientRectangle);
            titleBarHeight = screenRectangle.Top - Top;
        }


        private bool renaming = false;
        private void nameBox_TextChanged(object sender, EventArgs e)
        {
            renaming = true;
            try
            {
                path.name = nameBox.Text;
                if (nameBox.BackColor == Color.Red)
                    nameBox.BackColor = Color.White;
            }
            catch
            {
                nameBox.BackColor = Color.Red;
            }
            renaming = false;
        }

        private void roomBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            object item = roomBox.SelectedItem;
            if (item is GameRoom)
            {
                roomView = item as GameRoom;
                if (roomView.drawBackColor)
                    panel.BackColor = roomView.backColor;
            }
            else
            {
                roomView = null;
                panel.BackColor = default;
                panel.BackgroundImage = null;
            }
        }

        private Pen pen = new Pen(Color.Red, 2);
        private void panel_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.TranslateTransform(scrollX, scrollY);

            // draw room
            if (roomBox.SelectedItem is GameRoom room)
            {
                // draw without snap
                room.editor?.boardPanel_Paint(/*drawSnap:*/false, e/*, panel.Size + panel.Size*/); // this will paint on our panel because we pass e
            }

            // draw snap
            using (Pen pen = new Pen(Color.Black))
            {
                for (int x = -scrollX, snapX = (int)snapXBox.Value; snapX > 6 && x < -scrollX + panel.Width; x += snapX)
                    e.Graphics.DrawLine(pen, x, -scrollY, x, -scrollY + panel.Height);
                for (int y = -scrollY, snapY = (int)snapYBox.Value; snapY > 6 && y < -scrollY + panel.Height; y += snapY)
                    e.Graphics.DrawLine(pen, -scrollX, y, -scrollX + panel.Width, y);
            }

            // draw path
            if (path.points.Count < 2)
                return;

            for (int p = 0; p < path.points.Count; p++)
            {
                if (path.points.Count > p + 1)
                {
                    e.Graphics.DrawLine(pen, path.points[p].x, path.points[p].y, path.points[p + 1].x, path.points[p + 1].y);
                }
                else
                {
                    if (path.close)
                        e.Graphics.DrawLine(pen, path.points[p].x, path.points[p].y, path.points[0].x, path.points[0].y);
                }
            }
        }

        private void closeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            path.close = closeCheckBox.Checked;
            panel.Invalidate();
        }

        private void panel_MouseClick(object sender, MouseEventArgs e)
        {
            bool add = true;
            /*
            foreach (Control control in panel.Controls)
            {
                if (e.Location.X >= control.Location.X && e.Location.X <= control.Location.X + control.Size.Width &&
                    e.Location.Y >= control.Location.Y && e.Location.Y <= control.Location.Y + control.Size.Height)*
                        add = false;
            }
            */
            if (add)
            {
                var pt = new PathPoint(new Point((int)(e.Location.X / snapXBox.Value) * (int)snapXBox.Value,
                                                 (int)(e.Location.Y / snapYBox.Value) * (int)snapYBox.Value) - new Size(scrollX, scrollY));
                path.points.Add(pt);
                int index = path.points.Count - 1;
                AddPointToPanel(pt, index);
            }
        }

        private void AddPointToPanel(PathPoint pt, int index)
        {
            int btnSize = 10;
            Button button = new Button { Location = pt - new Size(btnSize / 2, btnSize / 2) + new Size(scrollX, scrollY), Size = new Size(btnSize, btnSize), Cursor = Cursors.Arrow, Tag = pt, BackColor = Color.Green };
            button.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                    holdedBtn = button;
            };
            button.MouseMove += (s, e) =>
            {
                if (e.Button == MouseButtons.Left && holdedBtn == button)
                {
                    Point loc = panel.PointToClient(Cursor.Position);
                    loc = new Point((int)(loc.X / snapXBox.Value) * (int)snapXBox.Value,
                                    (int)(loc.Y / snapYBox.Value) * (int)snapYBox.Value);
                    button.Location = loc;
                }
            };
            button.MouseUp += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    if (holdedBtn == button)
                    {
                        holdedBtn = null;
                    }
                    pt.x = button.Location.X - scrollX;
                    pt.y = button.Location.Y - scrollY;
                    panel.Invalidate();
                }
            };
            panel.Controls.Add(button);
            panel.Invalidate();

            pointsListBox.Items.Add(pt);
        }

        private void okBtn_Click(object sender, EventArgs e)
        {
            Close();
        }

        private Button holdedBtn = null;

        private void deleteBtn_Click(object sender, EventArgs e)
        {
            if (pointsListBox.SelectedIndex >= 0) {
                PathPoint point = pointsListBox.SelectedItem as PathPoint;
                Button btn = null;
                foreach (var ctrl in panel.Controls.OfType<Button>())
                {
                    if (ctrl.Tag == point)
                    {
                        btn = ctrl;
                        break;
                    }
                }
                path.points.Remove(point);
                pointsListBox.Items.Remove(point);
                if (pointsListBox.Items.Count > 0)
                    pointsListBox.SelectedIndex = pointsListBox.Items.Count - 1;
                panel.Controls.Remove(btn);
                btn.Dispose();
                panel.Invalidate();
            }
        }

        private bool skipPointBoxesValueChanged = false;
        private void pointXBox_ValueChanged(object sender, EventArgs e)
        {
            if (!skipPointBoxesValueChanged && pointsListBox.SelectedItem is PathPoint point)
                point.x = (int)(sender as NumericUpDown).Value;
        }

        private void pointYBox_ValueChanged(object sender, EventArgs e)
        {
            if (!skipPointBoxesValueChanged && pointsListBox.SelectedItem is PathPoint point)
                point.y = (int)(sender as NumericUpDown).Value;
        }

        private void pointSpeedBox_ValueChanged(object sender, EventArgs e)
        {
            if (!skipPointBoxesValueChanged && pointsListBox.SelectedItem is PathPoint point)
                point.speed = (int)(sender as NumericUpDown).Value;
        }

        private void pointsListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (pointsListBox.SelectedItem is PathPoint point)
            {
                skipPointBoxesValueChanged = true;
                pointXBox.Value = point.x;
                pointYBox.Value = point.y;
                pointSpeedBox.Value = point.speed;
                skipPointBoxesValueChanged = false;
            }
        }

        private const int scrollBtnPower = 170;
        private void scrollLeftBtn_Click(object sender, EventArgs e)
        {
            scrollX += scrollBtnPower;
            UpdateScroll();
        }

        private void scrollRightBtn_Click(object sender, EventArgs e)
        {
            scrollX -= scrollBtnPower;
            UpdateScroll();
        }

        private void scrollUpBtn_Click(object sender, EventArgs e)
        {
            scrollY += scrollBtnPower;
            UpdateScroll();
        }

        private void scrollDownBtn_Click(object sender, EventArgs e)
        {
            scrollY -= scrollBtnPower;
            UpdateScroll();
        }

        private void UpdateScroll()
        {
            foreach (Button btn in panel.Controls.OfType<Button>())
            {
                if (btn.Tag is PathPoint pt)
                {
                    btn.Location = new Point(pt.x, pt.y) + new Size(scrollX, scrollY);
                }
            }
            panel.Invalidate();
            UpdatePanelDataText();
        }

        private Point lastPanelDataMouseLoc = Point.Empty;
        private void UpdatePanelDataText(Point? mouseLoc = null)
        {
            if (!mouseLoc.HasValue)
                mouseLoc = lastPanelDataMouseLoc;
            else
                lastPanelDataMouseLoc = mouseLoc.Value;
            panelDataLbl.Text = $"x: {mouseLoc.Value.X}   y: {mouseLoc.Value.Y}   Area: ({-scrollX}, {-scrollY}) -> ({-scrollX + panel.Size.Width}, {-scrollY + panel.Size.Height})";
        }

        private void snapXBox_ValueChanged(object sender, EventArgs e)
        {
            panel.Invalidate();
        }

        private void snapYBox_ValueChanged(object sender, EventArgs e)
        {
            panel.Invalidate();
        }

        private void panel_MouseMove(object sender, MouseEventArgs e)
        {
            Point loc = new Point((int)(e.Location.X / snapXBox.Value) * (int)snapXBox.Value,
                                  (int)(e.Location.Y / snapYBox.Value) * (int)snapYBox.Value) - new Size(scrollX, scrollY);
            UpdatePanelDataText(loc);
        }

        private void centerBtn_Click(object sender, EventArgs e)
        {
            if (!path.points.Any())
            {
                return;
            }

            // find edges points
            int lowestX = path.points[0].x, lowestY = path.points[0].y, highestX = path.points[0].x, highestY = path.points[0].y;
            foreach (var point in path.points)
            {
                if (point.x < lowestX)
                    lowestX = point.x;
                if (point.x > highestX)
                    highestX = point.x;
                if (point.y < lowestY)
                    lowestY = point.y;
                if (point.y > highestY)
                    highestY = point.y;
            }
            
            // find center point
            Point center = new Point((lowestX + highestX) / 2, (lowestY + highestY) / 2);

            // move scroll to center point
            scrollX = center.X - (panel.DisplayRectangle.Width / 2);
            scrollY = center.Y - (panel.DisplayRectangle.Height / 2);

            UpdateScroll();
        }

        private void addBtn_Click(object sender, EventArgs e)
        {
            
        }
    }
}
