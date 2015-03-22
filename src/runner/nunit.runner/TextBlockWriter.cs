// ***********************************************************************
// Copyright (c) 2015 Charlie Poole
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

using System;
using System.Diagnostics;
using System.IO;
using NUnit.Framework.Internal;
using NUnit.Runner.ViewModel;
using Xamarin.Forms;

namespace NUnit.Runner
{
    /// <summary>
    /// TextBlockWriter is a TextWriter that sends it's 
    /// output to a Silverlight TextBlock.
    /// </summary>
    public class TextBlockWriter : TextWriter
    {
        private readonly TestViewModel _model;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextBlockWriter"/> class.
        /// </summary>
        /// <param name="model">The test view model.</param>
        public TextBlockWriter(TestViewModel model)
        {
            _model = model;
        }

        /// <summary>
        /// Writes a character to the text stream.
        /// </summary>
        /// <param name="value">The character to write to the text stream.</param>
        /// <exception cref="T:System.ObjectDisposedException">
        /// The <see cref="T:System.IO.TextWriter"/> is closed.
        /// </exception>
        /// <exception cref="T:System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        public override void Write(char value)
        {
            _model.Results += value.ToString();
        }

        /// <summary>
        /// Writes a string to the text stream.
        /// </summary>
        /// <param name="value">The string to write.</param>
        /// <exception cref="T:System.ObjectDisposedException">
        /// The <see cref="T:System.IO.TextWriter"/> is closed.
        /// </exception>
        /// <exception cref="T:System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        public override void Write(string value)
        {
            _model.Results += value;
        }

        /// <summary>
        /// Writes a string followed by a line terminator to the text stream.
        /// </summary>
        /// <param name="value">The string to write. If <paramref name="value"/> is null, only the line termination characters are written.</param>
        /// <exception cref="T:System.ObjectDisposedException">
        /// The <see cref="T:System.IO.TextWriter"/> is closed.
        /// </exception>
        /// <exception cref="T:System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        public override void WriteLine(string value)
        {
            Write(value);
            Write(Environment.NewLine);
            Debug.WriteLine(value);
        }

        /// <summary>
        /// When overridden in a derived class, returns the <see cref="T:System.Text.Encoding"/> in which the output is written.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// The Encoding in which the output is written.
        /// </returns>
        public override System.Text.Encoding Encoding
        {
            get { return System.Text.Encoding.UTF8; }
        }
    }
}
