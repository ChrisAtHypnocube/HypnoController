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
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using Hypnocube.Demo.ViewModel;
using Hypnocube.Device;
using Hypnocube.MVVM;
using Hypnocube.SerialTester.ViewModel;

namespace Hypnocube.HypnoController.ViewModel
{
    public class HypnoControllerViewModel : ViewModelBase
    {
        public HypnoControllerViewModel()
        {
            ExecuteHyperlinkCommand = new RelayCommand(o => ExecuteHyperlink("http://www.hypnocube.com"));
        }

        #region Status Property

        private string status = "Status?";

        /// <summary>
        ///     Gets or sets status text.
        /// </summary>
        public string Status
        {
            get { return status; }
            set
            {
                // return true on change
                SetField(ref status, value);
            }
        }

        #endregion

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

        public ICommand ExecuteHyperlinkCommand { get; private set; }

        private void ExecuteHyperlink(string hyperlink)
        {
            var uri = new Uri(hyperlink);
            Process.Start(uri.AbsoluteUri);
        }

        /// <summary>
        ///     Attach various viewmodels together
        /// </summary>
        /// <param name="connectionControlViewManager"></param>
        /// <param name="serialTesterViewModel"></param>
        /// <param name="loggingControlViewModel"></param>
        public void CompleteWiring(
            ConnectionControlViewManager connectionControlViewManager,
            SerialTesterViewModel serialTesterViewModel, 
            LoggingControlViewModel loggingControlViewModel,
            DemoControlViewModel demoControlViewModel)
        {
            connectionControlViewManager.Messager = loggingControlViewModel;
            serialTesterViewModel.Messager = loggingControlViewModel;
            demoControlViewModel.Messager = loggingControlViewModel;

            demoControlViewModel.EnableLoggingAction = b => loggingControlViewModel.IsLoggingEnabled = b;


            this.connectionControlViewManager = connectionControlViewManager;
            this.serialTesterViewModel = serialTesterViewModel;
            this.loggingControlViewModel = loggingControlViewModel;
            this.demoControlViewModel = demoControlViewModel;

            connectionControlViewManager.PropertyChanged += ConnectionControlViewManagerOnPropertyChanged;
            SetDevices(connectionControlViewManager);
        }

        // local view managers
        private ConnectionControlViewManager connectionControlViewManager;
        private SerialTesterViewModel serialTesterViewModel;
        private LoggingControlViewModel loggingControlViewModel;
        private DemoControlViewModel demoControlViewModel;


        /// <summary>
        /// Set new devices if main one changed
        /// </summary>
        /// <param name="connectionViewManager"></param>
        void SetDevices(ConnectionControlViewManager connectionViewManager)
        {
            Device = connectionViewManager.Device;
            serialTesterViewModel.Device =  Device as HypnoLsdController;
            demoControlViewModel.Device =  Device as HypnoLsdController;
        }

        private void ConnectionControlViewManagerOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "Device")
                SetDevices(sender as ConnectionControlViewManager);
        }

        internal void Closing()
        {
            demoControlViewModel.SaveSettings();
        }
    }
}