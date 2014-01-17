using System;
using System.Drawing;
using System.Windows.Forms;

namespace INFOIBV {
    public partial class INFOIBV : Form {
        private Bitmap InputImage;
        private Bitmap OutputImage;

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
            if (OutputImage != null) OutputImage.Dispose();                 // Reset output image
            Color[,] Image = new Color[InputImage.Size.Width, InputImage.Size.Height]; // Create array to speed-up operations (Bitmap functions are very slow)

            // Setup progress bar
            progressBar.Visible = true;
            progressBar.Minimum = 1;
            progressBar.Maximum = InputImage.Size.Width * InputImage.Size.Height;
            progressBar.Value = 1;
            progressBar.Step = 1;

            // Copy input Bitmap to array            
            for (int x = 0; x < InputImage.Size.Width; x++) {
                for (int y = 0; y < InputImage.Size.Height; y++) {
                    Image[x, y] = InputImage.GetPixel(x, y);                // Set pixel color in array at (x,y)
                }
            }

            //==========================================================================================
            // Start of own code
            //==========================================================================================

            
            Image = Image.ApplyKernel(Kernels.XDerivateKernel, Functors.Sum);
            goto Skip;
            // Gray scale
            for (int x = 0; x < InputImage.Size.Width; x++) {
                for (int y = 0; y < InputImage.Size.Height; y++) {
                    Color pixelColor = Image[x, y];                         // Get the pixel color at coordinate (x,y)
                    var grayColor = (int) (pixelColor.R * 0.299 + pixelColor.G * 0.587 + pixelColor.B * 0.114);
                    Image[x, y] = Color.FromArgb(grayColor, grayColor, grayColor);                             // Set the new pixel color at coordinate (x,y)
                    progressBar.PerformStep();                              // Increment progress bar
                }
            }
            // Negative threshold
            Image = Image.Threshold(200, 0, 255);
            Skip:
            //==========================================================================================
            // End of own code
            //==========================================================================================

            // Copy array to output Bitmap
            OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height); // Create new output image
            for (int x = 0; x < InputImage.Size.Width; x++) {
                for (int y = 0; y < InputImage.Size.Height; y++) {
                    OutputImage.SetPixel(x, y, Image[x, y]);               // Set the pixel color at coordinate (x,y)
                }
            }

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