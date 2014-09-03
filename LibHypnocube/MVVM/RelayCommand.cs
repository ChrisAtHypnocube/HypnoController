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
using System.Windows.Input;

// from article http://msdn.microsoft.com/en-us/magazine/dd419663.aspx#id0090030
// see also http://blogs.msdn.com/b/mikehillberg/archive/2009/03/20/icommand-is-like-a-chocolate-cake.aspx

namespace Hypnocube.MVVM
{
    /// <summary>
    ///     Represent a simple to use ICommand implementation.
    ///     Good for representing commands applicable to View Models in the Model-View-ViewModel (MVVM) pattern
    /// </summary>
    /// <example>
    ///     use in View model like
    ///     public ICommand CloseCommand {get; private set; } ....
    ///     CloseCommand = new RelayCommand(param=>Close(), param=>CanClose);
    ///     use in XAML like [Button Command="{Binding Path=CloseCommand}"]
    /// </example>
    public class RelayCommand : ICommand
    {
        #region Fields

        /// <summary>
        ///     Is execution allowed
        /// </summary>
        private readonly Predicate<object> canExecute;

        /// <summary>
        ///     What to execute
        /// </summary>
        private readonly Action<object> execute;

        #endregion // Fields

        #region Constructors

        /// <summary>
        ///     Create a new command that executes the given delegate when fired.
        /// </summary>
        /// <param name="execute">The action to execute</param>
        public RelayCommand(Action<object> execute)
            : this(execute, null)
        {
        }

        /// <summary>
        ///     Create a new command that executes the given delegate when fired.
        /// </summary>
        /// <param name="execute">The action to execute.</param>
        /// <param name="canExecute">The predicate to determine if action is allowable.</param>
        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            this.execute = execute;
            this.canExecute = canExecute;
        }

        #endregion // Constructors

        #region ICommand Members

        /// <summary>
        ///     Return true if this command can execute.
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return canExecute == null || canExecute(parameter);
        }

        /// <summary>
        ///     Event to trigger execution state changes.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        ///     Execute the command action on the given parameter.
        /// </summary>
        /// <param name="parameter">The parameter to execute</param>
        public void Execute(object parameter)
        {
            execute(parameter);
        }

        #endregion
    }
}