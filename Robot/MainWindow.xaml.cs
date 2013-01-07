#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.DataVisualization.Charting;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

#endregion

namespace Robot
{
    /// <summary>
    ///   Interaction logic for MainWindow.Xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private RobotArm _robotArm = new RobotArm();
        private IList<Line> _armArea;

        private bool _learned;
        private IList<NNValues> _nnValues;


        private int _nnValuesCount = 100;
        private int _iterations;
        private int _step
        {
            get
            {
                int step = 1;
                if (_iterations > _nnValuesCount)
                    step = _iterations / _nnValuesCount;
                return step;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            GenerateArmArea();
        }

        private void BtnLearnClick(object sender, RoutedEventArgs e)
        {
            _robotArm = new RobotArm();

            _iterations = Convert.ToInt32(textBox1.Text);
            _nnValues = _robotArm.StartLearning(_iterations, _nnValuesCount);

            var weights = new List<string>();
            for (int i = 0; i < _nnValues[0].Weights.Count; i++)
            {
                for (int j = 0; j < _nnValues[0].Weights[i].Length; j++)
                {
                    weights.Add(string.Format("N-{0}-{1}", i, j));
                }
            }
            Weigths.ItemsSource = weights;

            radioVal.IsChecked = true;
            _learned = true;
        }

        private void GenerateArmArea()
        {
            if (_armArea == null)
            {
                _armArea = new List<Line>(180 * 3);

                var anglesList = new List<double[]>(180 * 3);
                for (int i = 0; i < 180; i++)
                {
                    anglesList.Add(new double[] { 180, i });
                }

                for (int i = 180; i >= 0; i--)
                {
                    anglesList.Add(new double[] { 0, i });
                }

                for (int i = 0; i < 180; i++)
                {
                    anglesList.Add(new double[] { i, 0 });
                }

                Point[] points = _robotArm.GetArmPoints(anglesList[0]);
                var beforePoint = new Point(points[1].X + RobotArm.AttachPoint.X, points[1].Y + RobotArm.AttachPoint.Y);

                for (int i = 1; i < anglesList.Count; i++)
                {
                    points = _robotArm.GetArmPoints(anglesList[i]);

                    var point = new Point(points[1].X + RobotArm.AttachPoint.X, points[1].Y + RobotArm.AttachPoint.Y);

                    var myLine = new Line
                        {
                            Stroke = Brushes.Brown,
                            X1 = beforePoint.X,
                            Y1 = beforePoint.Y,
                            X2 = point.X,
                            Y2 = point.Y,
                            StrokeThickness = 1
                        };
                    _armArea.Add(myLine);

                    beforePoint = point;
                }

                foreach (Line line in _armArea)
                {
                    Canvas.Children.Add(line);
                }
            }
        }

        private void Canvas1MouseDown(object sender, MouseButtonEventArgs e)
        {
            Canvas1MouseMove(sender, e);
        }

        protected void DrawLine(double x1, double y1, double x2, double y2)
        {
            DrawLine(x1, y1, x2, y2, Brushes.Black);
        }

        protected void DrawLine(double x1, double y1, double x2, double y2, Brush brush)
        {
            var myLine = new Line
                {
                    Stroke = brush,
                    X1 = x1 % Canvas.Width,
                    X2 = x2 % Canvas.Width,
                    Y1 = y1 % Canvas.Height,
                    Y2 = y2 % Canvas.Height,
                    StrokeThickness = 1
                };
            Canvas.Children.Add(myLine);
        }

        private void Canvas1MouseMove(object sender, MouseEventArgs e)
        {
            if (!_learned) return;

            Point point = e.GetPosition(Canvas);

            point.X -= RobotArm.AttachPoint.X;
            point.Y -= RobotArm.AttachPoint.Y;

            double[] resultAngles = _robotArm.Eval(new Point(point.X, point.Y));
            double[] previewAngles = _robotArm.GetArmAngles(point);

            Repaint();
            DrawArm(resultAngles);
            if (previewAngles[0] != 0 && previewAngles[1] != 0)
                DrawArm(previewAngles, Brushes.Red);
        }

        private void Repaint()
        {
            Canvas.Children.RemoveRange(180 * 3, Canvas.Children.Count - 180 * 3);
        }

        private void DrawArm(double[] angles, Brush brush)
        {
            Point[] points = _robotArm.GetArmPoints(angles);

            points[0].X += RobotArm.AttachPoint.X;
            points[0].Y += RobotArm.AttachPoint.Y;

            points[1].X += RobotArm.AttachPoint.X;
            points[1].Y += RobotArm.AttachPoint.Y;

            DrawLine(RobotArm.AttachPoint.X, RobotArm.AttachPoint.Y, points[0].X, points[0].Y, brush);
            DrawLine(points[0].X, points[0].Y, points[1].X, points[1].Y, brush);
        }

        private void DrawArm(double[] angles)
        {
            DrawArm(angles, Brushes.Black);
        }

        private void BtnArmTestClick(object sender, RoutedEventArgs e)
        {
            var t = new Task(delegate
                {
                    for (int i = 0; i < 180; i++)
                    {
                        for (int j = 0; j < 180; j++)
                        {
                            var angles = new double[] { i, j };

                            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                                {
                                    Repaint();
                                    DrawArm(angles, Brushes.Blue);
                                    //Thread.Sleep(1);

                                    //points[1].X += RobotArm.AttachPoint.X;
                                    //points[1].Y += RobotArm.AttachPoint.Y;

                                    //var point = new Rectangle { Width = 1, Height = 1, Fill = new SolidColorBrush(Colors.Black) };

                                    //Canvas.SetTop(point, points[1].Y);
                                    //Canvas.SetLeft(point, points[1].X);

                                    //Canvas.Children.Add(point);
                                }));
                        }
                    }
                });
            t.Start();
        }

        private void radioSerieA_Checked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            if (checkBox == null) return;
            bool? value = checkBox.IsChecked;
            LineSerieA.Visibility = value.HasValue && value.Value ? Visibility.Visible : Visibility.Hidden;
        }

        private void radioSerieB_Checked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            if (checkBox == null) return;
            bool? value = checkBox.IsChecked;
            LineSerieB.Visibility = value.HasValue && value.Value ? Visibility.Visible : Visibility.Hidden;
        }

        private void radioVal_Checked(object sender, RoutedEventArgs e)
        {
            if (radioVal.IsChecked.HasValue && radioVal.IsChecked.Value)
            {
                ResetChartSeries();
                for (int i = 0; i < _nnValues[0].Values.Length; i++)
                {
                    int i1 = i;
                    AppendChartSerie(_nnValues.Select((t, index) => new KeyValuePair<int, double>(index * _step, t.Values[i1])));
                }
            }

            radioSerieA.IsChecked = true;
            radioSerieB.IsChecked = true;
        }


        private void radioOut_Checked(object sender, RoutedEventArgs e)
        {
            if (radioOut.IsChecked.HasValue && radioOut.IsChecked.Value)
            {
                ResetChartSeries();
                for (int i = 0; i < _nnValues[0].Output.Length; i++)
                {
                    int i1 = i;
                    AppendChartSerie(_nnValues.Select((t, index) => new KeyValuePair<int, double>(index * _step, t.Output[i1])));
                }
            }

            radioSerieA.IsChecked = true;
            radioSerieB.IsChecked = true;
        }

        private void radioIn_Checked(object sender, RoutedEventArgs e)
        {
            if (radioIn.IsChecked.HasValue && radioIn.IsChecked.Value)
            {
                ResetChartSeries();
                for (int i = 0; i < _nnValues[0].Input.Length; i++)
                {
                    int i1 = i;
                    AppendChartSerie(_nnValues.Select((t, index) => new KeyValuePair<int, double>(index * _step, t.Input[i1])));
                }
            }

            radioSerieA.IsChecked = true;
            radioSerieB.IsChecked = true;
        }

        private void radioError_Checked(object sender, RoutedEventArgs e)
        {
            if (radioError.IsChecked.HasValue && radioError.IsChecked.Value)
            {
                ResetChartSeries();
                for (int i = 0; i < _nnValues[0].Errors.Length; i++)
                {
                    int i1 = i;
                    AppendChartSerie(_nnValues.Select((t, index) => new KeyValuePair<int, double>(index * _step, t.Errors[i1])));
                }
            }

            radioSerieA.IsChecked = true;
            radioSerieB.IsChecked = true;
        }

        private void RadioErrorSqrChecked(object sender, RoutedEventArgs e)
        {
            if (radioErrorSqr.IsChecked.HasValue && radioErrorSqr.IsChecked.Value)
            {
                ResetChartSeries();
                AppendChartSerie(_nnValues.Select((nnValue, index) => new KeyValuePair<int, double>(index * _step, nnValue.ErrorsSqr)));
            }

            radioSerieA.IsChecked = true;
            radioSerieB.IsChecked = true;
        }

        private void ResetChartSeries()
        {
            lineChart.Series.Clear();
        }

        private void AppendChartSerie(IEnumerable<KeyValuePair<int, double>> data)
        {
            var series = new LineSeries
            {
                DependentValuePath = "Value",
                IndependentValuePath = "Key",
                ItemsSource = data,
                IsSelectionEnabled = true,
            };
            lineChart.Series.Add(series);
        }

        private void Weigths_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = (sender as ComboBox);
            if (comboBox == null) return;
            if (comboBox.SelectedIndex == 0) return;

            var neuronIndex = comboBox.SelectedValue.ToString().Split('-');
            int x = Convert.ToInt32(neuronIndex[1]);
            int y = Convert.ToInt32(neuronIndex[2]);

            ResetChartSeries();

            for (int i = 0; i < _nnValues[0].Weights[x][y].Length; i++)
            {
                int i1 = i;
                AppendChartSerie(_nnValues.Select((nnValue, index) => new KeyValuePair<int, double>(index * _step, nnValue.Weights[x][y][i1])));
            }
        }
    }
}