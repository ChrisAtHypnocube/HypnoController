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
    internal class RasterBar : DemoBase
    {
        public RasterBar(int w, int h)
            : base(w, h)
        {
        }

        public override void Update()
        {
            base.Update();

            Fill(0, 0, 0);

            var barHt = 5;

            var angle = Frame/15.0;

            // create three bar x,y,z values and h value
            double gd, bd;
            double h = 0;

            var maxBars = 3;
            var bars = new Bar[maxBars];

            var da = Math.PI*2/maxBars;
            for (var bar = 0; bar < maxBars; ++bar)
            {
                bars[bar] = new Bar();
                bars[bar].hue = 1.0/maxBars*bar;
                bars[bar].y = Math.Sin(angle + da*bar);
                bars[bar].z = Math.Cos(angle + da*bar);
            }

            // sort by depth
            for (var outer = 0; outer < maxBars; ++outer)
                for (var inner = outer; inner < maxBars; ++inner)
                {
                    if (bars[outer].z > bars[inner].z)
                    {
                        var temp = bars[outer];
                        bars[outer] = bars[inner];
                        bars[inner] = temp;
                    }
                }

            for (var bar = 0; bar < 3; ++bar)
            {
                for (var j = 0; j < barHt; ++j)
                {
                    var l = 0.7*(0.2 + Math.Sin(Math.PI*j/(barHt - 1))) + bars[bar].z/7.0;
                    h = bars[bar].hue;
                    double rd;
                    HslToRgb(h, 1.0, l, out rd, out gd, out bd);

                    var r = (byte) (rd*255.0);
                    var g = (byte) (gd*255.0);
                    var b = (byte) (bd*255.0);
                    for (var i = 0; i < Width; ++i)
                    {
                        var tt = Math.Sin(angle);
                        var offset = Math.Sin((h + 0.5) + Math.PI*3/Width*i + (h + 0.3)*Frame/7.0)*3 +
                                     Math.Sin(Frame/20.0*(h - 0.4));
                        offset *= Math.Sin((i/12.0 + Frame*0.1)*(h - 0.3));
                        var y = (int) (offset + j + bars[bar].y*(Height - barHt/2.0)/2.0 + Height/2.0 - barHt/2.0);
                        SetPixel(i, y, r, g, b);
                    }
                }
                h += 1.0/3.0;
            }
        }

        #region Nested type: Bar

        private class Bar
        {
            /// <summary>
            ///     Base color
            /// </summary>
            public double hue;

            /// <summary>
            ///     Height off center [-1,1]
            /// </summary>
            public double y;

            /// <summary>
            ///     depth [-1,1]
            /// </summary>
            public double z;
        }

        #endregion
    }
}