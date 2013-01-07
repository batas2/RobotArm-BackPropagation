namespace Robot
{
    public interface INeuralNetwork
    {
        NNValues Learn(double[] input, double[] output);
        double[] Eval(double[] input);
    }
}