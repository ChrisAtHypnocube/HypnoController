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
using System.Diagnostics;

namespace Hypnocube.Demo.Model.Demos
{
    internal class Plasma3 : DemoBase
    {
        public Plasma3(int w, int h)
            : base(w, h)
        {
        }


        // over-generic way to evaluate term like
        // sum m=1 to n of Am Cos[Bm*i + Cm *j + Dm * t + Em]
        // where each const A,B,C,D,E may also be another term
        // each returns value in [-1,1]


        private double Compute(double dx, double dy, double angle,
            double scaleX = 0.5, double scaleXscale = 5,
            double scaleY = 0.333, double scaleYscale = 5,
            double rotateSpeed = 0.1, double rotateScale = Math.PI,
            double translateXScale = 4, double translateYScale = 4
            )
        {
            // scale
            var srx = dx*(Math.Sin(angle*scaleX) + scaleXscale);
            var sry = dy*(Math.Cos(angle*scaleY) + scaleYscale);

            // rotate dx,dy
            var rt = Math.Sin(angle*rotateSpeed)*rotateScale;
            var dxr = srx*Math.Sin(rt) + sry*Math.Cos(rt);
            var dyr = -srx*Math.Cos(rt) + sry*Math.Sin(rt);

            // translate
            var dxt = dxr + (Math.Sin(dx) + Math.Cos(dy) + Math.Cos(rt)*translateXScale);
            var dyt = dyr + (Math.Cos(dx/1.1) + Math.Cos(dy - dx) + Math.Sin(rt)*translateYScale);
            var r = Math.Cos(dxt)*Math.Sin(dyt);

            return r;
        }

        // convert value in [-n to n] to 0-255
        private int Convert(double r, int n)
        {
            // r in [-n,n]
            Debug.Assert(-n <= r && r <= n);
            r += n; // [0,2n]
            r /= n; // [0,2]
            Debug.Assert(0 <= r && r <= 2);
            if (r > 1) r = 2 - r; // reflect
            Debug.Assert(0 <= r && r <= 1);

            // r in 0,1
            return (int) (r*255);
        }

        public override void Update()
        {
            base.Update();

            var angle = Frame/10.0;
            var min = Math.Min(Width, Height);
            for (var i = 0; i < Width; ++i)
                for (var j = 0; j < Height; ++j)
                {
#if false
                    var j2 = j + Math.Sin(frame / 20.0) * 1.2 + 20.5 * Math.Cos(i * Math.PI * 2.0 / Width * Math.Sin(frame / 30.0) + frame / 12.0);
                    var red = (int)((Math.Cos(angle + j2 / 10.0) + 1) * 255.0 / 2);
                    var green = (int)((Math.Sin(angle * 1.2 + j2 / 11.0) + 1) * 255.0 / 2);
                    var blue = (int)((Math.Cos(10 + angle + j2 / 17.0) + 1) * 255.0 / 2);
                    SetPixel(i, j, red, green, blue);
#else
                    // scaled coords, [-1,1] is smallest dimension
                    var dx = (i - Width/2.0)/min;
                    var dy = (j - Height/2.0)/min;

                    var r = Compute(dx, dy, angle, 0.5, 5, 0.333, 5, 0.11, Math.PI, 4, 4);
                    r += Compute(dx, dy, angle*1.1, 0.6, 7, 0.43, 4, 0.07, Math.PI, 5, 5);
                    var red = Convert(r, 2);

                    var g = Compute(dx, dy, angle + 0.1, 0.5, 5, 0.333, 5, 0.11, Math.PI, 4, 4);
                    g += Compute(dx, dy, angle/1.3, 0.6, 7, 0.43, 4, 0.07, Math.PI, 5, 5);
                    var green = Convert(g, 2);

                    var b = Compute(dx, dy, angle + 2.3, 0.5, 5, 0.333, 5, 0.11, Math.PI, 4, 4);
                    b += Compute(dx, dy, angle*1.2, 0.6, 7, 0.43, 4, 0.07, Math.PI, 5, 5);
                    var blue = Convert(b, 2);


                    SetPixel(i, j, red, green, blue);
#endif
                }
        }

        #region Nested type: Term

        private class Term
        {
            public Term(params object[] args)
            {
                if ((args.Length%5) != 0)
                    throw new Exception("Number of arguments must be multiple of 5");
            }
        }

        #endregion
    }
}