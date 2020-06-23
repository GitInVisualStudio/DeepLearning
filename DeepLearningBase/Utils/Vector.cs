using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DeepLearningBase.Utils
{
    [Serializable]
    public struct Vector
    {
        private float[] values;

        public float this[int index]
        {
            get
            {
                return Values[index];
            }
            set
            {
                Values[index] = value;
            }
        }

        public int Dimensions
        {
            get
            {
                if (values == null)
                    return 0;
                return values.Length;
            }
        }

        public float Length
        {
            get
            {
                float var1 = 0;
                return var1;
            }
        }

        public float[] Values { get => values; set => values = value; }

        public float X => values[0];
        public float Y => values[1];

        public void Normalize() => this /= Length;

        public Vector(int dimensions)
        {
            this.values = new float[dimensions];
        }

        public Vector(params float[] values)
        {
            this.values = values;
        }

        public static Vector operator +(Vector v1, float v2)
        {
            for (int i = 0; i < v1.Dimensions; i++)
                v1[i] += v2;
            return v1;
        }

        public static Vector operator +(Vector v1, Vector v2)
        {
            for (int i = 0; i < v1.Dimensions; i++)
                v1[i] += v2[i];
            return v1;
        }

        public static Vector operator -(Vector v1, Vector v2)
        {
            for (int i = 0; i < v1.Dimensions; i++)
                v1[i] -= v2[i];
            return v1;
        }


        public static Vector operator -(Vector v1, float v2)
        {
            return v1 + (-v2);
        }

        public static Vector operator *(Vector v1, float v2)
        {
            for (int i = 0; i < v1.Dimensions; i++)
                v1[i] *= v2;
            return v1;
        }

        public static Vector operator *(Vector v1, Vector v2)
        {
            for (int i = 0; i < v1.Dimensions; i++)
                v1[i] *= v2[i];
            return v1;
        }

        public static Vector operator /(Vector v1, float v2)
        {
            for (int i = 0; i < v1.Dimensions; i++)
                v1[i] /= v2;
            return v1;
        }

        public override string ToString()
        {
            string output = "";
            for (int i = 0; i < Dimensions; i++)
                output += $" d_{i}: " + this[i];
            return output;
        }

        public void ForEach(Action<float> action)
        {
            for (int i = 0; i < Dimensions; i++)
                action(this[i]);
        }

        public Vector ForEach(Func<float, float> func)
        {
            for (int i = 0; i < Dimensions; i++)
                this[i] = func(this[i]);
            return this;
        }

        public float Sum()
        {
            float var1 = 0;
            ForEach(x => var1 += x);
            return var1;
        }
    }
}
