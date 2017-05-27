using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing.Imaging;


namespace Image2ASCII
{
    public static class HelperMethods
    {

        // Se puede jugar con las resoluciones de imagen para ver cual da mejores resultados?
        public static Image Convert2ASCII(Image BW_Image, int? w, int? h, List<WeightedChar> characters, out List<List<string>> ResultText)
        {

            /*
             * ALGORITHM:
             * 
             *  1- Get target Image size (w=Width,h=Height)
             *  2- Create Result Image with white background and size W = w*character_image_width
             *                                                        H = h*character_image_height
             *  3- Create empty string to hold the text   
             *  
             *  4- for (int j=0;j=target_Image_Height;j++) --> ALL ROWS 
             *       5- Create row text string
             *       for (int i=0;i=target_Image_Width;i++) --> ALL COLUMNS
             *          6- Get target pixel weight
             *          7- Get closest weight from character list
             *          8- Paste character image in position w = i*character_image_width
             *                                               h = j*character_image_height
             *            ¡¡ Be careful with coordinate system when placing Images !!
             *          9- Add (string)character to row text string
             *       10- Add row text string to text holding string
             *  11 - return resulting Image & Text
             */

            if (w == null & h == null)
            {
                w = 1;
                h = 1;
            }
            int width = (int)w;
            int height = (int)h;

            // Be careful when Image.Width/widh or Image.Height/height are not integer values (coherent approximations)
            Image ResultImage = new Bitmap(BW_Image.Width * characters[0].CharacterImage.Width/width, BW_Image.Height * characters[0].CharacterImage.Height/height);
            Graphics drawing = Graphics.FromImage(ResultImage);
            drawing.Clear(Color.White);
            ResultText = new List<List<string>> { };
            Bitmap BlackAndWhite = (Bitmap)BW_Image;

            for (int j = 0; j < BW_Image.Height; j+=height) // ROW
            {
                List<string> RowText = new List<string> { };
                for (int i=0; i <BW_Image.Width; i+=width) // COLUMN
                {
                    double targetvalue = 0;

                    for (int nj=j; nj<j+height; nj++)
                    {
                        for (int ni = i; ni < i+width; ni++)
                        {
                            // Sum all the pixels in neighbourhood and average
                            try
                            {
                                targetvalue += BlackAndWhite.GetPixel(ni, nj).R;
                            }
                            catch (Exception)
                            {
                                targetvalue += 128;
                            }
                        }
                    }
                    targetvalue /= (width * height);
                    WeightedChar closestchar = characters.Where(t=>Math.Abs(t.Weight-targetvalue)==characters.Min(e => Math.Abs(e.Weight - targetvalue))).FirstOrDefault();
                    RowText.Add(closestchar.Character);
                    drawing.DrawImage(closestchar.CharacterImage, (i/width) * closestchar.CharacterImage.Width, (j/height) * closestchar.CharacterImage.Height);
                }
                ResultText.Add(RowText);
            }
            drawing.Dispose();
            return (Image)ResultImage;    
        }

        public unsafe static Image Convert2ASCIIColor(Image ResizedImage_O, List<WeightedChar> characters, List<List<string>> ImageText)
        {
            /*
             * ALGORITHM
             * 1- Create result image with white background
             *  2- for (int j=0;j=target_Image_Height;j++) --> ALL ROWS 
             *       for (int i=0;i=target_Image_Width;i++) --> ALL COLUMNS
             *          6- Get target pixel color, get target character from string
             *          7- Create Image with the correct size, color and character
             *          8- Paste character image in position w = i*character_image_width
             *                                               h = j*character_image_height
             *            ¡¡ Be careful with coordinate system when placing Images !!
             *  11 - return resulting Image
             */


            // Needed variables for iteration

            // Result Image and graphics object
            Image ResultImage = new Bitmap(ResizedImage_O.Width * characters[0].CharacterImage.Width, ResizedImage_O.Height * characters[0].CharacterImage.Height);
            Graphics drawing = Graphics.FromImage(ResultImage);
            drawing.Clear(Color.White);
            // ResizedImage Bitmap data and byte-encoded pixel lenght
            var bmp = (Bitmap)ResizedImage_O;
            var bitmapdata = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                             System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            int PixelSize = 4;
            // character image size
            int width = characters[0].CharacterImage.Width;
            int height = characters[0].CharacterImage.Height;

            

            // foreach pixel in image
            for (int j = 0; j < ResizedImage_O.Height; j++)
            {
                byte* destPixels = (byte*)bitmapdata.Scan0 + (j * bitmapdata.Stride);

                for (int i = 0; i < ResizedImage_O.Width; i++)
                {
                    // get pixel color
                    var B = (int)destPixels[i * PixelSize]; // B
                    var G = (int)destPixels[i * PixelSize + 1]; // G
                    var R = (int)destPixels[i * PixelSize + 2]; // R
                    // get character
                    var character = ImageText[j][i];
                    // create char image
                    var charimage = DrawText(character, Color.FromArgb(R, G, B), Color.White, new SizeF(width, height));
                    // paste char image 
                    drawing.DrawImage(charimage, i * charimage.Width, j * charimage.Height);

                }
            }
            bmp.UnlockBits(bitmapdata);
            drawing.Dispose();
            return (Image)ResultImage;    
        }

        public static Image ResizeImage(Image Imagen, PictureBox pictureBox)
        {
            Image resizedimage = Imagen;

            if (((double)pictureBox.Width / (double)Imagen.Width) < ((double)pictureBox.Height / (double)Imagen.Height))
                resizedimage = (Image)(new Bitmap((Bitmap)resizedimage, new Size((int)(((double)pictureBox.Width / (double)resizedimage.Width) * (double)resizedimage.Width), (int)(((double)pictureBox.Width / (double)resizedimage.Width) * (double)resizedimage.Height))));
            else
                resizedimage = (Image)(new Bitmap((Bitmap)resizedimage, new Size((int)(((double)pictureBox.Height / (double)resizedimage.Height) * (double)resizedimage.Width), (int)(((double)pictureBox.Height / (double)resizedimage.Height) * (double)resizedimage.Height))));
            return resizedimage;
        }

        public static Bitmap Grayscale(Image image)
        {
            Bitmap btm = new Bitmap(image);
            for (int i = 0; i < btm.Width; i++)
            {
                for (int j = 0; j < btm.Height; j++)
                {
                    int ser = (int)(btm.GetPixel(i, j).R*0.3 + btm.GetPixel(i, j).G*0.59 + btm.GetPixel(i, j).B*0.11);
                    btm.SetPixel(i, j, Color.FromArgb(ser, ser, ser));
                }
            }
            return btm;
        }
        
        public unsafe static void AdjustContrast(Bitmap bmp, double contrast)
        {
            {
                byte[] contrast_lookup_table = new byte[256];
                double newValue = 0;
                double c = (100.0 + contrast) / 100.0;

                c *= c;

                for (int i = 0; i < 256; i++)
                {
                    newValue = (double)i;
                    newValue /= 255.0;
                    newValue -= 0.5;
                    newValue *= c;
                    newValue += 0.5;
                    newValue *= 255;

                    if (newValue < 0)
                        newValue = 0;
                    if (newValue > 255)
                        newValue = 255;
                    contrast_lookup_table[i] = (byte)newValue;
                }

                var bitmapdata = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                    System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                int PixelSize = 4;

                for (int y = 0; y < bitmapdata.Height; y++)
                {
                    byte* destPixels = (byte*)bitmapdata.Scan0 + (y * bitmapdata.Stride);
                    for (int x = 0; x < bitmapdata.Width; x++)
                    {
                        destPixels[x * PixelSize] = contrast_lookup_table[destPixels[x * PixelSize]]; // B
                        destPixels[x * PixelSize + 1] = contrast_lookup_table[destPixels[x * PixelSize + 1]]; // G
                        destPixels[x * PixelSize + 2] = contrast_lookup_table[destPixels[x * PixelSize + 2]]; // R
                        //destPixels[x * PixelSize + 3] = contrast_lookup[destPixels[x * PixelSize + 3]]; //A
                    }
                }
                bmp.UnlockBits(bitmapdata);
            }
        }

        public static Image DrawText(string text, Color textColor, Color backColor, SizeF WidthAndHeight)
        {

            // Get char width for insertion point calculation purposes
            Image dummy_img = new Bitmap(1, 1);
            Graphics dummy_drawing = Graphics.FromImage(dummy_img);
            SizeF textSize = dummy_drawing.MeasureString(text, System.Drawing.SystemFonts.DefaultFont);
            dummy_img.Dispose();
            dummy_drawing.Dispose();

            // Create a dummy bitmap just to get a graphics object
            Image img = new Bitmap(1, 1);
            Graphics drawing = Graphics.FromImage(img);

            // Free up resources taken by the dummy image and old graphics object
            img.Dispose();
            drawing.Dispose();

            // Create a new image of the right size
            img = new Bitmap((int)WidthAndHeight.Width, (int)WidthAndHeight.Height);
            // Get a graphics object
            drawing = Graphics.FromImage(img);

            // Paint the background
            drawing.Clear(backColor);

            // Create a brush for the text
            Brush textBrush = new SolidBrush(textColor);
			// El punto de inserción del carácter se puede afinar más (Trial & Error)
            drawing.DrawString(text, System.Drawing.SystemFonts.DefaultFont, textBrush, (WidthAndHeight.Width - textSize.Width) / 2, 0);
            drawing.Save();

            textBrush.Dispose();
            drawing.Dispose();

            return img;
        }


    }
}

