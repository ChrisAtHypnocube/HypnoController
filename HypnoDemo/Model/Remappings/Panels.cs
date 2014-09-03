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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Hypnocube.Demo.Model.Remappings
{
    /// <summary>
    ///     Generic remapper that creates a mapping table based on a sequence
    ///     of box definitions.
    /// </summary>
    internal class Panels : Remapper
    {
        private byte[] dst;

        /// <summary>
        ///     Mapping. Given pixel (i,j), entry (i+j*Width) contains
        ///     the index (position * 3) where this pixel is output.
        /// </summary>
        private int[] mapping;

        private List<PanelBox> panels;

        public Panels()
        {
            //Width = 200;
            //Height = 25;
            //Strands = 1;

            Width = 200;
            Height = 50;
            Strands = 4;

            // create a default mapping
            var panels = new[]
            {
                // single box covering the entire thing
                new PanelBox(0, 0, Width - 1, Height - 1, false, true)
            };
            CreateMapping(panels);
        }

        /// <summary>
        ///     Given a list of panels, compute the mapping.
        ///     As soon as enough pixels are reached for one strand, the
        ///     next batch goes on the next strand, until all boxes are parsed.
        ///     Thus give all panels on first strind, then second strand, etc.
        ///     Any unmapped pixels will not be output
        /// </summary>
        /// <param name="panels"></param>
        public void CreateMapping(IEnumerable<PanelBox> panels)
        {
            // make local copy in case of size, strand changes
            this.panels = new List<PanelBox>();
            foreach (var p in panels)
                this.panels.Add(new PanelBox(p.XIn, p.YIn, p.XOut, p.YOut, p.Reversing, p.XMajor));
            CreateMapping();
        }

        // see comment on other form of this function
        private void CreateMapping()
        {
            // find size of mapping
            var pixelCount = 0;
            foreach (var p in panels)
            {
                var dx = Math.Abs(p.XIn - p.XOut) + 1;
                var dy = Math.Abs(p.YIn - p.YOut) + 1;
                pixelCount += dx*dy;
            }
            if (mapping == null || mapping.Length != pixelCount)
                mapping = new int[pixelCount];

            // mark all as -1, meaning unused
            for (var i = 0; i < mapping.Length; ++i)
                mapping[i] = -1;

            // walk panels, adding to output
            var dstIndex = 0; // first pixel to output
            foreach (var p in panels)
            {
                var dy = Math.Sign(p.YOut - p.YIn); // main y direction, 1,0,or -1
                var dx = Math.Sign(p.XOut - p.XIn); // main x direction, 1,0,or -1

                var x1 = p.XIn;
                var x2 = p.XOut;
                var y1 = p.YIn;
                var y2 = p.YOut;


                if (!p.XMajor)
                {
                    // transpose
                    var t = x1;
                    x1 = y1;
                    y1 = t;
                    t = x2;
                    x2 = y2;
                    y2 = t;
                    t = dx;
                    dx = dy;
                    dy = t;
                }

                // now process it. Handles x major and x minor at once
                var y = y1 - dy; // start back one
                do
                {
                    // next row (col if major reversed)
                    y += dy;


                    // check reversing
                    var reverseRow = p.Reversing && ((y - y1) & 1) == 1;
                    if (reverseRow)
                    {
                        // reverse row
                        var t = x1;
                        x1 = x2;
                        x2 = t;
                        dx = -dx;
                    }

                    // process row (col if major reversed)
                    var x = x1 - dx; // start back one
                    do
                    {
                        x += dx; // next entry

                        // compute correct place to place the next destination
                        var index = p.XMajor ? x + y*Width : y + x*Width;

                        try
                        {
                            if (mapping[index] != -1)
                                Trace.TraceError("ERROR! Panel mapping double write. Check panel definitions");
                            mapping[index] = dstIndex;
                        }
                        catch
                        {
                            Trace.TraceError("ERROR! Panel mapping out of bounds");
                        }

                        dstIndex += Strands;
                        if (dstIndex >= mapping.Length)
                        {
                            // next strand. Subtract height and add one
                            dstIndex -= mapping.Length;
                            dstIndex += 1; // next strand
                        }
                    } while (x != x2);

                    // undo any reversing
                    if (reverseRow)
                    {
                        // un - reverse row
                        var t = x1;
                        x1 = x2;
                        x2 = t;
                        dx = -dx;
                    }
                } while (y != y2);
            }
            // finally, mult any used by 3 for final RGB index (speeds up mapping later)
            var missedCount = 0;
            for (var i = 0; i < mapping.Length; ++i)
            {
                if (mapping[i] != -1)
                    mapping[i] *= 3;
                else
                    missedCount++;
            }
            if (missedCount != 0)
                Trace.TraceWarning("Warning! Panel mapping missed " + missedCount + " entries");
        }

        public override byte[] MapImage(uint[] src, byte[] gamma)
        {
            if (gamma == null)
                gamma = DefaultGamma;

            // keep memory allocated from frame to frame if possible
            if (dst == null || dst.Length != Width*Height*3)
                dst = new byte[Width*Height*3];

            for (var i = 0; i < Width; ++i)
                for (var j = 0; j < Height; ++j)
                {
                    var srcIndex = i + j*Width;
                    var p = src[srcIndex]; // bgra
                    var dstIndex = mapping[srcIndex];
                    dst[dstIndex++] = gamma[(p >> 8) & 255]; // g
                    dst[dstIndex++] = gamma[(p >> 16) & 255]; // r
                    dst[dstIndex] = gamma[(p >> 0) & 255]; // b
                }

            return dst;
        }

        #region Nested type: PanelBox

        public class PanelBox
        {
            public PanelBox(int x1, int y1, int x2, int y2, bool reversing, bool xmajor)
            {
                XIn = x1;
                YIn = y1;
                XOut = x2;
                YOut = y2;
                Reversing = reversing;
                XMajor = xmajor;
            }

            /// <summary>
            ///     input screen X coord for box
            /// </summary>
            public int XIn { get; private set; }

            /// <summary>
            ///     input screen Y coord for box
            /// </summary>
            public int YIn { get; private set; }

            /// <summary>
            ///     output screen X coord for box
            /// </summary>
            public int XOut { get; private set; }

            /// <summary>
            ///     output screen Y coord for box
            /// </summary>
            public int YOut { get; private set; }

            /// <summary>
            ///     Does the direction reverse at each end of the box? Or start in the same direction each pass
            /// </summary>
            public bool Reversing { get; private set; }

            /// <summary>
            ///     Is X the major direction?  If so, box is scanned in X direction, then Y direction, else reversed
            /// </summary>
            public bool XMajor { get; private set; }
        }

        #endregion
    }
}