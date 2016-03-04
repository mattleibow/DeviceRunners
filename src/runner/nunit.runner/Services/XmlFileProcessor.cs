using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

using NUnit.Framework.Interfaces;

using PCLStorage;

using FileAccess = PCLStorage.FileAccess;

namespace NUnit.Runner.Services
{
    public class XmlFileProcessor : TestResultProcessor
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

        private async Task WriteXmlResultFile(ITestResult testResult)
        {
            const string OutputFolderName = "NUnitTestsOutput";
            const string OutputXmlReportName = "nunit_result.xml";
            var localStorageFolder = FileSystem.Current.LocalStorage;

            var existResult = await localStorageFolder.CheckExistsAsync(OutputFolderName);
            if (existResult == ExistenceCheckResult.FileExists)
            {
                var existingFile = await localStorageFolder.GetFileAsync(OutputFolderName);
                await existingFile.DeleteAsync();
            }

            var outputFolder = await localStorageFolder.CreateFolderAsync(OutputFolderName, CreationCollisionOption.OpenIfExists);
            IFile xmlResultFile = await outputFolder.CreateFileAsync(OutputXmlReportName, CreationCollisionOption.ReplaceExisting);
            using (var resultFileStream = new StreamWriter(await xmlResultFile.OpenAsync(FileAccess.ReadAndWrite)))
            {
                string xmlString = testResult.ToXml(true).OuterXml;
                await resultFileStream.WriteAsync(xmlString);
            }
        }
    }
}