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
public enum GameState { Activated, Normal, Movement, Test }

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
        private Tile[][] field;
        private Point activeTile;
        private Point oldActiveTile;
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
                        field[i][j + delta] = field[i][j];
                        field[i][j + delta].SetShift(new Point(0, -delta * size));
                    }
                    if (removedTiles[i][j]) delta++;
                }
                for (int j = 0; j < delta; j++) {
                    int ind = rnd.Next(0, 5);
                    field[i][j] = new Tile(figureKinds[ind].Type, figureKinds[ind].Color);
                    field[i][j].SetShift(new Point(0, -delta * size));
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
                    if (field[i][j] == field[i - 1][j] && field[i][j] == field[i + 1][j])
                    {
                        removedTiles[i][j]     = true;
                        removedTiles[i - 1][j] = true;
                        removedTiles[i + 1][j] = true;
                    }
                }

            for (int i = 0; i < COUNT; i++)
                for (int j = 1; j < COUNT - 1; j++)
                {
                    if (field[i][j] == field[i][j - 1] && field[i][j] == field[i][j + 1])
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

            field = new Tile[COUNT][];
            for (int i = 0; i < COUNT; i++)
            {
                field[i] = new Tile[COUNT];
                for (int j = 0; j < COUNT; j++)
                {
                    int ind = rnd.Next(0, 5);
                    field[i][j] = new Tile(figureKinds[ind].Type, figureKinds[ind].Color);
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
                    field[i][j].Draw(paintbox, position, size);
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
            field[activeTile.X][activeTile.Y].StopRotate();
        }

        private void GameField_MouseClick(object sender, MouseEventArgs e)
        {
            field[activeTile.X][activeTile.Y].StopRotate();

            float width = this.GameField.Width / COUNT;
            Point size = new Point(Convert.ToInt32(width), Convert.ToInt32(width));
            Point newPos = new Point(e.X / size.X, e.Y / size.Y);

            if (newPos.X >= COUNT || newPos.Y >= COUNT || newPos.X < 0 || newPos.Y < 0)
                return;

            if (state == GameState.Normal)
            {
                activeTile = newPos;
                state = GameState.Activated;
                field[activeTile.X][activeTile.Y].StartRotate();
            }
            else if (state == GameState.Activated)
            {
                Point oldPos = activeTile;
                state = GameState.Normal;

                int dx = Math.Abs(oldPos.X - newPos.X);
                int dy = Math.Abs(oldPos.Y - newPos.Y);
                if (dx + dy != 1)
                    return;

                swap(oldPos, newPos);
                activeTile = newPos;
                oldActiveTile = oldPos;

                state = GameState.Test;
                movementTimer.Start();
            }
        }

        private void repaintTimer_Tick(object sender, EventArgs e)
        {
            GameField.Refresh();
        }

        private void swap(Point pos1, Point pos2)
        {
            int size = Convert.ToInt32(this.GameField.Width / COUNT);

            Tile std = field[pos1.X][pos1.Y];
            field[pos1.X][pos1.Y] = field[pos2.X][pos2.Y];
            field[pos2.X][pos2.Y] = std;

            field[pos2.X][pos2.Y].SetShift(new Point((pos1.X - pos2.X) * size,
                                                     (pos1.Y - pos2.Y) * size));

            field[pos1.X][pos1.Y].SetShift(new Point((pos2.X - pos1.X) * size,
                                                     (pos2.Y - pos1.Y) * size));
        }

        private void movementTimer_Tick(object sender, EventArgs e)
        {
            for (int i = 0; i < COUNT; i++)
                for (int j = 0; j < COUNT; j++)
                    if (field[i][j].IsAnimated) return;

            int dscore = CheckField();
            score += dscore;
            ScoreLabel.Text = "Score: " + Convert.ToString(score);

            if (dscore == 0)
            {
                if (state == GameState.Test) swap(activeTile, oldActiveTile);
                state = GameState.Normal;
                (sender as Timer).Stop();
            }
            if (state == GameState.Test) state = GameState.Movement;
            RemoveTiles();
        }
    }

}
