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
using System.Collections.Generic;

namespace Hypnocube.Demo.Model.Demos
{
    internal class Heartbeat : DemoBase
    {
        private List<Beat> beats;

        public Heartbeat(int w, int h) : base(w, h)
        {
        }

        public override void Update()
        {
            base.Update();

            var numBeats = 3;
            if (beats == null || numBeats != beats.Count)
            {
                beats = new List<Beat>();
                for (var i = 0; i < numBeats; ++i)
                {
                    var b = new Beat();
                    b.x = Rand.Next(Width);
                    b.dx = (Rand.Next(3) - 1.5)/3.0;
                    beats.Add(b);
                    b.h = 1.0/(numBeats)*i;
                    b.param = Rand.NextDouble();
                }
            }

            Fade(0, 0, 0, 0.04);

            foreach (var beat in beats)
            {
                var x1 = (int) (beat.x + Math.Sin(Frame/10.0*beat.param)*2);
                var x2 = (int) (beat.x + Math.Cos(Frame/13.0*beat.param)*2);
                beat.x += beat.dx;
                if (beat.x < 0 || Width <= beat.x)
                {
                    beat.dx = -beat.dx;
                    beat.x += beat.dx;
                }

                var y =
                    (int)
                        ((Math.Sin(beat.param*Frame/10.0 + Frame/20.0 /*+Math.Cos(beat.param+frame/15.0)*/) + 1)/2.0*
                         Height/2.0);

                int r, g, b;

                DrawLine(x1, y, x2, Height - 1 - y, (i, j) =>
                {
                    var dy = Math.Abs(Height/2.0 - j)/(Height/2.0);
                    dy = (dy + 1)/2*0.8;
                    HslToRgb(beat.h, dy, 0.5, out r, out g, out b);
                    MaxPixel(i, j, r, g, b);
                });

                HslToRgb(beat.h, 1.0, 0.5, out r, out g, out b);
                MaxPixel(x1, y, r, g, b);
                MaxPixel(x2, Height - 1 - y, r, g, b);


                beat.h = beat.h + 0.001;
                if (beat.h > 1) beat.h -= 1;
            }
        }

        #region Nested type: Beat

        private class Beat
        {
            public double dx;
            public double h;
            public double param;
            public double x;
        }

        #endregion
    }
}