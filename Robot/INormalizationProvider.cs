namespace Robot
{
    public interface INormalizationProvider
    {
        double[] NormalizeOutput(double[] values);
        double[] UnnormalizeOutput(double[] values);
        double[] NormalizeInput(double[] values);
        double[] UnnormalizeInput(double[] values);
    }
}