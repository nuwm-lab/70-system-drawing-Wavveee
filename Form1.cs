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

            // --- Початкові межі області згідно з завданням ---
            double xMin = 0.0;
            double xMax = 0.5;
            double step = 0.01; // Крок для гладкості графіка

            // Знаходимо min/max y для масштабування
            double yMin = double.MaxValue;
            double yMax = double.MinValue;
            
            for (double x = xMin; x <= xMax; x += step)
            {
                double y = (2.5 * Math.Pow(x, 3)) / (Math.Exp(2 * x) + 2);
                yMin = Math.Min(yMin, y);
                yMax = Math.Max(yMax, y);
            }
            
            // Додамо невеликий запас для Y-осі
            double yMargin = (yMax - yMin) * 0.15; // Трохи більший запас, бо графік починається з 0
            yMin = 0.0; // Графік починається з (0, 0), тому yMin = 0
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
            Font axisFont = new Font("Arial", 9);
            
            // Кроки для сітки: xGridStep = 0.1 відповідає вашому dx
            double xGridStep = 0.1; 
            double yGridStep = (yMax - yMin) / 5.0; // 5 основних горизонтальних ліній

            // Місце для малювання осі X на екрані
            float xAxisScreenY = (float)(height - padding - (0.0 - yMin) * scaleY);

            // Малювання вертикальних ліній сітки та міток X
            for (double x = xMin; x <= xMax; x += xGridStep)
            {
                float screenX = (float)((x - xMin) * scaleX) + startX;
                
                // Сітка
                g.DrawLine(gridPen, screenX, startY, screenX, startY + drawHeight);
                
                // Мітки X
                string xLabel = x.ToString("F1");
                SizeF labelSize = g.MeasureString(xLabel, axisFont);
                // Мітка X розміщується біля осі X
                g.DrawString(xLabel, axisFont, Brushes.Black, screenX - labelSize.Width / 2, xAxisScreenY + 2);
            }

            // Малювання горизонтальних ліній сітки та міток Y
            for (double y = 0.0; y <= yMax; y += yGridStep) // Починаємо з y=0
            {
                float screenY = (float)(height - padding - (y - yMin) * scaleY);
                
                // Сітка
                g.DrawLine(gridPen, startX, screenY, startX + drawWidth, screenY);
                
                // Мітки Y
                string yLabel = y.ToString("F3");
                SizeF labelSize = g.MeasureString(yLabel, axisFont);
                g.DrawString(yLabel, axisFont, Brushes.Black, startX - labelSize.Width - 5, screenY - labelSize.Height / 2);
            }
            
            // Малювання осей X та Y
            // Вісь Y: лінія зліва області малювання
            g.DrawLine(axisPen, startX, startY, startX, startY + drawHeight); 
            // Вісь X: лінія, що відповідає y=0
            g.DrawLine(axisPen, startX, xAxisScreenY, startX + drawWidth, xAxisScreenY); 

            // Додавання міток осей
            g.DrawString("X", axisFont, Brushes.Black, startX + drawWidth - 10, xAxisScreenY - 15);
            g.DrawString("Y", axisFont, Brushes.Black, startX - 15, startY - 15);
            g.DrawString("0", axisFont, Brushes.Black, startX - 15, xAxisScreenY + 2); // Мітка початку координат

            // --- Малювання графіка функції ---
            Pen graphPen = new Pen(Color.Blue, 2);
            var points = new System.Collections.Generic.List<PointF>();

            for (double x = xMin; x <= xMax; x += step)
            {
                double y = (2.5 * Math.Pow(x, 3)) / (Math.Exp(2 * x) + 2);

                // Перетворення координат з математичних у екранні
                float screenX = (float)((x - xMin) * scaleX) + startX;
                // screenY тепер відносно yMin=0
                float screenY = (float)(height - padding - (y - yMin) * scaleY);

                points.Add(new PointF(screenX, screenY));
            }

            if (points.Count > 1)
                g.DrawLines(graphPen, points.ToArray());

            this.Text = "Графік: y = 2.5x^3 / (e^(2x) + 2)";
        }
    }
}