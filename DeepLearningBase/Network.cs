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
        private Matrix[] deriv_weights;
        private Vector[] deriv_biases;
        public event EventHandler<float> OnChange;
        private float learning_rate;

        [XmlIgnore]
        public Dictionary<Vector, Vector> TrainingData { get => trainingData; set => trainingData = value; }

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
            deriv_biases = new Vector[layer.Length];
            deriv_weights= new Matrix[layer.Length];
        }

        public Network(float learning_rate, int[] layer, int seed = 0)
        {
            this.learning_rate = learning_rate;
            this.Layer = new Layer[layer.Length - 1];
            for (int i = 0; i < layer.Length - 1; i++)
                this.Layer[i] = new Layer(layer[i], layer[i + 1], seed);
            deriv_biases = new Vector[layer.Length];
            deriv_weights = new Matrix[layer.Length];
        }

        public Network(string path)
        {
            Network network = ResourceManager.Deserialize<Network>(path);
            this.Layer = network.Layer;
            deriv_biases = new Vector[layer.Length];
            deriv_weights = new Matrix[layer.Length];
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
            float sum = 0;
            object sumLocker = new object();
            Parallel.ForEach(batch.Keys, (Vector input) =>
            {
                Vector output = batch[input];
                var (acc, _) = Backprop(input, output);
                lock (sumLocker) //no im not stupid
                    sum += acc;
            });

            AddDeriv();

            return sum / (float)batch.Keys.Count;
        }

        private void AddDeriv()
        {
            Parallel.For(0, layer.Length, (int k) =>
            {
                Layer layer = this[k];
                layer.Biases -= deriv_biases[k] * learning_rate;
                layer.Weights -= deriv_weights[k] * learning_rate;
                deriv_biases[k] = default;
                deriv_weights[k] = default;
            });
        }

        public (float, float) Backprop(Vector input, Vector output)
        {
            input = GetOutput(input);
            int index = GetOutputIndex_(input);
            Vector error = input - output;
            error *= 2;
            
            for (int i = Layer.Length - 1; i >= 0; i--)
            {
                Layer layer = this[i];
                error = layer.GetDerivError(error);
                Matrix weights = layer.GetDerivWeights(error);
                Vector biases = layer.GetDerivBiases(error);
                Vector activation = layer.GetDerivActivation(error);

                deriv_weights[i] += weights;
                deriv_biases[i] += biases;

                error = activation;
            }

            return (output[index], (error * error).Sum());
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

        private void SetLearningRate(int epoch)
        {           
            learning_rate = (1 / (float)(2 + 1 * epoch)) * 0.2f;
        }

        public void Train(Track track, Optimizer optimizer, int epochs = 1, int batch_size = 100)
        {
            for (int i = 0; i < epochs; i++)
            {
                SetLearningRate(i);
                switch (optimizer)
                {
                    case Optimizer.GradientDescent:
                        OnChange?.Invoke(this, BackpropBatch(trainingData, track));
                        break;
                    case Optimizer.MiniBatchGradientDescent:
                        float var1 = 0;
                        int runs = 0;
                        for (int index = 0; index < trainingData.Count; index += batch_size)
                        {
                            Dictionary<Vector, Vector> batch = GetNextBatch(index, batch_size);
                            float value = BackpropBatch(batch, track);
                            var1 += value;
                            runs++;
                            OnChange?.Invoke(this, var1/(float)runs);
                        }
                        break;
                    case Optimizer.StochasticGradientDescent:
                        foreach (Vector input in trainingData.Keys)
                        {
                            var (acc, los) = Backprop(input, trainingData[input]);
                            AddDeriv();
                        }
                        break;
                }
            }
        }

        public Dictionary<Vector, Vector> GetNextBatch(int index, int size)
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

        public void LoadTrainingData(string path) => this.trainingData = ResourceManager.Deserialize<TrainingData>(path).GetTrainingData();        

        public void SaveTrainingData(string path) => ResourceManager.Serialize(new TrainingData(this), path);

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
