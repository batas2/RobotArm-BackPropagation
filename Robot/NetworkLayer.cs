#region

using System;
using System.Threading.Tasks;

#endregion

namespace Robot
{
    public class NetworkLayer : INetworkLayer
    {
        private readonly int _neuronCount;
        private readonly Random _rand = new Random();


        private NetworkLayer _next;
        private NetworkLayer _prev;

        private double[][] _weights;
        private double[] _sums;
        private double[] _values;

        public NetworkLayer(int neuronCount)
        {
            _neuronCount = neuronCount;
            _sums = new double[_neuronCount];
            _values = new double[_neuronCount];
        }

        public NetworkLayer(int neuronCount, NetworkLayer prev)
            : this(neuronCount)
        {
            Prev = prev;
            InitRandomWeights(neuronCount, prev.NeuronCount);
        }

        public NetworkLayer(int neuronCount, double[][] weights)
            : this(neuronCount)
        {
            _weights = weights;
        }

        public NetworkLayer(int neuronCount, int prevNeuronCount)
            : this(neuronCount)
        {
            InitRandomWeights(neuronCount, prevNeuronCount);
        }

        private void InitRandomWeights(int neuronCount, int prevNeuronCount)
        {
            _weights = new double[prevNeuronCount][];
            for (int i = 0; i < prevNeuronCount; i++)
            {
                _weights[i] = new double[neuronCount];
                for (int j = 0; j < neuronCount; j++)
                {
                    _weights[i][j] = _rand.NextDouble();
                }
            }
        }


        #region ICloneable Members

        public double[][] CloneWeights()
        {
            var w = new double[Weights.Length][];

            for (int i = 0; i < Weights.Length; i++)
            {
                w[i] = new double[Weights[i].Length];
                Array.Copy(Weights[i], w[i], Weights[i].Length);
            }

            return w;
        }

        public object Clone()
        {
            return new NetworkLayer(_neuronCount, CloneWeights());
        }

        #endregion

        #region INetworkLayer Members

        public int NeuronCount
        {
            get { return _neuronCount; }
        }

        public double[] Values
        {
            get { return _values; }
            set { _values = value; }
        }

        public double[][] Weights
        {
            get { return _weights; }
        }

        public NetworkLayer Next
        {
            get { return _next; }
            set
            {
                value.Prev = this;
                _next = value;
            }
        }

        public NetworkLayer Prev
        {
            get { return _prev; }
            set
            {
                _prev = value;
                value._next = this;
            }
        }

        #endregion

        public void CalcForward(double[] input)
        {
            //Debug.WriteLine(input.Aggregate("VALUES: ", (current, t) => current + (t + " ; ")));

            _sums = new double[_neuronCount];
            _values = new double[_neuronCount];

            for (int i = 0; i < _neuronCount; i++)
            {
                for (int j = 0; j < Prev.NeuronCount; j++)
                {
                    _sums[i] += Weights[j][i] * Prev.Values[j];
                }

                _values[i] = Sigmoid(_sums[i]);
            }
            if (Next != null)
            {
                Next.CalcForward(_values);
            }
        }

        private static double SigmoidD(double d)
        {
            double sig = Sigmoid(d);
            return sig * (1 - sig);
        }

        private static double Sigmoid(double d)
        {
            return 1.0 / (1.0 + Math.Exp(1.0 - d));
        }

        public void CalcBackwardOutput(double[] errors)
        {
            //Debug.Assert(errors.Length == _values.Length && errors.Length == _neuronCount);

            var delta = new double[NeuronCount];
            for (int i = 0; i < NeuronCount; i++)
            {
                delta[i] = errors[i] * SigmoidD(_sums[i]);
            }

            for (int i = 0; i < NeuronCount; i++)
            {
                for (int j = 0; j < Prev.NeuronCount; j++)
                {
                    Weights[j][i] += RobotArm.LearnRate * Prev.Values[j] * delta[i];
                }
            }

            if (Prev == null) return;
            Prev.CalcBackward(delta);
        }

        public void CalcBackward(double[] deltaIn)
        {
            if (Prev == null) return;
            var deltaOut = new double[NeuronCount];
            var errors = new double[NeuronCount];

            for (int j = 0; j < NeuronCount; j++)
            {
                for (int l = 0; l < Next.NeuronCount; l++)
                {
                    errors[j] += Next.Weights[j][l] * deltaIn[l];
                }

                deltaOut[j] = SigmoidD(_sums[j]) * errors[j];
            }

            for (int j = 0; j < NeuronCount; j++)
            {
                for (int k = 0; k < Prev.NeuronCount; k++)
                {
                    Weights[k][j] += RobotArm.LearnRate * Prev.Values[k] * deltaOut[j];
                }
            }

            Prev.CalcBackward(deltaOut);
        }
    }
}