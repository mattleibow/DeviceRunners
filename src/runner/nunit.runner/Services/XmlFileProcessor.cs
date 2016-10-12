// ***********************************************************************
// Copyright (c) 2016 Charlie Poole
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

using NUnit.Framework.Interfaces;

using PCLStorage;

using FileAccess = PCLStorage.FileAccess;

namespace NUnit.Runner.Services
{
    class XmlFileProcessor : TestResultProcessor
    {
        public XmlFileProcessor(TestOptions options)
            : base(options)
        {
        }

        public override async Task Process(ITestResult testResult)
        {
            if (Options.CreateXmlResultFile)
            {
                try
                {
                    await WriteXmlResultFile(testResult).ConfigureAwait(false);
                }
                catch (Exception)
                {
                    Debug.WriteLine("Fatal error while trying to write xml result file!");
                    throw;
                }
            }

            if (Successor != null)
            {
                await Successor.Process(testResult).ConfigureAwait(false);
            }
        }

        async Task WriteXmlResultFile(ITestResult testResult)
        {
            string outputFolderName = Path.GetDirectoryName(Options.ResultFilePath);
            string outputXmlReportName = Path.GetFileName(Options.ResultFilePath);

            await CreateFolderRecursive(outputFolderName);

            IFolder outputFolder = new FileSystemFolder(outputFolderName);
            IFile xmlResultFile = await outputFolder.CreateFileAsync(outputXmlReportName, CreationCollisionOption.ReplaceExisting);
            using (var resultFileStream = new StreamWriter(await xmlResultFile.OpenAsync(FileAccess.ReadAndWrite)))
            {
                string xmlString = testResult.ToXml(true).OuterXml;
                await resultFileStream.WriteAsync(xmlString);
            }
        }

        /// <summary>
        /// Create a folder from the full folder path if it does not exist
        /// Throws exception if acess to the path is denied
        /// </summary>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        private static async Task CreateFolderRecursive(string folderPath)
        {
            string[] segments = new Uri(folderPath).Segments;

            string path = segments[0];

            for (int i = 0; i < segments.Length - 1; i++)
            {
                IFolder folder = new FileSystemFolder(path);

                var res = await folder.CheckExistsAsync(segments[i + 1]);
                if (res != ExistenceCheckResult.FolderExists)
                {
                    await folder.CreateFolderAsync(segments[i + 1], CreationCollisionOption.OpenIfExists);
                }
                path = Path.Combine(path, segments[i + 1]);
            }
        }
    }
}