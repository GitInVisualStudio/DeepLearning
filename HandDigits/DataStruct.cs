using DeepLearningBase;
using DeepLearningBase.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandDigits
{
    [Serializable]
    public struct DataStruct
    {
        private Network network;
        private List<Vector> inputs;
        private List<Vector> outputs;

        public Network Network { get => network; set => network = value; }
        public List<Vector> Inputs { get => inputs; set => inputs = value; }
        public List<Vector> Outputs { get => outputs; set => outputs = value; }

        public DataStruct(Network network, Dictionary<Vector, Vector> trainingData)
        {
            this.network = network;
            inputs = trainingData.Keys.ToList();
            outputs = trainingData.Values.ToList();
        }

        public Dictionary<Vector, Vector> GetTrainingData()
        {
            Dictionary<Vector, Vector> trainingData = new Dictionary<Vector, Vector>();
            for (int i = 0; i < inputs.Count; i++)
            {
                trainingData.Add(inputs[i], outputs[i]);
            }
            return trainingData;
        }
    }
}
