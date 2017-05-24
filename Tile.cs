using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Match3
{
    public class Tile : System.Object
    {
        public float Angle
        {
            get { return angle; }
            set { angle = value; }
        }
        public Point Shift
        {
            get { return shift; }
            set { shift = value; }
        }
        public bool IsAnimated
        {
            get { return isAnimated; }
            set { isAnimated = value; }
        }

        const int SPEED = 3;
        const float ANGLE_SPEED = 3.0f;
        const int DSIZE = 8;

        private float angle;
        private bool isAnimated;
        private Point shift;
        private FigureType type;
        private Color color;
        private Timer rotateTimer;
        private Timer translationTimer;

        public Tile(FigureType _type, Color _color)
        {
            angle = 0.0f;
            shift = new Point(0, 0);
            type = _type;
            color = _color;
            isAnimated = false;

            rotateTimer = new Timer();
            rotateTimer.Interval = 10;
            rotateTimer.Tick += new EventHandler(RotateTickEvent);

            translationTimer = new Timer();
            translationTimer.Interval = 10;
            translationTimer.Tick += new EventHandler(TranslationTickEvent);
            translationTimer.Start();
        }

        public void RotateTickEvent(object sender, EventArgs e)
        {
            angle += 3f;
        }

        public void StartRotate()
        {
            rotateTimer.Start();
        }

        public void StopRotate()
        {
            rotateTimer.Stop();
            angle = 0.0f;
        }

        public void TranslationTickEvent(object sender, EventArgs e)
        {
            if (Math.Abs(shift.X + shift.Y) < SPEED)
            {
                isAnimated = false;
                return;
            }
            shift.X -= SPEED * Math.Sign(shift.X);
            shift.Y -= SPEED * Math.Sign(shift.Y);
        }

        public void SetShift(Point newShift)
        {
            shift = newShift;
            isAnimated = true;
        }

        public static bool operator ==(Tile a, Tile b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Tile a, Tile b)
        {
            return !a.Equals(b);
        }

        public override bool Equals(System.Object obj)
        {
            Tile other = obj as Tile;
            if (color == other.color && type == other.type) return true;
            return false;
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public void Draw(Graphics g, Point pos, Point size)
        {
            int shift_x = shift.X + pos.X + size.X / 2;
            int shift_y = shift.Y + pos.Y + size.Y / 2;
            g.TranslateTransform(shift_x, shift_y);
            g.RotateTransform(angle);

            SolidBrush brush = new SolidBrush(Color.FromArgb(255, color));
            if (type == FigureType.Rectangle)
            {
                g.FillRectangle(brush, new RectangleF(-size.X / 2 + DSIZE,
                                                      -size.Y / 2 + DSIZE,
                                                       size.X - 2 * DSIZE,
                                                       size.Y - 2 * DSIZE));
            }
            else
            {
                double r = Math.Sqrt(Math.Pow(size.X - 2 * DSIZE, 2) + Math.Pow(size.Y - 2 * DSIZE, 2)) / 2;

                Point[] points = new Point[3]
                {
                    new Point(-size.X / 2 + DSIZE, size.Y / 2 - DSIZE),
                    new Point(0, -Convert.ToInt32(r)),
                    new Point( size.X / 2 - DSIZE, size.Y / 2 - DSIZE)
                };
                g.FillPolygon(brush, points);
            }

            g.RotateTransform(-angle);
            g.TranslateTransform(-shift_x, -shift_y);
        }
    }
}
