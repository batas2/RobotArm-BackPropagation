using System;

namespace Robot
{
    public interface INetworkLayer : ICloneable
    {
        int NeuronCount { get; }
        double[] Values { get; set; }
        double[][] Weights { get; }
        NetworkLayer Next { get; }
        NetworkLayer Prev { get; set; }
    }
}