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

        private static Random random;

        public Layer()
        {
        }

        public Layer(int size, int connections, int seed = 0)
        {
            if (random == null && seed == 0)
                random = new Random();
            else if(random == null)
                random = new Random(seed);
            this.Biases = new Vector(connections);
            this.Activation = new Vector(size);
            this.Weights = new Matrix(size, connections);
            this.Weights.ForEach(x => ((float)random.NextDouble() * 2 - 1));
            this.biases.ForEach(x => ((float)random.NextDouble() * 2 - 1));
        }

        public Vector GetOutput(Vector activation) => (Weights * (this.Activation = activation)).ForEach(MathUtils.Sigmoid);
        
        public Vector GetDerivError(Vector error) => (Weights * this.Activation + Biases).ForEach(MathUtils.DerivativeSigmoid) * error;

        public Vector GetDerivBiases(Vector error) => error;

        public Matrix GetDerivWeights(Vector error)
        {
            Matrix deriv_weights = new Matrix(Weights.Width, Weights.Height);
            for (int y = 0; y < Weights.Height; y++)
                for (int x = 0; x < Weights.Width; x++)
                    deriv_weights[x, y] = this.Activation[x] * error[y];
            return deriv_weights;
        }

        public Vector GetDerivActivation(Vector error)
        {
            Vector deriv_activation = new Vector(Weights.Width);
            for (int y = 0; y < Weights.Height; y++)
                for (int x = 0; x < Weights.Width; x++)
                    deriv_activation[x] += this.Weights[x, y] * error[y];
            return deriv_activation;
        }

    }
}
