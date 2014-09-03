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
using System.Data.Common;

namespace Hypnocube.Demo.Model.Remappings
{
    public abstract class Remapper
    {
        // width of the underlying image
        protected static byte[] DefaultGamma;

        public List<int> SupportedStrands { get; private set; }

        protected Remapper()
        {
            SupportedStrands = new List<int>();
        }

        static Remapper()
        {
            DefaultGamma = new byte[256];
            for (var i = 0; i < 256; ++i)
                DefaultGamma[i] = (byte) i;
        }

        public int Width { get; set; }

        // height of the underlying image
        public int Height { get; set; }

        /// <summary>
        ///     The number of strands used
        /// </summary>
        public int Strands { get; set; }

        /// <summary>
        ///     Given a BGRA surface (one pixel per uint), return
        ///     a byte array to send to the device
        /// </summary>
        /// <param name="src"></param>
        /// <param name="gamma">Gamma table to use, or null if none</param>
        /// <returns></returns>
        public abstract byte[] MapImage(uint[] src, byte[] gamma);
    }
}