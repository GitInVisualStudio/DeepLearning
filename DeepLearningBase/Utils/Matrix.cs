using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace DeepLearningBase.Utils
{
    [Serializable]
    public struct Matrix
    {
        private float[][] values;

        public int Width => values.GetLength(0);
        public int Height => values.First().GetLength(0);

        public Matrix(int width, int height)
        {
            values = new float[width][];
            for (int x = 0; x < width; x++)
                values[x] = new float[height];
        }

        public Matrix(params Vector[] v1)
        {
            int width = v1.First().Dimensions;
            int height = v1.Length;
            values = new float[width][];
            for (int x = 0; x < width; x++)
                values[x] = new float[height];
            for (int i = 0; i < Height; i++)
                for (int k = 0; k < Width; k++)
                    this[k, i] = v1[i][k];
        }

        public Matrix(params float[][] v1)
        {
            int width = v1.First().Length;
            int height = v1.Length;
            values = new float[width][];
            for (int x = 0; x < width; x++)
                values[x] = new float[height];
            for (int i = 0; i < Height; i++)
                for (int k = 0; k < Width; k++)
                    this[k, i] = v1[i][k];
        }

        public float this[int x, int y]
        {
            get
            {
                return Values[x][y];
            }
            set
            {
                Values[x][y] = value;
            }
        }

        public float[][] Values { get => values; set => values = value; }

        public static Vector operator *(Matrix m, Vector v)
        {
            Vector product = new Vector(m.Height);
            for (int y = 0; y < m.Height; y++)
            {
                for (int x = 0; x < m.Width; x++)
                {
                    product[y] += v[x] * m[x, y];
                }
            }
            return product;
        }

        public static Matrix operator -(Matrix m1, Matrix m2)
        {
            for (int x = 0; x < m1.Width; x++)
                for (int y = 0; y < m1.Height; y++)
                    m1[x, y] -= m2[x, y];
            return m1;
        }

        public static Matrix operator +(Matrix m1, Matrix m2)
        {
            for (int x = 0; x < m1.Width; x++)
                for (int y = 0; y < m1.Height; y++)
                    m1[x, y] += m2[x, y];
            return m1;
        }

        public static Matrix operator *(Matrix m1, float value)
        {
            for (int x = 0; x < m1.Width; x++)
                for (int y = 0; y < m1.Height; y++)
                    m1[x, y] *= value;
            return m1;
        }

        public Matrix ForEach(Func<float, float> func)
        {
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    this[x, y] = func(this[x, y]);
            return this;
        }
    }
}
