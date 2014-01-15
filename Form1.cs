using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace INFOIBV
{
    public partial class INFOIBV : Form
    {
        private Bitmap InputImage;
        private Bitmap OutputImage;

        public INFOIBV()
        {
            InitializeComponent();
        }

        private void LoadImageButton_Click(object sender, EventArgs e)
        {
           if (openImageDialog.ShowDialog() == DialogResult.OK)             // Open File Dialog
            {
                string file = openImageDialog.FileName;                     // Get the file name
                imageFileName.Text = file;                                  // Show file name
                if (InputImage != null) InputImage.Dispose();               // Reset image
                InputImage = new Bitmap(file);                              // Create new Bitmap from file
                if (InputImage.Size.Height <= 0 || InputImage.Size.Width <= 0 ||
                    InputImage.Size.Height > 512 || InputImage.Size.Width > 512) // Dimension check
                    MessageBox.Show("Error in image dimensions (have to be > 0 and <= 512)");
                else
                    pictureBox1.Image = (Image) InputImage;                 // Display input image
            }
        }

        private void applyButton_Click(object sender, EventArgs e)
        {
            if (InputImage == null) return;                                 // Get out if no input image
            if (OutputImage != null) OutputImage.Dispose();                 // Reset output image
            OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height); // Create new output image
            Color[,] Image = new Color[InputImage.Size.Width, InputImage.Size.Height]; // Create array to speed-up operations (Bitmap functions are very slow)

            // Setup progress bar
            progressBar.Visible = true;
            progressBar.Minimum = 1;
            progressBar.Maximum = InputImage.Size.Width * InputImage.Size.Height;
            progressBar.Value = 1;
            progressBar.Step = 1;

            // Copy input Bitmap to array            
            for (int x = 0; x < InputImage.Size.Width; x++)
            {
                for (int y = 0; y < InputImage.Size.Height; y++)
                {
                    Image[x, y] = InputImage.GetPixel(x, y);                // Set pixel color in array at (x,y)
                }
            }

            //==========================================================================================
            // Start of own code
            //==========================================================================================
            
            for (int x = 0; x < InputImage.Size.Width; x++)
            {
                for (int y = 0; y < InputImage.Size.Height; y++)
                {
                    Color pixelColor = Image[x, y];                         // Get the pixel color at coordinate (x,y)
                    var grayColor = (int) (pixelColor.R * 0.299 + pixelColor.G * 0.587 + pixelColor.B * 0.114);
                    Image[x, y] = Color.FromArgb(grayColor, grayColor, grayColor);                             // Set the new pixel color at coordinate (x,y)
                    progressBar.PerformStep();                              // Increment progress bar
                }
            }
            Image = Image.Threshold(200, 0, 255);

            //==========================================================================================
            // End of own code
            //==========================================================================================

            // Copy array to output Bitmap
            for (int x = 0; x < InputImage.Size.Width; x++)
            {
                for (int y = 0; y < InputImage.Size.Height; y++)
                {
                    OutputImage.SetPixel(x, y, Image[x, y]);               // Set the pixel color at coordinate (x,y)
                }
            }
            
            pictureBox2.Image = OutputImage;                         // Display output image
            progressBar.Visible = false;                                    // Hide progress bar
        }
        
        private void saveButton_Click(object sender, EventArgs e)
        {
            if (OutputImage == null) return;                                // Get out if no output image
            if (saveImageDialog.ShowDialog() == DialogResult.OK)
                OutputImage.Save(saveImageDialog.FileName);                 // Save the output image
        }

    }

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
            return thresholdFunc(image, trueValue, falseValue, v => v > threshold);
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
            return thresholdFunc(image, trueValue, falseValue, v => v > minThreshold && v < maxThreshold);
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
        private static Color[,] thresholdFunc(Color[,] image, int trueValue, int falseValue, Predicate<int> predicate) {
            return colorThresholdFunc(image, trueValue, falseValue, color => predicate(color.R));
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
        private static Color[,] colorThresholdFunc(Color[,] image, int trueValue, int falseValue, Predicate<Color> predicate) {
            var minResultColor = Color.FromArgb(falseValue, falseValue, falseValue);
            var maxResultColor = Color.FromArgb(trueValue, trueValue, trueValue);
            var result = new Color[image.GetLength(0), image.GetLength(1)];
            for (int x = 0; x < image.GetLength(0); x++)
                for (int y = 0; y < image.GetLength(1); y++)
                    if (predicate(image[x, y]))
                        result[x, y] = maxResultColor;
                    else
                        result[x, y] = minResultColor;
            return result;
        }
    }
}
