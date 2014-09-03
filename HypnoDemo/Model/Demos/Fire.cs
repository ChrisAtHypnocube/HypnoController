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
    internal class Fire : DemoBase
    {
        private int[,] fireBuffer;
        private int[] firePalette;

        public Fire(int w, int h) : base(w, h)
        {
        }

        public override void Update()
        {
            base.Update();
            const int gap = 3; // space of the fire buffer not shown
            if (fireBuffer == null || fireBuffer.GetLength(0) != Width || fireBuffer.GetLength(1) != Height + gap)
            {
                fireBuffer = new int[Width, Height + gap];
                firePalette = new int[256*3];
            }

            //generate the palette
            for (var x = 0; x < 256; x++)
            {
                //HsltoRgb used to generate colors:
                //Hue goes from 0 to 85: red to yellow in general
                //Saturation is always the maximum: 255
                //Lightness is 0..255 for x=0..128, and 255 for x=128..255
                double rd, gd, bd;
                var h1 = ((x) & 255)/255.0/3.0;
                h1 = (Frame & 255)/255.0;
                HslToRgb(h1, 1, Math.Min(1.0, x/255.0*2), out rd, out gd, out bd);
                //set the palette to the calculated RGB value
                firePalette[x*3] = (int) ((rd*255.0));
                firePalette[x*3 + 1] = (int) ((gd*255.0));
                firePalette[x*3 + 2] = (int) ((bd*255.0));
            }

            var w = Width;
            var h = Height + gap;

            //randomize the bottom row of the fire buffer
            for (var x = 0; x < w; x++)
                fireBuffer[x, h - 1] = Rand.Next(256);

            //do the fire calculations for every pixel, from top to bottom
            for (var y = 0; y < h - 1; y++)
                for (var x = 0; x < w; x++)
                {
                    fireBuffer[x, y] =
                        ((fireBuffer[(x - 1 + w)%w, (y + 1)%h]
                          + fireBuffer[(x)%w, (y + 1)%h]
                          + fireBuffer[(x + 1)%w, (y + 1)%h]
                          + fireBuffer[(x)%w, (y + 2)%h])
                         *32)/132; // 140; // this division controls the falloff amount
                }

            //set the drawing buffer to the fire buffer, using the palette colors
            for (var x = 0; x < w; x++)
                for (var y = 0; y < h - gap; y++)
                {
                    var colorIndex = fireBuffer[x, y]*3;
#if false
                    double rd, gd, bd;
                    // take 1/3 of the [0,1] cyclically as a color
                    var cc = colorIndex/3;
                    HslToRgb(((cc + frame) & 255) / 255.0 / 3, 1, Math.Min(1.0, cc / 255.0 * 2), out rd, out gd, out bd);

                    int r = (int) (rd*255.0);
                    int g = (int)(gd * 255.0);
                    int b = (int)(bd * 255.0);
#else
                    var r = firePalette[colorIndex++];
                    var g = firePalette[colorIndex++];
                    var b = firePalette[colorIndex];
#endif
                    SetPixel(x, y, r, g, b);
                }

            //// black check
            //for (var i = 0; i < Width; ++i)
            //    for (var j = 0; j < 4 * Height / 5; ++j)
            //    {
            //        int r, g, b;
            //        GetPixel(i, j, out r, out g, out b);
            //        if (r != 0 || g != 0 || b != 0)
            //            return;
            //    }
            //Debug.WriteLine("Frame black fire " + frame);
        }
    }
}