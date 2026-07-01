using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcadeMaker.Core.Resources.Serializeables;
using ArcadeMaker.Core.Models;
using ArcadeMaker.IDE.Editors.Object.ObjectProperties;
using Exp;

namespace ArcadeMaker.IDE.Items
{
    public class GameObject : GameItem, ISetsIcon //, IContainsScript
    {
        public static Bitmap Icon => Properties.Resources.object32;

        public List<IDEObjectProperty> ExtraProperties { get; } = [];
        public List<ObjectEvent> Events { get; } = [];

        public bool CompiledSyntaxTree { get; set; } = false;

        public bool compiledModelsTree = false;

        private string _part2script = null;
        private GameSprite _sprite = null;
        public GameSprite sprite
        {
            get
            {
                return _sprite;
            }
            set
            {
                _sprite = value;
                if (treeImageIndex >= 0)
                {
                    if (sprite != null && treeNode != null)
                    {
                        treeNode.ImageIndex = _sprite.treeImageIndex;
                        treeNode.SelectedImageIndex = _sprite.treeImageIndex;
                    }
                    else
                    {
                        Global.form1.treeImages.Images[treeImageIndex] = new System.Drawing.Bitmap(1, 1);
                        treeNode.ImageIndex = treeImageIndex;
                        treeNode.SelectedImageIndex = treeImageIndex;
                    }
                    Global.form1?.RefreshTreeView();
                }
            }
        }

        private Image collisionIconSpriteImage;
        private static readonly Image emptyCollisionIcon = ObjectEditor.GetIcon(ObjectEvent.EventType.Collision)!;
        internal Image CollisionIcon
        {
            get
            {
                // if the sprite preview was changed since last get, recreate the icon
                if (field == null || collisionIconSpriteImage != sprite?.image)
                {
                    if (sprite?.image == null)
                        return emptyCollisionIcon;

                    try
                    {
                        Bitmap bundle = new(emptyCollisionIcon.Width * 2, emptyCollisionIcon.Height);
                        using Graphics bg = Graphics.FromImage(bundle);
                        bg.DrawImage(emptyCollisionIcon, 0, 0);
                        bg.DrawImage(sprite.image, emptyCollisionIcon.Width, 0, emptyCollisionIcon.Width, emptyCollisionIcon.Height);

                        collisionIconSpriteImage = sprite.image;
                        field = bundle;

                        return bundle;
                    }
                    catch (Exception ex)
                    {
                        _ = ex;
                    }
                }

                return field ?? emptyCollisionIcon;
            }
        }


        public new ObjectEditor editor
        {
            get
            {
                if (editorClosed)
                {
                    base.editor = new ObjectEditor(this);
                }
                return base.Editor as ObjectEditor;
            }
            set
            {
                base.editor = value;
            }
        }
        public GameObject(string name) : base(name)
        {
            getEditor += (s, e) =>
            {
                e = this.editor;
            };
            editor = new ObjectEditor(this);
            base.NameChanged += (s, e) =>
            {
                compiledModelsTree = false;
            };
        }

        public bool solid = false;
        public int depth;
        public GameObject parent;

        //internal EventScripts? GetEventScripts(ObjectEvent ev)
        //{
        //    return Events.FirstOrDefault(es => es.Event == ev);
        //}
    }
}