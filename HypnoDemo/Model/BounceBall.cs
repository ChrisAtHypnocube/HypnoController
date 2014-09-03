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
using Hypnocube.Demo.Model.Demos;

namespace Hypnocube.Demo.Model
{
    public class BounceBall
    {
        public int r, g, b;
        public double vx;
        public double vy;
        public double x;
        public double y;

        public BounceBall(int width, int height, bool linear)
        {
            x = DemoBase.Rand.Next(width);
            y = DemoBase.Rand.Next(height);
            if (linear)
            {
                vx = DemoBase.Rand.Next(2) * 2 - 1;
                vy = DemoBase.Rand.Next(2) * 2 - 1;
            }
            else
                vx = DemoBase.Rand.NextDouble() * 2 - 1;

            double rd, gd, bd;
            var h = DemoBase.Rand.NextDouble();
            DemoBase.HslToRgb(h, 0.5, 0.5, out rd, out gd, out bd);

            r = (int)(rd * 255.0);
            g = (int)(gd * 255.0);
            b = (int)(bd * 255.0);

        }
        public void UpdateLinear(int width, int height)
        {
            x += vx;
            if (x >= width || 0 >= x)
                vx = -vx;
            y += vy;
            if (y >= height || 0 >= y)
                vy = -vy;

        }


        public void UpdateAcceleration(int width, int height, double acceleration)
        {
            if (y + vy < 0 || height - 1 < vy + y)
            {
                vy = -vy;
                //vy = -acceleration;
            }
            else
                vy += acceleration;
            y += vy;

            if (x + vx < 0 || width - 1 < vx + x)
                vx = -vx;
            x += vx;

            //var decay = 0.99;
            //vy *= decay;
            var energy = vx * vx + vy * vy;
            if (energy < 0)
            {
                vy = DemoBase.Rand.NextDouble() * 2 - 1;
                vx = DemoBase.Rand.NextDouble() * 2 - 1;
            }

        }
    }
}
