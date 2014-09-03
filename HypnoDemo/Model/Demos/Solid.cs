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
    internal class Solid : DemoBase
    {
        public Solid(int w, int h)
            : base(w, h)
        {
        }

        public override void Update()
        {
            base.Update();

            double rd, gd, bd;
            var h1 = (Frame & 511)/511.0;
            h1 = ScaleHue(h1, 2);

            HslToRgb(h1, 1, 0.5, out rd, out gd, out bd);

            var r = (int) (rd*255);
            var g = (int) (gd*255);
            var b = (int) (bd*255);
            Fill(r, g, b);
        }
    }
}