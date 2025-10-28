using System;
using System.Drawing;
using System.Windows.Forms;

namespace LabWork
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            this.Paint += Form1_Paint;
            this.Resize += (s, e) => Invalidate();
            this.DoubleBuffered = true;
            this.Text = "Графік y = sin(x)";
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.Clear(Color.White);

            int width = this.ClientSize.Width;
            int height = this.ClientSize.Height;

            // Межі області
            double xMin = 0.0;
            double xMax = 0.5;
            double dx = 0.1;

            // Знаходимо min/max y для масштабування
            double yMin = double.MaxValue;
            double yMax = double.MinValue;

            for (double x = xMin; x <= xMax; x += dx)
            {
                double y = (2.5 * Math.Pow(x, 3)) / (Math.Exp(2 * x) + 2);
                yMin = Math.Min(yMin, y);
                yMax = Math.Max(yMax, y);
            }

            float scaleX = width / (float)(xMax - xMin);
            float scaleY = height / (float)(yMax - yMin);

            Pen graphPen = new Pen(Color.Blue, 2);

            // Формуємо точки графіка
            var points = new System.Collections.Generic.List<PointF>();

            for (double x = xMin; x <= xMax; x += dx)
            {
                double y = (2.5 * Math.Pow(x, 3)) / (Math.Exp(2 * x) + 2);

                float screenX = (float)((x - xMin) * scaleX);
                float screenY = (float)(height - (y - yMin) * scaleY);

                points.Add(new PointF(screenX, screenY));
            }

            if (points.Count > 1)
                g.DrawLines(graphPen, points.ToArray());

            // Підпис заголовку
            this.Text = "Графік: y = 2.5x^3 / (e^(2x) + 2)";
        }
    }
}