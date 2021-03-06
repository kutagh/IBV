﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace INFOIBV {
    public partial class INFOIBV : Form {
        private Bitmap InputImage;
        private Bitmap OutputImage;
        private int ThresholdValue = 1; // needs to be 1 for binary image operators
        public INFOIBV() {
            InitializeComponent();
        }

        private void LoadImageButton_Click(object sender, EventArgs e) {
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

        private void applyButton_Click(object sender, EventArgs e) {
            if (InputImage == null) return;                                 // Get out if no input image
            if (OutputImage != null) {
                OutputImage.Dispose();                 // Reset output image
                OutputImage = null;
            }
            Color[,] image = new Color[InputImage.Size.Width, InputImage.Size.Height]; // Create array to speed-up operations (Bitmap functions are very slow)

            // Setup progress bar
            progressBar.Visible = true;
            progressBar.Minimum = 1;
            progressBar.Maximum = 100;
            progressBar.Value = 33;
            progressBar.Step = 1;

            // Copy input Bitmap to array            
            for (int x = 0; x < InputImage.Size.Width; x++) {
                for (int y = 0; y < InputImage.Size.Height; y++) {
                    image[x, y] = InputImage.GetPixel(x, y);                // Set pixel color in array at (x,y)
                }
            }

            //==========================================================================================
            // Start of own code
            //==========================================================================================
            goto Test;
            goto GreenScale;
            goto NegativeThreshold;

        Test:
            var red = image.ColorThresholdFunc(ThresholdValue, 0, c => c.R > 100 && c.G < 50 && c.B < 50);
            var blue = image.ColorThresholdFunc(ThresholdValue, 0, c => c.B > 100 && c.G < 125 && c.R < 50);
            var redblue = Operators.Op(red, blue, (a, b) => a + b);
           
            redblue = redblue.Dilate(Kernels.DiamondElement5x5);
            redblue = redblue.Erode (Kernels.DiamondElement5x5);
            redblue = redblue.CountObjects();

            var boundz = redblue.BoundingBox();
            var tmp = redblue.ArrayToBitmap();
            using ( Graphics g=Graphics.FromImage( tmp))
                foreach (Color key in boundz.Keys) {
                    g.DrawRectangle(new Pen(Color.Red), boundz[key]);
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                }
            image = tmp.BitmapToArray();
            goto End;
        RedThreshold:
            image = image.ColorThresholdFunc(ThresholdValue, 0, c => c.R > 100 && c.G < 50 && c.B < 50);
        goto End;
        
        BlueThreshold:
            image = image.ColorThresholdFunc(ThresholdValue, 0, c => c.B > 100 && c.G < 125 && c.R < 50);
        goto End;

        XDerivate:
            image = image.ApplyKernel(Kernels.XDerivateKernel, Functors.KernelSampleToValue.Sum, Functors.DoubleToDouble.Multiply);

        GreenScale:
            image = image.Transform(Functors.ColorToColor.ToGrayScaleGreen);
        goto NegativeThreshold;

        Grayscale:
            // Gray scale
            image = image.Transform(Functors.ColorToColor.ToGrayScale);
            //goto NegativeThreshold;
            goto End;
            goto Dilate;

        NegativeThreshold:
            // Negative threshold
            image = image.Threshold(3, 1, 0);
            //image = image.Threshold(200, 0, 1);
            goto Labelling;
            goto End;
            goto Erode;

        Labelling:
            image = image.CountObjects();
            
            Dictionary<Color, Tuple<double, double>> centroids = image.Centroids();
            Dictionary<Color, int> area = image.Areas();
            Dictionary<Color, int> momOrder = image.MomentOfOrder(0, 0);
            goto ShowBounds;
            goto End;

        ShowBounds:
            Dictionary<Color, Rectangle> bounds = image.BoundingBox();
            Dictionary<Color, double> rectularties = image.ObjectRectangularity();
            var circularity = image.Circularity();
            Bitmap img = image.ArrayToBitmap();
            // paint
            using ( Graphics g=Graphics.FromImage(img) )
                foreach ( Color key in bounds.Keys ) {
                    g.DrawRectangle(new Pen(Color.Red), bounds[key]);
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.DrawString(String.Format("c: {0}", Math.Round(circularity[key], 2).ToString()), new Font("Thaoma", 16), Brushes.Green, bounds[key]);

                }

            /* Circularity range
            Circle:     0.86 - 1.00
            Driehoek:   0.78 - 0.85
            Rectangle:  0.50 - 0.70
            Octagon:    0.95 - 1.00
            Diamond:    0.40 - 0.50
             
            // Rectangularity range
            Circle:     0.75 - 0.85
            Driehoek:   0.55 - 0.65
            Rectangle:  0.95 - 1.05
            Octagon:    0.70 - 0.80
            Diamond:    0.55 - 0.60
            */

            // convert back
            image = img.BitmapToArray();

            

            goto End;

        Dilate:
            image = image.Dilate(Kernels.CrossElement3x3);
            goto End;

        Erode:
            image = image.Erode(Kernels.CrossElement3x3);
            goto Dilate;

        End:
            //==========================================================================================
            // End of own code
            //==========================================================================================
            progressBar.Value = 66;
            // Copy array to output Bitmap
            OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height); // Create new output image
            for (int x = 0; x < InputImage.Size.Width; x++) {
                for (int y = 0; y < InputImage.Size.Height; y++) {
                    OutputImage.SetPixel(x, y, image[x, y]);               // Set the pixel color at coordinate (x,y)
                }
            }
            progressBar.Value = 100;
            pictureBox2.Image = OutputImage;                         // Display output image
            progressBar.Visible = false;                                    // Hide progress bar
        }

        private void saveButton_Click(object sender, EventArgs e) {
            if (OutputImage == null) return;                                // Get out if no output image
            if (saveImageDialog.ShowDialog() == DialogResult.OK)
                OutputImage.Save(saveImageDialog.FileName);                 // Save the output image
        }




    }
}