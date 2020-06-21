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
using System.Threading.Tasks;
using System.Windows.Forms;
using Test.Properties;

namespace Test
{
    public partial class MainWindow : Form
    {
        public const int IMG_SIZE = 28;
        private string[] data;
        private Timer timer;
        private int index;
        private Network network;

        public MainWindow()
        {
            InitializeComponent();
            network = new Network(IMG_SIZE * IMG_SIZE, 16, 16, 10);
            data = Regex.Split(Resources.train, "\n");
            timer = new Timer()
            {
                Interval = 1
            };
            timer.Tick += Timer_Tick;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            network.SaveNetwork(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\network.xml");
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            Dictionary<Vector, Vector> batch = GetNextBatch();
            network.BackpropBatch(batch);
            float loss = network.GetAverageLoss(batch);
            chart.Series["Error"].Points.AddY(loss);
        }

        private Dictionary<Vector, Vector> GetNextBatch()
        {
            Dictionary<Vector, Vector> batch = new Dictionary<Vector, Vector>();
            List<Image> images = LoadImages(index, 100);
            foreach(Image image in images)
            {
                Vector output = new Vector(10);
                output[image.Label] = 1;
                batch.Add(image.Vector, output);
            }
            index += 100;
            return batch;
        }

        private List<Image> LoadImages(int index, int length)
        {
            List<Image> images = new List<Image>();

            if (index >= data.Length)
                this.index = index = 0;

            for(int i = index; i < data.Length; i++)
            {
                if (i > index + length)
                    continue;
                string[] pixel = Regex.Split(data[i], ",");
                if (pixel.Length <= 1)
                    continue;
                Vector vector = new Vector(pixel.Length - 1);
                for(int k = 0; k < pixel.Length - 1; k++)
                    vector[k] = float.Parse(pixel[k + 1]) / 255.0f;
                images.Add(new Image(int.Parse(pixel.First()), vector));
            }
            return images;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (timer.Enabled)
                timer.Stop();
            else
                timer.Start();
        }
    }
}
