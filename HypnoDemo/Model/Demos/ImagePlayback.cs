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
using System.IO;

namespace Hypnocube.Demo.Model.Demos
{
    internal class ImagePlayback : DemoBase
    {
        private int counter;


        public ImagePlayback(int w, int h)
            : base(w, h)
        {
            // todo - needs filepath to work
            //this.filepath = filepath;
        }

        private string filepath;

        /// <summary>
        ///     Create a filename for the given string
        /// </summary>
        /// <param name="counter"></param>
        /// <returns></returns>
        public static string Filename(string filePath, int counter)
        {
            var filename = String.Format("HCPlayback{0}.PNG", counter.ToString("D6"));
            return Path.Combine(filePath, filename);
        }

        public override void Reset()
        {
            base.Reset();
            counter = 0;
        }

        public override void Update()
        {
            base.Update();

            var filename = Filename(filepath, counter++);
            if (!File.Exists(filename))
            {
                counter = 0;
                filename = Filename(filepath,counter++);
            }
            if (!File.Exists(filename))
                return; // nothing to do

            var pngData = File.ReadAllBytes(filename);

            var surf = FromMemoryPNG(pngData);

            var xdelta = 25; // used to shift image to center our demo off the seam

            for (var i = 0; i < Width; ++i)
                for (var j = 0; j < Height; ++j)
                {
                    int r, g, b;
                    surf.GetPixel(i, j, out r, out g, out b);
                    SetPixel(i - xdelta, j, r, g, b);
                }
        }
    }
}