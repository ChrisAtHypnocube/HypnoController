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
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Hypnocube.Device;
using Hypnocube.MVVM;

namespace Hypnocube.HypnoController.ViewModel
{
    /// <summary>
    ///     Handle the view for a device connection.
    ///     A device is an interface to serial gadgets for now
    /// </summary>
    public class ConnectionControlViewManager : NotifiableBase
    {
        private readonly DispatcherTimer portScanTimer = new DispatcherTimer();

        public ConnectionControlViewManager()
        {
            DeviceTypes = new ObservableCollection<Type>();
            DeviceTypes.Add(typeof(HypnoLsdController)); // todo - reflect later
            SelectedDeviceType = DeviceTypes[0];

            PortNames = new ObservableCollection<string>();
            ConnectCommand = new RelayCommand(o => Connect());
            SendTextCommand = new RelayCommand(o => SendText());
            CommandList = new ObservableCollection<string>();

            // ping occasionally to detect serial port changes
            portScanTimer.Interval = TimeSpan.FromMilliseconds(500);
            portScanTimer.Tick += (o, e) => RefreshPortNames();
            portScanTimer.Start();
        }

        /// <summary>
        ///     Legal port names
        /// </summary>
        public ObservableCollection<string> PortNames { get; private set; }

        public ObservableCollection<Type> DeviceTypes { get; private set; }

        #region SelectedDeviceType Property
        private Type selectedDeviceType = null;
        /// <summary>
        /// Gets or sets property description...
        /// </summary>
        public Type SelectedDeviceType
        {
            get { return selectedDeviceType; }
            set
            {
                // return true if there was a change.
                if (SetField(ref selectedDeviceType, value))
                    ChangeDevice(selectedDeviceType);
            }
        }

        #endregion

        /// <summary>
        /// Set the default baud rate from the type
        /// </summary>
        /// <param name="devType"></param>
        private void ChangeDevice(Type devType)
        {
            // close any existing device
            if (Device != null)
                Device.Close();
            Device = null;
            // todo - get from device
            var t = new HypnoLsdController();
            BaudRate = t.DefaultBaudRate;
        }

        #region BaudRate Property
        private int baudRate = 1234;
        /// <summary>
        /// Gets or sets the selected baudrate.
        /// </summary>
        public int BaudRate
        {
            get { return baudRate; }
            set
            {
                // return true if there was a change.
                SetField(ref baudRate, value);
            }
        }
        #endregion

        #region IsConnected Property
        private bool isConnected = false;
        /// <summary>
        /// Gets or sets if the device is connected.
        /// </summary>
        public bool IsConnected
        {
            get { return isConnected; }
            set
            {
                // return true if there was a change.
                SetField(ref isConnected, value);
            }
        }
        #endregion

        public ICommand SendTextCommand { get; private set; }
        public void SendText()
        {
            try
            {
                if (Device.IsConnected)
                {
                    Device.WriteString(TextCommand + "\r\n");
                    if (!CommandList.Contains(TextCommand))
                        CommandList.Add(TextCommand);
                }
            }
            catch (Exception e)
            {
                Messager.AddError(e.ToString());
            }
        }


        #region TextCommand Property

        private string textCommand = "help";

        /// <summary>
        ///     Gets or sets the command to send as text.
        /// </summary>
        public string TextCommand
        {
            get { return textCommand; }
            set
            {
                // return true on change
                SetField(ref textCommand, value);
            }
        }

        #endregion

        public ObservableCollection<string> CommandList { get; private set; }




        /// <summary>
        ///     Connect to the selected port
        /// </summary>
        public ICommand ConnectCommand { get; private set; }

        public IMessage Messager { get; set; }

        private void RefreshPortNames()
        {
            // get two lists, and if not the same, replace one
            var physicalNames = SerialPort.GetPortNames();
            var localNames = PortNames.ToArray(); // local copy to prevent locking/size changing issues

            var matches = physicalNames.Length == localNames.Length;
            if (matches)
            {
                // same length, check contents
                foreach (var name in physicalNames)
                    matches &= localNames.Contains(name);
                foreach (var name in localNames)
                    matches &= physicalNames.Contains(name);
            }
            if (!matches)
            {
                var msg = Messager;
                if (msg != null)
                    msg.AddMessage("COM Ports changed");
                // replace internals
                var curSelected = SelectedPortName;

                // copy in new ones
                PortNames.Clear();
                foreach (var name in physicalNames)
                    PortNames.Add(name);

                var oldNameOk = !String.IsNullOrEmpty(curSelected) && PortNames.Contains(curSelected);

                if (oldNameOk)
                    SelectedPortName = curSelected;
                else if (PortNames.Any())
                    SelectedPortName = PortNames[0];
                else
                    SelectedPortName = "";

                // todo - disconnect when pulled? handle crashes?
            }
        }

        public void Close()
        {
            var d = Device;
            if (d != null)
            {
                if (d.IsConnected)
                    d.Close();
                d.MessageReceived -= MessageReceived;
            }
            IsConnected = false;
            Device = null;
        }

        /// <summary>
        ///     Connect to the selected port
        /// </summary>
        public void Connect()
        {
            var dev = Device;
            if (dev != null && dev.IsConnected)
            {   // disconnect
                Close();
                return;
            }

            var type = SelectedDeviceType;
            if (type == null)
            {
                MessageBox.Show("Select a device type first");
                return;
            }

            // close any previous device
            Close();

            Device = (SerialDeviceBase)Activator.CreateInstance(type, null);

            Device.ConnectionSpeed = BaudRate;
            Device.MessageReceived += MessageReceived; 
            if (Device.Open(SelectedPortName))
            {
                IsConnected = Device.IsConnected;
            }
            else
            {
                MessageBox.Show("Did not connect");
            }
        }
        private void MessageReceived(object sender, SerialDeviceBase.MessageReceivedArgs messageReceivedArgs)
        {
            var m = Messager;
            if (m != null)
            {
                m.AddMessage(messageReceivedArgs.Data, false);
                m.AddMessage(messageReceivedArgs.Data, true);
            }
        }

        #region Device Property

        private SerialDeviceBase device = null;

        /// <summary>
        ///     Gets or sets the current device.
        /// </summary>
        public SerialDeviceBase Device
        {
            get { return device; }
            set
            {
                // return true if there was a change.
                SetField(ref device, value);
            }
        }

        #endregion

        #region SelectedPortName Property

        private string selectedPortName = "";

        /// <summary>
        ///     Gets or sets the selected port name.
        /// </summary>
        public string SelectedPortName
        {
            get { return selectedPortName; }
            set
            {
                // return true on change
                SetField(ref selectedPortName, value);
            }
        }

        #endregion
    }
}