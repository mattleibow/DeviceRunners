// ***********************************************************************
// Copyright (c) 2016 NUnit Project
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
using System.Threading.Tasks;
using NUnit.Runner.Helpers;

namespace NUnit.Runner.Services
{
    class XmlFileProcessor : TestResultProcessor
    {
        public XmlFileProcessor(TestOptions options)
            : base(options) { }

        public override async Task Process(ResultSummary result)
        {
            if (Options.CreateXmlResultFile == false)
                return;

            try
            {
                await WriteXmlResultFile(result).ConfigureAwait(false);
            }
            catch (Exception)
            {
                Debug.WriteLine("Fatal error while trying to write xml result file!");
                throw;
            }

            if (Successor != null)
            {
                await Successor.Process(result).ConfigureAwait(false);
            }
        }

        async Task WriteXmlResultFile(ResultSummary result)
        {
            string outputFolderName = Path.GetDirectoryName(Options.ResultFilePath);

            Directory.CreateDirectory(outputFolderName);

            using (var resultFileStream = new StreamWriter(Options.ResultFilePath, false))
            {
                var xml = result.GetTestXml().ToString();
                await resultFileStream.WriteAsync(xml);
            }
        }
    }
}