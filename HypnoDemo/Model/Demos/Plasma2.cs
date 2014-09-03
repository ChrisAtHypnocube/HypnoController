#region License
// The MIT License (MIT)
// Copyright (c) 2013-2014 Hypnocube, LLC
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
#endregion
using System;

namespace Hypnocube.Demo.Model.Demos
{
    internal class Plasma2 : DemoBase
    {
        // ScaleColor

        private readonly Surface img1; // 3 image bitmaps
        private readonly Surface img2; // 3 image bitmaps
        private readonly Surface img3; // 3 image bitmaps

        private double angle; // the current offset for all movement calculations

        public Plasma2(int w, int h)
            : base(w, h)
        {
            img1 = new Surface(2*Width, 2*Height);
            img2 = new Surface(2*Width, 2*Height);
            img3 = new Surface(2*Width, 2*Height);

            FillImages();
        }


/* Idea:
   Create 3 source bitmaps, twice the size of the destination (image) bitmap in each direction
   and put some cool patterns on them (concentric circles in this case, colored nicely).
   To make the image, move the destination image over the source bitmaps independently,
   and or the bits together from the 3 source images

   As an added effect, do a parallax on the source as they are read in to make them appear 
   wavy. This can all be done in 24 bit rather quickly for a neat effect
*/
        // Ported from Chris Lomont old C++ code to C#, Dec 2013

        private void ScaleComponent(ref double color)
        {
            color = (color + 1)/2.0;
            color *= color; // square shifts towards 0
            color *= 200; // now in RGB range
        } // ScaleComponent

        private void ScaleColor(ref double red, ref double green, ref double blue)
        {
            // color in -1 to 1  color out = scaled towards black, and in 0,200 range
            ScaleComponent(ref red);
            ScaleComponent(ref green);
            ScaleComponent(ref blue);
        }


        private void FillImages()
        {
            // fill the source images
            // create some interesting color backgrounds in the source images
            for (var ypos = 0; ypos < 2*Height; ypos++)
                for (var xpos = 0; xpos < 2*Width; xpos++)
                {
                    double distance;
                    int dx, dy;
                    double red, green, blue;

                    dx = xpos - Width/2;
                    dy = ypos - Height/2;
                    distance = Math.Sqrt(dx*dx + dy*dy);

                    red = Math.Sin(distance/21.0);
                    green = Math.Sin(distance/10.0);
                    blue = Math.Sin(distance/16.0);
                    ScaleColor(ref red, ref green, ref blue);
                    img1.SetPixel(xpos, ypos, (byte) red, (byte) green, (byte) blue);

                    red = Math.Cos(distance/48.0 + 2*Math.Sin(distance/28.0));
                    green = Math.Cos(2*distance/31.0 - 3*Math.Sin(distance/45.0));
                    blue = Math.Sin(1.1 - distance/24.0);
                    ScaleColor(ref red, ref green, ref blue);
                    img2.SetPixel(xpos, ypos, (byte) red, (byte) green, (byte) blue);

                    red = Math.Sin(distance/20.0 + 3*Math.Sin(distance/25.0));
                    green = Math.Cos(2*distance/31.0);
                    blue = Math.Sin(0.1 - 1.1*distance/12.0);
                    ScaleColor(ref red, ref green, ref blue);
                    img3.SetPixel(xpos, ypos, (byte) red, (byte) green, (byte) blue);
                }
        } // FillImages


        public override void Update()
        {
            base.Update();

            int src_width, src_height;
            int dest_width, dest_height;

            // get image and dest sizes
            src_width = img1.Width;
            src_height = img1.Height;
            dest_width = Width;
            dest_height = Height;

            // these are the 3 source movement offsets
            var x1 = (int) ((dest_width/2)*Math.Cos(angle) + dest_width/2);
            var y1 = (int) ((dest_height/2)*Math.Sin(1.7*angle) + dest_height/2);
            var x2 = (int) ((dest_width/2)*Math.Cos(1.5*angle + 1) + dest_width/2);
            var y2 = (int) ((dest_height/2)*Math.Sin(0.5*angle + 1) + dest_height/2);
            var x3 = (int) ((dest_width/2)*Math.Cos(angle) + dest_width/2);
            var y3 = (int) ((dest_height/2)*Math.Sin(angle) + dest_height/2);

            angle += 0.05; // increment for next pass


            for (var ypos = 0; ypos < dest_height; ypos++)
            {
                // find start offsets mod src_width for parallax effect to get waves
                var offset1 = (int) (src_width + src_width/8*Math.Sin(angle + 4.0*ypos/dest_height))%src_width;
                var offset2 = (int) (src_width + src_width/8*Math.Sin(1.2*angle + 3.5*ypos/dest_height))%src_width;
                var offset3 = (int) (src_width + src_width/8*Math.Sin(-angle + 2.7*ypos/dest_height))%src_width;

                // start for source
                var p1x = x1 + offset1; // new locations, utilizing the parallax effects
                var p2x = x2 + offset2;
                var p3x = x3 + offset3;
                var p1y = y1;
                var p2y = y2;
                var p3y = y3;

                for (var xpos = 0; xpos < dest_width; xpos++)
                {
                    int r1, g1, b1, r2, g2, b2, r3, g3, b3;
                    img1.GetPixel(p1x%src_width, p1y%src_height, out r1, out g1, out b1);
                    img2.GetPixel(p2x%src_width, p2y%src_height, out r2, out g2, out b2);
                    img3.GetPixel(p3x%src_width, p3y%src_height, out r3, out g3, out b3);
                    SetPixel(xpos, ypos, r1 | r2 | r3, g1 | g2 | g3, b1 | b2 | b3);
                    p1x++;
                    p2x++;
                    p3x++;
                }
                y1++; // next line
                y2++; // next line
                y3++; // next line
            }
        }
    }
}