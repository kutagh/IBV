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
        /// Generic operator function that combines two gray scale images into a new gray scale image.
        /// </summary>
        /// <param name="first">Color 2D array of the first image</param>
        /// <param name="second">Color 2D array of the second image</param>
        /// <param name="op">Operator function that takes a gray scale value from both images and produces a result gray scale value</param>
        /// <returns>Color 2D array that represents the image after operator has been applied</returns>
        public static Color[,] Op(Color[,] first, Color[,] second, Func<int, int, int> op) {
            return Op(first, second, (a, b) => { var v = op(a.R, b.R); return Color.FromArgb(v, v, v); });
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

        static int startValue = 20, stepSize = 5;
        /// <summary>
        /// Labels the objects.
        /// </summary>
        /// <param name="image">Image to label, binary with 0=background & 1=object</param>
        /// <returns>A labeled image</returns>
        public static Color[,] CountObjects(this Color[,] image) {
            // Copy to a separate array to prevent modifying the original image.
            Color[,] result = new Color[image.GetLength(0), image.GetLength(1)];
            Array.Copy(image, result, image.Length);
            int gray = startValue, r = 0, g = 0, b = 0;
            for (int i = 0; i < result.GetLength(0); i++) {
                for (int j = 0; j < result.GetLength(1); j++) {
                    if (result[i, j].R == 1) {
                        if (gray > 0) gray+=stepSize;
                        if (r > 0) r += stepSize;
                        if (gray > 255) { gray = 0; r = startValue; }
                        if (g > 0) g += stepSize;
                        if (r > 255) { r = 0; g = startValue; }
                        if (b > 0) b += stepSize;
                        if (g > 255) { g = 0; b = startValue; }
                        if (b > 255) { b = 0; }

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
        public static Dictionary<Color, int> Areas(this Color[,] image) {
            return image.MomentOfOrder(0, 0);
        }

        /// <summary>
        /// Generates a chain code for every object in the labeled image.
        /// </summary>
        /// <param name="image">A labeled image to process</param>
        /// <returns>A Dictionary with chain codes per color label</returns>
        public static Dictionary<Color, int[]> ChainCode(this Color[,] image) {
            var result = new Dictionary<Color, int[]>();
            var background = Color.FromArgb(0, 0, 0);
            for (int x = 0; x < image.GetLength(0); x++)
                for (int y = 0; y < image.GetLength(1); y++) {
                    // Only process colors we haven't encountered yet.
                    if (image[x, y] == background || result.ContainsKey(image[x, y])) continue;
                    Color color = image[x, y];
                    var chainCode = new Queue<int>();
                    int orientation = 0;
                    int dx = x, dy = y;
                    // We have the top-left pixel of the image, so no pixel to the left or top of it.
                    if (x < image.GetLength(0) - 1 && image[x + 1, y] == color) {
                        dx++;
                        chainCode.Enqueue(0);
                    }
                    else if (y < image.GetLength(1) - 1 && image[x, y + 1] == color) {
                        dy++;
                        chainCode.Enqueue(3);
                        orientation = 3;
                    }

                    while (x != dx || y != dy) {
                        byte curDir = 1;
                        var translated = translate(dx, dy, (orientation + curDir) % 4);
                        while (!(translated.Item1 > 0 && translated.Item1 < image.GetLength(0) &&
                            translated.Item2 > 0 && translated.Item2 < image.GetLength(1) &&
                            image[translated.Item1, translated.Item2] == color)) {
                            curDir += 3;
                            curDir %= 4;
                            translated = translate(dx, dy, (orientation + curDir) % 4);
                        }
                        orientation += curDir;
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
        public static Dictionary<Color, int> Perimeters(this Color[,] image) {
            return image.ChainCode().ToDictionary(x => x.Key, x => x.Value.Count());
        }

        /// <summary>
        /// Calculates the circularity of every object in a labeled image.
        /// </summary>
        /// <param name="image">A labeled image to process</param>
        /// <returns>A Dictionary with circularity per color label</returns>
        public static Dictionary<Color, double> Circularity(this Color[,] image) {
            return image.Areas().Zip(image.Perimeters(), (A, l) => new KeyValuePair<Color, double>(A.Key, (4 * Math.PI * A.Value) / (l.Value * l.Value))).ToDictionary(x => x.Key, x => x.Value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static Dictionary<Color, Tuple<double, double>> Centroids(this Color[,] image) {

            var areas = image.Areas();
            var xCentroids = image.MomentOfOrder(1, 0).Zip(areas, (a, b) => new KeyValuePair<Color, double>(a.Key, a.Value / b.Value));
            var yCentroids = image.MomentOfOrder(0, 1).Zip(areas, (a, b) => new KeyValuePair<Color, double>(a.Key, a.Value / b.Value));

            return xCentroids.Zip(yCentroids, (a, b) => new KeyValuePair<Color, Tuple<double, double>>(a.Key, new Tuple<double, double>(a.Value, b.Value))).ToDictionary(x => x.Key, x => x.Value);
        }

        public static Dictionary<Color, int> MomentOfOrder(this Color[,] image, int p, int q) {

            Dictionary<Color, int> result = new Dictionary<Color, int>();

            for (int i = 0; i < image.GetLength(0); i++) {
                for (int j = 0; j < image.GetLength(1); j++) {
                    if (image[i, j].R != 0) {
                        if (!result.ContainsKey(image[i, j]))
                            result.Add(image[i, j], 0);

                        result[image[i, j]] += (int) Math.Pow(i, p) * (int) Math.Pow(j, q);
                    }
                }
            }

            return result;
        }


        public static Dictionary<Color, double> CentralMoments(this Color[,] image, int p, int q) {

            Dictionary<Color, double> result = new Dictionary<Color, double>();
            Dictionary<Color, Tuple<double, double>> centroids = image.Centroids();

            for (int i = 0; i < image.GetLength(0); i++) {
                for (int j = 0; j < image.GetLength(1); j++) {
                    if (image[i, j].R != 0) {
                        if (!result.ContainsKey(image[i, j]))
                            result.Add(image[i, j], 0);

                        double xCent = centroids[image[i, j]].Item1;
                        double yCent = centroids[image[i, j]].Item2;

                        result[image[i, j]] += Math.Pow(i - xCent, p) * Math.Pow(j - yCent, q);
                    }
                }
            }

            return result;
        }

        public static Dictionary<Color, double> AxisOfLeastMomentInertia(this Color[,] image) {

            Dictionary<Color, double> result = new Dictionary<Color, double>();
            Dictionary<Color, double> mu_11s = image.CentralMoments(1, 1);
            Dictionary<Color, double> mu_20s = image.CentralMoments(2, 0);
            Dictionary<Color, double> mu_02s = image.CentralMoments(0, 2);

            foreach (Color key in mu_11s.Keys) {

                double mu_11 = mu_11s[key];
                double mu_20 = mu_20s[key];
                double mu_02 = mu_02s[key];

                result.Add(key, 0.5 * Math.Atan((2 * mu_11) / (mu_20 - mu_02)));// * (180/Math.PI));
            }

            return result;
        }

        public static Dictionary<Color, Tuple<int, int>> FirstPixels(this Color[,] image) {

            Dictionary<Color, Tuple<int, int>> result = new Dictionary<Color, Tuple<int, int>>();

            for (int i = 0; i < image.GetLength(0); i++)
                for (int j = 0; j < image.GetLength(1); j++)
                    if (image[i, j] != Color.FromArgb(0, 0, 0) && !result.ContainsKey(image[i, j]))
                        result.Add(image[i, j], new Tuple<int, int>(i, j));

            return result;
        }

        public static Dictionary<Color, Rectangle> BoundingBox(this Color[,] image) {

            Dictionary<Color, Tuple<int, int>> firstPixels = image.FirstPixels();
            Dictionary<Color, int[]> chainCode = image.ChainCode();
            Dictionary<Color, List<Tuple<int,int>>> borders = new Dictionary<Color, List<Tuple<int, int>>>();
            Dictionary<Color, double> thetas = image.AxisOfLeastMomentInertia();
            Dictionary<Color, Tuple<double, double>> centroids = image.Centroids();

            Dictionary<Color, Rectangle> result = new Dictionary<Color, Rectangle>();

            foreach (Color key in chainCode.Keys) {

                int[] value = chainCode[key];
                Tuple<int, int> currentPixel = firstPixels[key];
                List<Tuple<int, int>> listPixels = new List<Tuple<int, int>>();
                listPixels.Add(currentPixel);

                borders.Add(key, listPixels);

                for (int i = 0; i < value.Length; i++) {
                    currentPixel = translate(currentPixel.Item1, currentPixel.Item2, value[i]);
                    borders[key].Add(currentPixel);
                    //borders.Add(key, listPixels);
                }

                
                foreach (Tuple<int,int> c in borders[key]) 
                    image[c.Item1, c.Item2] = Color.White;

                
			 
                double theta = thetas[key];
                double centroidX = centroids[key].Item1;
                double centroidY = centroids[key].Item2;
                double sinTheta = Math.Sin(theta);
                double cosTheta = Math.Cos(theta);

                // Find min max for key
                int minX= 512, minY= 512, maxX = 0, maxY = 0;

                foreach ( Tuple<int,int> coord in borders[key] ) {

                    

                    double x = coord.Item1 - centroidX;
                    double y = coord.Item2 - centroidY;
                    int newX = (int)((cosTheta * x) + ( sinTheta * y ));
                    int newY = (int)((-1 * sinTheta * x) + (cosTheta * y));

                    if ( newX < minX )
                        minX = newX;
                    if (newX > maxX)
                        maxX = newX;
                    if (newY < minY)
                        minY = newY;
                    if (newY > maxY)
                        maxY = newY;
                }
                if (new int[4] { minX, minY, maxX, maxY }.Any(x => x == int.MinValue || x == int.MaxValue))
                    continue;
                Rectangle boundingBox = new Rectangle(minX + (int)centroidX, minY + (int)centroidY, Math.Abs(minX) + maxX, Math.Abs(minY) + maxY);
                result.Add(key, boundingBox);
                
            }


            return result;

        }

        public static Bitmap ArrayToBitmap(this Color[,] image) {

            int width = image.GetLength(0);
            int height = image.GetLength(1);
            Bitmap OutputImage = new Bitmap(width, height); // Create new output image

            for ( int x = 0; x < width; x++ ) {
                for ( int y = 0; y < height; y++ ) {
                    OutputImage.SetPixel(x, y, image[x, y]); // Set the pixel color at coordinate (x,y)
                }
            }

            return OutputImage;
        }

        public static Color[,] BitmapToArray(this Bitmap image) {

            int width = image.Size.Width;
            int height = image.Size.Height;
            Color[,] OutputImage = new Color[width, height];

            for ( int x = 0; x < width; x++ ) {
                for ( int y = 0; y < height; y++ ) {
                    OutputImage[x, y] = image.GetPixel(x, y);
                }
            }

            return OutputImage;
        }

        public static Dictionary<Color, double> ObjectRectangularity(this Color[,] image) {
            Dictionary<Color, double> result = new Dictionary<Color, double>();
            Dictionary<Color, int> areas = image.Areas();
            Dictionary<Color, Rectangle> bounds = image.BoundingBox();

            foreach ( Color key in areas.Keys )
                result.Add(key, areas[key] / (double)( bounds[key].Height * bounds[key].Width ) );

            /*
            Dictionary<Color, Rectangle> bounds = image.BoundingBox();
            Dictionary<Color, int> areas = image.Areas();
            Dictionary<Color, double> result = bounds.ToDictionary(x => x.Key, x => (double)area[x.Key] / (double)( x.Value.Width * x.Value.Height ));*/

            return result;
        }

        public enum Arity { Circularity, Rectangularity }

        public enum Shape { Circle = 0, Triangle = 1, Rectangle = 2}

        public static Dictionary<Color, Shape> RecognizeObjectsAsShapes(this Color[,] image, Arity arity, Shape shape) {

            Dictionary<Color, double> circularities = image.Circularity();
            Dictionary<Color, double> rectangularities = image.ObjectRectangularity();
            Tuple<double,double>[] circularityRanges = ThresholdValues.Circularities;
            Tuple<double,double>[] rectangularityRanges = ThresholdValues.Rectangularities;

            Dictionary<Color, Shape> result = new Dictionary<Color, Shape>();

            foreach ( Color key in circularities.Keys ) {

                double c = circularities[key];
                double r = rectangularities[key];

                List<int> inRangeOf = new List<int>();

                for ( int i = 0; i < circularityRanges.Length; i++ ) {
                    double low = circularityRanges[i].Item1;
                    double high = circularityRanges[i].Item2;
                    if ( c >= low && c <= high ) {
                        inRangeOf.Add(i);
                    }
                }

                foreach ( int j in inRangeOf ) {

                    double low = rectangularityRanges[j].Item1;
                    double high = rectangularityRanges[j].Item2;

                    if ( r >= low && r <= high )
                        result.Add(key, (Shape)j);
                    
                }

            }


            return result;
        }
    }
}