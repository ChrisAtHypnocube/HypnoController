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
using Hypnocube.Demo.Model.Raytracer.Math3D;

namespace Hypnocube.Demo.Model.Demos
{
    internal class Ripples : DemoBase
    {
        private const int Border = 3;
        private int[,] buffer1;
        private int[,] buffer2;
        private Surface sw;

        public Ripples(int w, int h)
            : base(w, h)
        {
        }

        private void MakeBackground()
        {
            sw = new Surface(Width, Height);

            sw.Fill(0, 0, 128);

            var top = new Vector3D(0, 0, 64);
            var bottom = new Vector3D(64, 64, 255);

            // gradient
            for (var j = 0; j < Height; j++)
            {
                var a = (double) j/Height;
                var color = bottom*a + top*(1 - a);
                var r = (int) color.X;
                var g = (int) color.Y;
                var b = (int) color.Z;
                sw.DrawLine(0, j, Width - 1, j, r, g, b);
            }

#if false
            var font = new FontController();

            int curPos = 0;

            var xpos = curPos;
            var ypos = Height / 2 - font.Font.LineHeight / 2;
            var gap = (Height - font.Font.LineHeight) * 0.4;

            font.Render(ref xpos, ref ypos, "Hypnocube!",
                        Width, Height,
                        (i, j, r, g, b, a) =>
                        {
                            if (a != 0)
                            {
                                i += 5;
                                int r1, g1, b1;
                                var j2 = (int)(j + Math.Sin(frame / 8.0 + i * 9.0 / Width) * gap);
                                sw.GetPixel(i, j2, out r1, out g1, out b1);

                                //j2 = j;
                                r1 = a * r / 255 + (255 - a) * r1 / 255;
                                g1 = a * g / 255 + (255 - a) * g1 / 255;
                                b1 = a * b / 255 + (255 - a) * b1 / 255;
                                sw.SetPixel(i, j2, r1, g1, b1);
                            }
                        }
                );
#endif

            // checkerboard
            //for (var i = 0; i < Width; ++i)
            //    for (var j = 0; j < Height; ++j)
            //    {
            //        var c = (j/16 + i/16) & 1;
            //        int r, g, b;
            //        r = g = b = 255;
            //        if (c==1)
            //            r = g = 0;
            //        sw.SetPixel(i, j, r, g, b);
            //    }
        }

        public override void Update()
        {
            base.Update();

            if (buffer1 == null || buffer2 == null)
            {
                buffer1 = new int[Width + 2*Border, Height + 2*Border];
                buffer2 = new int[Width + 2*Border, Height + 2*Border];
                MakeBackground();
            }


            var damping = 0.80;

            // update water from bufer 1 to buffer 2
            for (var i = 1; i < buffer1.GetLength(0) - 1; ++i)
                for (var j = 1; j < buffer1.GetLength(1) - 1; ++j)
                {
                    buffer2[i, j] = (buffer1[i - 1, j] + buffer1[i + 1, j] + buffer1[i, j + 1] + buffer1[i, j - 1])/2 -
                                    buffer2[i, j];
                    buffer2[i, j] = (int) (buffer2[i, j]*damping);
                }

            // swap buffers
            var temp = buffer1;
            buffer1 = buffer2;
            buffer2 = temp;


            // draw buffer1
            for (var i = Border; i < Width + Border; ++i)
                for (var j = Border; j < Height + Border; ++j)
                {
                    int r, g, b;

                    var xOffset = buffer1[i - 1, j] - buffer1[i + 1, j];
                    var yOffset = buffer1[i, j - 1] - buffer1[i, j + 1];
                    var shade = xOffset;

                    // texture - get from image using i+xoffset, j+yoffset
                    sw.GetPixel(i - Border + xOffset/3, j - Border + yOffset/3, out r, out g, out b);

                    // add shading
                    // (r,g,b)+=shading;

                    r = Math.Max(Math.Min(r + shade, 255), 0);
                    g = Math.Max(Math.Min(g + shade, 255), 0);
                    b = Math.Max(Math.Min(b + shade, 255), 0);

                    SetPixel(i - Border, j - Border, r, g, b);
                }

            // lissajous drops
            for (var p = 0; p < 7; ++p)
            {
                var t = Frame/50.0 + 2*Math.PI*p/7;
                var x1 = 1.0*Math.Sin(1*t + 0);
                var y1 = 1.0*Math.Sin(4*t + 0);

                var x = (int) (x1*Width/2 + Width/2);
                var y = (int) (y1*Height/2 + Height/2);
                if (0 <= x && 0 <= y && x < Width && y < Height)
                {
                    var mx = Math.Min(Width, Height);
                    buffer1[x, y] = 2*mx + 2*mx;
                }
            }

            if (Rand.NextDouble() < 0.6)
            {
                // add random drop
                var x = Rand.Next(Width);
                var y = Rand.Next(Height);

                var mx = Math.Min(Width, Height);
                buffer1[x, y] = Rand.Next(2*mx) + 2*mx;
            }
        }
    }
}