using ArcadeMaker.IDE;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ArcadeMaker.IDE.Items
{
    public class GameSprite : GameItem, ISetsIcon
    {
        public static Bitmap Icon => Properties.Resources.sprite;

        public readonly HandlerList<Bitmap> images = new HandlerList<Bitmap>();

        public void Import(string[] pathes, ResXResourceReader? resourceReader = null)
        {
            images.Clear();
            int i = 0;
            foreach (string path in pathes)
            {
                Bitmap spriteSheet = null!;
                if (resourceReader == null)
                {
                    using (Stream reader = new FileStream(path, FileMode.Open, FileAccess.Read))
                        spriteSheet = (Bitmap)Bitmap.FromStream(reader);
                }
                else
                {
                    var resDictionary = resourceReader?.GetEnumerator();
                    while (resDictionary!.MoveNext())
                    {
                        if (path.Equals(resDictionary.Key))
                        {
                            using MemoryStream imageStream = new(Convert.FromBase64String((string)resDictionary.Value!));
                            spriteSheet = (Bitmap)Bitmap.FromStream(imageStream);
                            break;
                        }
                    }
                    if (spriteSheet == null)
                        throw new Exception($"Subimage {i} of sprite {name} was not found in the path {path}.");
                }
                
                if (Global.ImageFileIsSpriteStrip(path, out int count))
                {
                    int width = spriteSheet.Width / count;
                    bool dispose = true;
                    try
                    {
                        int frameCount = spriteSheet.Width / width; // Assuming frames are stacked horizontally

                        for (i = 0; i < frameCount; i++)
                        {
                            Rectangle frameRect = new Rectangle(i * width, 0, width, spriteSheet.Height);
                            images.Add(spriteSheet.Clone(frameRect, PixelFormat.Format32bppArgb));
                        }
                    }
                    catch (Exception ex)
                    {
                        string err = "Could not load sprite strip.\n";
#if DEBUG
                        err += "\n[Debug Mode]\n" + ex + "\n\n";
#endif
                        err += "Do you want to load this file as a normal sprite?";

                        try
                        {
                            if (MessageBox.Show("Error loading sprite", err, MessageBoxButtons.YesNo) == DialogResult.Yes)
                            {
                                dispose = false;
                                images.Add(spriteSheet);
                            }
                        }
                        catch { }
                    }
                    finally
                    {
                        if (dispose)
                            spriteSheet.Dispose();
                    }
                }
                else
                {
                    images.Add(spriteSheet);
                }
                i++;
            }
        }

        public int originX = 0, originY = 0;
        public Bitmap image
        {
            get
            {
                return images.FirstOrDefault();
            }
            /*
            set
            {
                Image = value;
                var handler = ImageChanged;
                handler?.Invoke(this, value);
            }
            */
        }

        public bool preciseMask = false, separateMask = false;
        public int maskTop, maskRight, maskLeft, maskBottom; // relevant on manual mode only
        public int maskAlphaTolerance = 0;

        public bool maskBounding_auto = true, maskBounding_fullImage = false, maskBounding_manual = false;

        public new SpriteEditor editor
        {
            get
            {
                if (editorClosed)
                {
                    base.editor = new SpriteEditor(this);
                }
                return base.Editor as SpriteEditor;
            }
            set
            {
                base.editor = value;
            }
        }
        public GameSprite(string name) : base(name)
        {
            getEditor += (s, e) =>
            {
                var activateGet = editor;
            };
            editor = new SpriteEditor(this);

            images.CollectionChanged += (s, e) =>
            {
                if (treeImageIndex >= 0)
                {
                    if (images.Count > 0 && images[0] != null)
                    {
                        int w = Global.form1.treeImages.ImageSize.Width;
                        int h = Global.form1.treeImages.ImageSize.Height;
                        Global.form1.treeImages.Images[treeImageIndex] = images[0].ResizeImage(w, h);
                    }
                    else
                        Global.form1.treeImages.Images[treeImageIndex] = new Bitmap(1, 1);
                }
                Global.form1?.RefreshTreeView();
            };
        }
    }

    public class HandlerList<T> : List<T>
    {
        public event EventHandler<T[]> ItemsAdded;
        public event EventHandler<EventArgs> CollectionChanged;

        public new T this[int index]
        {
            get
            {
                return base[index];
            }
            set
            {
                base[index] = value;
                HandleItemsAdded(new T[] { value });
            }
        }

        private void HandleItemsAdded(T[] items)
        {
            var handler = ItemsAdded;
            if (handler != null)
                handler.Invoke(this, items);
            HandleChange();
        }

        private void HandleChange()
        {
            var handler1 = CollectionChanged;
            if (handler1 != null)
                handler1.Invoke(this, new EventArgs());
        }

        public new void Add(T item)
        {
            base.Add(item);
            HandleItemsAdded(new T[] { item });
        }

        public new void AddRange(IEnumerable<T> items)
        {
            base.AddRange(items);
            HandleItemsAdded(items.ToArray());
        }

        public new void Insert(int index, T item)
        {
            base.Insert(index, item);
            HandleItemsAdded(new T[] { item });
        }

        public new void InsertRange(int index, IEnumerable<T> items)
        {
            base.InsertRange(index, items);
            HandleItemsAdded(items.ToArray());
        }

        public new void Remove(T item)
        {
            base.Remove(item);
            HandleChange();
        }

        public new void RemoveAll(Predicate<T> match)
        {
            base.RemoveAll(match);
            HandleChange();
        }

        public new void RemoveAt(int index)
        {
            base.RemoveAt(index);
            HandleChange();
        }

        public new void RemoveRange(int index, int count)
        {
            base.RemoveRange(index, count);
            HandleChange();
        }

        public new void Reverse()
        {
            base.Reverse();
            HandleChange();
        }

        public new void Reverse(int index, int count)
        {
            base.Reverse(index, count);
            HandleChange();
        }

        public new void Sort()
        {
            base.Sort();
            HandleChange();
        }

        public new void Sort(IComparer<T> comparer)
        {
            base.Sort(comparer);
            HandleChange();
        }

        public new void Sort(Comparison<T> comparison)
        {
            base.Sort(comparison);
            HandleChange();
        }

        public new void Sort(int index, int count, IComparer<T> comparer)
        {
            base.Sort(index, count, comparer);
            HandleChange();
        }

        public new void Clear()
        {
            base.Clear();
            HandleChange();
        }
    }
}
