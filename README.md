# Image2ASCII
Program that converts images to ASCII art images. The user can interact with the program via simple friendly GUI.

The project file is: Image2ASCII.csproj 

## How does it work?

### Grayscale conversion
At initialization all the characters in the selected set of ASCII characters are weighted via -> character_weight = # of black pixels / # of white pixels. Once all the charatcers have a weight assigned they are then linearly mapped to a 0-255 scale. 

After that, for each pixel in the image the program looks for the character with the closest weight to the pixel's intensity and replaces the pixel for that character.

### Color conversion
It uses the same procedure as explained above but adds a last step which is changing the color of the character to the color of the pixel that is being substituted.

The main problem with this approach is that the resulting image has a low color density, which is a thing that has to be improved in future versions.

## Program Logic

The program logic can be found in the files:
- Initialize.cs
- HelperMethods.cs


