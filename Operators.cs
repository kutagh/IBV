using System;
using System.Linq;
using System.Collections.Generic;
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
        public static Color[,] ThresholdFunc(this Color[,] image, int trueValue, int falseValue, Predicate<int> predicate) {
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
        public static Color[,] ColorThresholdFunc(this Color[,] image, int trueValue, int falseValue, Predicate<Color> predicate) {
            var minResultColor = Color.FromArgb(falseValue, falseValue, falseValue);
            var maxResultColor = Color.FromArgb(trueValue, trueValue, trueValue);
            return Transform(image, c => predicate(c) ? maxResultColor : minResultColor);
        }

        /// <summary>
        /// Transform individual pixels in a Color 2D array according to a transformation function.
        /// </summary>
        /// <param name="image">A Color 2D image</param>
        /// <param name="transformer">Transformation function that transforms a color into a new color</param>
        /// <returns>A transformed gray scale Color 2D array of the image</returns>
        public static Color[,] Transform(this Color[,] image, Func<Color, Color> transformer) {
            var result = new Color[image.GetLength(0), image.GetLength(1)];
            for (int x = 0; x < image.GetLength(0); x++)
                for (int y = 0; y < image.GetLength(1); y++)
                    result[x, y] = transformer(image[x, y]); ;
            return result;
        }

        /// <summary>
        /// Transform individual pixels in a gray scale Color 2D array according to a transformation function.
        /// </summary>
        /// <param name="image">A gray scale Color 2D image</param>
        /// <param name="transformer">Transformation function that transforms a gray value to a new gray value</param>
        /// <returns>A transformed gray scale Color 2D array of the image</returns>
        public static Color[,] Transform(this Color[,] image, Func<int, int> transformer) {
            return Transform(image, c => {
                var transformed = transformer(c.R);
                return Color.FromArgb(transformed, transformed, transformed);
            });
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
        /// Applies an image processing kernel to a Color 2D gray scale array, with a gray scale result. Uses a default multiplication function when applying the kernel to get a sample.
        /// </summary>
        /// <param name="image">Color 2D array representing the image</param>
        /// <param name="kernel">Kernel to use to generate a sample for the functor. Must have odd size or weird behavior can happen</param>
        /// <param name="functor">Functor that transforms a sample into a single value in the range of [0, 255]</param>
        /// <param name="defaultValue">Value used when the sample uses pixels outside of the image boundary. Defaults to zero</param>
        /// <returns>A Color 2D array representing the image after applying the kernel</returns>
        public static Color[,] ApplyKernel(this Color[,] image, double[,] kernel, Func<double[,], double> functor, double defaultValue = 0) {
            Func<double, double, double> transformator = Functors.DoubleToDouble.Multiply;
            return ApplyKernel(image, kernel, functor, transformator, defaultValue);
        }

        /// <summary>
        /// Applies an image processing kernel to a Color 2D gray scale array, with a gray scale result.
        /// </summary>
        /// <param name="image">Color 2D array representing the image</param>
        /// <param name="kernel">Kernel to use to generate a sample for the functor. Must have odd size or weird behavior can happen</param>
        /// <param name="functor">Functor that transforms a sample into a single value in the range of [0, 255]</param>
        /// <param name="transformator">Functor that takes a value from the image and the corresponding kernel value and gives a result.</param>
        /// <param name="defaultValue">Value used when the sample uses pixels outside of the image boundary. Defaults to zero</param>
        /// <returns>A Color 2D array representing the image after applying the kernel</returns>
        public static Color[,] ApplyKernel(this Color[,] image, double[,] kernel, Func<double[,], double> functor, Func<double, double, double> transformator, double defaultValue = 0) {
            var result = new Color[image.GetLength(0), image.GetLength(1)];
            int middleX = kernel.GetLength(0) / 2, middleY = kernel.GetLength(1) / 2;
            for (int x = 0; x < image.GetLength(0); x++) // Image iteration
                for (int y = 0; y < image.GetLength(1); y++) {
                    var section = new double[kernel.GetLength(0), kernel.GetLength(1)];
                    for (int dx = 0; dx < kernel.GetLength(0); dx++) // Kernel iteration
                        for (int dy = 0; dy < kernel.GetLength(1); dy++) {
                            int curX = x + dx - middleX, curY = y + dy - middleY; // Shorthand for current position
                            if (curX < 0 || curX >= image.GetLength(0) || curY < 0 || curY >= image.GetLength(1))
                                section[dx, dy] = defaultValue; // Outside of image boundary, use default value that shouldn't interfere with kernel operation
                            else // Apply transformator on image grayscale value and kernel value
                                section[dx, dy] = transformator(image[x + dx - middleX, y + dy - middleY].R, kernel[dx, dy]);
                        }
                    var res = (int) functor(section); // Kernel has been applied on the pixel, now computing result for said pixel
                    result[x, y] = Color.FromArgb(res, res, res);
                }
            return result;
        }

        /// <summary>
        /// Dilate an image using a given structuring element.
        /// </summary>
        /// <param name="image">Image to dilate</param>
        /// <param name="structuringElement">Structuring element to use</param>
        /// <returns>A dilated image</returns>
        public static Color[,] Dilate(this Color[,] image, double[,] structuringElement) {
            return ApplyKernel(image, structuringElement, Functors.KernelSampleToValue.Max, Functors.DoubleToDouble.Threshold(double.MinValue), double.MinValue);
        }

        /// <summary>
        /// Erode an image using a given structuring element.
        /// </summary>
        /// <param name="image">Image to erode</param>
        /// <param name="structuringElement">Structuring element to use</param>
        /// <returns>An eroded image</returns>
        public static Color[,] Erode(this Color[,] image, double[,] structuringElement) {
            return ApplyKernel(image, structuringElement, Functors.KernelSampleToValue.Min, Functors.DoubleToDouble.Threshold(double.MaxValue), double.MaxValue);
        }

        /// <summary>
        /// Labels the objects.
        /// </summary>
        /// <param name="image">Image to label, binary with 0=background & 1=object</param>
        /// <returns>A labeled image</returns>
        public static Color[,] CountObjects(this Color[,] image) {
            // Copy to a separate array to prevent modifying the original image.
            Color[,] result = new Color[image.GetLength(0), image.GetLength(1)];
            Array.Copy(image, result, image.Length);
            int gray = 2, r = 0, g = 0, b = 0;
            for (int i = 0; i < result.GetLength(0); i++) {
                for (int j = 0; j < result.GetLength(1); j++) {
                    if (result[i, j].R == 1) {
                        if (gray > 0) gray++;
                        if (r > 0) r++;
                        if (gray == 256) { gray = 0; r = 2; }
                        if (g > 0) g++;
                        if (r == 256) { r = 0; g = 2; }
                        if (b > 0) b++;
                        if (g == 256) { g = 0; b = 2; }

                        floodfill(result, i, j, gray > 0 ? Color.FromArgb(gray, gray, gray) : Color.FromArgb(r, g, b));
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Flood fill an object with a certain color value.
        /// </summary>
        /// <param name="image">Image to modify</param>
        /// <param name="row">X position of object origin pixel</param>
        /// <param name="col">Y position of object origin pixel</param>
        /// <param name="color">Color to assign to object</param>
        private static void floodfill(Color[,] image, int row, int col, Color color) {

            int rows = image.GetLength(0);
            int cols = image.GetLength(1);

            // Queue to keep record of positions to traverse
            Queue<Tuple<int, int>> q = new Queue<Tuple<int, int>>();

            // Directions
            Tuple<int,int>[] directions = { new Tuple<int,int>(-1,0),
                                            new Tuple<int,int>( 1,0),
                                            new Tuple<int,int>(0,-1),
                                            new Tuple<int,int>( 0,1)  };

            // Enqueue initial pos
            q.Enqueue(new Tuple<int, int>(row, col));

            // Mark element visited
            image[row, col] = color;

            while (q.Count != 0) {

                // Pop element
                Tuple<int, int> processing = q.Dequeue();
                int r = processing.Item1;
                int c = processing.Item2;

                // Output region
                // Not Neccessary

                // Check adjacencies
                foreach (Tuple<int, int> direction in directions) {

                    int dr = r + direction.Item1;
                    int dc = c + direction.Item2;

                    if (dr >= 0 && dr < rows && dc >= 0 && dc < cols && image[dr, dc].R == 1) {
                        q.Enqueue(new Tuple<int, int>(dr, dc));
                        image[dr, dc] = color; // Mark as visited
                    }
                }
            }
        }

        /// <summary>
        /// Converts a labeled image into a dictionary of areas. Warning: Changes alpha from 255 to 128...
        /// </summary>
        /// <param name="image">Image to process</param>
        /// <returns>Dictionary with area per color</returns>
        public static Dictionary<Color, int> Area(this Color[,] image) {
            var result = new Dictionary<Color, int>();
            var background = Color.FromArgb(0,0,0);
            for (int x = 0; x < image.GetLength(0); x++)
                for (int y = 0; y < image.GetLength(1); y++) {
                    // Only process colors we haven't encountered yet.
                    if (image[x, y] == background || image[x,y].A == 128 || result.ContainsKey(image[x,y])) continue;
                    Color color = image[x, y];
                    int total = 0;
                    var queue = new Queue<Tuple<int, int>>();
                    queue.Enqueue(new Tuple<int, int>(x, y));
                    while (queue.Count > 0) {
                        var current = queue.Dequeue();
                        int dx = current.Item1;
                        int dy = current.Item2;
                        if (dx < 0 || dx >= image.GetLength(0) || dy < 0 || dy >= image.GetLength(1) 
                            || image[dx, dy].ToArgb() != color.ToArgb()) continue;
                        total++;
                        image[dx, dy] = Color.FromArgb(128, color);
                        queue.Enqueue(new Tuple<int, int>(dx + 1, dy));
                        queue.Enqueue(new Tuple<int, int>(dx - 1, dy));
                        queue.Enqueue(new Tuple<int, int>(dx, dy + 1));
                        queue.Enqueue(new Tuple<int, int>(dx, dy - 1));
                    }

                    result.Add(color, total);
                }

            return result;
        }

        /// <summary>
        /// Generates a chain code for every object in the labeled image.
        /// </summary>
        /// <param name="image">A labeled image to process</param>
        /// <returns>A Dictionary with chain codes per color label</returns>
        public static Dictionary<Color, int[]> ChainCode(this Color[,] image) {
            var result = new Dictionary<Color, int[]>();
            var background = Color.FromArgb(0,0,0);
            for (int x = 0; x < image.GetLength(0); x++)
                for (int y = 0; y < image.GetLength(1); y++) {
                    // Only process colors we haven't encountered yet.
                    if (image[x, y] == background || result.ContainsKey(image[x, y])) continue;
                    Color color = image[x, y];
                    var chainCode = new Queue<int>();
                    int orientation = 0;
                    int dx = x, dy = y;
                    // We have the top-left pixel of the image, so no pixel to the left or top of it.
                    if (x < 255 && image[x + 1, y] == color) {
                        dx++;
                        chainCode.Enqueue(0);
                    }
                    else if (y < 255 && image[x, y + 1] == color) {
                        dy++;
                        chainCode.Enqueue(3);
                        orientation = 3;
                    }

                    while (x != dx || y != dy) {
                        byte curDir = 1;
                        var translated = translate(dx, dy, (orientation+ 4 + curDir) % 4);
                        while (!(translated.Item1 > 0 && translated.Item1 < image.GetLength(0) &&
                            translated.Item2 > 0 && translated.Item2 < image.GetLength(1) &&
                            image[translated.Item1, translated.Item2] == color)) {
                            curDir--;
                            curDir %= 4;
                            translated = translate(dx, dy, (orientation + curDir) % 4);
                        }
                        orientation += 4 + curDir;
                        orientation %= 4;
                        chainCode.Enqueue(orientation);
                        dx = translated.Item1;
                        dy = translated.Item2;
                    }

                    result.Add(color, chainCode.ToArray());
                }

            return result;
        }

        private static Tuple<int, int> translate(int x, int y, int orientation) {
            switch (orientation) {
                case 0: return new Tuple<int, int>(x + 1, y);
                case 1: return new Tuple<int, int>(x, y - 1);
                case 2: return new Tuple<int, int>(x - 1, y);
                case 3: return new Tuple<int, int>(x, y + 1);
                default: return new Tuple<int, int>(x, y);
            }
        }


        /// <summary>
        /// Calculates the perimeter of every object in a labeled image.
        /// </summary>
        /// <param name="image">A labeled image to process</param>
        /// <returns>A Dictionary with perimeter per color label</returns>
        public static Dictionary<Color, int> Perimeter(this Color[,] image) {
            return image.ChainCode().ToDictionary(x => x.Key, x => x.Value.Count());
        }

    }
}