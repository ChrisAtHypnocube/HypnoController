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

namespace Hypnocube.Demo.Model
{
    /// <summary>
    ///     BitmapFont format
    ///     Follows AngelCode format
    ///     see http://www.angelcode.com/products/bmfont/doc/render_text.html
    ///     and http://www.angelcode.com/products/bmfont/doc/file_format.html
    /// </summary>
    public sealed class BitmapFont
    {
        /// <summary>
        ///     Name of the font
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     How far to move the cursor between rendered lines
        /// </summary>
        public int LineHeight { get; set; }

        /// <summary>
        ///     How far below the top of the font the base of the character should be placed.
        ///     Characters can extend above and below this
        /// </summary>
        public int Base { get; set; }

        /// <summary>
        ///     Pixel height
        /// </summary>
        public int RenderedSize { get; set; }

        /// <summary>
        ///     Pixel Width, if monospaced, else -1
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        ///     Create new empty bitmap font
        /// </summary>
        /// <param name="getPixel">Given i,j in the source image, return the pixel in ARGB format</param>
        public BitmapFont(string name, int lineHeight, int base1, int renderedSize, int[] charDefs,
            Func<int, int, uint> getPixel, int width = -1)
        {
            Name = name;
            LineHeight = lineHeight;
            Base = base1;
            RenderedSize = renderedSize;
            Width = width;

            Characters = new BitmapCharacter[256];
            KerningList = new List<Kerning>();

            for (var i = 0; i < charDefs.Length; i += 10)
            {
                var id = charDefs[i + 0];
                var x = charDefs[i + 1];
                var y = charDefs[i + 2];
                var w = charDefs[i + 3];
                var h = charDefs[i + 4];
                var xoffset = charDefs[i + 5];
                var yoffset = charDefs[i + 6];
                var xadvance = charDefs[i + 7];
                var page = charDefs[i + 8];
                var chnl = charDefs[i + 9];
                var c = new BitmapCharacter(id, x, y, w, h, xoffset, yoffset, xadvance, page, chnl, getPixel);
                Characters[id] = c;
            }
            // fill rest
            for (var i = 0; i < 256; i++)
                if (Characters[i] == null)
                    Characters[i] = new BitmapCharacter();
        }


        public class BitmapCharacter
        {
            /// <summary>
            ///     Order of symbols from the font making program from AngelCode
            /// </summary>
            /// <param name="id"></param>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <param name="width"></param>
            /// <param name="height"></param>
            /// <param name="xoffset"></param>
            /// <param name="yoffset"></param>
            /// <param name="xadvance"></param>
            /// <param name="page"></param>
            /// <param name="chnl"></param>
            public BitmapCharacter(int id, int x, int y, int width, int height, int xoffset, int yoffset, int xadvance,
                int page, int chnl, Func<int, int, uint> getPixel)
            {
                Height = height;
                Width = width;
                XAdvance = xadvance;
                XOffset = xoffset;
                YOffset = yoffset;
                Data = new uint[width, height];
                for (var j = 0; j < height; ++j)
                    for (var i = 0; i < width; ++i)
                        Data[i, j] = getPixel(i + x, j + y);
            }

            public BitmapCharacter()
            {
            }

            /// <summary>
            ///     Pixel width in texture and output
            /// </summary>
            public int Width { get; set; }

            /// <summary>
            ///     Pixel height in texture and output
            /// </summary>
            public int Height { get; set; }

            /// <summary>
            ///     Distance from cursor to left of where to draw
            /// </summary>
            public int XOffset { get; set; }

            /// <summary>
            ///     Distance from cell top to top of character
            /// </summary>
            public int YOffset { get; set; }

            /// <summary>
            ///     How far to advance the cursor after this character
            /// </summary>
            public int XAdvance { get; set; }

            /// <summary>
            ///     Pixel data, in ARGB format
            /// </summary>
            public uint[,] Data { get; set; }
        }


        /// <summary>
        ///     ASCII characters
        /// </summary>
        public BitmapCharacter[] Characters { get; private set; }

        public List<Kerning> KerningList { get; private set; }

        /// <summary>
        ///     Per character kerning information
        /// </summary>
        public class Kerning
        {
            public int First { get; set; }
            public int Second { get; set; }
            public int Amount { get; set; }
        }

#if false
    /// <summary>
    /// Create a font from a file.
    /// Return null if an error.
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
        public static BitmapFont ReadFile(string filename)
        {
            var font = new BitmapFont();
            var bytes = File.ReadAllBytes(filename);

            // start with ASCII "BMF"
            if (bytes.Length < 3 || bytes[0] != 66 || bytes[1] != 77 || bytes[2] != 70)
                return null; // wrong header
            var version = bytes[3];
            if (version > 3)
                return null; // unsupported version

            var index = 4; // start here

            Func<int, int> Read = n =>
            {
                var val = 0;
                for (var i = 0; i < n; ++i)
                    val = bytes[index++];
                return val;
            };
            Func<int, uint> ReadU = n => (uint)Read(n);
            int pages = 0;

            while (index < bytes.Length)
            {
                var type = bytes[index++];
                var size = Read(4); // block size, not including type and 4 byte size field
                var next = size + index; // should end here
                switch (type)
                {
                    case 1: // general info - how font was generated
                        {
                            var fontSize = Read(2); // size of TT font
                            var bits = Read(1);
                            // bit 0: smooth, bit 1: unicode, bit 2: italic, bit 3: bold, bit 4: fixedHeight, bits 5-7: reserved
                            var smooth = (bits & 1) != 0;  // smoothing was on
                            var unicode = (bits & 2) != 0; // a unicode charset
                            var italic = (bits & 4) != 0;  // font is italic
                            var bold = (bits & 8) != 0;    // font is bold
                            var fixedHeight = (bits & 16) != 0;

                            var charSet = Read(1); // name of OEM charset, when not Unicode
                            var stretchH = Read(2); // 100% means no stretch
                            var aa = Read(1); // supersampling level used
                            var paddingUp = Read(1);
                            var paddingRight = Read(1);
                            var paddingDown = Read(1);
                            var paddingLeft = Read(1);
                            var spacingH = Read(1);
                            var spacingV = Read(1); // character spacing
                            var outline = Read(1); // outline thickness
                            var sb = new StringBuilder();
                            while (bytes[index] != 0)
                                sb.Append((char)bytes[index++]);
                            var name = sb.ToString(); // name of true type font used
                            index++; // skip 0
                        }

                        break;
                    case 2: // common
                        {
                            font.LineHeight = Read(2); // uint 0  
                            font.Base = Read(2); // uint 2  
                            var scaleW = Read(2); // uint 4  
                            var scaleH = Read(2); // uint 6  
                            pages = Read(2); // uint 8 number of texture pages 
                            var bitField = Read(1); // bits 10 bits 0-6: reserved, bit 7: packed 
                            var packed = (bitField & 128) != 0; // if packed, each color channel has monochrome characters and alpha channel describes what's in channels

                            // each channel: Set to 0 if the channel holds the glyph data, 1 if it holds the outline, 2 if it holds the glyph and the outline, 3 if its set to zero, and 4 if its set to one.
                            var alphaChnl = Read(1); // uint 11  
                            var redChnl = Read(1); //uint 12  
                            var greenChnl = Read(1); //uint 13  
                            var blueChnl = Read(1); //uint 14 
                        }

                        break;
                    case 3: // page names for textures
                        {
                            for (var i = 0; i < pages; ++i)
                            {
                                var sb = new StringBuilder();
                                while (bytes[index] != 0)
                                    sb.Append((char)bytes[index++]);
                                var name = sb.ToString();
                                index++; // skip 0
                            }

                        }
                        break;
                    case 4: // chars
                        {
                            // The number of characters in the file can be computed by taking the size of 
                            // the block and dividing with the size of the charInfo structure, i.e.: numChars = charsBlock.blockSize/20.
                            if ((size % 20) != 0)
                                return null; // wrong size
                            var num = size / 20;
                            for (var i = 0; i < num; ++i)
                            {
                                var c = new BitmapCharacter();
                                var id = Read(4); // character ID (unicode?)
                                //uint 0+c*20 These fields are repeated until all characters have been described 
                                c.X = Read(2); // uint 4+c*20   texture X position
                                c.Y = Read(2); //uint 6+c*20    texture Y position
                                c.Width = Read(2); //uint 8+c*20  texture width
                                c.Height = Read(2); //uint 10+c*20  texture height
                                c.XOffset = Read(2); // int 12+c*20  offset when copying to screen
                                c.YOffset = Read(2); // int 14+c*20  
                                c.XAdvance = Read(2); //int 16+c*20  advance after drawing character
                                var page = Read(1); // uint 18+c*20   // texture page where character is found
                                var chnl = Read(1); // uint 19+c*20 // texture where found (1=blue,2-green,4-red,8=alpha,15=all channels)
                                font.Characters[id] = c;
                            }
                        }
                        break;
                    case 5: // kerning pairs
                        {
                            if ((size % 10) != 0)
                                return null; // wrong size
                            var num = size / 10;
                            for (var i = 0; i < num; ++i)
                            {
                                // This block is only in the file if there are any kerning pairs with amount differing from 0.
                                var k = new Kerning();
                                // uint 0+c*10 These fields are repeated until all kerning pairs have been described 
                                k.First = Read(4); // first character id
                                k.Second = Read(4); // uint 4+c*10  second character id
                                k.Amount = Read(2); // int 8+c*6 amount to adjust x position
                                font.KerningList.Add(k);
                            }
                        }
                        break;
                    default:
                        return null; // unknown block type
                }
                if (next != index)
                    return null; // wrong block size
            }

            return font;
        }
#endif
    }
}