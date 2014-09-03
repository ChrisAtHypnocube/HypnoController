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
namespace Hypnocube.Demo.Model.Demos
{
    internal class Genetic : DemoBase
    {
        private byte[,] geneticBuffer1;
        private byte[,] geneticBuffer2;

        public Genetic(int w, int h) : base(w, h)
        {
        }

        public override void Update()
        {
            base.Update();
            if ((Frame%2) == 0)
                return;
            var w = Height;
            var h = Width;
            var colors = 7;
            if (geneticBuffer1 == null || geneticBuffer1.GetLength(0) != w || geneticBuffer1.GetLength(1) != h)
            {
                geneticBuffer1 = new byte[w, h];
                geneticBuffer2 = new byte[w, h];
                for (var i = 0; i < w; i++)
                    for (var j = 0; j < h; ++j)
                        geneticBuffer1[i, j] = (byte) Rand.Next(colors);
            }

            // morph buffer to genetic2
            for (var i = 0; i < w; i++)
                for (var j = 0; j < h; ++j)
                {
                    geneticBuffer2[i, j] = geneticBuffer1[i, j];
                    // if any neighbor cyclic neighbor, kill current cell
                    var c = (geneticBuffer1[i, j] + 1)%colors;
                    if (geneticBuffer1[i, (j + 1)%h] == c ||
                        geneticBuffer1[i, (j - 1 + h)%h] == c ||
                        geneticBuffer1[(i + 1)%w, j] == c ||
                        geneticBuffer1[(i - 1 + w)%w, j] == c
                        )
                    {
                        geneticBuffer2[i, j] = (byte) c;
                    }
                }

            // swap buffers
            var t = geneticBuffer1;
            geneticBuffer1 = geneticBuffer2;
            geneticBuffer2 = t;

            var colors1 = new int[colors*3];

            for (var c = 0; c < colors1.Length; c += 3)
            {
                double hue = Frame & 255; // 0-255
                hue /= 255.0; // 0-1
                hue += c/2.0/colors1.Length;
                while (hue > 1) hue -= 1;
                double r, g, b;
                HslToRgb(hue, 0.8, 0.5, out r, out g, out b);

                colors1[c] = (int) (r*255);
                colors1[c + 1] = (int) (g*255);
                colors1[c + 2] = (int) (b*255);
            }


            // color output
            for (var i = 0; i < w; i++)
                for (var j = 0; j < h; ++j)
                {
                    int r, g, b;
                    var c = geneticBuffer1[i, j]%(colors1.Length/3);
                    c *= 3;
                    r = colors1[c++];
                    g = colors1[c++];
                    b = colors1[c];
                    SetPixel(j, i, r, g, b);

                    // add noise
                    if (Rand.Next(1000) < 50)
                        geneticBuffer1[i, j] = (byte) Rand.Next(colors);
                }
        }
    }
}