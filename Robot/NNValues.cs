using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace Robot
{
    public class NNValues : DynamicObject
    {
        public double ErrorsSqr { get; set; }
        public double[] Errors { get; set; }
        public double[] Values { get; set; }
        public double[] Input { get; set; }
        public double[] Output { get; set; }
        public IList<double[][]> Weights { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine(Input.Aggregate("Input: ", (current, t) => current + (t + " ; ")));
            sb.AppendLine(Output.Aggregate("Output: ", (current, t) => current + (t + " ; ")));
            sb.AppendLine(Values.Aggregate("Values: ", (current, t) => current + (t + " ; ")));
            sb.AppendLine(Errors.Aggregate("Error: ", (current, t) => current + (t + " ; ")));

            return sb.ToString();
        }
    }
}