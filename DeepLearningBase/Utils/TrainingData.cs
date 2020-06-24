using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepLearningBase.Utils
{
    /// <summary>
    /// struct that can be serialized, bc a dictionary cant
    /// </summary>
    [Serializable]
    public struct TrainingData
    {
        private List<Vector> inputs;
        private List<Vector> outputs;

        public List<Vector> Inputs { get => inputs; set => inputs = value; }
        public List<Vector> Outputs { get => outputs; set => outputs = value; }

        public TrainingData(Network network)
        {
            inputs = network.TrainingData.Keys.ToList();
            outputs = network.TrainingData.Values.ToList();
        }

        public Dictionary<Vector, Vector> GetTrainingData()
        {
            Dictionary<Vector, Vector> trainingData = new Dictionary<Vector, Vector>();
            for (int i = 0; i < inputs.Count; i++)
                trainingData.Add(inputs[i], outputs[i]);
            return trainingData;
        }

    }
}
