using ArcadeMaker.IDE.Items;
using Microsoft.VisualBasic;
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
    public partial class SpriteManager : Form
    {
        private ImageList images = new ImageList();
        private GameSprite sprite;

        public SpriteManager(GameSprite sprite)
        {
            InitializeComponent();
            this.sprite = sprite;
            if (sprite.images.Any())
                images.ImageSize = sprite.images[0].Size;
            foreach (Bitmap bitmap in sprite.images)
            {
                AddImage(bitmap);
            }
            headImage = sprite.image;
        }

        private void SpriteManager_Load(object sender, EventArgs e)
        {
            imageListView.LargeImageList = images;
        }

        private void newSpriteBtn_Click(object sender, EventArgs e)
        {
            Form selectSizeFrm = null;
            NumericUpDown widthBox = new NumericUpDown { Value = 32, Location = new Point(5, 5), Maximum = 2000 };
            NumericUpDown heightBox = new NumericUpDown { Value = 32, Location = new Point(widthBox.Location.X + widthBox.Size.Width + 5, 5), Maximum = 2000 };
            Button okBtn = new Button { Text = "OK", Location = new Point(5, 30) };
            Button cancelBtn = new Button { Text = "Cancel", Location = new Point(okBtn.Location.X + okBtn.Size.Width + 5, 30) };
            okBtn.Click += (s, ea) => {
                selectSizeFrm.Close();
                AddImage(new Bitmap((int)widthBox.Value, (int)heightBox.Value));
            };
            cancelBtn.Click += (s, ea) => selectSizeFrm.Close();
            selectSizeFrm = new Form { Controls = { widthBox, heightBox, okBtn, cancelBtn }, Text = "Sprite Size" };
            selectSizeFrm.AcceptButton = okBtn;
            selectSizeFrm.StartPosition = FormStartPosition.CenterScreen;
            selectSizeFrm.ShowDialog();
        }

        private void AddImage(Bitmap image)
        {
            if (images.Images.Count == 0)
                images.ImageSize = image.Size;
            images.Images.Add(image);
            ListViewItem item = new ListViewItem { Text = "Image " + images.Images.Count };
            item.ImageIndex = images.Images.Count - 1;
            item.BackColor = Color.Gray;
            item.Tag = image;
            imageListView.Items.Add(item);

            saveSpriteBtn.Enabled = true;
        }

        private void importSpriteBtn_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                foreach (string path in openFileDialog.FileNames)
                {
                    Bitmap image = (Bitmap)Global.ImageFromFile(path);
                    AddImage(image);
                }
            }
        }

        private void imageListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            EnableMoveButtons(imageListView.SelectedItems.Count == 1);
        }

        private void EnableMoveButtons(bool enabled = true)
        {
            moveLeftBtn.Enabled = enabled;
            moveRightBtn.Enabled = enabled;
        }

        private void imageListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (imageListView.SelectedItems.Count > 0)
            {
                Bitmap bmp = imageListView.SelectedItems[0].Tag as Bitmap;
                int index = imageListView.SelectedIndices[0];

                SpriteDesigner designer = new SpriteDesigner(bmp);
                designer.Finished += (s, ea) =>
                {
                    images.Images[index] = designer.image;
                    imageListView.Refresh();
                };
                designer.ShowDialog();
            }
        }

        private Bitmap headImage = null;
        public event EventHandler<Bitmap> HeadImageChanged;

        private void okBtn_Click(object sender, EventArgs e)
        {
            sprite.images.Clear();
            foreach (Bitmap image in images.Images)
            {
                sprite.images.Add(image);
            }
            if (images.Images.Count > 0 && headImage != (Bitmap)images.Images[0])
            {
                headImage = (Bitmap)images.Images[0];
                var handler = HeadImageChanged;
                handler?.Invoke(this, headImage);
            }
            Close();
        }

        private void moveLeftBtn_Click(object sender, EventArgs e)
        {
            ImageMove(right: false);
        }

        private void moveRightBtn_Click(object sender, EventArgs e)
        {
            ImageMove(right: true);
        }

        private void ImageMove(bool right)
        {
            int direction = -1;
            if (right)
                direction = 1;

            int sind = imageListView.SelectedIndices[0];
            if (sind == (right ? images.Images.Count - 1 : 0))
                return;
            Bitmap memory = (Bitmap)images.Images[sind + direction];
            images.Images[sind + direction] = images.Images[sind];
            images.Images[sind] = memory;
            imageListView.Refresh();
            imageListView.Items[sind + direction].Selected = true;
        }

        private void imageListView_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && imageListView.SelectedIndices.Count > 0 && images.Images.Count > 0)
            {
                images.Images.RemoveAt(imageListView.SelectedIndices[0]);
            }
        }
    }
}