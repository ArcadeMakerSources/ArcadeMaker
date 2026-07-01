using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace ArcadeMaker.IDE.Items
{
    public class GameBackground : GameItem, ISetsIcon
    {
        public static Bitmap Icon => Properties.Resources.background;

        private Bitmap _image = null;

        public Bitmap image
        {
            get
            {
                return _image;
            }
            set
            {
                _image = value;

                if (treeImageIndex >= 0)
                {
                    int w = Global.form1.treeImages.ImageSize.Width;
                    int h = Global.form1.treeImages.ImageSize.Height;
                    Global.form1.treeImages.Images[treeImageIndex] = _image == null ? new Bitmap(1, 1) : _image.ResizeImage(w, h);
                    Global.form1.RefreshTreeView();
                }
            }
        }

        public new BackgroundEditor editor
        {
            get
            {
                if (editorClosed)
                {
                    base.editor = new BackgroundEditor(this);
                }
                return base.Editor as BackgroundEditor;
            }
            set
            {
                base.editor = value;
            }
        }

        public GameBackground(string name) : base(name)
        {
            instances.Add(this);
            CheckInvitation();
            getEditor += (s, e) =>
            {
                var activateGet = editor;
            };
            editor = new BackgroundEditor(this);
        }

        private static readonly List<GameBackground> instances = new List<GameBackground>();

        private static readonly List<GameBackgroundInvitation> invitations = new List<GameBackgroundInvitation>();
        public static void Invite(RoomBackground sender, string name)
        {
            invitations.Add(new GameBackgroundInvitation(name, sender));
            for (int i = instances.Count - 1; i >= 0; i--)
                instances[i].CheckInvitation();
        }

        private void CheckInvitation()
        {
            foreach (GameBackgroundInvitation invitation in invitations)
            {
                if (invitation.name == name)
                    invitation.sender.image = this;
            }
        }
    }

    public struct GameBackgroundInvitation
    {
        public string name;
        public RoomBackground sender;

        public GameBackgroundInvitation(string name, RoomBackground sender)
        {
            this.name = name;
            this.sender = sender;
        }
    }
}
