using System.Diagnostics;

namespace Robot
{
    public class RobotArmNormalizationProvider : INormalizationProvider
    {
        private readonly int _inputLength;
        private readonly int _outputLength;

        public RobotArmNormalizationProvider(int inputLength, int outputLength)
        {
            _inputLength = inputLength;
            _outputLength = outputLength;
        }

        #region INormalizationProvider Members

        public double[] NormalizeOutput(double[] values)
        {
            Debug.Assert(values.Length == _outputLength);

            Debug.Assert(values[0] >= 0 && values[0] <= 180);
            Debug.Assert(values[1] >= 0 && values[1] <= 360);

            var result = new double[_outputLength];

            result[0] = (values[0]/180);
            result[1] = (values[1]/180);

            result[0] = (result[0]*0.8) + 0.1;
            result[1] = (result[1]*0.8) + 0.1;

            Debug.Assert(result[0] >= 0.1 && result[0] <= 0.9);
            Debug.Assert(result[1] >= 0.1 && result[1] <= 0.9);

            return result;
        }

        public double[] UnnormalizeOutput(double[] values)
        {
            Debug.Assert(values.Length == _outputLength);

            Debug.Assert(values[0] >= 0.1 && values[0] <= 0.9);
            Debug.Assert(values[1] >= 0.1 && values[1] <= 0.9);

            var result = new double[_outputLength];

            result[0] = (values[0] - 0.1)/0.8;
            result[1] = (values[1] - 0.1)/0.8;

            result[0] *= 180;
            result[1] *= 180;

            Debug.Assert(result[0] >= 0 && result[0] <= 180);
            Debug.Assert(result[1] >= 0 && result[1] <= 360);

            return result;
        }

        public double[] NormalizeInput(double[] values)
        {
            Debug.Assert(values.Length == _inputLength);

            var result = new double[_inputLength];

            result[0] = (values[0] + (RobotArm.ArmLength))/(RobotArm.ArmLength*3);
            result[1] = (values[1] + (RobotArm.ArmLength*2))/(RobotArm.ArmLength*4);

            result[0] = (result[0]*0.8) + 0.1;
            result[1] = (result[1]*0.8) + 0.1;

            Debug.Assert(result[0] >= 0.1 && result[0] <= 0.9);
            Debug.Assert(result[1] >= 0.1 && result[1] <= 0.9);

            return result;
        }

        public double[] UnnormalizeInput(double[] values)
        {
            Debug.Assert(values.Length == _inputLength);

            Debug.Assert(values[0] >= 0.1 && values[0] <= 0.9);
            Debug.Assert(values[1] >= 0.1 && values[1] <= 0.9);

            var result = new double[_inputLength];

            result[0] = (values[0] - 0.1)/0.8;
            result[1] = (values[1] - 0.1)/0.8;

            result[0] = (result[0]*(RobotArm.ArmLength*3)) - (RobotArm.ArmLength);
            result[1] = (result[1]*(RobotArm.ArmLength*4)) - (RobotArm.ArmLength*2);

            return result;
        }

        #endregion
    }
}