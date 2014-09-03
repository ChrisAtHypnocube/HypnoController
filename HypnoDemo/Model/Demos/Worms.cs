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
    class Worms : DemoBase
    {

        public Worms(int w, int h)
            : base(w, h)
        {
        }

        List<Worm> worms = new List<Worm>(); 

        class Worm
        {
            public int life, maxLife;
            public double dir,speed,x,y,hs;

            public Worm(int x,int y, int maxLife)
            {
                this.x = x;
                this.y = y;
                this.maxLife = maxLife;
                this.speed = (DemoBase.Rand.NextDouble() + 2) / 3.0;
                this.dir = DemoBase.Rand.NextDouble() * Math.PI* 2;
                hs = DemoBase.Rand.NextDouble();
            }
            
        }


        public override void Update()
        {
            base.Update();

            Fade(0, 0, 0, 0.05);

            if (worms.Count < 100 && DemoBase.Rand.NextDouble() < 0.1)
                worms.Add(new Worm(DemoBase.Rand.Next(0, Width), DemoBase.Rand.Next(0, Height), 1000));

            for (var i = 0; i < worms.Count; i++)
            {
                var worm = worms[i];
                worm.dir += (DemoBase.Rand.NextDouble() - 0.5)*3*Math.PI/180.0;
                var angle = worm.dir;
                worm.x += Math.Cos(angle)*worm.speed;
                worm.y += Math.Sin(angle)*worm.speed;

                var x = (int) worm.x;
                var y = (int) worm.y;

                var hue = (double)worm.life/worm.maxLife + worm.hs;
                int r, g, b;
                HslToRgb(hue,1,.5,out r,out g, out b);
                SetPixel(x, y, r, g, b);

                if (worm.life ++ > worm.maxLife || x < 0 || y < 0 || x >= Width || y >= Height)
                {
                    worms.RemoveAt(i);
                }

                if (DemoBase.Rand.NextDouble() > 0.9 && worms.Count < 1000)
                {
                    worms.Add(new Worm(x, y, 1+worm.maxLife/10));
                }
            }

        }

    }
}
