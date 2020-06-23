using DeepLearningBase.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HandDigits
{
    public class Canvas : Panel
    {
        private List<List<Vector>> lines;
        private List<Vector> line;
        private bool down;
        private Bitmap bitmap;
        public event EventHandler<Vector> OnChange;

        public Canvas() : base()
        {
            lines = new List<List<Vector>>();
            line = new List<Vector>();
            DoubleBuffered = true;
        }

        public void Reset()
        {
            line.Clear();
            foreach (List<Vector> line in lines)
                line.Clear();
            lines.Clear();
            this.Refresh();
        }

        public Vector GetVector()
        {
            Bitmap bitmap = new Bitmap(28, 28);
            float scaleX = 28f / Width;
            float scaleY = 28f / Height;

            Graphics g = Graphics.FromImage(bitmap);
            g.ScaleTransform(scaleX, scaleY);
            Render(g);
            g.Dispose();

            Vector vector = new Vector(bitmap.Width * bitmap.Height);
            for (int x = 0; x < bitmap.Width; x++)
                for (int y = 0; y < bitmap.Height; y++)
                    vector[x + y * bitmap.Width] = bitmap.GetPixel(x, y).R / 255.0f;

            return vector;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            down = true;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (down)
            {
                line.Add(new Vector(e.X, e.Y));
                this.Refresh();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if(down)
            {
                if (line.Count > 0)
                    lines.Add(line);
                line = new List<Vector>();
                OnChange?.Invoke(this, GetVector());
            }
            down = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.FillRectangle(Brushes.Black, 0, 0, Width, Height);
            if(bitmap != null)
            {
                e.Graphics.DrawImage(bitmap, 0, 0);
                bitmap = null;
                return;
            }
            Render(e.Graphics);
        }

        private void Render(Graphics g)
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Vector point;
            if (line.Count > 0)
            {
                point = line.First();
                for (int i = 1; i < line.Count; i++)
                {
                    Vector next = line[i];
                    g.DrawLine(new Pen(Color.White, 5), point.X, point.Y, next.X, next.Y);
                    point = next;
                }
            }
            foreach (List<Vector> line in lines)
            {
                point = line.First();
                for (int i = 1; i < line.Count; i++)
                {
                    Vector next = line[i];
                    g.DrawLine(new Pen(Color.White, 5), point.X, point.Y, next.X, next.Y);
                    point = next;
                }
            }
        }
    }
}
