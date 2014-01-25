using System;
using System.Drawing;

namespace INFOIBV {
    public static class Functors {
        public static class KernelSampleToValue {
            public static Func<double[,], double> Sum = section => {
                double result = 0;
                for (int x = 0; x < section.GetLength(0); x++)
                    for (int y = 0; y < section.GetLength(1); y++)
                        result += section[x, y];
                return result + 127;
            };

            public static Func<double[,], double> Min = section => {
                double lowest = section[0, 0];
                for (int x = 0; x < section.GetLength(0); x++)
                    for (int y = 0; y < section.GetLength(1); y++)
                        if (section[x, y] < lowest)
                            lowest = section[x, y];

                return lowest;
            };

            public static Func<double[,], double> Max = section => {
                double highest = section[0, 0];
                for (int x = 0; x < section.GetLength(0); x++)
                    for (int y = 0; y < section.GetLength(1); y++)
                        if (section[x, y] > highest)
                            highest = section[x, y];

                return highest;
            };
        }
        public static class ColorToColor {

            public static Func<Color, Color> ToGrayScale = c => {
                var grayColor = (int) (c.R * 0.299 + c.G * 0.587 + c.B * 0.114);
                return Color.FromArgb(grayColor, grayColor, grayColor);
            };

            // Refactor this
            public static Func<Color, Color> ToGrayScaleGreen = c => {
                var grayColor = (int)( c.G  );
                return Color.FromArgb( grayColor, grayColor, grayColor );
            };
        }
        
        public static class DoubleToDouble {
            public static Func<double, double, double> Multiply = (a, b) => a * b;
            public static Func<double, double, double> Threshold(double v) { return (a, b) => b == 1 ? a : v; }
        }
    }
}
