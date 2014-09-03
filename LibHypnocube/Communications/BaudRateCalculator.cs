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
using System.Security.Cryptography;
using System.Windows.Input;
using Hypnocube.MVVM;

namespace Hypnocube.Communications
{
    public enum SerialDeviceType
    {
        PIC32,
        FtdiLow,
        FtdiHigh
    }


    /// <summary>
    /// Class to provide some baud rate generation and error metrics
    /// TODO - rewrite using the baud calculator in the HypnoLsdController code
    /// </summary>
    public static class BaudRateCalculator
    {

        /// <summary>
        /// Compute some parameters for baud generators.
        /// Some devices, such as PIC32, require a clock rate.
        /// Others, such as FTDI chips, have pre-specified clock rates
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="desiredBaud"></param>
        /// <param name="clockRate"></param>
        /// <returns></returns>
        public static BaudRateSettings Compute(SerialDeviceType deviceType, int desiredBaud,int clockRate = 0)
        {
            switch (deviceType)
            {
                case SerialDeviceType.PIC32:
                    return ComputePic32Parameters(desiredBaud, clockRate);
                    case SerialDeviceType.FtdiLow:
                    return ComputeFtdiLowParameters(desiredBaud);
                    case SerialDeviceType.FtdiHigh:
                    return ComputeFtdiHighParameters(desiredBaud);
                    default:
                    throw new NotImplementedException("Unknown serial device type " + deviceType);
            }
            return null;
        }

        static BaudRateSettings ComputePic32Parameters(int desiredBaud, int clockRate)
        {
            // find best for PIC
            // rate = clk/(4*(d+1)), d=clk/(4*rate) - 1
            var picClk = clockRate;
            var picDiv = picClk/(4*desiredBaud) - 1;
            var rate1 = picClk/(4.0*(picDiv + 1));
            var rate2 = picClk/(4.0*(picDiv + 2));
            var rate = 0.0;
            if (Math.Abs(PercentError(desiredBaud, rate1)) < Math.Abs(PercentError(desiredBaud, rate2)))
                rate = rate1;
            else
            {
                rate = rate2;
                picDiv++; // this one better
            }


            if (picDiv < 0 || 65535 < picDiv)
            {
                return new BaudRateSettings(
                    SerialDeviceType.PIC32,
                    desiredBaud,
                    Double.NaN,
                    picDiv.ToString() + " out of range 0-65535"
                    );
            }

            return new BaudRateSettings(SerialDeviceType.PIC32,
                desiredBaud,
                rate,
                picDiv.ToString()
                );
        }

        static BaudRateSettings ComputeFtdiLowParameters(int desiredBaud)
        {

            //FTDI low: 
            // 3M internal clock, subdivided by n+d
            // n is 2-16384 (inclusive)
            // d is 0,1/8,2/8,3/8,4/8,5/8,6/8,7/8. Ft8U232AM only allowed 1/8,2/8,4/8.
            // Special cases: n=0 gives 3M, n=1 gives 2M. No sub integers allowed between 0 and 2.
            // If rate within +-3%, should be ok.
            var ftdiLowDiv = (8*3000000/(desiredBaud));
            var rate1 = (double) (8*3000000)/ftdiLowDiv;
            var rate2 = (double) (8*3000000)/(ftdiLowDiv + 1);
            ftdiLowDiv = Math.Abs(PercentError(desiredBaud, rate1)) < Math.Abs(PercentError(desiredBaud, rate2))
                ? ftdiLowDiv
                : ftdiLowDiv + 1;
            var rate = (double) (8*3000000)/ftdiLowDiv;
            // special cases:
            if (desiredBaud == 3000000 || desiredBaud == 2000000)
            {
                ftdiLowDiv = 0;
                rate = desiredBaud;
                if (desiredBaud == 2000000)
                    ftdiLowDiv = 8;
            }

            var n = ftdiLowDiv/8;
            var d = ftdiLowDiv & 7;

            if (n < 2 || 16383 < n || d < 0 || 7 < d)
            {
                return new BaudRateSettings(
                    SerialDeviceType.FtdiLow,
                    desiredBaud,
                    Double.NaN, 
                    String.Format("(n,d)=({0},{1}) out of range", n, d)
                    );
            }

            return new BaudRateSettings(
                SerialDeviceType.FtdiLow,
                desiredBaud,
                rate,
                String.Format("(n,d)=({0},{1})", n, d)
                );
        }
        static BaudRateSettings ComputeFtdiHighParameters(int desiredBaud)
        {

            //FTDI high: 
            // can use the same as FTDI Low, and also this:
            // 12M internal clock, subdivided by n+d
            // n is 1-16384 (inclusive)
            // d is 0,1/8,2/8,3/8,4/8,5/8,6/8,7/8.
            // If rate within +-3%, should be ok.
            var ftdiHighDiv = (8*12000000/(desiredBaud));
            var rate1 = (double) (8*12000000)/ftdiHighDiv;
            var rate2 = (double) (8*12000000)/(ftdiHighDiv + 1);
            ftdiHighDiv = Math.Abs(PercentError(desiredBaud, rate1)) < Math.Abs(PercentError(desiredBaud, rate2))
                ? ftdiHighDiv
                : ftdiHighDiv + 1;
            var rate = (double) (8*12000000)/ftdiHighDiv;
            // special cases:
            // todo - remove 7,9,10,11M rates?

            var n = ftdiHighDiv/8;
            var d = ftdiHighDiv & 7;



            if (n < 1 || 16383 < n || d < 0 || 7 < d)
            {
                return new BaudRateSettings(
                    SerialDeviceType.FtdiLow,
                    desiredBaud,
                    Double.NaN,
                    String.Format("(n,d)=({0},{1}) out of range", n, d)
                    );
            }

            return new BaudRateSettings(
                SerialDeviceType.FtdiLow,
                desiredBaud,
                rate,
                String.Format("(n,d)=({0},{1})", n, d)
                );
        }

        static double PercentError(double desiredRate, double actualRate)
        {
            return 100.0*(desiredRate - actualRate)/desiredRate;
        }

        /// <summary>
        ///     Given the clock divider for the PIC, compute the resulting baud rate.
        ///     The clock divider should be in 0-65535
        /// </summary>
        /// <param name="clockDivider">The clock divider in 0 to 65535</param>
        /// <returns>The baud rate resulting from the given clock divider</returns>
        public static int ComputePic32BaudRateFromClockDivider(int picClockSpeed, int clockDivider)
        {
            return picClockSpeed / (4 * (clockDivider + 1));
        }


    }
}