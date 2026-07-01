using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace ArcadeMaker.IDE.Items
{
    public class GamePath : GameItem, ISetsIcon
    {
        public static Bitmap Icon => Properties.Resources.path;

        public List<PathPoint> points = new List<PathPoint>();
        public bool close = false;

        public new PathEditor editor
        {
            get
            {
                if (editorClosed)
                {
                    base.editor = new PathEditor(this);
                }
                return base.Editor as PathEditor;
            }
            set
            {
                base.editor = value;
            }
        }

        public GamePath(string name) : base(name)
        {
            getEditor += (s, e) =>
            {
                var activateGet = editor;
            };
            editor = new PathEditor(this);
        }
    }

    public class PathPoint
    {
        public int x = 0, y = 0, speed = 100;

        public PathPoint(int x, int y, int speed = 100)
        {
            this.x = x;
            this.y = y;

            if (speed < 0 || speed > 100)
                throw new Exception("PathPoint speed range is 0-100");

            this.speed = speed;
        }

        public PathPoint(Point point)
        {
            this.x = point.X;
            this.y = point.Y;
        }

        public PathPoint() { }

        public static Point operator -(PathPoint left, Size right)
        {
            return new Point(left.x - right.Width, left.y - right.Height);
        }

        public override string ToString()
        {
            return $"({x}, {y})     sp: {speed}%";
        }
    }
}
