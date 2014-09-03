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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hypnocube.Demo.Model.Demos
{
    class Voroni :DemoBase
    {
        public Voroni(int w, int h) : base(w, h)
        {
            
        }

        private readonly List<BounceBall> balls = new List<BounceBall>();


        public override void Update()
        {
            base.Update();

            var count = 5;
            if (balls == null || balls.Count < count)
            {
                while (balls.Count < count)
                    balls.Add(new BounceBall(Width, Height, true));

                var colors = new int[] {255,0,0,0,255,0,0,0,255, 255,255,0, 0,255,255};
                for (var i = 0; i < count; ++i)
                {
                    balls[i].r = colors[i*3 + 0];
                    balls[i].g = colors[i*3 + 1];
                    balls[i].b = colors[i*3 + 2];
                }

            }
            foreach (var ball in balls)
                ball.UpdateLinear(Width, Height);

            for (var i = 0; i < Width; ++i)
                for (var j = 0; j < Height; ++j)
                {
                    var ball = balls[0];
                    var bestDist = Double.MaxValue;
                    foreach (var b in balls)
                    {
                        var dx = b.x - i;
                        var dy = b.y - j;
                        var d = dx*dx + dy*dy;
                        if (d < bestDist)
                        {
                            ball = b;
                            bestDist = d;
                        }
                    }
                    SetPixel(i, j, ball.r/3, ball.g/3, ball.b/3);
                }
            foreach (var b in balls)
                SetPixel((int) b.x, (int) b.y, b.r, b.g, b.b);

        }
    }
}
