using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeepLearning4Deppen
{
    public partial class Form1 : Form
    {
        private List<PointF> points;
        private float m = 1, n = 0;
        private Timer timer;
        private const float LEARNING_RATE = 0.01f;
        private int length = 5;
        private bool running;

        public Form1()
        {
            InitializeComponent();
            timer = new Timer();
            timer.Interval = 10;
            timer.Tick += Timer_Tick;
            points = new List<PointF>();
            DoubleBuffered = true;

            MouseClick += OnClick;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            this.Refresh();
        }

        private void OnClick(object sender, MouseEventArgs e)
        {
            float scale = 200f / length;
            PointF point = new PointF((e.X - 30) / scale, (length * scale - (e.Y - 20)) / scale);
            if (point.X < 0 || point.X > length || point.Y < 0 || point.Y > length)
                return;
            points.Add(point);
            this.Refresh();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Draw(e.Graphics);
        }

        public void Draw(Graphics g)
        {
            int offsetX = 30, offsetY = 20;
            #region Koordinaten System
            Font font = new Font("Arial", 11);
            g.DrawString("Y", font, Brushes.Black, 0, 0);
            g.DrawString("X", font, Brushes.Black, 230, 240);
            g.TranslateTransform(offsetX, offsetY);
            //g.ScaleTransform(200f / LENGTH, 200f / LENGTH);
            float scale = 200f / length;
            g.DrawLine(Pens.Black, 0, 0, 0, length * scale);
            g.DrawLine(Pens.Black, 0, length * scale, length * scale, length * scale);
            for (float y = 0; y < length; y+= length/4f)
            {
                g.DrawString(((length - y)).ToString(), font, Brushes.Black, 0 - g.MeasureString(((length - y)).ToString(), font).Width, y * scale);
                g.DrawLine(new Pen(Color.Black, 2), -2, y * scale, 3, y * scale);
            }

            for(float x = 0; x <= length; x += length/4f)
            {
                g.DrawString((x).ToString(), font, Brushes.Black, x * scale, 200);
                g.DrawLine(new Pen(Color.Black, 2), x * scale, length * scale - 2, x * scale, length * scale + 3);
            }
            #endregion
            font = new Font("Arial", 5);
            #region Draw Points
            foreach (PointF p in points)
            {
                g.FillRectangle(new SolidBrush(Color.LightGreen), p.X * scale, (length - p.Y) * scale, 10, 10);
                g.DrawRectangle(Pens.Black, p.X * scale, (length - p.Y) * scale, 10, 10);
                g.DrawString((int)p.X + ", " + (int)p.Y, font, Brushes.Black, p.X * scale, (length - p.Y) * scale + 10);
            }
            #endregion

            #region Draw Line
            g.DrawLine(Pens.Red, 0, length * scale - GetOutput(0) * scale, length * scale, length * scale - GetOutput(length) * scale);
            #endregion

            if (!running)
                return;

            chart.Series["Loss"].Points.AddY(GetLoss());
            GradientDesent();

        }

        private void GradientDesent()
        {
            foreach (PointF f in points)
            {
                float sum = 0;
                    sum += GetGradient_m(f.Y, f.X);
                float stepSize = sum * LEARNING_RATE * (1f / length);
                float m = this.m - stepSize;
                sum = 0;
                    sum += GetGradient_n(f.Y, f.X);
                stepSize = sum * LEARNING_RATE * (1f / length);
                n = n - stepSize;
                //Console.WriteLine($"Slope: {m} Intercept: {n}");
                this.m = m;
            }
        }

        private float GetGradient_m(float value, float input)
        {
            return -input * 2 * (value - (n + m * input));
        }

        private float GetGradient_n(float value, float input)
        {
            return -2 * (value - (n + m * input));
        }

        private float GetLoss()
        {
            float sum = 0;
            foreach (PointF f in points)
                sum += Math.Abs(f.Y - (n + m * f.X));
            return sum;
        }

        private float GetOutput(float input)
        {
            return m * input + n;
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            chart.Series["Loss"].Points.Clear();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (!running)
            {
                running = true;
                btnStart.Text = "Stop";
                timer.Start();
            }
            else
            {
                running = false;
                btnStart.Text = "Start";
                timer.Stop();
            }
        }

        private void btmDelete_Click(object sender, EventArgs e)
        {
            points = new List<PointF>();
            this.Refresh();
        }

        private void nudScale_ValueChanged(object sender, EventArgs e)
        {
            length = (int)nudScale.Value;
            this.Refresh();
        }

        private float[] AddArray(float[] a1, float[] a2, float[] output = null)
        {
            if (output == null)
                output = a1;
            for (int i = 0; i < output.Length; i++)
                output[i] = a1[i] + a2[i];
            return output;
        }
    }
}
