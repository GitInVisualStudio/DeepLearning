using DeepLearningBase.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepLearningBase
{
    [Serializable]
    public class Layer
    {
        private Matrix weights;
        private Vector biases;
        private Vector activation;

        public int Size => Weights.Height;
        public int Connections => Weights.Width;

        public Vector Biases { get => biases; set => biases = value; }
        public Vector Activation { get => activation; set => activation = value; }
        public Matrix Weights { get => weights; set => weights = value; }

        private static Random random = new Random();

        public Layer()
        {
        }

        public Layer(int size, int connections)
        {
            this.Biases = new Vector(connections);
            this.Activation = new Vector(size);
            this.Weights = new Matrix(size, connections);
            this.Weights.ForEach(x => ((float)random.NextDouble() * 2 - 1));
        }

        public Vector GetOutput(Vector activation)
        {
            return (Weights * (this.Activation = activation)).ForEach(MathUtils.Sigmoid);
        }

        public Vector Backprop(Vector error)
        {
            Vector activation = Weights * this.Activation + Biases;
            Matrix deriv_weights = new Matrix(Weights.Width, Weights.Height);
            Vector deriv_biases = new Vector(Weights.Height);
            Vector deriv_activation = new Vector(Weights.Width);
            for (int y = 0; y < Weights.Height; y++)//y = k
            {
                deriv_biases[y] = MathUtils.DerivativeSigmoid(activation[y]) * (error[y]);
                for (int x = 0; x < Weights.Width; x++) //x = J
                {
                    deriv_activation[x] += this.Weights[x, y] * MathUtils.DerivativeSigmoid(activation[y]) * error[y];
                    deriv_weights[x, y] = this.Activation[x] * MathUtils.DerivativeSigmoid(activation[y]) * error[y];
                }
            }
            Biases -= deriv_biases * Network.LEARNING_RATE;
            Weights -= deriv_weights * Network.LEARNING_RATE;
            return deriv_activation * -Network.LEARNING_RATE;
        }

    }
}
