using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;



namespace Image2ASCII
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            CharSet = Initialize.GenerateFontWeights();
        }

        // PROPERTIES

        public Image Imagen_O { get; set; }
        public Image ResizedImage_O { get; set; }
        public Image Imagen_BW { get; set; }
        public Image ResultImage { get; set; }
        public List<WeightedChar> CharSet { get; set; }
        public Image AdjustedContrast {get; set;}
        public List<List<string>> ImageText { get; set; }

        // EVENT METHODS

        // Open file browser and let user pick an image
        // Image is then scaled to fit pictureBox dimensions
        private void button1_Click(object sender, EventArgs e)
        {
	        DialogResult result = openFileDialog1.ShowDialog(); // Show the dialog. (File explorer)
            if (result == DialogResult.OK) // Test result.
            {
                string file = openFileDialog1.FileName;
                List<string> Matches = new List<string> { "PNG", "JPG", "BMP", "JPEG" , "TIFF"};

                if (Matches.Any(t => file.ToUpper().Contains(t))) // Check if selected file format is an image
                {
                    try
                    {
                        Imagen_O = Image.FromFile(file);
                        if (Imagen_O != null)
                        {
                            ResizedImage_O = (Image)HelperMethods.ResizeImage(Imagen_O, pictureBox1);
                            pictureBox1.Image = ResizedImage_O; // Place Image in pictureBox
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message); // Theoretically this should never fire
                    }
                }
                else MessageBox.Show("Format not supported.\nPlease select an image with a supported format.","Warning");
            }
            
        }



        private void button2_Click_1(object sender, EventArgs e)
        {
            List<List<string>> ResultText = new List<List<string>> { };
            if (Imagen_O != null)
            {
                if (AdjustedContrast != null)
                {
                    AdjustedContrast = (Image)HelperMethods.ResizeImage(AdjustedContrast, pictureBox1);
                    Imagen_BW = (Image)HelperMethods.Grayscale(AdjustedContrast); // Get grayscale image
                }
                else
                {
                    Imagen_BW = (Image)HelperMethods.Grayscale(ResizedImage_O); // Get grayscale image
                }
                // Test if there are Width and Height values on the pixel per character textboxes
                int? width = null;
                int? height = null;
                if  (PixelsToCharacterWidth.Text != "" & PixelsToCharacterHeight.Text != "")
                {
                    // Convert them to integers
                    try
                    {
                        width = Int32.Parse(PixelsToCharacterWidth.Text);
                        height = Int32.Parse(PixelsToCharacterHeight.Text);
                    }
                    catch (Exception)
                    {

                        throw new System.FormatException("Width and Height values must be integers");
                    }

                }

                ResultImage = HelperMethods.Convert2ASCII(Imagen_BW, width, height, CharSet,out ResultText);
                ImageText = ResultText;
                if (IsBlackAndWhite.Checked)
                    pictureBox2.Image = HelperMethods.ResizeImage(ResultImage, pictureBox2);
                else if (IsColor.Checked)
                {
                    if (AdjustedContrast != null)
                    {
                        AdjustedContrast = (Image)HelperMethods.ResizeImage(AdjustedContrast, pictureBox1);
                        ResultImage = HelperMethods.Convert2ASCIIColor(AdjustedContrast, CharSet, ImageText);
                    }
                    else
                        ResultImage = HelperMethods.Convert2ASCIIColor(ResizedImage_O, CharSet, ImageText);
                    pictureBox2.Image = HelperMethods.ResizeImage(ResultImage, pictureBox2);
                }
                ImageText = new List<List<string>>{};
                MessageBox.Show("Conversion finished!","Task completed");

            }
            else
            {
                MessageBox.Show("No image found to convert.\nPlease select an image to convert.","Warning");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (ResultImage != null)
            {
                DialogResult result = saveFileDialog1.ShowDialog(); // Show the dialog. (File explorer)
                if (result == DialogResult.OK) // Test result.
                {
                        ResultImage.Save(saveFileDialog1.FileName);
                }
            }
            else
                MessageBox.Show("Please first convert image.","Warning");
        }


        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            if (Imagen_O != null)
            {
                AdjustedContrast = new Bitmap(Imagen_O);
                HelperMethods.AdjustContrast((Bitmap)AdjustedContrast, trackBar1.Value);
                pictureBox1.Image = HelperMethods.ResizeImage(AdjustedContrast, pictureBox1);
            }
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            trackBar1.Value = 0;
            AdjustedContrast = null;
            ResizedImage_O = null;
            Imagen_BW = null;
            ResultImage = null;
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }


    }
}
