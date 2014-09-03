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
    internal class Wormhole : DemoBase
    {
        private readonly int[] pal = new int[256]; // 0RGB as bytes
        private int[,] grid;

        public Wormhole(int w, int h)
            : base(w, h)
        {
            Init();
        }

        private void Init()

        {
            const int w = 200; // grid.GetLength(0);
            var h = 50; // grid.GetLength(1) * 2;

            grid = new int[w, h];

            // values defining the shape
            double stretch = 25;
            double xCenter = w/2;
            double yCenter = h/4;
            double divisions = 1200;
            double spokes = 2400;

            // set palette
            for (var k = 0; k < 16; k++)
                for (var l = 0; l < 16; l++)
                {
                    var b = 63;
                    var g = 4*(k%16);
                    var r = 4*(l%16);
                    var i = k + 16*l;
                    pal[i] = ((r*4) << 16) + ((g*4) << 8) + (b*4);
                }

//Do all the work!
//convert r,theta,z to x,y,z to screen x,y
//plot the point
//z=-1.0+(log(2.0*j/DIVS) is the line that sets the math eqn for plot
//Feel free to try other functions!
//Cylindrical coordinates, i.e. z=f(r,theta)


            for (var j = 1; j < divisions + 1; j++)
                for (var i = 0; i < spokes; i++)
                {
                    var z = -1.0 + (Math.Log((2.0*j/divisions)));
                    var x = (w*j/divisions*Math.Cos(2*Math.PI*i/spokes));
                    var y = (h*j/divisions*Math.Sin(2*Math.PI*i/spokes));
                    y = y - stretch*z;
                    x += xCenter;
                    y += yCenter;
                    var color = ((i/8)%16) + 16*((j/6)%16);
                    if ((x >= 0) && (x < w) && (y >= 0) && (y < h))
                        grid[(int) x, (int) y] = color;
                }
        }

        private void PalRight(int[] pal)
        {
            for (var i = 0; i < 16; ++i)
            {
                var t = pal[i*16];
                for (var j = 0; j < 16 - 1; ++j)
                    pal[i*16 + j] = pal[i*16 + j + 1];
                pal[i*16 + 15] = t;
            }
        }

        private void PalDown(int[] pal)
        {
            for (var j = 0; j < 16; ++j)
            {
                var t = pal[j];
                for (var i = 0; i < 16 - 1; ++i)
                    pal[i*16 + j] = pal[(i + 1)*16 + j];
                pal[j + 15*16] = t;
            }
        }


        public override void Update()
        {
            base.Update();

            Fill(0, 0, 0);

            var pal2 = new int[256];
#if true
            //for (var i = 0; i < 256; ++i)
            //    pal2[i] = pal[i];

            int r = 0, g = 0, b = 0;

            Action<int, int> draw = (i, j) =>
            {
                var index = i + j*16;

                pal2[index] = (r << 16) + (g << 8) + b;
            };

            var h = Frame/480.0;

            for (var i = 0; i < 4; ++i)
            {
                HslToRgb(h, 1, 0.5, out r, out g, out b);
                DrawLine(i, i, i, 15 - i, draw);
                HslToRgb(h + 0.25, 1, 0.5, out r, out g, out b);
                DrawLine(i, 15 - i, 15 - i, 15 - i, draw);
                HslToRgb(h + 0.5, 1, 0.5, out r, out g, out b);
                DrawLine(15 - i, 15 - i, 15 - i, i, draw);
                HslToRgb(h + 0.75, 1, 0.5, out r, out g, out b);
                DrawLine(15 - i, i, i, i, draw);
            }


#else


            var h1 = (Math.Sin(frame/30.0)+1)/2;
            h1 = 0;
            var h2 = (h1 + 0.5);
            if (h2 > 1) h2 -= 1;

            // set palette
            for (var k = 0; k < 16; k++)
                for (var l = 0; l < 16; l++)
                {
                    //var b = 63;
                    //var g = 4 * (k % 16);
                    //var r = 4 * (l % 16);
                    int r1, r2, g1, g2, b1, b2;
                    double l1 = (Math.Sin(k / 16.0*Math.PI)+1)/4;
                    double l2 = (Math.Sin(l / 16.0*Math.PI)+1)/4;
                    l1 = l2 = 0.5;
                    HslToRgb(h1, 1.0, l1, out r1, out g1, out b1);
                    HslToRgb(h2, 1.0, l2, out r2, out g2, out b2);
                    var r = (r1 + r1)/2;
                    var g = (g1 + g1) / 2;
                    var b = (b1 + b1) / 2;
                    var i = k + 16 * l;
                    pal2[i] = ((r) << 16) + ((g) << 8) + (b);
                }
#endif


            var angle = Frame/50.0;
            var rs = Math.Cos(angle)*128 + 128;
            var ds = Math.Sin(angle)*128 + 128;

            for (var i = 0; i < rs; ++i)
                PalRight(pal2);
            for (var i = 0; i < ds; ++i)
                PalDown(pal2);

            var cx = grid.GetLength(0)/2;
            var cy = grid.GetLength(1)/2;

            for (var i = 0; i < Width; ++i)
                for (var j = 0; j < Height; ++j)
                {
                    var gi = cx + (i - Width/2);
                    var gj = cy + (j - Height/2);
                    var color = pal2[grid[gi, gj]];
                    r = (color >> 16) & 255;
                    g = (color >> 8) & 255;
                    b = (color >> 0) & 255;
                    SetPixel(i, j, r, g, b);
                }
        }
    }
}