using DeepLearningBase;
using DeepLearningBase.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Test.Properties;

namespace Test
{
    public partial class MainWindow : Form
    {
        public const int IMG_SIZE = 28;
        private string[] data;
        private Dictionary<Vector, Vector> trainingData;
        private Network network;

        public MainWindow()
        {
            InitializeComponent();
            DoubleBuffered = true;

            data = Regex.Split(Resources.train, "\n");
            trainingData = new Dictionary<Vector, Vector>();
            List<Image> images = LoadImages(0, 1000);
            foreach (Image image in images)
            {
                if (image == null)
                    continue;
                Vector output = new Vector(10);
                output[image.Label] = 1;
                trainingData.Add(image.Vector, output);
            }

            network = new Network(0.1f, new int[] { IMG_SIZE * IMG_SIZE, 16, 16, 10 });

            network.TrainingData = trainingData;
            network.OnChange += Network_OnBatch;
        }

        private void Network_OnBatch(object sender, float e)
        {
            Console.WriteLine(e);
            chart.Series["Accuracy"].Points.AddY(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            network.SaveNetwork(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\network.xml");
        }

        private List<Image> LoadImages(int index, int length)
        {
            List<Image> images = new List<Image>();
            Parallel.For(0, length, (int i) =>
            {
                if (i + index >= data.Length)
                    index = 0;
                if (i + index >= data.Length)
                    return;
                string[] pixel = Regex.Split(data[i + index], ",");
                if (pixel.Length <= 1)
                    return;
                Vector vector = new Vector(pixel.Length - 1);
                for (int k = 0; k < pixel.Length - 1; k++)
                    vector[k] = float.Parse(pixel[k + 1]) / 255.0f;
                lock (images)
                    images.Add(new Image(int.Parse(pixel.First()), vector));

            });
            return images;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            network.Train(Network.Track.ACC, Network.Optimizer.MiniBatchGradientDescent, 3);
        }

        private void Test_Click(object sender, EventArgs e)
        {
            Dictionary<Vector, Vector> batch = network.GetNextBatch(0, 100);
            float acc = 0;
            int right = 0;
            int tested = 0;
            foreach (Vector input in batch.Keys)
            {
                Vector output = batch[input];
                int index = network.GetOutputIndex(input);
                tested++;
                if (output[index] == 1)
                    right++;
                acc = (float)right / (float)tested;
            }
            lblAcc.Text = acc.ToString();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            using(FileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "xml files (*.xml)|*.xml";
                if (dialog.ShowDialog() == DialogResult.Cancel)
                    return;
                network = new Network(dialog.FileName);
                network.TrainingData = trainingData;
            }
        }
    }
}
