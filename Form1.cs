using System;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

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
            this.Text = "Графік: y = 2.5x^3 / (e^(2x) + 2)";
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.White);

            int width = this.ClientSize.Width;
            int height = this.ClientSize.Height;

            // Межі області
            double xMin = 0.0;
            double xMax = 2.0; // Збільшимо xMax для більш репрезентативного графіка
            double dx = 0.01; // Зменшимо dx для більш гладкого графіка

            // Знаходимо min/max y для масштабування
            double yMin = double.MaxValue;
            double yMax = double.MinValue;
            
            // Крок для обчислення функції
            double step = 0.01;

            for (double x = xMin; x <= xMax; x += step)
            {
                double y = (2.5 * Math.Pow(x, 3)) / (Math.Exp(2 * x) + 2);
                yMin = Math.Min(yMin, y);
                yMax = Math.Max(yMax, y);
            }
            
            // Додамо невеликий запас для Y-осі
            double yMargin = (yMax - yMin) * 0.1;
            yMin -= yMargin;
            yMax += yMargin;

            // Запас для осей (у пікселях)
            int padding = 40; 
            int drawWidth = width - 2 * padding;
            int drawHeight = height - 2 * padding;

            float scaleX = drawWidth / (float)(xMax - xMin);
            float scaleY = drawHeight / (float)(yMax - yMin);
            
            // Координати початку області малювання
            int startX = padding;
            int startY = padding;


            // --- Малювання сітки (Grid) та осей ---
            Pen gridPen = new Pen(Color.LightGray, 1) { DashStyle = DashStyle.Dot };
            Pen axisPen = new Pen(Color.Black, 2);
            Font axisFont = new Font("Arial", 8);
            
            // Кроки для сітки
            double xGridStep = (xMax - xMin) / 10.0;
            double yGridStep = (yMax - yMin) / 10.0;

            // Малювання вертикальних ліній сітки та міток X
            for (double x = xMin; x <= xMax; x += xGridStep)
            {
                float screenX = (float)((x - xMin) * scaleX) + startX;
                g.DrawLine(gridPen, screenX, startY, screenX, startY + drawHeight);
                
                string xLabel = x.ToString("F1");
                SizeF labelSize = g.MeasureString(xLabel, axisFont);
                g.DrawString(xLabel, axisFont, Brushes.Black, screenX - labelSize.Width / 2, startY + drawHeight + 5);
            }

            // Малювання горизонтальних ліній сітки та міток Y
            for (double y = yMin; y <= yMax; y += yGridStep)
            {
                float screenY = (float)(height - padding - (y - yMin) * scaleY);
                g.DrawLine(gridPen, startX, screenY, startX + drawWidth, screenY);
                
                string yLabel = y.ToString("F2");
                SizeF labelSize = g.MeasureString(yLabel, axisFont);
                g.DrawString(yLabel, axisFont, Brushes.Black, startX - labelSize.Width - 5, screenY - labelSize.Height / 2);
            }
            
            // Малювання осей X та Y
            // Вісь X: лінія внизу області малювання
            g.DrawLine(axisPen, startX, startY + drawHeight, startX + drawWidth, startY + drawHeight); 
            // Вісь Y: лінія зліва області малювання
            g.DrawLine(axisPen, startX, startY, startX, startY + drawHeight); 

            // Додавання стрілок на осі (якщо потрібно, для простоти пропущено, але осі є)
            // Мітки осей
            g.DrawString("X", axisFont, Brushes.Black, startX + drawWidth - 10, startY + drawHeight + 5);
            g.DrawString("Y", axisFont, Brushes.Black, startX - 15, startY - 15);


            // --- Малювання графіка функції ---
            Pen graphPen = new Pen(Color.Blue, 2);
            var points = new System.Collections.Generic.List<PointF>();

            for (double x = xMin; x <= xMax; x += step)
            {
                double y = (2.5 * Math.Pow(x, 3)) / (Math.Exp(2 * x) + 2);

                // Перетворення координат з математичних у екранні
                float screenX = (float)((x - xMin) * scaleX) + startX;
                float screenY = (float)(height - padding - (y - yMin) * scaleY);

                points.Add(new PointF(screenX, screenY));
            }

            if (points.Count > 1)
                g.DrawLines(graphPen, points.ToArray());

            // Підпис заголовку (дублюється у конструкторі, але тут для впевненості)
            this.Text = "Графік: y = 2.5x^3 / (e^(2x) + 2)";
        }
    }
}