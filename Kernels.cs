using System;
using System.Drawing;

namespace INFOIBV {
    public static class Kernels {
        public static double[,] YDerivateKernel = new double[1, 3] { { -0.5, 0, 0.5 } };
        public static double[,] XDerivateKernel = new double[3, 1] { { -0.5 }, { 0 }, { 0.5 } };
        public static double[,] SquareElement3x3 = new double[3, 3] {
            { 1, 1, 1},
            { 1, 1, 1},
            { 1, 1, 1}
        };
        public static double[,] SquareElement5x5 = new double[5, 5] {
            { 1, 1, 1, 1, 1},
            { 1, 1, 1, 1, 1},
            { 1, 1, 1, 1, 1},
            { 1, 1, 1, 1, 1},
            { 1, 1, 1, 1, 1}
        };
        public static double[,] CrossElement3x3 = new double[3, 3] {
            { 0, 1, 0},
            { 1, 1, 1},
            { 0, 1, 0}
        };
        public static double[,] CrossElement5x5 = new double[5, 5] {
            { 0, 0, 1, 0, 0},
            { 0, 0, 1, 0, 0},
            { 1, 1, 1, 1, 1},
            { 0, 0, 1, 0, 0},
            { 0, 0, 1, 0, 0}
        };
        public static double[,] DiamondElement5x5 = new double[5, 5] {
            { 0, 0, 1, 0, 0},
            { 0, 1, 1, 1, 0},
            { 1, 1, 1, 1, 1},
            { 0, 1, 1, 1, 0},
            { 0, 0, 1, 0, 0}
        };
        public static double[,] DiamondElement9x9 = new double[9, 9] {
                {0,0,0,0,1,0,0,0,0},
                {0,0,0,1,1,1,0,0,0},
                {0,0,1,1,1,1,1,0,0},
                {0,1,1,1,1,1,1,1,0},
                {1,1,1,1,1,1,1,1,1},
                {0,1,1,1,1,1,1,1,0},
                {0,0,1,1,1,1,1,0,0},
                {0,0,0,1,1,1,0,0,0},
                {0,0,0,0,1,0,0,0,0},  
            };
    }
}
