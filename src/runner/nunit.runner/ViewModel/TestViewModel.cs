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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Windows.Input;
using NUnit.Framework.Api;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using Xamarin.Forms;

namespace NUnit.Runner.ViewModel
{
    public class TestViewModel : BaseViewModel, ITestListener
    {
        private readonly ITestAssemblyRunner _runner;
        private bool _running;

        public TestViewModel()
        {
            Results = new ObservableCollection<ITestResult>();
            //_writer = new TextBlockWriter(this);
            _runner = new NUnitTestAssemblyRunner(new DefaultTestAssemblyBuilder());
            RunTestsCommand = new Command(o => ExecuteTests(), o => !_running);
        }

        public ObservableCollection<ITestResult> Results { get; set; }

        public ICommand RunTestsCommand { set; get; }

        /// <summary>
        /// Adds an assembly to be tested.
        /// </summary>
        /// <param name="testAssembly">The test assembly.</param>
        /// <returns></returns>
        internal bool AddTest(Assembly testAssembly)
        {
            return _runner.Load(testAssembly, new Dictionary<string, string>()) != null;
        }

        private void ExecuteTests()
        {
            _running = true;
            Results.Clear();
            //WriteHeader(_writer);
            //WriteRuntimeEnvironment(_writer);

            ITestResult result = _runner.Run(this, TestFilter.Empty);

            //var reporter = new ResultReporter(result, _writer);

            //reporter.ReportResults();

            //ResultSummary summary = reporter.Summary;

            //this.Total.Text = summary.TestCount.ToString();
            //this.Failures.Text = summary.FailureCount.ToString();
            //this.Errors.Text = summary.ErrorCount.ToString();
            //var notRunTotal = summary.SkipCount + summary.InvalidCount + summary.IgnoreCount;
            //this.NotRun.Text = notRunTotal.ToString();
            //this.Passed.Text = summary.PassCount.ToString();
            //this.Inconclusive.Text = summary.InconclusiveCount.ToString();

            //this.Notice.Visibility = Visibility.Collapsed;
            _running = false;
        }

        /// <summary>
        /// Writes the header.
        /// </summary>
        /// <param name="writer">The writer.</param>
        //private static void WriteHeader(TextWriter writer)
        //{
        //    Assembly executingAssembly = Assembly.GetExecutingAssembly();
        //    AssemblyName assemblyName = AssemblyHelper.GetAssemblyName(executingAssembly);
        //    Version version = assemblyName.Version;
        //    string copyright = "Copyright (C) 2015, Charlie Poole";
        //    string build = "";

        //    object[] attrs = executingAssembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
        //    if (attrs.Length > 0)
        //    {
        //        var copyrightAttr = (AssemblyCopyrightAttribute)attrs[0];
        //        copyright = copyrightAttr.Copyright;
        //    }

        //    attrs = executingAssembly.GetCustomAttributes(typeof(AssemblyConfigurationAttribute), false);
        //    if (attrs.Length > 0)
        //    {
        //        var configAttr = (AssemblyConfigurationAttribute)attrs[0];
        //        build = string.Format("({0})", configAttr.Configuration);
        //    }

        //    writer.WriteLine("NUnit {0} {1}", version.ToString(3), build);
        //    writer.WriteLine(copyright);
        //    writer.WriteLine();
        //}

        /// <summary>
        /// Writes the runtime environment.
        /// </summary>
        /// <param name="writer">The writer.</param>
        //private static void WriteRuntimeEnvironment(TextWriter writer)
        //{
        //    writer.WriteLine("Runtime Environment -");
        //    writer.WriteLabelLine("   OS Version: ", Environment.OSVersion);
        //    writer.WriteLabelLine("  CLR Version: ", Environment.Version);
        //    writer.WriteLine();
        //}

        /// <summary>
        /// Called when a test has just started
        /// </summary>
        /// <param name="test">The test that is starting</param>
        public void TestStarted(ITest test)
        {
        }

        /// <summary>
        /// Called when a test has finished
        /// </summary>
        /// <param name="result">The result of the test</param>
        public void TestFinished(ITestResult result)
        {
            if (result.ResultState != ResultState.Success)
            {
                Results.Add(result);
            }
        }
    }
}
