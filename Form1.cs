using System;
using System.Collections.Generic;
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
            if (OutputImage != null) {
                OutputImage.Dispose();                 // Reset output image
                OutputImage = null;
            }
            Color[,] image = new Color[InputImage.Size.Width, InputImage.Size.Height]; // Create array to speed-up operations (Bitmap functions are very slow)

            // Setup progress bar
            progressBar.Visible = true;
            progressBar.Minimum = 1;
            progressBar.Maximum = InputImage.Size.Width * InputImage.Size.Height;
            progressBar.Value = 1;
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
            //goto NegativeThreshold;
            goto Grayscale;

        XDerivate:
            image = image.ApplyKernel(Kernels.XDerivateKernel, Functors.KernelSampleToValue.Sum, Functors.DoubleToDouble.Multiply);

        Grayscale:
            // Gray scale
            image = image.Transform(Functors.ColorToColor.ToGrayScaleGreen);
            //goto End;
            goto NegativeThreshold;
            //goto Dilate;

        NegativeThreshold:
            // Negative threshold
            image = image.Threshold(5, 0, 255);
            //goto End;
            goto Labelling;
            //goto Erode;

        Labelling:
            CountObjects(image);
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

            // Copy array to output Bitmap
            OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height); // Create new output image
            for (int x = 0; x < InputImage.Size.Width; x++) {
                for (int y = 0; y < InputImage.Size.Height; y++) {
                    OutputImage.SetPixel(x, y, image[x, y]);               // Set the pixel color at coordinate (x,y)
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

        int numObjects;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="image"></param>
        void CountObjects(Color[,] image) {
            int n = 0;
            for ( int i = 0; i < image.GetLength(0); i++ ) {
                for ( int j = 0; j < image.GetLength(1); j++ ) {
                    if ( image[i, j] == Color.FromArgb(0, 0, 0) ) {
                        int greyVAl = n * 5 + 1;
                        floodfill(image, i, j, Color.FromArgb(greyVAl, greyVAl, greyVAl));
                        n++;
                    }
                }
            }
        }

        private void floodfill(Color[,] image, int row, int col, Color color) {

            Color black = Color.FromArgb(0,0,0);

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

            while ( q.Count != 0 ) {

                // Pop element
                Tuple<int, int> processing = q.Dequeue();
                int r = processing.Item1;
                int c = processing.Item2;

                // Output region
                // Not Neccessary

                // Check adjacencies
                foreach ( Tuple<int, int> direction in directions ) {

                    int dr = r + direction.Item1;
                    int dc = c + direction.Item2;

                    if ( image[dr, dc] == black && dr < rows && dc < cols ) {
                        q.Enqueue(new Tuple<int, int>(dr, dc));
                        image[dr, dc] = color; // Mark as visited
                    }   

                }

            }


        }
    }
}