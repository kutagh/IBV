using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace INFOIBV {
    public partial class INFOIBV : Form {
        private Bitmap InputImage;
        private Bitmap OutputImage;
        private int ThresholdValue = 255; // needs to be 1 for binary image operators
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
            //goto Test;
            goto GreenScale;
            goto NegativeThreshold;

        Test:
            var red = image.ColorThresholdFunc(ThresholdValue, 0, c => c.R > 100 && c.G < 50 && c.B < 50);
            var blue = image.ColorThresholdFunc(ThresholdValue, 0, c => c.B > 100 && c.G < 125 && c.R < 50);
            image = Operators.Op(red, blue, (a, b) => a + b);
           
            image = image.Dilate(Kernels.DiamondElement9x9);
            image = image.Erode (Kernels.DiamondElement9x9);
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
            image = image.Threshold(5, 1, 0);
            //image = image.Threshold(200, 0, 1);
            goto Labelling;
            goto End;
            goto Erode;

        Labelling:
            image = image.CountObjects();            
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