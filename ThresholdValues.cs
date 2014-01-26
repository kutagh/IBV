using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace INFOIBV {
    public static class ThresholdValues {

        /// <summary>
        /// 0 = circle, 1 = triangle, 3 = rectangle
        /// </summary>
        public static Tuple<double,double>[] Circularities = new Tuple<double, double>[3] {
            new Tuple<double, double>(0.86, 1.00),
            new Tuple<double, double>(0.78, 0.85),
            new Tuple<double, double>(0.50, 0.70)
        };

        /// <summary>
        /// 0 = circle, 1 = triangle, 3 = rectangle
        /// </summary>
        public static Tuple<double,double>[] Rectangularities = new Tuple<double, double>[3] {
            new Tuple<double, double>(0.75, 0.85),
            new Tuple<double, double>(0.55, 0.65),
            new Tuple<double, double>(0.95, 1.05)
        };

    }
}
