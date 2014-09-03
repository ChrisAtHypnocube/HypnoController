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

// Version 1.0, Aug 2013 - Initial release
// LEDStrand controller interface
using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;

namespace Hypnocube.Device
{
    /// <summary>
    ///     Represent an instance of the LED strand controller  board
    ///     for controlling LED strands, such as the WS2812 or WS2812B
    /// </summary>
    public sealed class HypnoLsdController : SerialDeviceBase
    {
        #region public interface

        #region static items

        /// <summary>
        ///     The value of the sync byte used in drawing mode
        /// </summary>
        public static byte SyncByte
        {
            get { return 254; }
        }

        #endregion // static

        #region Properties

        #region Drawing Property

        private bool drawing = false;

        /// <summary>
        ///     Gets or sets if this is in drawing mode.
        ///     Send drawing command and token to enter, sends double sync to exit
        /// </summary>
        public bool Drawing
        {
            get { return drawing; }
            set
            {
                // return true if field is changed
                if (!SetField(ref drawing, value)) return;
                if (value)
                {
                    // enter drawing
                    WriteString("draw{0}", CommandEnd);
                }
                else
                {
                    // exit drawing - send double sync byte
                    WriteByte(SyncByte);
                    WriteByte(SyncByte);
                }
            }
        }

        #endregion

        #region ImageSize Property

        private int imageSize = 64*1*3;

        /// <summary>
        ///     Gets the size of an image in bytes. Change size with the SetSize method.
        /// </summary>
        public int ImageSize
        {
            get { return imageSize; }
            private set
            {
                // return true if field is changed
                SetField(ref imageSize, value);
            }
        }

        #endregion

        #region ImageWidth Property

        private int imageWidth = 0;

        /// <summary>
        ///     Gets the image width. Use the SetSize method to change it.
        /// </summary>
        [Feature(GroupName = "Miscellaneous Commands", Label = "Width", Help = "Number of strands",
            MinValue = 0, MaxValue = 16
            )]
        public int ImageWidth
        {
            get { return imageWidth; }
        }

        #endregion

        #region ImageHeight Property

        private int imageHeight = 0;

        /// <summary>
        ///     Gets the image height. Use the SetSize method to change it.
        /// </summary>
        public int ImageHeight
        {
            get { return imageHeight; }
        }

        #endregion

        #endregion

        #region Commands

        /// <summary>
        ///     Send the stats command
        /// </summary>
        public void DumpStats()
        {
            WriteString("stats{0}", CommandEnd);
        }

        /// <summary>
        ///     Send the reset stats command
        /// </summary>
        public void ResetStats()
        {
            WriteString("reset stats{0}", CommandEnd);
        }

        /// <summary>
        ///     Set the baud rate by sending the clock divider command
        /// </summary>
        /// <param name="divider">The clock divider in 0-65535</param>
        public override void SetDivider(ushort divider)
        {
            WriteString("set speed {0}{1}", divider, CommandEnd);
        }


        /// <summary>
        ///     Send the version command
        /// </summary>
        public void GetVersion()
        {
            WriteString("version{0}", CommandEnd);
        }

        /// <summary>
        ///     Send the info command
        /// </summary>
        public void DumpInfo()
        {
            WriteString("info{0}", CommandEnd);
        }

        /// <summary>
        ///     Send the get id command
        /// </summary>
        public void GetSpeed()
        {
            WriteString("get speed{0}", CommandEnd);
        }

        /// <summary>
        ///     Send the get id command
        /// </summary>
        public void GetId()
        {
            WriteString("get id{0}", CommandEnd);
        }

        /// <summary>
        ///     Send the set id command
        /// </summary>
        /// <param name="id">The user specified ID in 0-65535</param>
        public void SetId(ushort id)
        {
            WriteString("set id {0}{1}", id, CommandEnd);
        }

        /// <summary>
        ///     Send the help command
        /// </summary>
        [Feature(GroupName = "Miscellaneous Commands", Label = "Help", Help = "Get help from the gadget")]
        public void GetHelp()
        {
            WriteString("help{0}", CommandEnd);
        }

        /// <summary>
        ///     Send the save settings command
        /// </summary>
        public void SaveSettings()
        {
            WriteString("save settings{0}", CommandEnd);
        }

        /// <summary>
        ///     Send the timings command
        /// </summary>
        public void DumpTimings()
        {
            WriteString("timings{0}", CommandEnd);
        }

        /// <summary>
        ///     Send the sizes command to list the maximum height for each width.
        /// </summary>
        public void GetSizes()
        {
            WriteString("sizes{0}", CommandEnd);
        }


        /// <summary>
        ///     Send the get size command
        /// </summary>
        public void GetSize()
        {
            WriteString("get size{0}", CommandEnd);
        }

        /// <summary>
        ///     Set the image size.
        ///     Width clamped to 1-16.
        ///     Height clamped to 1-65535, then possibly clamped more by the device.
        ///     See the sizes command for maximum heights for each width.
        /// </summary>
        /// <param name="width">The width in 1-16</param>
        /// <param name="height">The height in 1 to the maximum for the given width</param>
        public void SetSize(int width, int height)
        {
            // clamp
            if (width < 1) width = 1;
            if (width > 16) width = 16;
            if (height < 1) height = 1;
            if (65535 < height) height = 65535;

            // set internal values
            imageHeight = height;
            imageWidth = width;

            NotifyPropertyChanged(() => ImageWidth);
            NotifyPropertyChanged(() => ImageHeight);

            WriteString("set size {0} {1}{2}", ImageWidth, ImageHeight, CommandEnd);
            ImageSize = ImageWidth*ImageHeight*3; // RGB
            tempImageBuffer = new byte[ImageSize + 1];
        }

        /// <summary>
        ///     Dump the ram buffer
        /// </summary>
        /// <param name="size"></param>
        public void DumpRam(ushort size)
        {
            WriteString("dump image {0}{1}", size, CommandEnd);
        }

        /// <summary>
        ///     Runs a connection test, which may take a long time
        ///     if the parameters are chosen poorly.
        ///     Sends bytes to the device for the test sequence,
        ///     causing returned bytes to give error message.
        /// </summary>
        /// Returns milliseconds elapsed
        public long TestConnection(ushort size, uint count, int errorsPerPass = 0, byte[] buffer = null)
        {
            if (buffer == null)
            {
                buffer = new byte[size];
                var rand = new Random();
                rand.NextBytes(buffer);
            }

            WriteString("test conn {0} {1}{2}", size, count, CommandEnd);
            Thread.Sleep(100);

            var errorIndex = 0;

            var timer = new Stopwatch();
            timer.Start();

            // send base copy of data
            WriteBytes(buffer);

            // send the copies to test
            for (var i = 0; i < count; ++i)
            {
                if (errorsPerPass > 0)
                {
                    // generate errors
                    for (var j = errorIndex; j < errorIndex + errorsPerPass; ++j)
                        buffer[j%size]++;
                }
                WriteBytes(buffer);
                if (errorsPerPass > 0)
                {
                    // fix errors
                    for (var j = errorIndex; j < errorIndex + errorsPerPass; ++j)
                        buffer[j%size]--;
                    errorIndex = (errorIndex + errorsPerPass)%size;
                }
            }

            timer.Stop();
            return timer.ElapsedMilliseconds;
        }


        /// <summary>
        ///     Run the given demo for the given number of milliseconds.
        ///     0 milliseconds means run until another command seen.
        ///     Demo index is 1 to the max shown in the Info command.
        ///     Demo 0 means all demos.
        /// </summary>
        /// <param name="demoIndex"></param>
        /// <param name="milliseconds"></param>
        public void RunDemo(int demoIndex, int milliseconds)
        {
            WriteString("rundemo {0} {1}{2}", demoIndex, milliseconds, CommandEnd);
        }

        /// <summary>
        ///     Set the delay until a demo starts to the given number of seconds when
        ///     no other commands are sent. 0 seconds means do not run demos.
        /// </summary>
        /// <param name="delay"></param>
        public void SetDemoDelay(int delay)
        {
            WriteString("demodelay {0}{1}", delay, CommandEnd);
        }

        /// <summary>
        ///     Set the diagnostic LED to on or off.
        /// </summary>
        /// <param name="on"></param>
        public void SetLedUsage(bool on)
        {
            WriteString("LED {0}{1}", on ? 1 : 0, CommandEnd);
        }

        #endregion

        #region Helper functions

        /// <summary>
        ///     Create the device controller
        /// </summary>
        public HypnoLsdController()
        {
            PicClockSpeed = 48000000;
            RamSize = 30000;
            DefaultBaudRate = 9600;
            ConnectionSpeed = DefaultBaudRate;
            SetSize(1, 250); // default size
        }

        /// <summary>
        ///     Write an image to the device. Make sure in drawing mode.
        ///     Replaces all bytes with value 254 with the value 255, as per the spec.
        ///     May throw.
        /// </summary>
        /// <param name="image">The image buffer to write</param>
        public void WriteImage(byte[] image)
        {
            if (image.Length != ImageSize)
                throw new Exception("Invalid image size. Must be length " + ImageSize);
            try
            {
                for (var i = 0; i < ImageSize; ++i)
                {
                    var dat = image[i];
                    if (dat == SyncByte)
                        dat = 255; // remap
                    tempImageBuffer[i] = dat;
                }
                tempImageBuffer[ImageSize] = SyncByte;
                WriteBytes(tempImageBuffer); // write image
            }
            catch (Exception exception)
            {
                Trace.TraceError("Exception: {0}", exception.ToString());
                throw;
            }
        }

        #endregion

        #endregion

        #region Private implementation

        /// <summary>
        ///     string used for end of a command
        /// </summary>
        private const string CommandEnd = "\r\n";


        /// <summary>
        ///     Temp image buffer used to store buffer to send
        /// </summary>
        private byte[] tempImageBuffer;

        /// <summary>
        ///     Serial data returned handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void SerialDataReceived(object sender, SerialDataReceivedEventArgs args)
        {
            var thePort = (SerialPort) sender;
            var bytes = thePort.BytesToRead;
            var data = new byte[bytes];
            thePort.Read(data, 0, bytes);

            OnDataEvent(data);
        }

        /// <summary>
        ///     Handle a data event
        /// </summary>
        /// <param name="data"></param>
        private void OnDataEvent(byte[] data)
        {
            var e = MessageReceived;
            if (e != null)
                e(this, new MessageReceivedArgs {Data = data});
        }

        #endregion

        // todo - does this need a Dispose?

        public void SkewTest(int skewDelay1, int skewDelay2, int skewDelay3, int length, int red, int green, int blue)
        {
            red &= 255;
            green &= 255;
            blue &= 255;
            var rgb = (uint) ((red << 16) | (green << 8) | (blue));
            WriteString("skewtest {0} {1} {2} {3} {4}{5}",
                length, skewDelay1, skewDelay2, skewDelay3, rgb,
                CommandEnd);
        }
    }
}