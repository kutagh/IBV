using System;
using System.Drawing;

namespace INFOIBV {
    public static class Kernels {
        public static double[,] YDerivateKernel = new double[1, 3] { { -0.5, 0, 0.5 } };
        public static double[,] XDerivateKernel = new double[3, 1] { { -0.5 }, { 0 }, { 0.5 } };
    }
}
