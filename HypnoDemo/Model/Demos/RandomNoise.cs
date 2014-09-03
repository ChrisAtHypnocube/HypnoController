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
namespace Hypnocube.Demo.Model.Demos
{
    internal class RandomNoise : DemoBase
    {
        public RandomNoise(int w, int h)
            : base(w, h)
        {
        }

        // scale a [0,1] value to [min,max]
        private double Scale(double val, double min, double max)
        {
            return val*(max - min) + min;
        }

        public override void Update()
        {
            base.Update();

            for (var i = 0; i < Width; ++i)
                for (var j = 0; j < Height; ++j)
                {
                    if (Rand.NextDouble() < 0.9)
                        continue;
                    var h = Rand.NextDouble(); // hue
                    var s = Rand.NextDouble(); // sat
                    var l = Rand.NextDouble(); // lum


                    double r, g, b;
                    HslToRgb(h, Scale(s, 0.4, 1), Scale(l, 0.15, 0.55), out r, out g, out b);
                    SetPixel(i, j, (int) (r*255), (int) (g*255), (int) (b*255));
                }
        }
    }
}