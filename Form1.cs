using System;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Linq;

namespace LabWork
{
    public partial class Form1 : Form
    {
        // --- Налаштування та кешування (без змін) ---
        private const double X_MIN = 0.0;
        private const double X_MAX = 0.5;
        private const double FUNCTION_STEP = 0.01;
        private const int PADDING = 40;
        private const float Y_MARGIN_FACTOR = 0.15f;
        private const double GRID_STEP_X = 0.1;

        private List<PointF> _graphPoints = new List<PointF>();
        private double _yMinCache = 0.0;
        private double _yMaxCache = 0.0;

        // --- Конструктор та Ініціалізація (без змін) ---
        public Form1()
        {
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | 
                          ControlStyles.AllPaintingInWmPaint | 
                          ControlStyles.UserPaint, true);

            InitializeComponent(); 
            
            this.Resize += (s, e) => 
            {
                CalculateGraphPoints(); 
                Invalidate();
            };
            this.Text = "Графік: y = 2.5x^3 / (e^(2x) + 2)";

            CalculateGraphPoints();
        }

        private double CalculateFunction(double x)
        {
            return (2.5 * Math.Pow(x, 3)) / (Math.Exp(2 * x) + 2);
        }

        private void CalculateGraphPoints()
        {
            // Обчислення _yMinCache та _yMaxCache
            double currentYMin = double.MaxValue;
            double currentYMax = double.MinValue;
            
            for (double x = X_MIN; x <= X_MAX; x += FUNCTION_STEP)
            {
                double y = CalculateFunction(x);
                currentYMin = Math.Min(currentYMin, y);
                currentYMax = Math.Max(currentYMax, y);
            }
            
            _yMinCache = 0.0;
            _yMaxCache = currentYMax + (currentYMax - currentYMin) * Y_MARGIN_FACTOR;

            // Формуємо точки графіка в МАТЕМАТИЧНИХ координатах
            _graphPoints.Clear();
            for (double x = X_MIN; x <= X_MAX; x += FUNCTION_STEP)
            {
                double y = CalculateFunction(x);
                _graphPoints.Add(new PointF((float)x, (float)y)); 
            }
        }

        // --- Метод Малювання ---

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.White);

            int drawWidth = this.ClientSize.Width - 2 * PADDING;
            int drawHeight = this.ClientSize.Height - 2 * PADDING;

            if (drawWidth <= 0 || drawHeight <= 0 || _graphPoints.Count <= 1) return;

            // --- 1. Налаштування системи координат (Graphics.Transform) ---
            
            float scaleX = (float)(drawWidth / (X_MAX - X_MIN));
            float scaleY = (float)(drawHeight / (_yMaxCache - _yMinCache));
            
            Matrix transformMatrix = new Matrix();
            
            // 1. Позиціонування початку координат (лівий нижній кут області малювання)
            transformMatrix.Translate(PADDING, this.ClientSize.Height - PADDING);
            
            // 2. Масштабування та віддзеркалення по Y (prepend - застосовується ПЕРЕД translate)
            transformMatrix.Scale(scaleX, -scaleY, MatrixOrder.Prepend);
            
            // 3. Компенсація початкових значень (prepend)
            transformMatrix.Translate(-(float)X_MIN, -(float)_yMinCache, MatrixOrder.Prepend);
            
            g.Transform = transformMatrix; 
            
            // --- 2. Малювання сітки (Grid) та осей ---
            
            // Створюємо перо з одиницею World - це дозволяє йому масштабуватися трансформацією
            using (Pen gridPen = new Pen(Color.LightGray, 0.0f) { DashStyle = DashStyle.Dot, Width = 1.0f / scaleX }) // FIX
            using (Pen axisPen = new Pen(Color.Black, 0.0f) { Width = 2.0f / scaleX }) // FIX
            {
                // Для сітки World Units не спрацює добре, повернемося до піксельних товщин після Reset
                // Або потрібно використовувати товщину пера, обернено пропорційну масштабу.
                // Спробуємо інший підхід: використати Pen.Width = 0 (Hairline Pen), який завжди 1 піксель.
                
                // Виправляємо Pen width, щоб він залишався 1 піксель на екрані
                using (Pen graphPen = new Pen(Color.Blue, 2.0f / scaleX)) // Pen width = 2 pixels on screen
                using (Pen hairlinePen = new Pen(Color.LightGray, 0.0f) { DashStyle = DashStyle.Dot }) // 1 pixel
                using (Pen axisLinePen = new Pen(Color.Black, 0.0f)) // 1 pixel
                {
                    graphPen.MiterLimit = 100f; // Для уникнення дивних з'єднань
                    
                    double yGridStep = (_yMaxCache - _yMinCache) / 5.0;

                    // Малювання сітки (використовуємо Pen.Width = 0 для 1 пікселя)
                    for (double x = X_MIN; x <= X_MAX; x += GRID_STEP_X)
                        g.DrawLine(hairlinePen, (float)x, (float)_yMinCache, (float)x, (float)_yMaxCache);

                    for (double y = _yMinCache; y <= _yMaxCache; y += yGridStep)
                        g.DrawLine(hairlinePen, (float)X_MIN, (float)y, (float)X_MAX, (float)y);
                    
                    // Малювання осей
                    g.DrawLine(axisLinePen, (float)X_MIN, (float)_yMinCache, (float)X_MIN, (float)_yMaxCache); 
                    g.DrawLine(axisLinePen, (float)X_MIN, (float)_yMinCache, (float)X_MAX, (float)_yMinCache); 
                    
                    // --- 3. Малювання графіка функції ---
                    if (_graphPoints.Count > 1)
                        g.DrawLines(graphPen, _graphPoints.ToArray());
                }
            }

            // --- 4. Малювання міток (Текст) ---
            
            Matrix currentTransform = g.Transform.Clone();
            g.ResetTransform(); 
            
            DrawAxisLabels(g, currentTransform);
        }

        // --- Допоміжний метод для малювання міток (без змін) ---
        private void DrawAxisLabels(Graphics g, Matrix transformMatrix)
        {
            using (Font axisFont = new Font("Arial", 9))
            using (SolidBrush textBrush = new SolidBrush(Color.Black))
            {
                double yGridStep = (_yMaxCache - _yMinCache) / 5.0;
                
                // Малювання міток X
                for (double x = X_MIN; x <= X_MAX; x += GRID_STEP_X)
                {
                    PointF[] mathPoint = new PointF[] { new PointF((float)x, (float)_yMinCache) };
                    transformMatrix.TransformPoints(mathPoint); 
                    float screenX = mathPoint[0].X;
                    float screenY = mathPoint[0].Y;

                    string xLabel = x.ToString("F1");
                    SizeF labelSize = g.MeasureString(xLabel, axisFont);
                    g.DrawString(xLabel, axisFont, textBrush, screenX - labelSize.Width / 2, screenY + 2);
                }

                // Малювання міток Y
                for (double y = _yMinCache; y <= _yMaxCache; y += yGridStep)
                {
                    PointF[] mathPoint = new PointF[] { new PointF((float)X_MIN, (float)y) };
                    transformMatrix.TransformPoints(mathPoint); 
                    float screenX = mathPoint[0].X;
                    float screenY = mathPoint[0].Y;

                    string yLabel = y.ToString("F3");
                    SizeF labelSize = g.MeasureString(yLabel, axisFont);
                    g.DrawString(yLabel, axisFont, textBrush, screenX - labelSize.Width - 5, screenY - labelSize.Height / 2);
                }
                
                // Мітка початку координат та мітки осей
                g.DrawString("0", axisFont, textBrush, (float)PADDING - 15, this.ClientSize.Height - PADDING + 2);
                g.DrawString("X", axisFont, textBrush, this.ClientSize.Width - PADDING + 5, this.ClientSize.Height - PADDING - 15);
                g.DrawString("Y", axisFont, textBrush, PADDING - 15, PADDING - 15);
            }
        }
    }
}