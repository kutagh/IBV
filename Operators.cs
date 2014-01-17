using System;
using System.Drawing;

namespace INFOIBV {
    public static class Operators {

        /// <summary>
        /// A threshold operation on a Color 2D array. Requires the colors to be in gray scale values, so that R, G and B have identical values.
        /// </summary>
        /// <param name="image">The Color 2D array to threshold</param>
        /// <param name="threshold">Threshold minimum value, exclusive</param>
        /// <param name="trueValue">The value that has to be assigned when the pixel value exceeds the threshold value</param>
        /// <param name="falseValue">The value that has to be assigned when the pixel value does not exceed the threshold value</param>
        /// <returns>A thresholded image</returns>
        public static Color[,] Threshold(this Color[,] image, int threshold, int trueValue = 1, int falseValue = 0) {
            return ThresholdFunc(image, trueValue, falseValue, v => v > threshold);
        }

        /// <summary>
        /// A threshold operation on a Color 2D array. Requires the colors to be in gray scale values, so that R, G and B have identical values.
        /// </summary>
        /// <param name="image">The Color 2D array to threshold</param>
        /// <param name="minThreshold">Threshold minimum value, exclusive</param>
        /// <param name="maxThreshold">Threshold maximum value, exclusive</param>
        /// <param name="trueValue">The value that has to be assigned when the pixel value exceeds the threshold value</param>
        /// <param name="falseValue">The value that has to be assigned when the pixel value does not exceed the threshold value</param>
        /// <returns>A thresholded image</returns>
        public static Color[,] ThresholdRange(this Color[,] image, int minThreshold, int maxThreshold, int trueValue = 1, int falseValue = 0) {
            return ThresholdFunc(image, trueValue, falseValue, v => v > minThreshold && v < maxThreshold);
        }

        /// <summary>
        /// A threshold operation on a Color 2D array. Requires the colors to be in gray scale values, so that R, G and B have identical values. This is the generic threshold function that can be specialized to a specific function.
        /// </summary>
        /// <param name="image">The Color 2D array to threshold</param>
        /// <param name="threshold">Threshold minimum value, exclusive</param>
        /// <param name="trueValue">The value that has to be assigned when the pixel value exceeds the threshold value</param>
        /// <param name="falseValue">The value that has to be assigned when the pixel value does not exceed the threshold value</param>
        /// <param name="predicate">Predicate for a pixel value that determines whether the pixel satisfies the threshold or not</param>
        /// <returns>A thresholded image</returns>
        public static Color[,] ThresholdFunc(Color[,] image, int trueValue, int falseValue, Predicate<int> predicate) {
            return ColorThresholdFunc(image, trueValue, falseValue, color => predicate(color.R));
        }

        /// <summary>
        /// A threshold operation on a Color 2D array. This is the generic threshold function that can be specialized to a specific function.
        /// </summary>
        /// <param name="image">The Color 2D array to threshold</param>
        /// <param name="threshold">Threshold minimum value, exclusive</param>
        /// <param name="trueValue">The value that has to be assigned when the pixel value exceeds the threshold value</param>
        /// <param name="falseValue">The value that has to be assigned when the pixel value does not exceed the threshold value</param>
        /// <param name="predicate">Predicate for a pixel value that determines whether the pixel satisfies the threshold or not</param>
        /// <returns>A thresholded image</returns>
        public static Color[,] ColorThresholdFunc(Color[,] image, int trueValue, int falseValue, Predicate<Color> predicate) {
            var minResultColor = Color.FromArgb(falseValue, falseValue, falseValue);
            var maxResultColor = Color.FromArgb(trueValue, trueValue, trueValue);
            var result = new Color[image.GetLength(0), image.GetLength(1)];
            for (int x = 0; x < image.GetLength(0); x++)
                for (int y = 0; y < image.GetLength(1); y++)
                    result[x, y] = predicate(image[x, y]) ? maxResultColor : minResultColor;
            return result;
        }

        /// <summary>
        /// Generic operator function that combines two images into a new image.
        /// </summary>
        /// <param name="first">Color 2D array of the first image</param>
        /// <param name="second">Color 2D array of the second image</param>
        /// <param name="op">Operator function that takes a color from both images and produces a result color</param>
        /// <returns>Color 2D array that represents the image after operator has been applied</returns>
        public static Color[,] Op(Color[,] first, Color[,] second, Func<Color, Color, Color> op) {
            if (first.GetLength(0) != second.GetLength(0) || first.GetLength(1) != second.GetLength(1))
                throw new ArgumentException("Dimensions of the two images do not match");
            var result = new Color[first.GetLength(0), first.GetLength(1)];
            for (int x = 0; x < first.GetLength(0); x++)
                for (int y = 0; y < first.GetLength(1); y++)
                    result[x, y] = op(first[x, y], second[x, y]);
            return result;
        }

        /// <summary>
        /// Applies an image processing kernel to a Color 2D gray scale array, with a gray scale result.
        /// </summary>
        /// <param name="image">Color 2D array representing the image</param>
        /// <param name="kernel">Kernel to use to generate a sample for the functor. Must have odd size or weird behavior can happen</param>
        /// <param name="functor">Functor that transforms a sample into a single value in the range of [0, 255]</param>
        /// <returns>A Color 2D array representing the image after applying the kernel</returns>
        public static Color[,] ApplyKernel(this Color[,] image, double[,] kernel, Func<double[,], double> functor) {
            var result = new Color[image.GetLength(0), image.GetLength(1)];
            int middleX = kernel.GetLength(0) / 2, middleY = kernel.GetLength(1) / 2;
            for (int x = 0; x < image.GetLength(0); x++)
                for (int y = 0; y < image.GetLength(1); y++) {
                    var section = new double[kernel.GetLength(0), kernel.GetLength(1)];
                    for (int dx = 0; dx < kernel.GetLength(0); dx++)
                        for (int dy = 0; dy < kernel.GetLength(1); dy++){
                            int curX = x + dx - middleX, curY = y + dy - middleY;
                            if (curX < 0 || curX >= image.GetLength(0) || curY < 0 || curY >= image.GetLength(1))
                                continue;
                            section[dx, dy] = image[x + dx - middleX, y + dy - middleY].R * kernel[dx, dy];
                        }
                    var res = (int)functor(section);
                    result[x, y] = Color.FromArgb(res, res, res);
                }
            return result;
        }
    }
}