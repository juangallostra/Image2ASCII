using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Image2ASCII
{
    public static class Initialize
    {
        /*
         * The methods contained in this class are executed at the inizialization
         * of the program, as the results from their calculations are needed for
         * the image conversion and are not user-dependent, thus not depending from
         * the actions taken by the user.
         * 
         * It essentially does, foreach ASCII character between 32-126, the following:
         *  - Calculate its weight, defined as: number of black pixels / number of pixels in character image
         *      Note: To understand the process in depth, please read the code and comments below.
         *  - Generate a square shaped image of the character, with the character in question located in the 
         *    center of the image. The character is drawn in black on top of a white background.
         *  - Associate the character in question (char type) to its image and weight.
         *  
         * All this character information (character, weight and image) is stored in a custom class (WeightedChar)
         * which can hold the 3 properties.
         * 
         * All the classes resulting from the calculations are stored in a List so we can access the results.
         */


        public static List<WeightedChar> GenerateFontWeights() // Collect chars, their Images and weights in a list of WeightedChar
        {
            List<WeightedChar> WeightedChars = new List<WeightedChar>();

            SizeF commonsize = GetGeneralSize(); // Get standard size (nxn square), which will be common to all CharImages

            for (int i = 32; i <= 126; i++) // Iterate through Chars
            {
                var forweighting = new WeightedChar(); // New object to hold Image, Weight and Char of new character
                char c = Convert.ToChar(i);
                if (!char.IsControl(c))
                {
                    forweighting.Weight = GetWeight(c, commonsize); // Get character weight
                    forweighting.Character = c.ToString(); // Get character representation (the actual char)
                    forweighting.CharacterImage = (Bitmap)HelperMethods.DrawText(c.ToString(), Color.Black, Color.White, commonsize); // Get character Image
                }
                WeightedChars.Add(forweighting);
            }

            WeightedChars = LinearMap(WeightedChars); // Linearly map character weights to be in the range 0-255 -> mapping linearly from: MinCalcWeight - MaxCalcWeight to 0-255; 
                                                      // This is done to be able to directly map pixels to characters
            return WeightedChars;
        }

        #region [GenerateFontWeights Helper methods]

        private static SizeF GetGeneralSize()
        {
            SizeF generalsize = new SizeF(0, 0);
            for (int i = 32; i <= 126; i++) // Iterate through contemplated characters calculating necessary width
            {
                char c = Convert.ToChar(i);
                // Create a dummy bitmap just to get a graphics object
                Image img = new Bitmap(1, 1);
                Graphics drawing = Graphics.FromImage(img);

                // Measure the string to see its dimensions using the graphics object
                SizeF textSize = drawing.MeasureString(c.ToString(), System.Drawing.SystemFonts.DefaultFont);
                // Update, if necessary, the max width and height
                if (textSize.Width > generalsize.Width)
                    generalsize.Width = textSize.Width;
                if (textSize.Height > generalsize.Height)
                    generalsize.Height = textSize.Height;
                // free all resources
                img.Dispose();
                drawing.Dispose();
            }
            // Make sure generalsize is made of integers 
            generalsize.Width = ((int)generalsize.Width);
            generalsize.Height = ((int)generalsize.Height);
            // and size defines a square to maintain image proportions
            // as the ASCII transformation will be 1 pixel = 1 character Image
            // thus substituting one pixel by one character image
            if (generalsize.Width > generalsize.Height)
                generalsize.Height = generalsize.Width;
            else
                generalsize.Width = generalsize.Height;

            return generalsize;
        }


       private static double GetWeight(char c, SizeF size)
        {
            var CharImage = HelperMethods.DrawText(c.ToString(), Color.Black, Color.White, size);

            Bitmap btm = new Bitmap(CharImage);
            double totalsum = 0;

            for (int i = 0; i < btm.Width; i++)
            {
                for (int j = 0; j < btm.Height; j++)
                {
                    Color pixel = btm.GetPixel(i, j);
                    totalsum = totalsum + (pixel.R 
                                        + pixel.G 
                                        + pixel.B)/3;
                }
            }
            // Weight = (sum of (R+G+B)/3 for all pixels in image) / Area. (Where Area = Width*Height )
            return totalsum / (size.Height * size.Width);
        }





       private static List<WeightedChar> LinearMap(List<WeightedChar> characters)
       {
           double max = characters.Max(c => c.Weight);
           double min = characters.Min(c => c.Weight);
           double range = 255;
           // y = mx + n (where y c (0-255))
           double slope = range / (max - min);
           double n = -min * slope;
           foreach(WeightedChar charactertomap in characters)
           {
               charactertomap.Weight = slope * charactertomap.Weight + n;
           }
           return characters;
       }

       #endregion
    }
}
