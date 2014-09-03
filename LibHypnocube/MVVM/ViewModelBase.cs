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
using System.Threading.Tasks;

namespace Hypnocube.MVVM
{
    /// <summary>
    ///     Create a view model. Assumes this is created on the UI thread
    /// </summary>
    public class ViewModelBase : NotifiableBase
    {
        // a factory that spawns tasks on the UI thread
        private readonly TaskFactory uiFactory;

        public ViewModelBase()
        {
            // Construct a TaskFactory that uses the UI thread's context
            uiFactory = new TaskFactory(TaskScheduler.FromCurrentSynchronizationContext());
        }

        /// <summary>
        ///     Dispatch a task onto the UI thread. Optionally make it synchronous
        /// </summary>
        /// <param name="action"></param>
        /// <param name="synchronous"></param>
        public void Dispatch(Action action, bool synchronous = false)
        {
            if (synchronous)
                uiFactory.StartNew(action).Wait();
            else
                uiFactory.StartNew(action);
        }
    }
}