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
public enum GameState { Activated, Normal }

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
        const int count = 10;

        private uint score;
        private uint time;
        private Field[][] fields;
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

            Figure[] figures = new Figure[5]
            {
                new Figure(FigureType.Trianlge, Color.Blue),
                new Figure(FigureType.Trianlge, Color.Red),
                new Figure(FigureType.Trianlge, Color.Green),
                new Figure(FigureType.Rectangle, Color.Green),
                new Figure(FigureType.Rectangle, Color.Red),
            };

            Random rnd = new Random();

            fields = new Field[count][];
            for (int i = 0; i < count; i++)
            {
                fields[i] = new Field[count];
                for (int j = 0; j < count; j++)
                {
                    int ind = rnd.Next(0, 5);
                    fields[i][j] = new Field(figures[ind].Type, figures[ind].Color);
                } 
            }

            timer.Start();
            repaintTimer.Start();
        }

        private void button_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void GameField_Paint(object sender, PaintEventArgs e)
        {
            Graphics paintbox = e.Graphics;
            float width = this.GameField.Width / count;
            Point size = new Point(Convert.ToInt32(width), Convert.ToInt32(width));
            
            for (int i = 0; i < count; i++)
                for (int j = 0; j < count; j++)
                {
                    Point position = new Point(Convert.ToInt32(width * i), Convert.ToInt32(width * j));
                    fields[i][j].Draw(paintbox, position, size);
                }

            Pen pen = new Pen(Color.Black, 2.0f);

            for (int i = 0; i <= count; i++)
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
            fields[activeField.X][activeField.Y].StopAnimation();
        }

        private void GameField_MouseClick(object sender, MouseEventArgs e)
        {
            if (state == GameState.Normal)
            {
                fields[activeField.X][activeField.Y].StopAnimation();
                float width = this.GameField.Width / count;
                Point size = new Point(Convert.ToInt32(width), Convert.ToInt32(width));

                activeField = new Point(e.X / size.X, e.Y / size.Y);
                state = GameState.Activated;
                fields[activeField.X][activeField.Y].StartAnimation();
            }
            if (state == GameState.Activated)
            {
                Point oldPos = activeField;
                fields[oldPos.X][oldPos.Y].StopAnimation();


                float width = this.GameField.Width / count;
                Point size = new Point(Convert.ToInt32(width), Convert.ToInt32(width));
                activeField = new Point(e.X / size.X, e.Y / size.Y);

                int dx = Math.Abs(oldPos.X - activeField.X);
                int dy = Math.Abs(oldPos.Y - activeField.Y);
                if (dx + dy != 1)
                {
                    fields[activeField.X][activeField.Y].StartAnimation();
                    return;
                }
                
            }
        }

        private void repaintTimer_Tick(object sender, EventArgs e)
        {
            GameField.Refresh();
        }
    }

    public class Field
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

        private float angle;
        private Point shift;
        private FigureType type;
        private Color color;
        private Timer animationTimer;

        public Field(FigureType _type, Color _color)
        {
            angle = 0.0f;
            shift = new Point(0, 0);
            type = _type;
            color = _color;

            animationTimer = new Timer();
            animationTimer.Interval = 10;
            animationTimer.Tick += new EventHandler(TickEvent);
        }

        public void TickEvent(object sender, EventArgs e)
        {
            angle += 3f;
        }

        public void StartAnimation()
        {
            animationTimer.Start();
        }

        public void StopAnimation()
        {
            animationTimer.Stop();
            angle = 0.0f;
            shift = new Point(0, 0);
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
                g.FillRectangle(brush, new RectangleF(-size.X/2 + 8, -size.Y / 2 + 8, size.X - 16, size.Y - 16));
            }
            else
            {
                Point[] points = new Point[3]
                {
                    new Point(-size.X / 2 + 8, size.Y / 2 - 8),
                    new Point(0, -Convert.ToInt32(Math.Sqrt(Math.Pow(size.X - 16, 2) + Math.Pow(size.Y - 16, 2)))/2),
                    new Point(size.X / 2 - 8, size.Y / 2 - 8)
                };
                g.FillPolygon(brush, points);
            }

            g.RotateTransform(-angle);
            g.TranslateTransform(-shift_x, -shift_y);
        }
    }
}
