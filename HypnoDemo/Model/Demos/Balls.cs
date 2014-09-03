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
    internal class Balls : DemoBase
    {
        private readonly List<BounceBall> balls = new List<BounceBall>();

        public Balls(int w, int h)
            : base(w, h)
        {
        }

        public override void Update()
        {
            base.Update();
            var count = Width*Height/30;
            if (balls == null || balls.Count < count)
            {
                while (balls.Count < count)
                    balls.Add(new BounceBall(Width, Height, true)); ;
            }
            Fade(0, 0, 0, 0.10);
            foreach (var ball in balls)
            {
                ball.UpdateLinear(Width,Height);
                MaxPixel((int)ball.x, (int)ball.y, ball.r, ball.g, ball.b);
            }
        }
    }
}