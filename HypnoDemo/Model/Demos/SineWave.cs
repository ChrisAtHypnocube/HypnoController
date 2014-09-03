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
    internal class SineWave : DemoBase
    {
        public SineWave(int w, int h) : base(w, h)
        {
        }

        private void Draw(int i, int j1, int j2, int r, int g, int b, Action<int, int, int, int, int> pixelAction)
        {
            if (i == 0) j2 = j1;

            if (j1 > j2)
            {
                var t = j1;
                j1 = j2;
                j2 = t;
            }
            for (var j = j1; j <= j2; ++j)
                pixelAction(i, j, r, g, b);
        }

        public override void Update()
        {
            base.Update();
            Fade(0, 0, 0, 0.3);
            var angle = Frame/15.0;

            var h1 = (Frame & 255)/255.0;
            var h2 = h1 + 1/3.0;
            var h3 = h2 + 1/3.0;
            if (h2 > 1) h2 -= 1;
            if (h3 > 1) h3 -= 1;
            double r1d, g1d, b1d, r2d, g2d, b2d, r3d, g3d, b3d;
            HslToRgb(h1, 1, 0.5, out r1d, out g1d, out b1d);
            HslToRgb(h2, 1, 0.5, out r2d, out g2d, out b2d);
            HslToRgb(h3, 1, 0.5, out r3d, out g3d, out b3d);

            var r1 = (int) (r1d*255);
            var g1 = (int) (g1d*255);
            var b1 = (int) (b1d*255);
            var r2 = (int) (r2d*255);
            var g2 = (int) (g2d*255);
            var b2 = (int) (b2d*255);
            var r3 = (int) (r3d*255);
            var g3 = (int) (g3d*255);
            var b3 = (int) (b3d*255);

            var lastJ = new int[3];
            for (var i = 0; i < Width; ++i)
            {
                var j = (int) ((
                    Math.Cos(i/7.0 - angle + Math.Cos(angle))*Math.Sin(i/10.0 + 2.5*Math.Cos(angle)) +
                    Math.Cos(i/9.0 - angle + Math.Cos(1.7*angle))*Math.Sin(i/13.0 + 1.5*Math.Cos(1.4*angle)) +
                    2
                    )*(Height)/4.0);

                Draw(i, j, lastJ[0], r1, g1, b1, (x, y, rr, gg, bb) => SetPixel(x, y, rr, gg, bb));
                lastJ[0] = j;

                j = (int) ((
                    Math.Cos(i/11.0 - 1.5*angle + Math.Cos(2.2*angle))*Math.Sin(i/11.0 + 1.7*Math.Cos(angle)) +
                    Math.Cos(i/13.0 - angle + Math.Cos(1.7*angle))*Math.Sin(i/13.0 + 2.1*Math.Cos(1.3*angle)) +
                    2
                    )*(Height)/4.0);
                Draw(i, j, lastJ[1], r2, g2, b2, (x, y, rr, gg, bb) => MaxPixel(x, y, rr, gg, bb));
                lastJ[1] = j;
                //MaxPixel(i, j, r2, g2, b2);
                j = (int) ((
                    Math.Cos(i/9.0 - angle + Math.Cos(1.1*angle))*Math.Sin(i/12.0 + 1.9*Math.Cos(angle)) +
                    Math.Cos(i/12.0 - angle + Math.Cos(1.6*angle))*Math.Sin(i/15.0 + 2.2*Math.Cos(1.1*angle)) +
                    2
                    )*(Height)/4.0);
                Draw(i, j, lastJ[2], r3, g3, b3, (x, y, rr, gg, bb) => MaxPixel(x, y, rr, gg, bb));
                lastJ[2] = j;
                //MaxPixel(i, j, r3, g3, b3);
            }
        }
    }
}