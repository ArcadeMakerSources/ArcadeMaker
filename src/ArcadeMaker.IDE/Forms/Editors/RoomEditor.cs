using ArcadeMaker.IDE.Items;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ArcadeMaker.IDE
{
    public partial class RoomEditor : Form
    {
        private GameRoom room = null;
        private GameObject selectedObj = null;
        private static Bitmap noSpriteIcon = null;
        public RoomEditor(GameRoom room)
        {
            InitializeComponent();
            bgListBox.DrawMode = DrawMode.OwnerDrawVariable;
            bgListBox.DrawMode = DrawMode.OwnerDrawVariable;
            boardPanel.SetDoubleBuffered(true);
            this.room = room;
            room.NameChanged += (s, e) =>
            {
                if (!renaming)
                    nameBox.Text = e.newName;
            };

            noSpriteIcon = Global.NoSpriteIcon;
        }

        private Size GetBoardPanelMaxSize()
        {
            int width = Size.Width - boardPanel.Location.X - 2 * (boardPanel.Location.X - (tabControl1.Location.X + tabControl1.Size.Width));
            int height = Size.Height - boardPanel.Location.Y - 2 * (boardPanel.Location.Y - (snapxBox.Location.Y + snapxBox.Size.Height));
            return new Size(width, height);
        }

        private void RoomEditor_Load(object sender, EventArgs e)
        {
            nameBox.Text = room.name;

            // room settings
            skipSetSettings = true;
            boardPanel.Size = new Size(room.size.width, room.size.height);
            captionBox.Text = room.caption;
            widthBox.Value = boardPanel.Size.Width;
            heightBox.Value = boardPanel.Size.Height;
            roomSpeedBox.Value = room.speed;
            persistentBox.Checked = room.persistent;
            skipSetSettings = false;

            // backgrounds
            drawBgColorBox.Checked = room.drawBackColor;
            backColorBox.BackColor = room.backColor;
            bgListBox.Items.AddRange(room.backgrounds);
            if (bgListBox.Items.Count > 0)
            {
                bgListBox.SelectedIndex = 0;
                LoadSelectedBackground();
            }
            backColorBox.BackColor = room.backColor;
            boardPanel.BackColor = room.backColor;

            // views
            useViewsBox.Checked = room.viewsEnabled;
            viewsList.Items.AddRange(room.views.ToArray());
            if (viewsList.Items.Count > 0)
            {
                viewsList.SelectedIndex = 0;
                LoadSelectedView();
            }

            objDetailsLbl.Text = "";
        }
        private bool renaming = false;
        private void nameBox_TextChanged(object sender, EventArgs e)
        {
            renaming = true;
            try
            {
                room.name = nameBox.Text;
                if (nameBox.BackColor == Color.Red)
                    nameBox.BackColor = Color.White;
            }
            catch
            {
                nameBox.BackColor = Color.Red;
            }
            renaming = false;
        }

        private bool skipSetSettings = false;
        private void widthBox_ValueChanged(object sender, EventArgs e)
        {
            if (!skipSetSettings)
            {
                room.size.width = (int)widthBox.Value;
                boardPanel.Size = room.size.ToFormSize();
            }
        }

        private void heightBox_ValueChanged(object sender, EventArgs e)
        {
            if (!skipSetSettings)
            {
                room.size.height = (int)heightBox.Value;
                boardPanel.Size = room.size.ToFormSize();
            }
        }

        private int objId = 1000;
        private void boardPanel_MouseClick(object sender, MouseEventArgs e)
        {
            Point position = GetSnappedPosition(e.Location);

            if (e.Button == MouseButtons.Left)
            {
                if (movingObj != null)
                {
                    movingObj = null;
                    return;
                }
                else if (selectedObj == null)
                    MessageBox.Show("Select an object to add");
                else
                {
                    RoomObject ro = new RoomObject($"R{room.index}INST{objId++}", position.X, position.Y, selectedObj);

                    if (deleteUnderlyingBox.Checked)
                    {
                        var underlyings = GetRoomObjectsByPosition(position); // at underlying removing, remove by a snapped position (unless <Alt> is pressed)
                        foreach (RoomObject obj in underlyings)
                        {
                            room.objects.Remove(obj);
                        }
                    }

                    room.objects.Add(ro);
                    boardPanel.Invalidate();
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                RoomObject obj = GetRoomObjectByPosition(e.Location); // at normal removing, remove by the exact location
                if (obj != null)
                {
                    if (ModifierKeys == Keys.Control)
                    {
                        ContextMenuStrip menu = new();
                        ToolStripMenuItem editCreationCodeBtn = new("Creation Code...");
                        editCreationCodeBtn.Click += (s, ea) =>
                        {
                            ScriptEditor editor = new ScriptEditor(obj, obj.Script);
                            editor.OKClicked += (ss, ee) => obj.Script = ee;
                            editor.ShowDialog();
                        };
                        menu.Items.Add(editCreationCodeBtn);
                        menu.Show(boardPanel, e.Location);
                    }
                    else
                    {
                        room.objects.Remove(obj);
                        boardPanel.Invalidate();
                    }
                }
            }
        }

        private void okBtn_Click(object sender, EventArgs e)
        {
            Close();
        }

        private RoomObject previewObj = null;
        public void boardPanel_Paint(object sender, PaintEventArgs e)
        {
            boardPanel_Paint(sender, e, null);
        }
        public void boardPanel_Paint(object sender, PaintEventArgs e, Size? panelSize)
        {
            if (panelSize == null || !panelSize.HasValue)
                panelSize = room.size.ToFormSize();

            int snapX = (int)snapxBox.Value;
            int snapY = (int)snapyBox.Value;

            // draw non-foreground backgrounds
            DrawBackground(e, foregrounds: false, panelSize.Value);

            // draw objects
            RoomObject[] objsToDraw = new RoomObject[room.objects.Count + (previewObj == null ? 0 : 1)];
            room.objects.CopyTo(objsToDraw, 0);
            if (previewObj != null)
                objsToDraw[objsToDraw.Length - 1] = previewObj;
            foreach (RoomObject ro in objsToDraw)
            {
                bool hasSprite = ro.obj.sprite != null;
                Bitmap? image = hasSprite ? ro.obj.sprite!.image : noSpriteIcon;
                if (image != null)
                {
                    if (image.HorizontalResolution != e.Graphics.DpiX || image.VerticalResolution != e.Graphics.DpiY)
                    {
                        image.SetResolution(e.Graphics.DpiX, e.Graphics.DpiY);
                    }
                    e.Graphics.DrawImage(image, new Point(ro.x - (hasSprite ? ro.obj.sprite.originX : 0), ro.y - (hasSprite ? ro.obj.sprite.originY : 0)));
                }
            }

            // draw foreground backgrounds
            DrawBackground(e, foregrounds: true, panelSize.Value);

            if ((sender == null || !(sender is bool)) || (sender is bool drawSnap && drawSnap))
            {
                // draw snap
                using (Pen pen = new Pen(Color.Black))
                {
                    for (int w = 0; snapX > 6 && w < boardPanel.Size.Width; w += snapX)
                    {
                        e.Graphics.DrawLine(pen, w, 0, w, boardPanel.Size.Height);
                    }
                    for (int h = 0; snapY > 6 && h < boardPanel.Size.Height; h += snapY)
                    {
                        e.Graphics.DrawLine(pen, 0, h, boardPanel.Size.Width, h);
                    }
                }
            }
        }

        private void DrawBackground(PaintEventArgs e, bool foregrounds, Size panelSize)
        {
            foreach (RoomBackground background in room.backgrounds)
            {
                if (background.visible && background.foreground == foregrounds && background.image != null && background.image.image != null)
                {
                    Bitmap bitmap = background.image.image;
                    bool dispose = false;
                    if (background.stretch && (background.image.image.Size.Width != room.size.width || background.image.image.Size.Height != room.size.height))
                    {
                        bitmap = bitmap.ResizeImage(room.size.width, room.size.height);
                        dispose = true;
                    }
                    try
                    {
                        int x = background.x, y = background.y, width = bitmap.Size.Width, height = bitmap.Size.Height;
                        int stX = x, stY = y;
                        if (background.tileHor)
                        {
                            while (x > 0)
                                x -= width;
                            while (x < -width)
                                x += width;
                        }
                        if (background.tileVer)
                        {
                            while (y > 0)
                                y -= height;
                            while (y < -height)
                                y += height;
                        }
                        stX = x;
                        stY = y;
                        width = panelSize.Width;
                        height = panelSize.Height;
                        for (; x < (background.tileHor ? width : stX + 1); x += bitmap.Size.Width)
                        {
                            for (y = stY; y < (background.tileVer ? height : stY + 1); y += bitmap.Size.Height)
                            {
                                e.Graphics.DrawImage(bitmap, x, y);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
#if DEBUG
                        MessageBox.Show("[Debug Mode]\nError occoured when drawing background:\n\n" + ex.ToString());
#endif
                    }
                    finally
                    {
                        if (dispose)
                        {
                            try
                            {
                                bitmap.Dispose();
                            }
                            catch { }
                        }
                    }
                }
            }
        }

        private void snapxBox_ValueChanged(object sender, EventArgs e)
        {
            if (snapxBox.Value > 7)
                boardPanel.Invalidate();
        }

        private void snapyBox_ValueChanged(object sender, EventArgs e)
        {
            if (snapyBox.Value > 7)
                boardPanel.Invalidate();
        }

        private void captionBox_TextChanged(object sender, EventArgs e)
        {
            if (!skipSetSettings)
                room.caption = captionBox.Text;
        }

        private void creationCodeBtn_Click(object sender, EventArgs e)
        {
            ScriptEditor editor = new ScriptEditor(room, room.Script);
            editor.OKClicked += (s, script) =>
            {
                room.Script = script;
            };
            editor.ShowDialog();
        }

        private void backColorBox_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (colorPicker.ShowDialog() == DialogResult.OK)
                {
                    room.backColor = colorPicker.Color;
                    boardPanel.BackColor = room.backColor;
                    backColorBox.BackColor = room.backColor;
                }
            }
        }

        private void bgImageBox_SelectionChanged(object sender, GameBackground e)
        {
            if (!skipSetBackground)
            {
                RoomBackground selectedBg = bgListBox.SelectedItem as RoomBackground;
                if (bgImageBox.Resource == null)
                {
                    selectedBg.image = null;
                }
                else
                {
                    bgVisibleBox.Checked = true;
                    selectedBg.image = bgImageBox.Resource;
                }
                boardPanel.Invalidate();
            }
        }

        private void useViewsBox_CheckedChanged(object sender, EventArgs e)
        {
            room.viewsEnabled = useViewsBox.Checked;
        }

        private void viewsList_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadSelectedView();
        }

        private void LoadSelectedView()
        {
            if (viewsList.SelectedItem is RoomView view)
                LoadView(view);
        }

        private bool skipSetView = false;
        private void LoadView(RoomView view)
        {
            skipSetView = true;
            viewVisibleBox.Checked = view.Visible;
            viewXBox.Value = view.X;
            viewYBox.Value = view.Y;
            viewWBox.Value = view.Width;
            viewHBox.Value = view.Height;
            viewPortXBox.Value = view.PortX;
            viewPortYBox.Value = view.PortY;
            viewPortWBox.Value = view.PortWidth;
            viewPortHBox.Value = view.PortHeight;
            viewObjectFollowingBox.Resource = view.ObjFollow;
            viewFollowHBorBox.Value = view.FollowHBor;
            viewFollowVBorBox.Value = view.FollowVBor;
            viewFollowHSpBox.Value = view.FollowHSp;
            viewFollowVSpBox.Value = view.FollowVSp;
            skipSetView = false;
        }

        private void LoadSelectedBackground()
        {
            if (bgListBox.SelectedItem != null && bgListBox.SelectedItem is RoomBackground bg)
                LoadBackground(bg);
        }

        private void LoadBackground(RoomBackground background)
        {
            skipSetBackground = true;
            bgVisibleBox.Checked = background.visible;
            bgForegroundBox.Checked = background.foreground;
            bgImageBox.Resource = background.image;
            bgTileHorBox.Checked = background.tileHor;
            bgTileVerBox.Checked = background.tileVer;
            bgXBox.Value = background.x;
            bgYBox.Value = background.y;
            bgStretchBox.Checked = background.stretch;
            bgHorSpdBox.Value = background.horSpd;
            bgVertSpdBox.Value = background.verSpd;
            skipSetBackground = false;
        }

        private void viewXBox_ValueChanged(object sender, EventArgs e)
        {
            if (!skipSetView)
                room.views[viewsList.SelectedIndex].X = (int)viewXBox.Value;
        }

        private void viewYBox_ValueChanged(object sender, EventArgs e)
        {
            if (!skipSetView)
                room.views[viewsList.SelectedIndex].Y = (int)viewYBox.Value;
        }

        private void viewWBox_ValueChanged(object sender, EventArgs e)
        {
            if (!skipSetView)
                room.views[viewsList.SelectedIndex].Width = (int)viewWBox.Value;
        }

        private void viewHBox_ValueChanged(object sender, EventArgs e)
        {
            if (!skipSetView)
                room.views[viewsList.SelectedIndex].Height = (int)viewHBox.Value;
        }

        private void viewVisibleBox_CheckedChanged(object sender, EventArgs e)
        {
            if (!skipSetView)
                room.views[viewsList.SelectedIndex].Visible = viewVisibleBox.Checked;
        }

        private int lastX = 0, lastY = 0;

        private void viewPortXBox_ValueChanged(object sender, EventArgs e)
        {
            if (!skipSetView)
                room.views[viewsList.SelectedIndex].PortX = (int)viewPortXBox.Value;
        }

        private void viewPortYBox_ValueChanged(object sender, EventArgs e)
        {
            if (!skipSetView)
                room.views[viewsList.SelectedIndex].PortY = (int)viewPortYBox.Value;
        }

        private void viewPortWBox_ValueChanged(object sender, EventArgs e)
        {
            if (!skipSetView)
                room.views[viewsList.SelectedIndex].PortWidth = (int)viewPortWBox.Value;
        }

        private void viewPortHBox_ValueChanged(object sender, EventArgs e)
        {
            if (!skipSetView)
                room.views[viewsList.SelectedIndex].PortHeight = (int)viewPortHBox.Value;
        }

        private void viewObjectFollowingBox_SelectionChanged(object sender, GameObject e)
        {
            if (!skipSetView)
                room.views[viewsList.SelectedIndex].ObjFollow = e;
        }

        private void viewFollowHBorBox_ValueChanged(object sender, EventArgs e)
        {
            if (!skipSetView)
                room.views[viewsList.SelectedIndex].FollowHBor = (int)viewFollowHBorBox.Value;
        }

        private void viewFollowVBorBox_ValueChanged(object sender, EventArgs e)
        {
            if (!skipSetView)
                room.views[viewsList.SelectedIndex].FollowVBor = (int)viewFollowVBorBox.Value;
        }

        private void viewFollowHSpBox_ValueChanged(object sender, EventArgs e)
        {
            if (!skipSetView)
                room.views[viewsList.SelectedIndex].FollowHSp = (int)viewFollowHSpBox.Value;
        }

        private void viewFollowVSpBox_ValueChanged(object sender, EventArgs e)
        {
            if (!skipSetView)
                room.views[viewsList.SelectedIndex].FollowVSp = (int)viewFollowVSpBox.Value;
        }

        private bool skipSetBackground = false;
        private void drawBgColorBox_CheckedChanged(object sender, EventArgs e)
        {
            if (!skipSetBackground)
            {
                room.drawBackColor = true;
                boardPanel.Invalidate();
            }
        }

        private void bgListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadSelectedBackground();
        }

        private RoomBackground SelectedBackground
        {
            get
            {
                return bgListBox.SelectedItem as RoomBackground;
            }
        }

        private void bgVisibleBox_CheckedChanged(object sender, EventArgs e)
        {
            if (!skipSetBackground)
            {
                SelectedBackground.visible = bgVisibleBox.Checked;
                bgListBox.Invalidate();
                boardPanel.Invalidate();
            }
        }

        private void bgForegroundBox_CheckedChanged(object sender, EventArgs e)
        {
            if (!skipSetBackground)
            {
                SelectedBackground.foreground = bgForegroundBox.Checked;
                bgListBox.Invalidate();
                boardPanel.Invalidate();
            }
        }

        private void bgTileHorBox_CheckedChanged(object sender, EventArgs e)
        {
            if (!skipSetBackground)
            {
                SelectedBackground.tileHor = bgTileHorBox.Checked;
                boardPanel.Invalidate();
            }
        }

        private void bgTileVerBox_CheckedChanged(object sender, EventArgs e)
        {
            if (!skipSetBackground)
            {
                SelectedBackground.tileVer = bgTileVerBox.Checked;
                boardPanel.Invalidate();
            }
        }

        private void bgXBox_ValueChanged(object sender, EventArgs e)
        {
            if (!skipSetBackground)
            {
                SelectedBackground.x = (int)bgXBox.Value;
                boardPanel.Invalidate();
            }
        }

        private void bgYBox_ValueChanged(object sender, EventArgs e)
        {
            if (!skipSetBackground)
            {
                SelectedBackground.y = (int)bgYBox.Value;
                boardPanel.Invalidate();
            }
        }

        private void bgStretchBox_CheckedChanged(object sender, EventArgs e)
        {
            if (!skipSetBackground)
            {
                SelectedBackground.stretch = bgStretchBox.Checked;
                boardPanel.Invalidate();
            }
        }

        private void bgHorSpdBox_ValueChanged(object sender, EventArgs e)
        {
            if (!skipSetBackground)
            {
                SelectedBackground.horSpd = (int)bgHorSpdBox.Value;
            }
        }

        private void bgVertSpdBox_ValueChanged(object sender, EventArgs e)
        {
            if (!skipSetBackground)
            {
                SelectedBackground.verSpd = (int)bgVertSpdBox.Value;
            }
        }

        private void bgListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            Font f = e.Font;
            if (room.backgrounds[e.Index].visible)
                f = new Font(e.Font, FontStyle.Bold);
            Brush brush = new SolidBrush(room.backgrounds[e.Index].foreground ? Color.Red : e.ForeColor);

            e.DrawBackground();
            e.Graphics.DrawString(((ListBox)(sender)).Items[e.Index].ToString(), f, brush, e.Bounds);
            e.DrawFocusRectangle();
        }

        private void objAddSelectPanel_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                objAddSelectBox.Menu?.Show(objAddSelectPanel, e.Location);
        }

        private void objAddSelectBox_SelectionChanged(object sender, GameObject e)
        {
            selectedObj = e;
            objAddSelectPanel.Image = e != null && e.sprite != null && e.sprite.images.Count > 0 ? e.sprite.image : null;
        }

        private RoomObject movingObj = null;
        private void boardPanel_MouseMove(object sender, MouseEventArgs e)
        {
            Point position = GetSnappedPosition(e.Location);
            RoomObject obj = GetRoomObjectByPosition(position);

            // details label
            string title = "Object:";
            objDetailsLbl.Text = obj == null ? title : title + " " + obj.obj.name;

            // delete & add
            if (e.Button == MouseButtons.Right || (e.Button == MouseButtons.Left && ModifierKeys == Keys.Shift && obj == null))
            {
                boardPanel_MouseClick(this, e);
            }

            // move
            else if (e.Button == MouseButtons.Left)
            {
                if (ModifierKeys == Keys.Control)
                {
                    if (movingObj == null)
                    {
                        if (obj != null)
                            movingObj = obj;
                    }
                    else
                    {
                        movingObj.x = position.X;
                        movingObj.y = position.Y;
                        boardPanel.Invalidate();
                    }
                }
                else
                {
                    if (previewObj == null)
                    {
                        if (selectedObj != null)
                            previewObj = new RoomObject("PREVIEW", position.X, position.Y, selectedObj);
                    }
                    else
                    {
                        previewObj.x = position.X;
                        previewObj.y = position.Y;
                    }
                    boardPanel.Invalidate();
                }
            }
            else
            {
                previewObj = null;
            }
        }

        private Point GetSnappedPosition(Point position)
        {
            int locx = ModifierKeys == Keys.Alt ? position.X : ((int)(position.X / snapxBox.Value) * (int)snapxBox.Value);
            int locy = ModifierKeys == Keys.Alt ? position.Y : ((int)(position.Y / snapyBox.Value) * (int)snapyBox.Value);
            return new Point(locx, locy);
        }

        private RoomObject GetRoomObjectByPosition(Point position)
        {
            return GetRoomObjectsByPosition(position, lastOnly: true).FirstOrDefault();
        }

        private void RoomEditor_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.Alt)
                e.IsInputKey = true;
        }

        private void roomSpeedBox_ValueChanged(object sender, EventArgs e)
        {
            if (!skipSetSettings)
                room.speed = (int)roomSpeedBox.Value;
        }

        private void persistentBox_CheckedChanged(object sender, EventArgs e)
        {
            if (!skipSetSettings)
                room.persistent = persistentBox.Checked;
        }

        private void resetAllCreationCodesBtn_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to reset creation code for all instances in this room?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                foreach (var roomInst in room.objects)
                {
                    roomInst.Script = null;
                }
            }
        }

        private RoomObject[] GetRoomObjectsByPosition(Point position, bool lastOnly = false)
        {
            Point snappedPos = GetSnappedPosition(position);
            int locx = snappedPos.X;
            int locy = snappedPos.Y;

            if (locx != lastX || locy != lastY)
            {
                lastX = locx;
                lastY = locy;

                mouseLocLbl.Text = "X: " + locx + "   Y: " + locy;
            }

            List<RoomObject> objs = new List<RoomObject>();

            // show details about the LAST RoomObject match in the list
            for (int i = room.objects.Count - 1; i >= 0; i--)
            {
                try
                {
                    int objX = room.objects[i].x, objY = room.objects[i].y;

                    int objImgW = room.objects[i].obj.sprite == null ? noSpriteIcon.Size.Width : (room.objects[i].obj.sprite.image == null ? 0 : room.objects[i].obj.sprite.image.Size.Width);
                    int objImgH = room.objects[i].obj.sprite == null ? noSpriteIcon.Size.Height : (room.objects[i].obj.sprite.image == null ? 0 : room.objects[i].obj.sprite.image.Size.Height);
                    if (position.X >= objX && position.X < objX + objImgW && position.Y >= objY && position.Y < objY + objImgH)
                    {
                        objs.Add(room.objects[i]);
                        if (lastOnly)
                            break;
                    }
                }
                catch { }
            }

            return objs.ToArray();
        }
    }
}
