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
    internal class Flags : DemoBase
    {
        private const int Big = 28; // pixels per large image
        private const int Small = 20;
        private const int SmallWidth = 32;
        private const int Dy = (Big + Small + 2);
        private static Surface flags;

        private readonly List<int> bigList = new List<int>();
        private readonly List<int> smallList = new List<int>();

        private int pos, pos2;

        public Flags(int w, int h) : base(w, h)
        {
        }

        public override void Update()
        {
            base.Update();
            if (flags == null)
                flags = FromMemoryPNG(FlagData.Data);

            var flagCount = flags.Height/Dy;

            while (bigList.Count < (Width + 2*flags.Width)/flags.Width)
                bigList.Add(Rand.Next(flagCount));
            while (smallList.Count < (Width + 2*SmallWidth)/SmallWidth)
                smallList.Insert(0, Rand.Next(flagCount));

            Fill(64, 64, 64);
            var x = pos;
            foreach (var d in bigList)
            {
                DrawFlag(x, 0, d, true);
                x += flags.Width + 1;
            }
            --pos;
            if (pos < -flags.Width)
            {
                pos += flags.Width;
                bigList.RemoveAt(0);
            }

            x = pos2;
            foreach (var d in smallList)
            {
                DrawFlag(x, Big + 2, d, false);
                x += SmallWidth + 1;
            }
            ++pos2;
            if (x - SmallWidth > Width)
            {
                pos2 -= SmallWidth;
                smallList.RemoveAt(smallList.Count - 1);
            }
        }

        private void DrawFlag(int x, int y, int index, bool large)
        {
            var h = index*Dy;
            var max = Big;
            if (!large)
            {
                h += Big + 1;
                max = Small;
            }


            for (var i = 0; i < flags.Width; ++i)
                for (var j = 0; j < max; ++j)
                {
                    int r, g, b;
                    flags.GetPixel(i, j + h, out r, out g, out b);
                    if ((flags.Image[i + (j + h)*flags.Width] & 0xFF000000) != 0)
                        SetPixel(i + x, j + y, r, g, b);
                }
        }
    }
}