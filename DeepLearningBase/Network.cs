using DeepLearningBase.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Security;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DeepLearningBase
{
    [Serializable]
    public class Network
    {
        private Layer[] layer;
        public Layer[] Layer { get => layer; set => layer = value; }
        public static float LEARNING_RATE = 0.1f;

        public Layer this[int index]
        {
            get
            {
                return Layer[index];
            }
        }

        public Network()
        {
        }

        public Network(params int[] layer)
        {
            this.Layer = new Layer[layer.Length - 1];
            for(int i = 0; i < layer.Length - 1; i++)
                this.Layer[i] = new Layer(layer[i], layer[i + 1]);
        }

        public Network(float learning_rate, int[] layer, int seed = 0)
        {
            LEARNING_RATE = learning_rate;
            this.Layer = new Layer[layer.Length - 1];
            for (int i = 0; i < layer.Length - 1; i++)
                this.Layer[i] = new Layer(layer[i], layer[i + 1], seed);
        }

        public Network(string path)
        {
            Network network = ResourceManager.Deserialize<Network>(path);
            this.Layer = network.Layer;
        }

        /// <summary>
        /// returns a vector of the activation
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public Vector GetOutput(Vector input)
        {
            for(int i = 0; i < Layer.Length; i++)
                input = this[i].GetOutput(input);
            return input;
        }

        /// <summary>
        /// returns the index of the neuron with the most activation
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public int GetOutputIndex(Vector input)
        {
            int var1 = -1;
            input = GetOutput(input);
            for (int i = 0; i < input.Dimensions; i++)
                if (var1 == -1 || input[i] > input[var1])
                    var1 = i;
            return var1;
        }

        public void SaveNetwork(string path) => ResourceManager.Serialize(this, path);

        public void BackpropBatch(Dictionary<Vector, Vector> batch)
        {
            List<object[]> sum = new List<object[]>();
            Parallel.ForEach(batch.Keys, (Vector input) =>
            {
                Vector output = batch[input];
                object[] obj = Backprop(input, output);
                lock (sum)
                    sum.Add(obj);
            });

            if (sum.Count <= 0)
                return;

            for (int k = 0; k < layer.Length; k++)
            {
                object[] values = (object[])sum[0][k];
                Matrix deriv_weights = (Matrix)values[0];
                Vector deriv_biases = (Vector)values[1];
                Parallel.For(1, sum.Count, (int i) =>
                {
                    object[] v = (object[])sum[i][k];
                    deriv_weights += (Matrix)v[0];
                    deriv_biases += (Vector)v[1];
                });
                Layer layer = this[k];
                layer.Biases -= deriv_biases / sum.Count;
                layer.Weights -= deriv_weights / sum.Count;
            }
        }

        public object[] Backprop(Vector input, Vector output)
        {
            Vector error = GetOutput(input) - output;
            error *= 2;
            object[] values = new object[Layer.Length];
            for (int i = Layer.Length - 1; i >= 0; i--)
            {
                Layer layer = this[i];
                error = layer.GetDerivError(error);
                Matrix weights = layer.GetDerivWeights(error);
                Vector biases = layer.GetDerivBiases(error);
                Vector activation = layer.GetDerivActivation(error);

                values[i] = new object[] { weights, biases };
                error = activation;
            }
            return values;
        }

        public Vector GetError(Vector input, Vector output)
        {
            Vector error = GetOutput(input) - output;
            return error * error;
        }

        public float GetAverageLoss(Dictionary<Vector, Vector> batch)
        {
            float loss = 0;
            foreach(Vector input in batch.Keys)
            {
                Vector output = batch[input];
                loss += GetError(input, output).Sum();
            }
            return loss / batch.Count;
        }
    }
}
