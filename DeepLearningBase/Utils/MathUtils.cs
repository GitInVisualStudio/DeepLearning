using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepLearningBase.Utils
{
    public class MathUtils
    {

        public static float Sigmoid(float value)
        {
            return (float)(1f / (1f + Math.Pow(Math.E, -value)));
        }

        public static float DerivativeSigmoid(float value)
        {
            return Sigmoid(value) * (1 - Sigmoid(value));
        }
    }
}
