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
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Hypnocube.Device;

namespace Hypnocube.SerialTester.Model
{
    /// <summary>
    ///     Provide support functions for the LED controller
    /// </summary>
    public sealed class Support
    {
        /// <summary>
        ///     Bytes grabbed by byte listener
        /// </summary>
        private ConcurrentQueue<byte> recordedBytes;

        /// <summary>
        ///     Send the buffer to the drawing mode buffer, then get the buffer back raw
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        private byte[] RoundTripBuffer(byte[] buffer, HypnoLsdController device)
        {
            var mode = device.Drawing;
            if (mode)
                device.Drawing = false; // exit to ensure at start of drawing buffer
            Thread.Sleep(500); // give some time for above messages to return
            device.Drawing = true;
            Thread.Sleep(500); // give some time for above messages to return
            device.WriteBytes(buffer);
            Thread.Sleep(500); // give some time for above messages to return
            device.Drawing = false;
            Thread.Sleep(1000); // give some time for above messages to return

            device.MessageReceived += ByteListener;

            // get the buffer back
            recordedBytes = new ConcurrentQueue<byte>();
            var start = new Stopwatch();
            start.Start();
            var lastSize = 0;
            var testLength = Math.Min(buffer.Length, device.RamSize);
            device.DumpRam((ushort) testLength);
            // read until timeout
            while (true)
            {
                if (start.ElapsedMilliseconds > 1000)
                    break;
                if (lastSize != recordedBytes.Count)
                {
                    // new data, reset timeout
                    start.Restart();
                    lastSize = recordedBytes.Count;
                }
            }

            device.MessageReceived -= ByteListener;

            Thread.Sleep(500);

            var answer = recordedBytes.ToArray();
            // clean message - remove final "OK\r\n"and initial message up to byte '\n' = 0x0A
            if (answer.Length > 0)
                answer = answer.SkipWhile(b => b != '\n').Skip(1).Take(testLength).ToArray();

            device.Drawing = mode;
            Thread.Sleep(500);

            return answer;
        }

        /// <summary>
        ///     Do a test that the remote image buffer is getting the right
        ///     byte structure in RAM when receiving bytes.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="message"></param>
        public void TestImage(HypnoLsdController device, Action<string> message)
        {
            // fill buffer and test wraparound of buffer
            var buffer = new byte[device.RamSize + 1000];

            // save these
            int w = device.ImageWidth, h = device.ImageHeight;

            // set to full size and zero it
            device.SetSize(16, 625);

            Thread.Sleep(500); // wait for above to settle

            // zero the buffer
            var returnedBytes = RoundTripBuffer(buffer, device);

            // amount expected back. Accounts for small and large wraparound buffers
            var testLength = Math.Min(buffer.Length, device.RamSize);

            // compare it
            var error = returnedBytes.Length != testLength;
            var errorCount = 0;
            if (!error)
            {
                for (var i = 0; i < returnedBytes.Length; ++i)
                    errorCount += buffer[i] != returnedBytes[i] ? 1 : 0;
            }
            if (error || errorCount != 0)
                message("Comparison error on zero image, " + errorCount + " places" +
                        (errorCount == 0
                            ? ". " + returnedBytes.Length + " bytes bytes returned out of " + testLength
                            : "") + "\n");
            else
                message("No comparison error on zero image\n");

            // restore settings
            device.SetSize(w, h);

            // create legal random image. Remove sync bytes
            var rand = new Random();
            rand.NextBytes(buffer);
            for (var i = 0; i < buffer.Length; ++i)
            {
                if (buffer[i] == 254)
                    buffer[i] = 255;
            }

            // get the buffer back
            returnedBytes = RoundTripBuffer(buffer, device);

            // compare it
            error = returnedBytes.Length != testLength;
            errorCount = 0;
            byte[] packedBuffer = null;
            if (!error)
            {
                packedBuffer = StripeBuffer(device, w, h, buffer);
                for (var i = 0; i < testLength; ++i)
                    errorCount += packedBuffer[i] != returnedBytes[i] ? 1 : 0;
            }
            if (error || errorCount != 0)
            {
                message("Comparison error on complex image, " + errorCount + " places" +
                        (errorCount == 0
                            ? ". " + returnedBytes.Length + " bytes bytes returned out of " + testLength
                            : "") +
                        "\n");
            }
            else
                message("No comparison error on complex image\n");
        }

        /// <summary>
        ///     Given a stream of bytes and image size, simulate the PIC image buffer
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="input">the sequence of bytes to simulate</param>
        /// <returns></returns>
        private byte[] StripeBuffer(HypnoLsdController device, int width, int height, byte[] input)
        {
            var buffer = new byte[device.RamSize];

            int i1 = 0, j1 = 0, color = 0;
            // given a byte, and two bits, extract them, ith at position 4, jth at position 0, then shift
            Func<byte, int, int, int, byte> bits = (b, i, j, shift) =>
            {
                var bi = (b >> i) & 1;
                var bj = (b >> j) & 1;
                return (byte) (((bi << 4) | bj) << shift);
            };
            Action<byte, int, int, int> write = (b, i, j, colr) =>
            {
                var span = j/625;
                var physicalJ = j%625;
                var physicalI = i + width*span;
                var line = physicalI/4;
                var shift = physicalI%4;
                var offset = physicalJ*48 + colr*16 + 4*line;

                // write to buffer at offset and next three positions
                // nibble pattern is 02461357
                var mask = (byte) (~((1 << shift) + (1 << (shift + 4))));
                buffer[offset + 0] = (byte) ((buffer[offset + 0] & mask) | bits(b, 5, 7, shift)); // insert 5 and 7 bits
                buffer[offset + 1] = (byte) ((buffer[offset + 1] & mask) | bits(b, 1, 3, shift)); // insert 1 and 3 bits
                buffer[offset + 2] = (byte) ((buffer[offset + 2] & mask) | bits(b, 4, 6, shift)); // insert 4 and 6 bits
                buffer[offset + 3] = (byte) ((buffer[offset + 3] & mask) | bits(b, 0, 2, shift)); // insert 0 and 2 bits
            };

            var spans = (16/width);
            var maxheight = spans*625;

            foreach (var datum in input)
            {
                write(datum, i1, j1, color);
                color = (color + 1)%3; // cycle RGB
                if (color == 0)
                {
                    i1++;
                    if (i1 == width)
                    {
                        i1 = 0;
                        j1++;
                        if (j1 == maxheight)
                            j1 = 0;
                    }
                }
            }


            return buffer;
        }


        private void ByteListener(object sender, SerialDeviceBase.MessageReceivedArgs args)
        {
            var e = recordedBytes;
            if (e != null)
            {
                foreach (var b in args.Data)
                    e.Enqueue(b);
            }
        }
    }
}