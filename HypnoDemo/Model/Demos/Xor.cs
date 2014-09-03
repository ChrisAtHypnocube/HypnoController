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
    class Xor :DemoBase
    {
        private int[,,] grids;
        public Xor(int w, int h)
            : base(w, h)
        {
            Init(w, h);
        }

        private int center;

        /// <summary>
        /// Create some grid shapes
        /// </summary>
        /// <param name="w"></param>
        /// <param name="h"></param>
        void Init( int w, int h)
        {
            var max = Math.Max(w, h); // biggest direction
            var s = max*4; // this big on a side
            var r = s/50.0; // radius of each circle

            center = s / 2; // center coords

            grids = new int[s,s,3]; 

            for (var i = 0; i < s; ++i)

                for (var j = 0; j < s; ++j)
                {
                    // compute a circle
                    var dx = center - i;
                    var dy = center - j;
                    var dist = Math.Sqrt(dx*dx + dy*dy);

                    var phase = (int)(dist/r);

                    grids[i, j, 0] = (phase&1)!=0?255:0;

                    //grids[i, j, 1] = 0;
                    //grids[i, j, 2] = 0;
                }

        }


        class Sampler
        {
            public double x, y;
            public double angle = 0, da = 0.05, a = 1, b = 1;

            public void Update()
            {
                angle += da;
                x = Math.Cos(a*angle);
                y = Math.Sin(b*angle);
            }
        }

        List<Sampler>  samplers = new List<Sampler>();

        public override void Update()
        {
            base.Update();

            while (samplers.Count < 2)
            {
                var s = new Sampler {angle = Rand.NextDouble()*Math.PI*2, a = (Rand.NextDouble() + 1)/2};
                s.da *= (Rand.NextDouble() + 2)/2;
                samplers.Add(s);
            }

            foreach (var s in samplers)
                s.Update();

            for (var i = -Width/2; i <= Width/2; ++i)
                for (var j = -Height/2; j <= Height/2; ++j)
                {
                    var color = 0;
                    foreach (var s in samplers)
                    {
                        var x = (int) (s.x*Width/3);
                        var y = (int) (s.y*Height/3);
                        color ^= grids[x+center+i, y+center+j, 0];
                    }
                    SetPixel(i+Width/2, j+Height/2, color, color, color);
                }


        }
    }
}
