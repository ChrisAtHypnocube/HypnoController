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
namespace Hypnocube.Communications
{
    /// <summary>
    /// Holds some useful values for various device baud rates.
    /// </summary>
    public sealed class BaudRateSettings
    {
        public BaudRateSettings(
            SerialDeviceType deviceType,
            int desiredBaud,
            double actualBaud,
            string setting
            )
        {
            DeviceType = deviceType;
            TargetRate = desiredBaud;
            ActualBaud = actualBaud;
            PercentError = ComputePercentError(desiredBaud, actualBaud);
            Setting = setting;
        }


        public SerialDeviceType DeviceType { get; private set; }

        /// <summary>
        /// Desired rate
        /// </summary>
        public int TargetRate { get; private set; }

        /// <summary>
        /// The actual rate the UART will run at
        /// </summary>
        public double ActualBaud { get; private set; }

        /// <summary>
        /// The setting to use. For most PICs, must be in 0-65535 to be valid
        /// </summary>
        public string Setting { get; private set; }

        /// <summary>
        /// Percent error from target to actual
        /// </summary>
        public double PercentError { get; private set; }

        static double ComputePercentError(double desiredRate, double actualRate)
        {
            return 100.0 * (desiredRate - actualRate) / desiredRate;
        }

    }
}