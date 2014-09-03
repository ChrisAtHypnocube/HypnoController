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

namespace Hypnocube.Demo.Model.Remappings
{
    internal class Flat960 : Remapper
    {
        private byte[] dst;

        public Flat960()
        {
            Width = 60;
            Height = 16;
            Strands = 1;
            SupportedStrands.Add(1);
            SupportedStrands.Add(2);
        }

        /// <summary>
        ///     Maps the image if there is one long strands:
        ///     strands go over and back and over and back
        /// </summary>
        /// <param name="src"></param>
        private void MapImageSingle(uint[] src)
        {
            var dstIndex = 0;
            for (var j = 0; j < Height; j += 2)
            {
                for (var i = 0; i < Width; ++i)
                {
                    var srcIndex = (i + j*Width);
                    var p = src[srcIndex]; // BRGA
                    dst[dstIndex++] = (byte) (p >> 24); // g
                    dst[dstIndex++] = (byte) (p >> 16); // r
                    dst[dstIndex++] = (byte) (p >> 8); // b
                }
                for (var i = Width - 1; i >= 0; --i)
                {
                    var srcIndex = (i + (j + 1)*Width);

                    var p = src[srcIndex]; // BRGA
                    dst[dstIndex++] = (byte) (p >> 24); // g
                    dst[dstIndex++] = (byte) (p >> 16); // r
                    dst[dstIndex++] = (byte) (p >> 8); // b
                }
            }
        }


        /// <summary>
        ///     Maps the image if there are two strands:
        ///     first strand is top half, second is bottom half,
        ///     strands go over and back and over and back
        /// </summary>
        /// <param name="src"></param>
        private void MapImageSplit(uint[] src)
        {
            var dstIndex = 0;
            for (var j = 0; j < Height/2; j += 2)
            {
                for (var i = 0; i < Width; ++i)
                {
                    var srcIndex = (i + j*Width);
                    var p = src[srcIndex]; // BRGA
                    dst[dstIndex++] = (byte) (p >> 24); // g
                    dst[dstIndex++] = (byte) (p >> 16); // r
                    dst[dstIndex++] = (byte) (p >> 8); // b

                    srcIndex = (i + (j + Height/2)*Width);

                    p = src[srcIndex]; // BRGA
                    dst[dstIndex++] = (byte) (p >> 24); // g
                    dst[dstIndex++] = (byte) (p >> 16); // r
                    dst[dstIndex++] = (byte) (p >> 8); // b
                }
                for (var i = Width - 1; i >= 0; --i)
                {
                    var srcIndex = (i + (j + 1)*Width);
                    var p = src[srcIndex]; // BRGA
                    dst[dstIndex++] = (byte) (p >> 24); // g
                    dst[dstIndex++] = (byte) (p >> 16); // r
                    dst[dstIndex++] = (byte) (p >> 8); // b

                    srcIndex = (i + (j + 1 + Height/2)*Width);
                    p = src[srcIndex]; // BRGA
                    dst[dstIndex++] = (byte) (p >> 24); // g
                    dst[dstIndex++] = (byte) (p >> 16); // r
                    dst[dstIndex++] = (byte) (p >> 8); // b
                }
            }
        }

        public override byte[] MapImage(uint[] src, byte[] gamma)
        {
            if (dst == null || dst.Length != Width*Height*3)
                dst = new byte[Width*Height*3];
            if (Strands == 1)
                MapImageSingle(src);
            else if (Strands == 2)
                MapImageSplit(src);
            else throw new NotImplementedException("Unsupported strand count " + Strands);
            return dst;
        }
    }
}