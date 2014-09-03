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
using System.ComponentModel;
using System.Windows;
using Hypnocube.Demo.ViewModel;
using Hypnocube.HypnoController.ViewModel;
using Hypnocube.SerialTester.ViewModel;

namespace Hypnocube.HypnoController.View
{
    /// <summary>
    ///     Interaction logic for HypnoControllerWindow.xaml
    /// </summary>
    public partial class HypnoControllerWindow : Window
    {
        public HypnoControllerWindow()
        {
            InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // attach all wiring - todo - make cleaner, use some DI or service locator?
            var vm = DataContext as HypnoControllerViewModel;
            if (vm != null)
                vm.CompleteWiring(
                    ConnectionControl.DataContext as ConnectionControlViewManager,
                    SerialTesterControl.DataContext as SerialTesterViewModel,
                    LoggingControl.DataContext as LoggingControlViewModel,
                    DemoControl.DataContext as DemoControlViewModel
                    );
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            var vm = DataContext as HypnoControllerViewModel;
            if (vm != null)
                vm.Closing();
        }
    }
}