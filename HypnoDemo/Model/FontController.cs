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
using System.Diagnostics;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Hypnocube.Demo.Model
{
    internal class FontController
    {
        private int h;
        private byte[] pixels;
        private int w;

        public FontController()
        {
            if (pixels == null)
                Load();
        }

        public BitmapFont Font { get; set; }

        private void Load()
        {
            var data = Font2.Data;
            var charData = Font2.CharacterDefs;
            using (var stream = new MemoryStream(data))
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = stream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();

                w = bitmap.PixelWidth;
                h = bitmap.PixelHeight;
                Debug.Assert(bitmap.Format == PixelFormats.Bgra32);
                pixels = new byte[w*h*4];
                bitmap.CopyPixels(pixels, w*4, 0);

                // rip characters, making a font
                Font = new BitmapFont(
                    Font2.Face, Font2.LineHeight, Font2.Base, Font2.Size, Font2.CharacterDefs,
                    (i, j) =>
                    {
                        var ind = (i + j*w)*4;
                        uint val = 0;
                        val |= (uint) (pixels[ind + 3] << 24); // A
                        val |= (uint) (pixels[ind + 2] << 16); // R 
                        val |= (uint) (pixels[ind + 1] << 8); // G
                        val |= (uint) (pixels[ind + 0] << 0); // B

                        return val;
                    }
                    );
            }
        }


        /// <summary>
        ///     Render the text at the given location
        ///     Calls the set pixel action, which takes an x,y location, then rgba bytes
        ///     only draws letters that overlap (0,0)-(w-1,h-1)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="text"></param>
        /// <param name="setPixel"></param>
        public void Render(
            ref int x, ref int y, string text,
            int w, int h,
            Action<int, int, byte, byte, byte, byte> setPixel)
        {
            foreach (var c in text)
            {
                var ch = Font.Characters[c];
                if (ch == null || ch.Width == 0)
                {
                    x += Font.RenderedSize; // assume this
                    continue;
                }

                // box test
                var x1 = x + ch.XOffset;
                var x2 = x1 + ch.Width;
                var y1 = y + ch.YOffset;
                var y2 = y1 + ch.Height;
                var misses = x2 < 0 || w <= x1 || y2 < 0 || h <= y1;
                if (!misses)
                {
                    // draw character
                    for (var j = 0; j < ch.Height; ++j)
                        for (var i = 0; i < ch.Width; ++i)
                        {
                            var argb = ch.Data[i, j];
                            var a = (byte) (argb >> 24);
                            if (a == 0) continue;
                            var r = (byte) (argb >> 16);
                            var g = (byte) (argb >> 8);
                            var b = (byte) (argb >> 0);

                            setPixel(i + x1, j + y1, r, g, b, a);
                        }
                }

                // update position
                // todo - add kerning support?
                x += ch.XAdvance;
            }
        }
    }
}