using System;
using System.Drawing;

namespace INFOIBV {
    public static class Functors {
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
}
