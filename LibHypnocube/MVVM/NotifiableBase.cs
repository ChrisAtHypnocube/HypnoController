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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Hypnocube.MVVM
{
    public class NotifiableBase : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Members

        /// <summary>
        ///     Occurs when a property value changes.
        /// </summary>
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        /// <summary>
        ///     Safely raises the property changed event.
        /// </summary>
        /// <param name="propertyName">The name of the property to raise.</param>
        protected virtual void NotifyPropertyChanged(string propertyName)
        {
            VerifyPropertyName(propertyName); // this is only called in Debug
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        ///     Safely raises the property changed event.
        /// </summary>
        /// <param name="selectorExpression">An expression like ()=>PropName giving the name of the property to raise.</param>
        protected virtual void NotifyPropertyChanged<T>(Expression<Func<T>> selectorExpression)
        {
            if (selectorExpression == null)
                throw new ArgumentNullException("selectorExpression");
            var body = selectorExpression.Body as MemberExpression;
            if (body == null)
                throw new ArgumentException("The body must be a member expression");
            NotifyPropertyChanged(body.Member.Name);
        }


        /// <summary>
        ///     While in debug, check a string to make sure it is a valid property name.
        /// </summary>
        /// <param name="propertyName">The name of the property to check.</param>
        [Conditional("DEBUG")]
        private void VerifyPropertyName(string propertyName)
        {
            var type = GetType();
            var propInfo = type.GetProperty(propertyName);
            Debug.Assert(propInfo != null, propertyName + " is not a property of " + type.FullName);
        }

        /// <summary>
        ///     Set a field if it is not already equal. Return true if there was a change.
        /// </summary>
        /// <param name="field">The field backing to update on change</param>
        /// <param name="value">The new value</param>
        /// <param name="propertyName">The member name, filled in automatically in C# 5.0 and higher.</param>
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            // avoid possible infinite loops
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            NotifyPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        ///     Set a field if it is not already equal. Return true if there was a change.
        /// </summary>
        /// <param name="field">The field backing to update on change</param>
        /// <param name="value">The new value</param>
        /// <param name="selectorExpression">An expression like ()=>PropName giving the name of the property to raise.</param>
        protected bool SetField<T>(ref T field, T value, Expression<Func<T>> selectorExpression)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            NotifyPropertyChanged(selectorExpression);
            return true;
        }
    }
}