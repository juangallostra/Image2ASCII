using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Image2ASCII
{
    public class WeightedChar
    {
        public string Character { get; set; }
        public Bitmap CharacterImage { get; set; }
        public double Weight { get; set; }
    }
}
