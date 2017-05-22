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

namespace Match3
{
    public partial class GameForm : Form
    {
        const int count = 10;

        private uint score;
        private uint time;
        private Field[][] fields;

        public GameForm()
        {
            score = 0;
            time = 60;

            InitializeComponent();
            ScoreLabel.Text = "Score: " + Convert.ToString(score);
            TimeLabel.Text = "Time: " + Convert.ToString(time);

            fields = new Field[count][];
            for (int i = 0; i < count; i++)
            {
                fields[i] = new Field[count];
                for (int j = 0; j < count; j++)
                {
                    fields[i][j] = new Field(FigureType.Trianlge, Color.Blue);
                }
            }

            timer.Start();
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

        public Field(FigureType _type, Color _color)
        {
            angle = 0.0f;
            shift = new Point(0, 0);
            type = _type;
            color = _color;
        }

        public void Draw(Graphics g, Point pos, Point size)
        {
            g.TranslateTransform(shift.X, shift.Y);
            g.RotateTransform(angle);

            SolidBrush brush = new SolidBrush(Color.FromArgb(255, color));
            if (type == FigureType.Rectangle)
            {
                g.FillRectangle(brush, new RectangleF(pos.X + 8, pos.Y + 8, size.X - 8, size.Y - 8));
            }
            else
            {
                Point[] points = new Point[3]
                {
                    new Point(pos.X + 8,          pos.Y + size.Y - 8),
                    new Point(pos.X + size.X / 2, pos.Y + 8),
                    new Point(pos.X + size.X - 8, pos.Y + size.Y - 8)
                };
                g.FillPolygon(brush, points);
            }
        }
    }
}
