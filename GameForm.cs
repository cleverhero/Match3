using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

public enum FigureType { Rectangle, Trianlge }
public enum GameState { Activated, Normal, Movement }

namespace Match3
{
    public struct Figure
    {
        private FigureType type;
        private Color color;

        public FigureType Type
        {
            get { return type; }
        }
        public Color Color
        {
            get { return color; }
        }

        public Figure(FigureType _type, Color _color)
        {
            type = _type;
            color = _color;
        }
    }

    public partial class GameForm : Form
    {
        const int COUNT = 8;

        static readonly Figure[] figureKinds = 
        {
            new Figure(FigureType.Trianlge,  Color.Blue),
            new Figure(FigureType.Trianlge,  Color.Red),
            new Figure(FigureType.Trianlge,  Color.Green),
            new Figure(FigureType.Rectangle, Color.Green),
            new Figure(FigureType.Rectangle, Color.Red),
        };

        bool[][] removedTiles;

        private int score;
        private uint time;
        private Tile[][] fields;
        private Point activeField;
        private GameState state;

        public GameForm()
        {
            score = 0;
            time = 60;
            state = GameState.Normal;

            InitializeComponent();
            ScoreLabel.Text = "Score: " + Convert.ToString(score);
            TimeLabel.Text = "Time: " + Convert.ToString(time);

            removedTiles = new bool[COUNT][];
            for (int i = 0; i < COUNT; i++)
                removedTiles[i] = new bool[COUNT];

            CreateNewField();
            state = GameState.Movement;
            movementTimer.Start();

            timer.Start();
            repaintTimer.Start();
        }

        private void RemoveTiles()
        { 
            int size = Convert.ToInt32(this.GameField.Width / COUNT);
            Random rnd = new Random();

            for (int i = 0; i < COUNT; i++)
            {
                int delta = 0;
                for (int j = COUNT - 1; j >= 0; j--)
                {
                    if (delta > 0)
                    {
                        fields[i][j + delta] = fields[i][j];
                        fields[i][j + delta].SetShift(new Point(0, -delta * size));
                    }
                    if (removedTiles[i][j]) delta++;
                }
                for (int j = 0; j < delta; j++) {
                    int ind = rnd.Next(0, 5);
                    fields[i][j] = new Tile(figureKinds[ind].Type, figureKinds[ind].Color);
                    fields[i][j].SetShift(new Point(0, -delta * size));
                }
            }        
        }

        private int CheckField()
        {
            int res = 0;

            for (int i = 0; i < COUNT; i++)
                for (int j = 0; j < COUNT; j++)
                    this.removedTiles[i][j] = false;

            for (int i = 1; i < COUNT - 1; i++)
                for (int j = 0; j < COUNT; j++)
                {
                    if (fields[i][j] == fields[i - 1][j] && fields[i][j] == fields[i + 1][j])
                    {
                        removedTiles[i][j]     = true;
                        removedTiles[i - 1][j] = true;
                        removedTiles[i + 1][j] = true;
                    }
                }

            for (int i = 0; i < COUNT; i++)
                for (int j = 1; j < COUNT - 1; j++)
                {
                    if (fields[i][j] == fields[i][j - 1] && fields[i][j] == fields[i][j + 1])
                    {
                        removedTiles[i][j]     = true;
                        removedTiles[i][j - 1] = true;
                        removedTiles[i][j + 1] = true;
                    }
                }

            for (int i = 0; i < COUNT; i++)
                for (int j = 0; j < COUNT; j++)
                    if (this.removedTiles[i][j] == true) res++;

            return res;
        }

        private void CreateNewField()
        {
            Random rnd = new Random();

            fields = new Tile[COUNT][];
            for (int i = 0; i < COUNT; i++)
            {
                fields[i] = new Tile[COUNT];
                for (int j = 0; j < COUNT; j++)
                {
                    int ind = rnd.Next(0, 5);
                    fields[i][j] = new Tile(figureKinds[ind].Type, figureKinds[ind].Color);
                }
            }
        }

        private void button_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void GameField_Paint(object sender, PaintEventArgs e)
        {
            Graphics paintbox = e.Graphics;
            float width = this.GameField.Width / COUNT;
            Point size = new Point(Convert.ToInt32(width), Convert.ToInt32(width));

            for (int i = 0; i < COUNT; i++)
                for (int j = 0; j < COUNT; j++)
                {
                    Point position = new Point(Convert.ToInt32(width * i), Convert.ToInt32(width * j));
                    fields[i][j].Draw(paintbox, position, size);
                }

            Pen pen = new Pen(Color.Black, 2.0f);

            for (int i = 0; i <= COUNT; i++)
            {
                Point p1 = new Point(Convert.ToInt32(width * i), 0);
                Point p2 = new Point(Convert.ToInt32(width * i), this.GameField.Width - 1);
                paintbox.DrawLine(pen, p1, p2);

                p1 = new Point(0, Convert.ToInt32(width * i));
                p2 = new Point(this.GameField.Width - 1, Convert.ToInt32(width * i));
                paintbox.DrawLine(pen, p1, p2);
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            time -= 1;
            TimeLabel.Text = "Time: " + Convert.ToString(time);
            if (time == 0) Close();
        }

        private void GameForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            timer.Stop();
            fields[activeField.X][activeField.Y].StopRotate();
        }

        private void GameField_MouseClick(object sender, MouseEventArgs e)
        {
            fields[activeField.X][activeField.Y].StopRotate();

            float width = this.GameField.Width / COUNT;
            Point size = new Point(Convert.ToInt32(width), Convert.ToInt32(width));
            Point newPos = new Point(e.X / size.X, e.Y / size.Y);

            if (newPos.X >= COUNT || newPos.Y >= COUNT || newPos.X < 0 || newPos.Y < 0)
                return;

            if (state == GameState.Normal)
            {
                activeField = newPos;
                state = GameState.Activated;
                fields[activeField.X][activeField.Y].StartRotate();
            }
            else if (state == GameState.Activated)
            {
                Point oldPos = activeField;
                state = GameState.Normal;

                int dx = Math.Abs(oldPos.X - newPos.X);
                int dy = Math.Abs(oldPos.Y - newPos.Y);
                if (dx + dy != 1)
                    return;

                Tile std = fields[newPos.X][newPos.Y];
                fields[newPos.X][newPos.Y] = fields[oldPos.X][oldPos.Y];
                fields[oldPos.X][oldPos.Y] = std;

                fields[oldPos.X][oldPos.Y].SetShift(new Point((newPos.X - oldPos.X) * size.X, 
                                                              (newPos.Y - oldPos.Y) * size.X));

                fields[newPos.X][newPos.Y].SetShift(new Point((oldPos.X - newPos.X) * size.X,
                                                              (oldPos.Y - newPos.Y) * size.X));

                state = GameState.Movement;
                movementTimer.Start();
            }
        }

        private void repaintTimer_Tick(object sender, EventArgs e)
        {
            GameField.Refresh();
        }

        private void movementTimer_Tick(object sender, EventArgs e)
        {
            for (int i = 0; i < COUNT; i++)
                for (int j = 0; j < COUNT; j++)
                    if (fields[i][j].IsAnimated) return;

            int dscore = CheckField();
            score += dscore;
            ScoreLabel.Text = "Score: " + Convert.ToString(score);

            if (dscore == 0)
            {
                (sender as Timer).Stop();
                state = GameState.Normal;
            }
            RemoveTiles();
        }
    }

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
                double r = Math.Sqrt(Math.Pow(size.X - 2 * DSIZE, 2) + Math.Pow(size.Y - 2 * DSIZE, 2))/ 2;

                Point[] points = new Point[3]
                {
                    new Point(-size.X / 2 + DSIZE, size.Y / 2 - DSIZE),
                    new Point(0, -Convert.ToInt32(r)),
                    new Point(size.X / 2 - DSIZE, size.Y / 2 - DSIZE)
                };
                g.FillPolygon(brush, points);
            }

            g.RotateTransform(-angle);
            g.TranslateTransform(-shift_x, -shift_y);
        }
    }
}
