using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepLearning4Deppen
{
    public class Data
    {
        private int label;
        private List<float> image;

        public int Label { get => label; set => label = value; }
        public List<float> Image { get => image; set => image = value; }

        public Data(int label, List<float> image)
        {
            Label = label;
            Image = image;
        }
    }
}
