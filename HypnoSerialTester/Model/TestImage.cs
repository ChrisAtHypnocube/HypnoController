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
namespace Hypnocube.SerialTester.Model
{
    /// <summary>
    ///     Generate test images for the given number of strands
    /// </summary>
    public sealed class TestImage
    {
        public byte[] data;
        private int height;
        private int width;

        public TestImage(int width, int height, int size)
        {
            this.width = width;
            this.height = height;
            data = new byte[size];
        }

        // print binary value along strand starting at i,j, in positive j direction
        private void PrintValue(int value, int bits, int i, int j, int r, int g, int b)
        {
            for (var k = 0; k < bits; ++k)
            {
                if ((value & 1) == 1)
                    SetPixel(i, j, r, g, b);
                else
                    SetPixel(i, j, 0, 0, 0);
                j++;
                value >>= 1;
            }
        }

        /// <summary>
        ///     Set pixel in GRB data array
        /// </summary>
        /// <param name="i">strand</param>
        /// <param name="j">along strand</param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        private void SetPixel(int i, int j, int r, int g, int b)
        {
            var index = (i + j*width)*3;
            if (index < 0 || data.Length <= index)
                return;
            data[index++] = (byte) g;
            data[index++] = (byte) r;
            data[index] = (byte) b;
        }

        /// <summary>
        /// </summary>
        /// <param name="frame">Frame of animation</param>
        /// <param name="speedDivider">Divisor for frame speed</param>
        /// <param name="brightness">bright colors 0-255</param>
        /// <param name="darkness">dark colors 0-255 (not black)</param>
        /// <param name="blockLength">Length of lit block</param>
        /// <param name="blockSpacing">Space between blocks</param>
        /// <param name="blockStrandDelta">Delta applied per strand for blocks</param>
        /// <param name="showEvenStrands"></param>
        /// <param name="showOddStrands"></param>
        /// <returns></returns>
        public void GenerateTestImage(
            int frame,
            int speedDivider,
            int brightness,
            int darkness,
            int blockLength,
            int blockSpacing,
            int blockStrandDelta,
            bool showEvenStrands,
            bool showOddStrands
            )
        {
            // clear
            for (var i = 0; i < data.Length; ++i)
                data[i] = 0;

            int r, g, b;

            int er = 0, eg = 0, eb = 0; // end strand color
            if ((frame%3) == 0)
                er = 255;
            else if ((frame%3) == 1)
                eg = 255;
            else
                eb = 255;

            if ((frame & 15) == 0)
            {
                //for (var i = 0; i < width; ++i)
                //   for (var j = 0; j < height; ++j)
                //      SetPixel(i,j,er,eg,eb);
            }

            // ends of 625 blocks
            for (var i = 0; i < width; ++i)
                for (var j = 0; j < height; j += 625)
                {
                    SetPixel(i, j, darkness, darkness, darkness);
                    SetPixel(i, j + 624, brightness, brightness, brightness);
                }

            // ends of strands
            r = g = b = brightness;
            for (var i = 0; i < width; ++i)
            {
                var iEven = (i & 1) == 0;
                if (showEvenStrands && iEven)
                {
                    Fill(i, 0, 1, 4, darkness, darkness, darkness);
                    Fill(i, height - 5, 1, 4, darkness, darkness, darkness);
                    SetPixel(i, 0, er, eg, eb);
                    SetPixel(i, height - 1, er, eg, eb);
                    PrintValue(i, 4, i, 1, r, 0, 0);
                    PrintValue(i, 4, i, height - 5, 0, g, 0);
                }
                if (showOddStrands && !iEven)
                {
                    Fill(i, 0, 1, 4, darkness, darkness, darkness);
                    Fill(i, height - 5, 1, 4, darkness, darkness, darkness);
                    SetPixel(i, 0, er, eg, eb);
                    SetPixel(i, height - 1, er, eg, eb);
                    PrintValue(i, 4, i, 1, r, 0, 0);
                    PrintValue(i, 4, i, height - 5, 0, g, 0);
                }
            }

            // sliding blocks in eight colors, staggered by strand
            var d = (blockLength + blockSpacing);
            for (var i = 0; i < width; ++i)
                for (var j = 0; j < height; ++j)
                {
                    var offset = (frame/speedDivider) + i*blockStrandDelta;
                    if (((j + offset)%d) < blockLength)
                    {
                        // draw pixel
                        var pos = (j + offset)%(8*d);
                        var colorIndex = (pos/d) & 7;
                        r = g = b = 0;
                        if ((colorIndex & 1) != 0) r = brightness;
                        if ((colorIndex & 2) != 0) g = brightness;
                        if ((colorIndex & 4) != 0) b = brightness;
                        if (r == 0 && g == 0 && b == 0)
                            r = g = b = darkness;
                        SetPixel(i, j, r, g, b);
                    }
                }

            // 8 bit frame counter at 0,0
            PrintValue(frame, 8, 0, 0, brightness, brightness, brightness);
        }

        private void Fill(int i1, int j1, int di, int dj, int r, int g, int b)
        {
            for (var i = i1; i <= i1 + di; i += di)
                for (var j = j1; j <= j1 + dj; j += dj)
                    SetPixel(i, j, r, g, b);
        }
    }
}