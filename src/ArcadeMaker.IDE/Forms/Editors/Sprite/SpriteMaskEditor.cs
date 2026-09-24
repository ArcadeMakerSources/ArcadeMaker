using ArcadeMaker.IDE.Items;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ArcadeMaker.IDE
{
    public partial class SpriteMaskEditor : Form
    {
        private SpriteMask[] masks;
        private SpriteMask Mask
        {
            get => masks[Sprite.separateMask ? PreviewImageIndex : 0];
        }

        private readonly GameSprite Sprite = null;
        private int top, left, right, bottom;
        private new int Top
        {
            get => top;
            set
            {
                if (value >= 0 && value < Mask.height)
                {
                    top = value;
                    boundingTopBox.Value = value;
                    previewBox.Invalidate();
                }
            }
        }
        private new int Left
        {
            get => left;
            set
            {
                if (value < Mask.width && value >= 0)
                {
                    left = value;
                    boundingLeftBox.Value = value;
                    previewBox.Invalidate();
                }
            }
        }
        private new int Right
        {
            get => right;
            set
            {
                if (value >= 0 && value < Mask.width)
                {
                    right = value;
                    boundingRightBox.Value = value;
                    previewBox.Invalidate();
                }
            }
        }
        private new int Bottom
        {
            get => bottom;
            set
            {
                if (value >= 0 && value < Mask.height)
                {
                    bottom = value;
                    boundingBottomBox.Value = value;
                    previewBox.Invalidate();
                }
            }
        }

        private void boundingFullOpt_CheckedChanged(object sender, EventArgs e)
        {
            if (boundingFullOpt.Checked)
            {
                Top = 0;
                Right = Mask.width - 1;
                Bottom = Mask.height - 1;
                Left = 0;
            }
        }

        private bool manualMode = false;
        private bool ManualMode
        {
            get => manualMode;
            set
            {
                manualMode = value;
                boundingLeftBox.Enabled = manualMode;
                boundingRightBox.Enabled = manualMode;
                boundingTopBox.Enabled = manualMode;
                boundingBottomBox.Enabled = manualMode;
            }
        }

        private void boundingManualOpt_CheckedChanged(object sender, EventArgs e)
        {
            ManualMode = boundingManualOpt.Checked;
        }

        private bool mouseDown = false;
        private Point mouseDownPos = new Point(0, 0);
        private void previewBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                mouseDown = true;
                mouseDownPos = e.Location;
            }
        }

        private void previewBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (mouseDown && e.Button == MouseButtons.Left)
                mouseDown = false;
        }

        private void previewBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (ManualMode && mouseDown)
            {
                Top = e.Location.Y >= mouseDownPos.Y ? mouseDownPos.Y : e.Location.Y;
                Right = e.Location.X >= mouseDownPos.X ? e.Location.X : mouseDownPos.X;
                Bottom = e.Location.Y >= mouseDownPos.Y ? e.Location.Y : mouseDownPos.Y;
                Left = e.Location.X >= mouseDownPos.X ? mouseDownPos.X : e.Location.X;
            }
        }

        private readonly Pen HighlightPen = new Pen(Color.FromArgb(70, 0, 0, 0));
        private void previewBox_Paint(object sender, PaintEventArgs e)
        {
            if (!ShowMaskPreview)
                return;

            int x = 0, y = 0;
            for (int i = 0; i < Mask.length; i++)
            {
                if (Mask[i] && x >= Left && x <= Right && y >= Top && y <= Bottom)
                    e.Graphics.DrawRectangle(HighlightPen, x, y, 1, 1);

                if (++x >= Mask.width)
                {
                    x = 0;
                    y++;
                }
            }
        }

        private bool OKClose = false;
        private void okBtn_Click(object sender, EventArgs e)
        {
            SaveChanges(true);
            OKClose = true;
            Close();
        }

        private void SaveChanges(bool dispose)
        {
            if (boundingAutoOpt.Checked)
                SetBoundingAutomatic();
            if (boundingFullOpt.Checked)
                boundingFullOpt_CheckedChanged(this, EventArgs.Empty);

            Sprite.maskTop = Top;
            Sprite.maskRight = Right;
            Sprite.maskBottom = Bottom;
            Sprite.maskLeft = Left;
            Sprite.separateMask = separateMasksBox.Checked;
            Sprite.maskAlphaTolerance = AlphaTolerance;
            Sprite.maskBounding_auto = boundingAutoOpt.Checked;
            Sprite.maskBounding_fullImage = boundingFullOpt.Checked;
            Sprite.maskBounding_manual = boundingManualOpt.Checked;

            if (dispose && masks != null)
                foreach (var mask in masks)
                    mask.Dispose();
        }

        private int PreviewImageIndex = 0;
        private void indexNextBtn_Click(object sender, EventArgs e)
        {
            if (++PreviewImageIndex >= Sprite.images.Count)
                PreviewImageIndex = 0;
            PreviewIndexMove();
        }

        private void indexPrevBtn_Click(object sender, EventArgs e)
        {
            if (--PreviewImageIndex < 0)
                PreviewImageIndex = Sprite.images.Count - 1;
            PreviewIndexMove();
        }

        private void PreviewIndexMove()
        {
            previewBox.Image = Sprite.images[PreviewImageIndex];
            imageIndexLbl.Text = PreviewImageIndex.ToString();
            indexNextBtn.Visible = PreviewImageIndex < Sprite.images.Count - 1;
            indexPrevBtn.Visible = PreviewImageIndex > 0;
        }

        private bool ShowMaskPreview = true;
        private void showCollisionMaskBox_CheckedChanged(object sender, EventArgs e)
        {
            ShowMaskPreview = showCollisionMaskBox.Checked;
            previewBox.Invalidate();
        }

        private bool SkipBoundingBoxValueChangedEvent = false;
        private void boundingLeftBox_ValueChanged(object sender, EventArgs e)
        {
            if (!SkipBoundingBoxValueChangedEvent)
            {
                SkipBoundingBoxValueChangedEvent = true;
                Left = (int)boundingLeftBox.Value;
                SkipBoundingBoxValueChangedEvent = false;
            }
        }

        private void boundingRightBox_ValueChanged(object sender, EventArgs e)
        {
            if (!SkipBoundingBoxValueChangedEvent)
            {
                SkipBoundingBoxValueChangedEvent = true;
                Right = (int)boundingRightBox.Value;
                SkipBoundingBoxValueChangedEvent = false;
            }
        }

        private void boundingTopBox_ValueChanged(object sender, EventArgs e)
        {
            if (!SkipBoundingBoxValueChangedEvent)
            {
                SkipBoundingBoxValueChangedEvent = true;
                Top = (int)boundingTopBox.Value;
                SkipBoundingBoxValueChangedEvent = false;
            }
        }

        private void boundingBottomBox_ValueChanged(object sender, EventArgs e)
        {
            if (!SkipBoundingBoxValueChangedEvent)
            {
                SkipBoundingBoxValueChangedEvent = true;
                Bottom = (int)boundingBottomBox.Value;
                SkipBoundingBoxValueChangedEvent = false;
            }
        }

        private int alphaTolerance = 0;
        private int AlphaTolerance
        {
            get => alphaTolerance;
            set
            {
                alphaTolerance = value;
                alphaToleranceBar.Value = alphaTolerance;
                alphaToleranceBox.Value = alphaTolerance;
                SetMasks();
                previewBox.Invalidate();
            }
        }

        private bool SkipAlphaToleranceEditorSet = false;
        private void alphaToleranceBar_Scroll(object sender, EventArgs e)
        {
            if (!SkipAlphaToleranceEditorSet)
            {
                SkipAlphaToleranceEditorSet = true;
                AlphaTolerance = alphaToleranceBar.Value;
                SkipAlphaToleranceEditorSet = false;
            }
        }

        private void alphaToleranceBox_ValueChanged(object sender, EventArgs e)
        {
            if (!SkipAlphaToleranceEditorSet)
            {
                SkipAlphaToleranceEditorSet = true;
                AlphaTolerance = (int)alphaToleranceBox.Value;
                SkipAlphaToleranceEditorSet = false;
            }
        }

        public SpriteMaskEditor(GameSprite sprite)
        {
            InitializeComponent();

            this.Sprite = sprite;

            SetMasks();

            boundingLeftBox.Maximum = Mask.width - 1;
            boundingRightBox.Maximum = Mask.width - 1;
            boundingTopBox.Maximum = Mask.height - 1;
            boundingBottomBox.Maximum = Mask.height - 1;

            if (sprite.maskBounding_manual)
            {
                this.Top = sprite.maskTop;
                this.Right = sprite.maskRight;
                this.Left = sprite.maskLeft;
                this.Bottom = sprite.maskBottom;
            }
            else if (sprite.maskBounding_fullImage)
                boundingFullOpt_CheckedChanged(this, EventArgs.Empty);
            else if (sprite.maskBounding_auto)
                SetBoundingAutomatic();

            boundingAutoOpt.Checked = Sprite.maskBounding_auto;
            boundingFullOpt.Checked = Sprite.maskBounding_fullImage;
            boundingManualOpt.Checked = Sprite.maskBounding_manual;

            Text += ": " + Sprite.name;

            detailsLbl.Text = string.Format("Width: {0}   Height: {1}\n\nNumber of subimages: {2}\n\nShow:", Mask.width, Mask.height, Sprite.images.Count);

            indexNextBtn.Visible = Sprite.images.Count > 1;

            SkipSeparateMasksBoxCheckedChangedEvent = true;
            separateMasksBox.Checked = Sprite.separateMask;
            SkipSeparateMasksBoxCheckedChangedEvent = false;

            Bitmap bg = new Bitmap(32, 32);
            {
                using (Graphics g = Graphics.FromImage(bg))
                {
                    g.FillRectangle(Brushes.White, 0, 0, 15, 15);
                    g.FillRectangle(Brushes.White, 15, 15, 15, 15);
                    g.FillRectangle(Brushes.LightGray, 15, 0, 15, 15);
                    g.FillRectangle(Brushes.LightGray, 0, 15, 15, 15);
                }
                previewBox.BackgroundImage = bg;
            }
            previewBox.Image = sprite.image;

            FormClosing += (s, e) =>
            {
                if (!OKClose)
                {
                    var result = MessageBox.Show("Warning", "Save the changes to the mask of sprite: " + Sprite.name + "?", MessageBoxButtons.YesNoCancel);
                    if (result == DialogResult.Yes)
                    {
                        SaveChanges(dispose: true);
                    }
                    else if (result == DialogResult.Cancel)
                    {
                        e.Cancel = true;
                    }
                }
            };
        }

        private bool SkipSeparateMasksBoxCheckedChangedEvent = false;
        private void separateMasksBox_CheckedChanged(object sender, EventArgs e)
        {
            if (!SkipSeparateMasksBoxCheckedChangedEvent)
            {
                Sprite.separateMask = separateMasksBox.Checked;
                SetMasks();
                previewBox.Invalidate();
            }
        }

        private void SpriteMaskEditor_Load(object sender, EventArgs e)
        {

        }

        private void SetMasks()
        {
            if (masks != null)
            {
                foreach (var mask in masks)
                    mask.Dispose();
            }

            if (Sprite.separateMask)
            {
                masks = new SpriteMask[Sprite.images.Count];
                for (int i = 0; i < Sprite.images.Count; i++)
                    masks[i] = new SpriteMask(Sprite.preciseMask, AlphaTolerance, Sprite.images[i]);
            }
            else
            {
                masks = new SpriteMask[1];
                masks[0] = new SpriteMask(Sprite.preciseMask, AlphaTolerance, Sprite.images.ToArray());
            }
        }

        private void boundingAutoOpt_CheckedChanged(object sender, EventArgs e)
        {
            if (boundingAutoOpt.Checked)
            {
                SetBoundingAutomatic();
            }
        }

        private void SetBoundingAutomatic()
        {
            SpriteMask.CalculateAutoBounding(Sprite, out int t, out int r, out int b, out int l);
            Top = t;
            Right = r;
            Bottom = b;
            Left = l;
        }
    }

    public class SpriteMask : IDisposable
    {
        public readonly int width, height;
        private readonly List<bool> points = new List<bool>();
        public int length
        {
            get => points.Count;
        }

        public bool this[int index]
        {
            get => points[index];
        }

        public SpriteMask(bool precise, int alphaTolerance, params Bitmap[] images)
        {
            this.width = images[0].Width;
            this.height = images[0].Height;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (precise)
                    {
                        bool found = false;
                        foreach (Bitmap image in images)
                        {
                            if (image.GetPixel(x, y).A > alphaTolerance)
                            {
                                found = true;
                                break;
                            }
                        }
                        points.Add(found);
                    }
                    else
                    {
                        points.Add(true);
                    }
                }
            }
        }

        public static void CalculateAutoBounding(GameSprite sprite, out int top, out int right, out int bottom, out int left)
        {
            Bitmap image = sprite.images[0];

            // find max top, left, right, down points of the mask
            // use new mask, so that this would work even when precise mask is false
            using (SpriteMask Mask = new SpriteMask(true, sprite.maskAlphaTolerance, sprite.images.ToArray()))
            {
                int maxTop = Mask.height - 1;
                int maxLeft = Mask.width - 1;
                int maxBottom = 0;
                int maxRight = 0;

                int x = 0, y = 0;
                for (int i = 0; i < Mask.length; i++)
                {
                    if (Mask[i])
                    {
                        if (y < maxTop)
                            maxTop = y;
                        if (x > maxRight)
                            maxRight = x;
                        if (y > maxBottom)
                            maxBottom = y;
                        if (x < maxLeft)
                            maxLeft = x;
                    }

                    if (++x >= Mask.width)
                    {
                        x = 0;
                        y++;
                    }
                }

                top = maxTop;
                right = maxRight;
                bottom = maxBottom;
                left = maxLeft;
            }
        }

        public bool IsDisposed { get; private set; } = false;
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            points.Clear();
            IsDisposed = true;
        }
    }
}