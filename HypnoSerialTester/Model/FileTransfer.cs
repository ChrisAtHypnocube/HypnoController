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
using System.IO;
using System.Threading;
using Hypnocube.Device;

namespace Hypnocube.SerialTester.Model
{
    internal class FileTransfer
    {
        internal void GetFile(HypnoLsdController device, string filename)
        {
        }

        internal void PutFile(HypnoLsdController device, string filename)
        {
            // read records until no more
            // a record is 128 bytes,
            // 1st byte is length N (0-126) of data bytes
            // then N data bytes (pad with 0 after N)
            // 128th byte is sum of bytes (checksum)
            // length 0 record with all 0 entries and 255 sum is end
            // after each record read correctly, returns 2 bytes: next record count mod 256 (0 based) and the checksum byte obtained
            // if record failed, resend with current record count mode 256 (asking for resend) and bit-inverted checksum byte
            var data = File.ReadAllBytes(filename);
            var block = new byte[128];
            var pos = 0; // position of data to send

            device.WriteString("putfile \"" + Path.GetFileName(filename) + "\"\r\n");
            Thread.Sleep(10);

            var len = 0;
            do
            {
                len = data.Length - pos;
                if (len > 126)
                    len = 126;
                block[0] = (byte) len;
                var checksum = 0;
                for (var i = 0; i < len; ++i)
                {
                    block[i + 1] = data[pos++];
                    checksum += block[i + 1];
                }
                // 0 fill 
                for (var i = len; i < 126; i++)
                    block[i + 1] = 0;
                block[127] = (byte) checksum;

                // send 4 bytes at a time, with delays
                var temp = new byte[4];
                for (var i = 0; i < block.Length; i += 4)
                {
                    for (var j = 0; j < 4; ++j)
                        temp[j] = block[j + i];
                    device.WriteBytes(temp);
                    Thread.Sleep(1);
                }
                //device.WriteBytes(block);
            } while (len != 0);
        }
    }
}