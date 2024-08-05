using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using System.Runtime.InteropServices;

namespace MazeSolver
{
    public partial class MainForm : Form
    {
        private int Infinity = 10000000;
        private int colorSensivity = 100;
        private int borderSensivity = 15;

        private Bitmap mazeBitmap;
        private Bitmap emptyMazeBitmap;

        private Point start, end;
        private Color freeSpaceColor, wallColor;

        private bool startPointAdded = false;
        private bool endPointAdded = false;
        private string imagePath;

        int[] directionX = new int[] { -1, 0, 1, 0 };
        int[] directionY = new int[] { 0, 1, 0, -1 };

        bool[,] accesible;
        int[,] distanceFromStart;
        int[,] distanceFromBorder;
        
        ArrayList path;

        public MainForm()
        {
            InitializeComponent();
        }

        public Bitmap DownscaleBitmap(Bitmap originalBitmap, int targetWidth, int targetHeight)
        {
            Bitmap resizedBitmap = new Bitmap(targetWidth, targetHeight);

            using (Graphics graphics = Graphics.FromImage(resizedBitmap))
            {
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                graphics.DrawImage(originalBitmap, 0, 0, targetWidth, targetHeight);
            }

            return resizedBitmap;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Image Files(*.BMP;*.JPG;*.GIF;*.PNG)|*.BMP;*.JPG;*.GIF;*.PNG)";

            if (fileDialog.ShowDialog() == DialogResult.OK )
            {
                imagePath = fileDialog.FileName; 
                mazeBitmap = new Bitmap(imagePath);
                mazeBitmap = DownscaleBitmap(mazeBitmap, 899, 622);
                emptyMazeBitmap = new Bitmap(imagePath);
                emptyMazeBitmap = DownscaleBitmap(emptyMazeBitmap, 899, 622);
                pictureBox.Image = mazeBitmap;

                accesible = new bool[mazeBitmap.Width, mazeBitmap.Height];
                distanceFromStart = new int[mazeBitmap.Width, mazeBitmap.Height];
                distanceFromBorder = new int[mazeBitmap.Width, mazeBitmap.Height];
            }
        }

        private void btnSolve_Click(object sender, EventArgs e)
        {
            MarkCellsAsAccesible();
            if (startPointAdded == false || endPointAdded == false)
            {
                MessageBox.Show("Please add a starting point(left click) and an ending point(right click)", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            while (borderSensivity >= 5 && !SolveMaze())
            {
                borderSensivity -= 5;
            }

            if (SolveMaze())
            {
                MarkPath();
            }
            else
            {
                MessageBox.Show("Path not found", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ResetMaze();
            }
        }

        private void ResetMaze()
        {
            startPointAdded = false;
            endPointAdded = false;
            mazeBitmap = new Bitmap(imagePath);
            mazeBitmap = DownscaleBitmap(mazeBitmap, 899, 622);
            pictureBox.Image = mazeBitmap;
        }

        private void MarkPath()
        {
            foreach (Point point in path)
                markPixels(point.X, point.Y, Color.Yellow, 2);
            pictureBox.Image = mazeBitmap;
        }

        private void MarkCellsAsAccesible()
        {
            int averageWallR = 0;
            int averageWallG = 0;   
            int averageWallB = 0;
            int nrPixels = 0;
            for (int i = 0; i < mazeBitmap.Width; i++)
            {
                for (int j = 0; j < mazeBitmap.Height; j++)
                {
                    if (ColorDifference(mazeBitmap.GetPixel(i, j), freeSpaceColor) > 2.1 * colorSensivity)
                    {
                        averageWallR += mazeBitmap.GetPixel(i, j).R;
                        averageWallG += mazeBitmap.GetPixel(i, j).G;
                        averageWallB += mazeBitmap.GetPixel(i, j).B;
                        ++nrPixels;
                    }
                }
            }
            averageWallR /= nrPixels;
            averageWallG /= nrPixels;
            averageWallB /= nrPixels;
            wallColor = Color.FromArgb(averageWallR, averageWallG, averageWallB);

            for (int i = 0; i < mazeBitmap.Width; i++)
            {
                for (int j = 0; j < mazeBitmap.Height; j++)
                {
                    if (ColorDifference(mazeBitmap.GetPixel(i, j), freeSpaceColor) <= colorSensivity + 110 ||
                        ColorDifference(mazeBitmap.GetPixel(i, j), wallColor) > 1.35 * colorSensivity)
                        accesible[i, j] = true;
                    else accesible[i, j] = false;
                }
            }
            Console.WriteLine(wallColor.ToString());
            Console.WriteLine(freeSpaceColor.ToString());
        }

        private double ColorDifference(Color color1, Color color2)
        {

            double difR = Math.Abs(color1.R - color2.R);
            double difG = Math.Abs(color1.G - color2.G);
            double difB = Math.Abs(color1.B - color2.B);

            return difR + difG + difB;
        }

        private bool SolveMaze()
        {
            ComputeDistances();
            ComputeDistancesFromBorders();

            if (distanceFromStart[end.X, end.Y] == Infinity)
                return false;

            ComputePath();

            return true;
        }

        private void ComputeDistancesFromBorders()
        {
            Queue<Point> queue = new Queue<Point>();
            for (int i = 0; i < mazeBitmap.Width; i++)
                for (int j = 0; j < mazeBitmap.Height; j++)
                {
                    if (accesible[i, j] == true)
                        distanceFromBorder[i, j] = Infinity;
                    else
                    {
                        distanceFromBorder[i, j] = 0;
                        queue.Enqueue(new Point(i, j));
                    }
                }

            while (queue.Count > 0)
            {
                Point point = queue.Dequeue();

                for (int direction = 0; direction < 4; ++direction)
                {
                    Point nextPoint = new Point(point.X + directionX[direction], point.Y + directionY[direction]);
                    if (validCoordonates(nextPoint) && accesible[nextPoint.X, nextPoint.Y])
                    {
                        if (distanceFromBorder[nextPoint.X, nextPoint.Y] > distanceFromBorder[point.X, point.Y] + 1)
                        {
                            distanceFromBorder[nextPoint.X, nextPoint.Y] = distanceFromBorder[point.X, point.Y] + 1;
                            queue.Enqueue(nextPoint);
                        }
                    }
                }
            }
        }

        private void ComputePath()
        {
            path = new ArrayList();

            int currentX = end.X;
            int currentY = end.Y;

            while (currentX != start.X || currentY != start.Y)
            {
                path.Add(new Point(currentX, currentY));

                int maximumDistanceFromBorder = 0;
                Point bestPoint = new Point();  

                for (int direction = 0; direction < 4; ++direction)
                {
                    Point nextPoint = new Point(currentX + directionX[direction], currentY + directionY[direction]);
                    if (validCoordonates(nextPoint) && distanceFromStart[nextPoint.X, nextPoint.Y] == distanceFromStart[currentX, currentY] - 1)
                    {
                        maximumDistanceFromBorder = Math.Max(maximumDistanceFromBorder, distanceFromBorder[nextPoint.X, nextPoint.Y]);

                        if (distanceFromBorder[nextPoint.X, nextPoint.Y] == maximumDistanceFromBorder)
                            bestPoint = nextPoint;
                    }
                }

                currentX = bestPoint.X;
                currentY = bestPoint.Y; 
            }
            path.Add(start);
        }

        private void ComputeDistances()
        {
            for (int i = 0; i < mazeBitmap.Width; i++)
                for (int j = 0; j < mazeBitmap.Height; j++)
                    distanceFromStart[i, j] = Infinity;
            distanceFromStart[start.X, start.Y] = 0;

            Queue<Point> queue = new Queue<Point>();
            queue.Enqueue(start);
            while (queue.Count > 0)
            {
                Point point = queue.Dequeue();

                if (point == end)
                    return;

                for (int direction = 0; direction < 4; ++direction)
                {
                    Point nextPoint = new Point(point.X + directionX[direction], point.Y + directionY[direction]);
                    if (validCoordonates(nextPoint) && accesible[nextPoint.X, nextPoint.Y] && distanceFromBorder[nextPoint.X, nextPoint.Y] >= borderSensivity)
                    {
                        if (distanceFromStart[nextPoint.X, nextPoint.Y] > distanceFromStart[point.X, point.Y] + 1)
                        {
                            distanceFromStart[nextPoint.X, nextPoint.Y] = distanceFromStart[point.X, point.Y] + 1;
                            queue.Enqueue(nextPoint);
                        }
                    }
                }
            }
        }

        private bool validCoordonates(Point point)
        {
            if (0 <= point.X && point.X < mazeBitmap.Width && 0 <= point.Y && point.Y < mazeBitmap.Height)
                return true;
            return false;
        }

        private void pictureBox_MouseClick(object sender, MouseEventArgs e)
        {
            Point clickedPoint = e.Location;

            if (pictureBox.Image != null && clickedPoint.X < mazeBitmap.Width && clickedPoint.Y < mazeBitmap.Height)
            {
                if (e.Button == System.Windows.Forms.MouseButtons.Left)
                {
                    start = clickedPoint;
                    startPointAdded = true;
                    freeSpaceColor = GetAverageColorInArea(clickedPoint.X, clickedPoint.Y, 3);
                    markPixels(clickedPoint.X, clickedPoint.Y, Color.Green, 5);
                    pictureBox.Image = mazeBitmap;
                }
                if (e.Button == System.Windows.Forms.MouseButtons.Right)
                {
                    end = clickedPoint;
                    endPointAdded = true;
                    markPixels(clickedPoint.X, clickedPoint.Y, Color.Red, 5);
                    pictureBox.Image = mazeBitmap;
                }
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            ResetMaze();
        }

        private Color GetAverageColorInArea(int x, int y, int size)
        {
            int averageR = 0;
            int averageG = 0;
            int averageB = 0;
            int nrPixels = 0;
            for (int i = Math.Max(0, x - size); i <= Math.Min(mazeBitmap.Width - 1, x + size); i++)
                for (int j = Math.Max(0, y - size); j <= Math.Min(mazeBitmap.Height - 1, y + size); j++)
                {
                    averageR += mazeBitmap.GetPixel(i, j).R;
                    averageG += mazeBitmap.GetPixel(i, j).G;
                    averageB += mazeBitmap.GetPixel(i, j).B;
                    ++nrPixels;
                }
            averageR /= nrPixels;
            averageG /= nrPixels;
            averageB /= nrPixels;
            return Color.FromArgb(averageR, averageG, averageB);
        }

        private void markPixels(int x, int y, Color color, int size)
        {
            for (int i = Math.Max(0, x - size); i <= Math.Min(mazeBitmap.Width - 1, x + size); i++)
                for (int j = Math.Max(0, y - size); j <= Math.Min(mazeBitmap.Height - 1, y + size); j++)
                    mazeBitmap.SetPixel(i, j, color);
        }
    }
}
