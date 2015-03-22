// ***********************************************************************
// Copyright (c) 2014 Charlie Poole
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

using System.IO;
using System.Globalization;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace NUnit.Runner
{
    internal static class TextWriterExtensions
    {
        public static void WriteLabel(this TextWriter writer, string label, object value)
        {
            writer.Write(label);
            writer.Write(value.ToString());
        }
    
        public static void WriteLabelLine(this TextWriter writer, string label, object value)
        {
            writer.WriteLabel(label, value);
            writer.WriteLine();
        }
    }

    /// <summary>
    /// ResultReporter writes the test results to a TextWriter.
    /// </summary>
    public class ResultReporter
    {
        private readonly TextWriter _writer;
        private readonly ITestResult _result;
        private readonly string _overallResult;

        private int _reportIndex;

        /// <summary>
        /// Constructs an instance of ResultReporter
        /// </summary>
        /// <param name="result">The top-level result being reported</param>
        /// <param name="writer">A TextWriter to which the report is written</param>
        public ResultReporter(ITestResult result, TextWriter writer)
        {
            _result = result;
            _writer = writer;

            _overallResult = result.ResultState.Status.ToString();
            if (_overallResult == "Skipped")
                _overallResult = "Warning";

            Summary = new ResultSummary(_result);
        }

        /// <summary>
        /// Gets the ResultSummary created by the ResultReporter
        /// </summary>
        private ResultSummary Summary { get; set; }

        /// <summary>
        /// Produces the standard output reports.
        /// </summary>
        public void ReportResults()
        {
            if (Summary.TestCount == 0)
                _writer.WriteLine("Warning: No tests found");

            _writer.WriteLine();

            WriteSummaryReport();

            if (_result.ResultState.Status == TestStatus.Failed)
                WriteErrorsAndFailuresReport();

            if (Summary.SkipCount + Summary.IgnoreCount > 0)
                WriteNotRunReport();
        }

        #region Summmary Report

        /// <summary>
        /// Prints the Summary Report
        /// </summary>
        private void WriteSummaryReport()
        {
            _writer.WriteLine("Test Run Summary");
            _writer.WriteLabelLine("   Overall result: ", _overallResult);

            _writer.WriteLabel("   Tests run: ", Summary.RunCount.ToString(CultureInfo.CurrentUICulture));
            _writer.WriteLabel(", Passed: ", Summary.PassCount.ToString(CultureInfo.CurrentUICulture));
            _writer.WriteLabel(", Errors: ", Summary.ErrorCount.ToString(CultureInfo.CurrentUICulture));
            _writer.WriteLabel(", Failures: ", Summary.FailureCount.ToString(CultureInfo.CurrentUICulture));
            _writer.WriteLabelLine(", Inconclusive: ", Summary.InconclusiveCount.ToString(CultureInfo.CurrentUICulture));

            var notRunTotal = Summary.SkipCount + Summary.IgnoreCount + Summary.InvalidCount;
            _writer.WriteLabel("     Not run: ", notRunTotal.ToString(CultureInfo.CurrentUICulture));
            _writer.WriteLabel(", Invalid: ",Summary.InvalidCount.ToString(CultureInfo.CurrentUICulture));
            _writer.WriteLabel(", Ignored: ", Summary.IgnoreCount.ToString(CultureInfo.CurrentUICulture));
            _writer.WriteLabelLine(", Skipped: ", Summary.SkipCount.ToString(CultureInfo.CurrentUICulture));

            _writer.WriteLabelLine("  Start time: ", _result.StartTime.ToString("u"));
            _writer.WriteLabelLine("    End time: ", _result.EndTime.ToString("u"));
            _writer.WriteLabelLine("    Duration: ", _result.Duration.TotalSeconds.ToString("0.000") + " seconds");
            _writer.WriteLine();
        }

        #endregion

        #region Errors and Failures Report

        private void WriteErrorsAndFailuresReport()
        {
            _reportIndex = 0;
            _writer.WriteLine("Errors and Failures");
            WriteErrorsAndFailures(_result);
            _writer.WriteLine();
        }

        private void WriteErrorsAndFailures(ITestResult result)
        {
            if (result.Test.IsSuite)
            {
                if (result.ResultState.Status == TestStatus.Failed)
                {
                    var suite = result.Test as TestSuite;
                    var site = result.ResultState.Site;
                    if (suite != null && (suite.TestType == "Theory" || site == FailureSite.SetUp || site == FailureSite.TearDown))
                            WriteSingleResult(result);
                    if (site == FailureSite.SetUp) return;
                }

                foreach (ITestResult childResult in result.Children)
                    WriteErrorsAndFailures(childResult);
            }
            else if (result.ResultState.Status == TestStatus.Failed)
                    WriteSingleResult(result);
        }

        #endregion

        #region Not Run Report

        /// <summary>
        /// Prints the Not Run Report
        /// </summary>
        private void WriteNotRunReport()
        {
            _reportIndex = 0;
            _writer.WriteLine("Tests Not Run");
            WriteNotRunResults(_result);
            _writer.WriteLine();
        }

        private void WriteNotRunResults(ITestResult result)
        {
            if (result.HasChildren)
                foreach (ITestResult childResult in result.Children)
                    WriteNotRunResults(childResult);
            else if (result.ResultState.Status == TestStatus.Skipped)
            {
                    WriteSingleResult(result);
            }
        }

        #endregion

        #region Helper Methods

        private static readonly char[] EOL_CHARS = { '\r', '\n' };

        private void WriteSingleResult(ITestResult result)
        {
            string status = result.ResultState.Label;
            if (string.IsNullOrEmpty(status))
                status = result.ResultState.Status.ToString();

            if (status == "Failed" || status == "Error")
            {
                var site = result.ResultState.Site.ToString();
                if (site == "SetUp" || site == "TearDown")
                    status = site + " " + status;
            }

            _writer.WriteLine();
            _writer.WriteLine("{0}) {1} : {2}", ++_reportIndex, status, result.FullName);

            if (!string.IsNullOrEmpty(result.Message))
                _writer.WriteLine(result.Message.TrimEnd(EOL_CHARS));

            if (!string.IsNullOrEmpty(result.StackTrace))
                _writer.WriteLine(result.StackTrace.TrimEnd(EOL_CHARS));
        }

        #endregion
    }
}
