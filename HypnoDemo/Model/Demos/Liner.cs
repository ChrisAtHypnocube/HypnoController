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
using System.Collections.Generic;

namespace Hypnocube.Demo.Model.Demos
{
    internal class Liner : DemoBase
    {
        private int lineMax = 8;


        private List<Line> lines;

        public Liner(int w, int h)
            : base(w, h)
        {
        }

        public override void Reset()
        {
            base.Reset();
            lineMax = Rand.Next(6) + 3;
        }

        private double RandDir()
        {
            return Rand.NextDouble()*4 - 2;
        }

        public override void Update()
        {
            base.Update();

            if (lines == null || lines.Count != lineMax)
            {
                lines = new List<Line>();
                for (var i = 0; i < lineMax; ++i)
                {
                    var w = new Line();
                    w.x1 = Rand.Next(Width);
                    w.x2 = Rand.Next(Width);
                    w.y1 = Rand.Next(Height);
                    w.y2 = Rand.Next(Height);
                    w.dx1 = RandDir();
                    w.dx2 = RandDir();
                    w.dy1 = RandDir();
                    w.dy2 = RandDir();
                    w.hue = Rand.NextDouble();
                    lines.Add(w);
                }
            }

            Fade(0, 0, 0, 0.1);

            foreach (var line in lines)
            {
                line.Step(Width, Height);

                int r, g, b;
                HslToRgb(line.hue, 0.5, 0.5, out r, out g, out b);
                DrawLine(
                    (int) line.x1, (int) line.y1,
                    (int) line.x2, (int) line.y2,
                    (x, y) => MaxPixel(x, y, r, g, b)
                    );
                line.hue += 0.02;
            }
        }

        #region Nested type: Line

        private class Line
        {
            public double dx1;
            public double dx2;
            public double dy1;
            public double dy2;

            public double hue;
            public double x1;
            public double x2;
            public double y1;
            public double y2;

            private void Add(ref double coord, ref double delta, double max)
            {
                var t = coord + delta;
                if (t < 0 || max < t)
                    delta = -delta;
                coord += delta;
            }

            public void Step(int w, int h)
            {
                Add(ref x1, ref dx1, w);
                Add(ref x2, ref dx2, w);
                Add(ref y1, ref dy1, h);
                Add(ref y2, ref dy2, h);
            }
        }

        #endregion
    }
}