using DeepLearningBase.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Security;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DeepLearningBase
{
    [Serializable]
    public class Network
    {
        [NonSerialized]
        private Dictionary<Vector, Vector> trainingData;
        private Layer[] layer;
        public Layer[] Layer { get => layer; set => layer = value; }
        public event EventHandler<float> OnChange;

        [XmlIgnore]
        public Dictionary<Vector, Vector> TrainingData { get => trainingData; set => trainingData = value; }

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

        private int GetOutputIndex_(Vector output)
        {
            int var1 = -1;
            for (int i = 0; i < output.Dimensions; i++)
                if (var1 == -1 || output[i] > output[var1])
                    var1 = i;
            return var1;
        }

        public void SaveNetwork(string path) => ResourceManager.Serialize(this, path);

        public float BackpropBatch(Dictionary<Vector, Vector> batch, Track track)
        {
            float var1 = 0;
            List<object[]> sum = new List<object[]>();
            Parallel.ForEach(batch.Keys, (Vector input) =>
            {
                Vector output = batch[input];
                object[] obj = Backprop(input, output, track);
                lock (sum)
                {
                    sum.Add(obj);
                    var1 += (float)obj[layer.Length];
                }
            });

            if (sum.Count <= 0)
                return -1;

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
                layer.Biases -= deriv_biases * LEARNING_RATE;
                layer.Weights -= deriv_weights * LEARNING_RATE;
            }
            return var1 / sum.Count;
        }

        public object[] Backprop(Vector input, Vector output, Track track)
        {
            input = GetOutput(input);
            int index = GetOutputIndex_(input);
            Vector error = input - output;
            error *= 2;
            object[] values = new object[Layer.Length + 1];

            switch (track)
            {
                case Track.ACC:
                    values[Layer.Length] = output[index];
                    break;
                case Track.LOSS:
                    values[Layer.Length] = (error * error).Sum();
                    break;
                case Track.NONE:
                    values[layer.Length] = 0;
                    break;
            }

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

        public void Train(Track track, Optimizer optimizer, int epochs = 1)
        {
            for (int i = 0; i < epochs; i++)
            {
                switch (optimizer)
                {
                    case Optimizer.GradientDescent:
                        OnChange?.Invoke(this, BackpropBatch(trainingData, track));
                        break;
                    case Optimizer.MiniBatchGradientDescent:
                        float var1 = 0;
                        int runs = 0;
                        for (int index = 0; index < trainingData.Count; index += 100)
                        {
                            Dictionary<Vector, Vector> batch = GetNextBatch(index, 100);
                            float value = BackpropBatch(batch, track);
                            var1 += value;
                            runs++;
                            OnChange?.Invoke(this, var1 / (float)runs);
                        }
                        break;
                    case Optimizer.StochasticGradientDescent:
                        foreach(Vector input in trainingData.Keys)
                        {
                            object[] obj = Backprop(input, trainingData[input], track);
                            for(int k = 0; k < Layer.Length; k++)
                            {
                                Matrix deriv_weights = (Matrix)obj[0];
                                Vector deriv_biases = (Vector)obj[1];
                                this[k].Biases -= deriv_biases * LEARNING_RATE;
                                this[k].Weights -= deriv_weights * LEARNING_RATE;
                            }
                        }
                        break;
                }
            }
        }

        private Dictionary<Vector, Vector> GetNextBatch(int index, int size)
        {
            Dictionary<Vector, Vector> batch = new Dictionary<Vector, Vector>();
            for (int i = 0; i < size; i++)
            {
                if (i + index >= trainingData.Count)
                    break;
                KeyValuePair<Vector, Vector> pair = trainingData.ElementAt(i + index);
                batch.Add(pair.Key, pair.Value);
            }
            return batch;
        }

        public enum Track : int
        {
            ACC = 0,
            LOSS = 1,
            NONE = 2
        }

        public enum Optimizer : int
        {
            MiniBatchGradientDescent = 0,
            StochasticGradientDescent = 1,
            GradientDescent = 2
        }
    }
}
