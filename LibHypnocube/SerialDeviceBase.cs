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
using System.Diagnostics;
using System.IO.Ports;
using System.Text;
using Hypnocube.Communications;
using Hypnocube.MVVM;

namespace Hypnocube
{
    /// <summary>
    ///     Items common to all Hypnocube Serial Devices
    /// </summary>
    public abstract class SerialDeviceBase : NotifiableBase, IDisposable
    {
        /// <summary>
        ///     Listen to this to get data messages from the device
        /// </summary>
        public EventHandler<MessageReceivedArgs> MessageReceived;

        protected SerialDeviceBase()
        {
            GadgetConnectionLocation = "";
        }

        #region IsConnected Property

        /// <summary>
        ///     Gets the IsConnected property. This observable property
        ///     indicates if the device is connected.
        ///     Return true if the gadget is connected,
        ///     else return false.
        /// </summary>
        private bool isConnected = false;

        /// <summary>
        ///     Gets or sets if the device is connected.
        /// </summary>
        public bool IsConnected
        {
            get { return isConnected; }
            private set
            {
                // return true on change
                SetField(ref isConnected, value);
            }
        }

        #endregion

        /// <summary>
        ///     The clock speed of the PIC in hz
        /// </summary>
        public int PicClockSpeed { get; protected set; }

        /// <summary>
        ///     The size of the image ram on the device
        /// </summary>
        public int RamSize { get; protected set; }

        /// <summary>
        ///     the default baud rate
        /// </summary>
        public int DefaultBaudRate { get; protected set; }

        #region FrameErrors Property

        private int frameErrors = 0;

        /// <summary>
        ///     Get the number of framing errors received.
        /// </summary>
        public int FrameErrors
        {
            get { return frameErrors; }
            private set
            {
                // return true on change
                SetField(ref frameErrors, value);
            }
        }

        #endregion

        #region ConnectionSpeed Property

        private int connectionSpeed = 9600;

        /// <summary>
        ///     Gets or sets the baud rate. Ignored if no active connection.
        ///     Note that only certain rates allowed by the dividers will work as desired.
        /// </summary>
        public int ConnectionSpeed
        {
            get { return connectionSpeed; }
            set
            {
                if (connectionSpeed == value) return; // avoid infinite loops

                if (!IsConnected) return;

                var br = BaudRateCalculator.Compute(SerialDeviceType.PIC32, value, PicClockSpeed);

                int divider;
                if (!Int32.TryParse(br.Setting, out divider) || (divider < 0) || (65535 < divider))
                {
                    throw new Exception("Invalid divider " + value + " not in range 0 to 65535");
                }
                connectionSpeed = (int) br.ActualBaud;
                SetDivider((ushort) divider);
                OpenPort(port.PortName);
                NotifyPropertyChanged(() => ConnectionSpeed);
            }
        }

        #endregion

        #region OverrunErrors Property

        private int overrunErrors = 0;

        /// <summary>
        ///     Gets the number of overruns seen.
        /// </summary>
        public int OverrunErrors
        {
            get { return overrunErrors; }
            private set
            {
                // return true on change
                SetField(ref overrunErrors, value);
            }
        }

        #endregion

        #region RXOverflowErrors Property

        private int rxOverflowErrors = 0;

        /// <summary>
        ///     Gets the number of receive overflows.
        /// </summary>
        public int RXOverflowErrors
        {
            get { return rxOverflowErrors; }
            private set
            {
                // return true on change
                SetField(ref rxOverflowErrors, value);
            }
        }

        #endregion



        /// <summary>
        ///     Set the divider :TODO - move to lower classes?
        /// </summary>
        /// <param name="divider"></param>
        public abstract void SetDivider(ushort divider);

        /// <summary>
        ///     Open the device, return true on success.
        /// </summary>
        /// <param name="portName">serial port name. </param>
        /// <returns> true on success, else false. </returns>
        public bool Open(string portName)
        {
            try
            {
                if (IsConnected)
                    Close();
                OpenPort(portName);
                return true;
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
                return false;
            }
        }

        #region RXParityErrors Property

        private int rxParityErrors = 0;

        /// <summary>
        ///     Gets the number of receive parity errors seen.
        /// </summary>
        public int RXParityErrors
        {
            get { return rxParityErrors; }
            private set
            {
                // return true on change
                SetField(ref rxParityErrors, value);
            }
        }

        #endregion

        #region TXFullErrors Property

        private int txFullErrors = 0;

        /// <summary>
        ///     Gets the number of transmit full errors seen.
        /// </summary>
        public int TXFullErrors
        {
            get { return txFullErrors; }
            private set
            {
                // return true on change
                SetField(ref txFullErrors, value);
            }
        }

        #endregion

        #region SerialErrorsReceived Property

        private int serialErrorsReceived = 0;

        /// <summary>
        ///     Gets the number of serial errors received.
        /// </summary>
        public int SerialErrorsReceived
        {
            get { return serialErrorsReceived; }
            private set
            {
                // return true on change
                SetField(ref serialErrorsReceived, value);
            }
        }


        /// <summary>
        ///     Write a string as ASCII to the device. Allows formatting like
        ///     String.Format
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void WriteString(string format, params object[] args)
        {
            var bytes = Encoding.ASCII.GetBytes(String.Format(format, args));
            WriteBytes(bytes);
        }


        /// <summary>
        ///     Write the bytes to the device if it is connected.
        /// </summary>
        public void WriteBytes(byte[] data)
        {
            if (IsConnected)
                port.Write(data, 0, data.Length);
        }

        /// <summary>
        ///     Write the byte to the device
        /// </summary>
        /// <param name="data"></param>
        public void WriteByte(byte data)
        {
            if (IsConnected)
                port.Write(new[] {data}, 0, 1);
        }

        /// <summary>
        ///     Closes the device and releases all resources.
        ///     Cannot reopen the device without creating a new one.
        ///     Used for IDisposable interface.
        /// </summary>
        public void Close()
        {
            if (IsConnected)
                Trace.TraceInformation("Disconnecting from " + GadgetConnectionLocation);
            ClosePort();
            Dispose(false);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Implementation

        /// <summary>
        ///     The serial port connected to
        /// </summary>
        private SerialPort port;

        #region IDisposable interface

        // Implement the standard IDisposable interface. For example, see
        // http://www.codeproject.com/KB/cs/idisposable.aspx


        /// <summary>
        ///     This variable detects redundant Dispose calls.
        /// </summary>
        private bool disposed;


        /// <summary>
        ///     Releases all resources used by the PIC32 gadget interface.
        /// </summary>
        public void Dispose()
        {
            Close();
        }

        /// <summary>
        ///     Dispose resources.
        /// </summary>
        /// <param name="disposing"> </param>
        /* protected virtual*/
        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // dispose managed resources
                    Close();
                }
                // dispose unmanaged resources
            }
            disposed = true;
            // call base.Disposed(disposing) here if there is one 
        }

        /// <summary>
        ///     This finalizer cleans up any local resources
        /// </summary>
        ~SerialDeviceBase()
        {
            Dispose(false);
        }

        #endregion

        /// <summary>
        ///     Call this on serial error
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ErrorReceived(object sender, SerialErrorReceivedEventArgs args)
        {
            switch (args.EventType)
            {
                case SerialError.Frame:
                    FrameErrors++;
                    break;
                case SerialError.Overrun:
                    OverrunErrors++;
                    break;
                case SerialError.RXOver:
                    RXOverflowErrors++;
                    break;
                case SerialError.RXParity:
                    RXParityErrors++;
                    break;
                case SerialError.TXFull:
                    TXFullErrors++;
                    break;
                default:
                    throw new Exception("Unknown error type in serial error: " + args.EventType);
            }
            SerialErrorsReceived++;
        }

        /// <summary>
        ///     Close the port.
        /// </summary>
        private void ClosePort()
        {
            if ((port != null) && (port.IsOpen))
            {
                port.DataReceived -= SerialDataReceived;
                port.ErrorReceived -= ErrorReceived;
                port.Close();
            }
            port = null;
            IsConnected = false;
        }

        /// <summary>
        ///     Try to open the given port. May throw exception if not possible
        /// </summary>
        private void OpenPort(string portName)
        {
            if (port != null)
                ClosePort();
            var rate = ConnectionSpeed;
            port = new SerialPort
            {
                BaudRate = rate,
                DataBits = 8,
                Parity = Parity.None,
                StopBits = StopBits.One,
                Handshake = Handshake.None,
                PortName = portName,
                Encoding = Encoding.GetEncoding("Windows-1252") // binary mode
            };
            PortName = portName;
            port.DataReceived += SerialDataReceived;
            port.ErrorReceived += ErrorReceived;

            SerialErrorsReceived = 0;
            RXOverflowErrors = 0;
            RXParityErrors = 0;
            FrameErrors = 0;
            OverrunErrors = 0;
            TXFullErrors = 0;

            try
            {
                port.Open();
                IsConnected = port != null && port.IsOpen;
            }

            catch (Exception exception)
            {
                Trace.TraceError("OpenPort Exception: {0}", exception.ToString());
                throw;
            }
        }

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

        #region PortName Property

        private string portName = "";

        /// <summary>
        ///     Gets or sets the name of the last openend port.
        /// </summary>
        public string PortName
        {
            get { return portName; }
            set
            {
                // return true if there was a change.
                SetField(ref portName, value);
            }
        }

        #endregion

        #endregion

        #region Nested type: MessageReceivedArgs

        /// <summary>
        ///     Class to hold data received
        /// </summary>
        public class MessageReceivedArgs : EventArgs
        {
            public byte[] Data { get; set; }
        }

        #endregion

        #region GadgetConnectionLocation Property

        private string gadgetConnectionLocation;

        /// <summary>
        ///     Gets or sets the GadgetConnectionLocation property. This observable property
        ///     indicates the where the gadget is connected to.
        /// </summary>
        public string GadgetConnectionLocation
        {
            get { return gadgetConnectionLocation; }
            private set
            {
                // return true on change
                SetField(ref gadgetConnectionLocation, value);
            }
        }

        #endregion
    }
}