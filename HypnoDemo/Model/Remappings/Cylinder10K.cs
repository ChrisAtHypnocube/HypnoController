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
    internal class Cylinder10K : Remapper
    {
        private byte[] dst;
        private byte[] tmp;

        public Cylinder10K()
        {
            //Width = 200;
            //Height = 25;
            //Strands = 1;

            Width = 200;
            Height = 50;
            Strands = 4;
            SupportedStrands.Add(1);
            SupportedStrands.Add(2);
            SupportedStrands.Add(4);
            SupportedStrands.Add(8);
        }


        public override byte[] MapImage(uint[] src, byte[] gamma)
        {
            if (gamma == null)
                gamma = DefaultGamma;

            if (dst == null || dst.Length != Width*Height*3)
                dst = new byte[Width*Height*3];
            if (tmp == null || tmp.Length != Width*Height*3)
                tmp = new byte[Width*Height*3];

            for (var i = 0; i < Width; ++i)
                for (var j = 0; j < Height; ++j)
                {
                    var srcIndex = i + j*Width;
#if true
                    // for the folded over 10K panel, arranged as:
                    // 3 2 1 0
                    // 4 5 6 7
                    var top = (j/25) == 0; // top row?
                    var panel = top ? (199 - i)/50 : (i/50 + 4); // which length of 50*25 = 1250 are we in?

                    int i2, j2;
                    if (top)
                    {
                        j2 = 24 - j;
                        var even = (j2 & 1) == 0;
                        j2 /= 2; // j2 now measures strands of length 100, over and back
                        i2 = even ? (199 - i)%50 : (i%50) + 50; // i2 measures length along strand of 100
                    }
                    else
                    {
                        j2 = j - 25; // now in range 0-24
                        var even = (j2 & 1) == 0;
                        j2 /= 2; // j2 now measures strands of length 100, over and back
                        i2 = even ? (i%50) : ((199 - i)%50) + 50; // i2 measures length along strand of 100
                    }
                    var dstIndex = panel*1250 + j2*100 + i2;
#else
    // method for the 5K block of leds
                    var dstIndex =
                        (i/50)*1250 + // panel
                        (j/2)*100 + // which strand of 100 over and back
                        (1-(j & 1))*(i%50) + // even j cases
                        (j & 1)*(99 - (i%50)); // even  j cases

#endif
                    var p = src[srcIndex]; // bgra
                    dstIndex *= 3;
                    dst[dstIndex++] = gamma[(p >> 8) & 255]; // g
                    dst[dstIndex++] = gamma[(p >> 16) & 255]; // r
                    dst[dstIndex] = gamma[(p >> 0) & 255]; // b
                }


            // panel 3 is missing, so remove all LEDs from 3*1250 to 4*1250
            // for (var i = 3*1250*3; i < dst.Length - 1250*3; ++i)
            //   dst[i] = dst[i + 1250*3];


            // special deleting of bytes for LEDs that burned out yet pass data through.
            // todo - move to other location and let them be set somehow
            //var delete = new[] { 1250 };
            //foreach (var pos in delete)
            //{
            //    var d = pos * 3;
            //    for (var i = d; i < dst.Length - 3; ++i)
            //        dst[i] = dst[i + 3];
            //}

            if (Strands == 1)
                return dst;

            // remap by strands
            if (Strands == 2)
            {
                // layout for 1 strand is
                // 3 2 1 0
                // 4 5 6 7

                // layout for 2 strand is
                // 3 2 1 0
                // 0 1 2 3

                for (var i = 0; i < 5000*3; i += 3)
                {
                    var j = i*2;
                    tmp[j] = dst[i];
                    tmp[j + 1] = dst[i + 1];
                    tmp[j + 2] = dst[i + 2];
                    tmp[j + 3] = dst[i + 5000*3];
                    tmp[j + 4] = dst[i + 5000*3 + 1];
                    tmp[j + 5] = dst[i + 5000*3 + 2];
                }
            }
            else if (Strands == 4)
            {
                // layout for 1 strand is
                // 3 2 1 0
                // 4 5 6 7

                // layout for 4 strand is
                // 1 0 1 0
                // 0 1 0 1

                for (var i = 0; i < 2500*3; i += 3)
                {
                    var j = i*4;
                    tmp[j] = dst[i];
                    tmp[j + 1] = dst[i + 1];
                    tmp[j + 2] = dst[i + 2];
                    tmp[j + 3] = dst[i + 2500*3];
                    tmp[j + 4] = dst[i + 2500*3 + 1];
                    tmp[j + 5] = dst[i + 2500*3 + 2];
                    tmp[j + 6] = dst[i + 5000*3];
                    tmp[j + 7] = dst[i + 5000*3 + 1];
                    tmp[j + 8] = dst[i + 5000*3 + 2];
                    tmp[j + 9] = dst[i + 7500*3];
                    tmp[j + 10] = dst[i + 7500*3 + 1];
                    tmp[j + 11] = dst[i + 7500*3 + 2];
                }
            }
            else if (Strands == 8)
            {
                // layout for 1 strand is
                // 3 2 1 0
                // 4 5 6 7

                // layout for 8 strand is
                // 0 0 0 0 (same order, interleaved)
                // 0 0 0 0

                for (var i = 0; i < 1250*3; i += 3)
                {
                    var j = i*8;
                    tmp[j] = dst[i];
                    tmp[j + 1] = dst[i + 1];
                    tmp[j + 2] = dst[i + 2];
                    tmp[j + 3] = dst[i + 1250*3];
                    tmp[j + 4] = dst[i + 1250*3 + 1];
                    tmp[j + 5] = dst[i + 1250*3 + 2];
                    tmp[j + 6] = dst[i + 2500*3];
                    tmp[j + 7] = dst[i + 2500*3 + 1];
                    tmp[j + 8] = dst[i + 2500*3 + 2];
                    tmp[j + 9] = dst[i + 3750*3];
                    tmp[j + 10] = dst[i + 3750*3 + 1];
                    tmp[j + 11] = dst[i + 3750*3 + 2];
                    tmp[j + 12] = dst[i + 5000*3];
                    tmp[j + 13] = dst[i + 5000*3 + 1];
                    tmp[j + 14] = dst[i + 5000*3 + 2];
                    tmp[j + 15] = dst[i + 6250*3];
                    tmp[j + 16] = dst[i + 6250*3 + 1];
                    tmp[j + 17] = dst[i + 6250*3 + 2];
                    tmp[j + 18] = dst[i + 7500*3];
                    tmp[j + 19] = dst[i + 7500*3 + 1];
                    tmp[j + 20] = dst[i + 7500*3 + 2];
                    tmp[j + 21] = dst[i + 8750*3];
                    tmp[j + 22] = dst[i + 8750*3 + 1];
                    tmp[j + 23] = dst[i + 8750*3 + 2];
                }
            }
            else
            {
                throw new Exception("Unsupported number of strands");
            }

            return tmp;
        }
    }
}