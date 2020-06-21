using DeepLearningBase.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    public class Image
    {
        private int label;
        private Vector vector;

        public Image(int label, Vector vector)
        {
            this.Label = label;
            this.Vector = vector;
        }

        public int Label { get => label; set => label = value; }
        public Vector Vector { get => vector; set => vector = value; }

        public Bitmap GetBitmap()
        {
            Bitmap bitmap = new Bitmap(MainWindow.IMG_SIZE, MainWindow.IMG_SIZE);
            int height = 0;
            for(int i = 0; i < vector.Dimensions; i++)
            {
                bitmap.SetPixel(i % MainWindow.IMG_SIZE, height, Color.FromArgb((int)(vector[i] * 255), 0,0,0));
                if (i % MainWindow.IMG_SIZE == 0 && i > 0)
                    height++;
            }
            return bitmap;
        }
    }
}
