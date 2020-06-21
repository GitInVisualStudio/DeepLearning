using DeepLearningBase.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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

        public Network(string path)
        {
            Network network = ResourceManager.Deserialize<Network>(path);
            this.Layer = network.Layer;
        }

        public Vector GetOutput(Vector input)
        {
            for(int i = 0; i < Layer.Length; i++)
                input = this[i].GetOutput(input);
            return input;
        }

        public void SaveNetwork(string path) => ResourceManager.Serialize(this, path);

        public void BackpropBatch(Dictionary<Vector, Vector> batch)
        {
            foreach(Vector input in batch.Keys)
            {
                Vector output = batch[input];
                Backprop(input, output);
            }
        }

        public void Backprop(Vector input, Vector output)
        {
            Vector error = output;
            for(int i = Layer.Length - 1; i >= 0; i--)
                error = this[i].Backprop(error);
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
