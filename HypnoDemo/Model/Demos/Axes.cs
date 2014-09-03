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
    internal class Axes : DemoBase
    {
        public Axes(int w, int h)
            : base(w, h)
        {
        }

        public override void Update()
        {
            base.Update();

            Fill(0, 0, 0);
            const int rd = 5;
            var offset1 = (int) (Math.Sin(Frame/50.0)*3*rd - 3*rd);
            var offset2 = (int) (Math.Sin(Frame/57.0 + 1)*3*rd - 3*rd);
            for (var i = offset1; i < Width; i += rd)
            {
                int r, g, b;
                var h = (double) i/Width;
                h = ScaleHue(h, 2);
                HslToRgb(h, 1.0, 0.5, out r, out g, out b);
                DrawLine(i, 0, i, Height - 1, r, g, b);
            }

            for (var j = offset2; j < Height; j += rd)
            {
                DrawLine(0, j, Width - 1, j,
                    (i1, j1) =>
                    {
                        int r, g, b;
                        var h = (double) i1/Width;
                        HslToRgb(h, 1.0, 0.5, out r, out g, out b);
                        MaxPixel(i1, j1, r, g, b);
                    });
            }
        }
    }
}