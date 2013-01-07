#region

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace Robot
{
    public class NeuralNetwork : INeuralNetwork, ICloneable
    {
        private readonly int _inputLength;
        private readonly int _outputLength;
        private NetworkLayer _inLayer;
        private NetworkLayer _outLayer;

        public NeuralNetwork(int inputLength, int outputLength)
        {
            _inputLength = inputLength;
            _outputLength = outputLength;

            InitLayers();
        }

        #region INeuralNetwork Members

        public NNValues Learn(double[] input, double[] output)
        {
            _inLayer.Values = input;
            _inLayer.Next.CalcForward(input);

            var errors = new double[output.Length];
            for (int i = 0; i < output.Length; i++)
            {
                errors[i] = output[i] - _outLayer.Values[i];
            }

            _outLayer.CalcBackwardOutput(errors);

            var result = new NNValues
                {
                    Errors = errors,
                    Input = input,
                    Output = output,
                    Values = _outLayer.Values,
                    ErrorsSqr = errors.Sum(error => error * error),
                    Weights = GetWeights()
                };

            return result;
        }

        public double[] Eval(double[] input)
        {
            _inLayer.Values = input;
            _inLayer.Next.CalcForward(input);

            var result = new double[_outputLength];
            Array.Copy(_outLayer.Values, result, _outputLength);

            return result;
        }

        #endregion

        private void InitLayers()
        {
            _inLayer = new NetworkLayer(_inputLength, _inputLength);
            var hidden = new NetworkLayer(5, _inLayer);
            var hidden1 = new NetworkLayer(8, hidden);
            var hidden2 = new NetworkLayer(5, hidden1);
            _outLayer = new NetworkLayer(_outputLength, hidden2);
        }

        public IList<double[][]> GetWeights()
        {
            var weights = new List<double[][]>();
            NetworkLayer layer = _inLayer.Next;
            while (layer != null)
            {
                weights.Add(layer.CloneWeights());
                layer = layer.Next;
            }
            return weights;
        }

        public object Clone()
        {
            var resultNN = new NeuralNetwork(_inputLength, _outputLength);
            resultNN._inLayer = (NetworkLayer)_inLayer.Clone();
            resultNN._outLayer = (NetworkLayer)_outLayer.Clone();

            var clonePrevLayer = _inLayer;
            NetworkLayer layer = _inLayer.Next;

            while (layer != null)
            {
                var cloneLayer = (NetworkLayer)layer.Clone();
                cloneLayer.Prev = clonePrevLayer;

                clonePrevLayer = cloneLayer;
                layer = layer.Next;
            }

            _outLayer.Prev = clonePrevLayer;
            return resultNN;
        }
    }
}