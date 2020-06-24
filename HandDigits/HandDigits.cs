using DeepLearningBase;
using DeepLearningBase.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace HandDigits
{
    public partial class HandDigits : Form
    {
        private Dictionary<Vector, Vector> trainingData;
        private Network network;
        private Canvas canvas;
        private Timer timer;

        public Dictionary<Vector, Vector> TrainingData { get => trainingData; set => trainingData = value; }
        public Network Network { get => network; set => network = value; }

        public HandDigits()
        {
            InitializeComponent();

            network = new Network(28 * 28, 16, 16, 10);

            DoubleBuffered = true;
            Controls.Add(canvas = new Canvas()
            {
                Width = 200,
                Height = 200
            });

            canvas.OnChange += Canvas_OnChange;

            timer = new Timer()
            {
                Interval = 1
            };

            trainingData = new Dictionary<Vector, Vector>();
        }

        private void Canvas_OnChange(object sender, Vector e)
        {
            lbl.Text = network.GetOutputIndex(e).ToString();
        }

        private void btnReset_Click(object sender, EventArgs e) => canvas.Reset();

        private void btnAdd_Click(object sender, EventArgs e)
        {
            Vector vector = canvas.GetVector();
            Vector output = new Vector(10);
            output[(int)nudLabel.Value] = 1;
            trainingData.Add(vector, output);
            canvas.Reset();
            this.Refresh();
        }

        public Bitmap VectorToBitmap(Vector vector)
        {
            int size = (int)Math.Sqrt(vector.Dimensions);
            Bitmap bitmap = new Bitmap(size, size);
            for (int x = 0; x < bitmap.Width; x++)
                for (int y = 0; y < bitmap.Height; y++)
                {
                    int color = (int)(255 - vector[x + y * size] * 255.0f);
                    bitmap.SetPixel(x, y, Color.FromArgb(color, Color.Black));
                }
            return new Bitmap(bitmap, size * 2, size * 2);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Render(e.Graphics);
        }

        private void Render(Graphics g)
        {
            g.DrawString("Training data: ", new Font("Arial", 10), Brushes.Black, Width - 17 - 28 * 2 - 100, 0);
            int offset = 0;
            foreach(Vector vector in trainingData.Keys)
            {
                Bitmap bitmap = VectorToBitmap(vector);
                
                g.DrawImage(bitmap, Width - 17 - 28 * 2, offset);
                offset += 28 * 2;
            }
        }

        private void btnTrain_Click(object sender, EventArgs e)
        {
            if (btnTrain.Text == "Train")
            {
                timer.Start();
                btnTrain.Text = "Stop training";
            }
            else
            {
                timer.Stop();
                btnTrain.Text = "Train";
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            using(FileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "xml files (*.xml)|*.xml|All files (*.*)|*.*";
                dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                if (dialog.ShowDialog() == DialogResult.Cancel)
                    return;
                ResourceManager.Serialize(new DataStruct(network, trainingData), dialog.FileName);
            }
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            using (FileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "xml files (*.xml)|*.xml|All files (*.*)|*.*";
                dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                if (dialog.ShowDialog() == DialogResult.Cancel)
                    return;
                DataStruct var1 = ResourceManager.Deserialize<DataStruct>(dialog.FileName);
                this.trainingData = var1.GetTrainingData();
                this.network = var1.Network;
                this.Refresh();
            }
        }

        private void btnOpenNetwork_Click(object sender, EventArgs e)
        {
            using (FileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "xml files (*.xml)|*.xml|All files (*.*)|*.*";
                dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                if (dialog.ShowDialog() == DialogResult.Cancel)
                    return;
                this.network = new Network(dialog.FileName);
                this.Refresh();
            }
        }
    }
}
