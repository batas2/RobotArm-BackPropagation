#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

#endregion

namespace Robot
{
    public class RobotArm
    {
        private const int InputLength = 2;
        private const int OutputLength = 2;

        public static double ArmLength = 80;
        public static double LearnRate = 0.2;

        public static Point AttachPoint = new Point(100, 200);

        private readonly INeuralNetwork _neuralNetwork;
        private readonly INormalizationProvider _normalizationProvider;
        private readonly Random _rand;
        private Dictionary<Point, double[]> _armAnglesCache;
        private IList<NNValues> _nnParameters;

        public RobotArm()
        {
            _rand = new Random();
            _normalizationProvider = new RobotArmNormalizationProvider(InputLength, OutputLength);
            _neuralNetwork = new NeuralNetwork(InputLength, OutputLength);

            CacheArmAngles();
        }

        private void CacheArmAngles()
        {
            _armAnglesCache = new Dictionary<Point, double[]>(180 * 180);

            for (int i = 0; i < 180; i++)
            {
                for (int j = 0; j < 180; j++)
                {
                    var angles = new double[] { i, j };
                    Point point = GetArmPoints(angles)[1];
                    var destPoint = new Point(Math.Round(point.X), Math.Round(point.Y));
                    if (!_armAnglesCache.ContainsKey(destPoint))
                        _armAnglesCache.Add(destPoint, angles);
                }
            }
        }

        private Point RotatePoint(Point p, double angle)
        {
            double s = SinDegree(angle);
            double c = CosDegree(angle);

            // rotate point
            double xnew = p.X * c - p.Y * s;
            double ynew = p.X * s + p.Y * c;

            return new Point(xnew, ynew);
        }

        public double[] GetArmAngles(Point point)
        {
            return _armAnglesCache.ContainsKey(point) ? _armAnglesCache[point] : new double[] { 0, 0 };
        }

        public Point[] GetArmPoints(double[] angles)
        {
            Debug.Assert(angles[0] >= 0 && angles[0] <= 180);
            Debug.Assert(angles[1] >= 0 && angles[1] <= 360);

            double sinX = SinDegree(angles[0]);
            double cosX = CosDegree(angles[0]);

            double sinY = SinDegree(angles[1]);
            double cosY = CosDegree(angles[1]);

            var armPoints = new Point[2];
            armPoints[0] = new Point((sinX * ArmLength),
                                     (-cosX * ArmLength));

            armPoints[1] = new Point((-sinY * ArmLength),
                                     (-cosY * ArmLength));

            armPoints[1] = RotatePoint(armPoints[1], angles[0]);

            armPoints[1].X += armPoints[0].X;
            armPoints[1].Y += armPoints[0].Y;

            return armPoints;
        }

        public double SinDegree(double degree)
        {
            return Math.Sin(degree * Math.PI / 180);
        }

        public double CosDegree(double degree)
        {
            return Math.Cos(degree * Math.PI / 180);
        }

        public double[] RandAngles()
        {
            return new[] { _rand.NextDouble() * 180, _rand.NextDouble() * 180 };
        }


        public IList<NNValues> StartLearning(int iterations, int nnValuesCount = 100)
        {
            int step = 1;
            if (iterations > nnValuesCount)
                step = iterations / nnValuesCount;

            _nnParameters = new List<NNValues>();

            
            for(int i = 0; i < iterations; i++)    
                {
                    double[] randAngles = RandAngles();
                    Point[] armPoints = GetArmPoints(randAngles);

                    double[] input = _normalizationProvider.NormalizeInput(new[] { armPoints[1].X, armPoints[1].Y });
                    double[] output = _normalizationProvider.NormalizeOutput(randAngles);

                    NNValues learnResult = _neuralNetwork.Learn(input, output);

                    if (i % step == 0)
                        _nnParameters.Add(learnResult);
                }

            return _nnParameters;
        }

        public double[] Eval(Point point)
        {
            double[] input = _normalizationProvider.NormalizeInput(new[] { point.X, point.Y });

            double[] nnResult = _neuralNetwork.Eval(input);

            double[] output = _normalizationProvider.UnnormalizeOutput(nnResult);

            return output;
        }
    }
}