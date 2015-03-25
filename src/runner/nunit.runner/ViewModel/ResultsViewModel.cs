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

using System.Collections.ObjectModel;
using NUnit.Framework.Interfaces;
using NUnit.Runner.Extensions;
using Xamarin.Forms;

namespace NUnit.Runner.ViewModel
{
    public class ResultViewModel
    {
        public ResultViewModel(ITestResult result)
        {
            TestResult = result;
            Name = result.FullName;
            Message = result.Message;
        }

        public ITestResult TestResult { get; private set; }
        public string Name { get; private set; }
        public string Message { get; private set; }

        /// <summary>
        /// Gets the color for this result.
        /// </summary>
        public Color Color
        {
            get { return TestResult.Color(); }
        }
    }

    public class ResultsViewModel : BaseViewModel
    {
        public ResultsViewModel(ITestResult testResult)
        {
            Results = new ObservableCollection<ResultViewModel>();
            AddTestResults(testResult);
        }

        /// <summary>
        /// A list of tests that did not pass
        /// </summary>
        public ObservableCollection<ResultViewModel> Results { get; private set; }

        /// <summary>
        /// Add all tests that did not pass to the Results collection
        /// </summary>
        /// <param name="result"></param>
        private void AddTestResults(ITestResult result)
        {
            if (result.Test.IsSuite)
            {
                foreach (var childResult in result.Children) 
                    AddTestResults(childResult);
            }
            else if (result.ResultState.Status != TestStatus.Passed)
            {
                Results.Add(new ResultViewModel(result));
            }
        }
    }
}
