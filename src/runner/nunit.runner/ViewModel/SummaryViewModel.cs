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

using System.Collections.Generic;
using System.Reflection;
using System.Windows.Input;
using NUnit.Framework.Api;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Runner.View;
using Nunit.Runner.ViewModel;
using Xamarin.Forms;

namespace NUnit.Runner.ViewModel
{
    public class SummaryViewModel : BaseViewModel
    {
        private readonly ITestAssemblyRunner _runner;
        private ResultSummary _results;
        private bool _running;

        public SummaryViewModel()
        {
            _runner = new NUnitTestAssemblyRunner(new DefaultTestAssemblyBuilder());
            RunTestsCommand = new Command(o => ExecuteTests(), o => !Running);
            ViewAllResultsCommand = new Command(
                async o => await Navigation.PushAsync(new ResultsView(new ResultsViewModel(Results.TestResult, true))),
                o => !HasResults);
            ViewFailedResultsCommand = new Command(
                async o => await Navigation.PushAsync(new ResultsView(new ResultsViewModel(Results.TestResult, false))),
                o => !HasResults);
        }

        /// <summary>
        /// The overall test results
        /// </summary>
        public ResultSummary Results
        {
            get { return _results; }
            set
            {
                if (Equals(value, _results)) return;
                _results = value;
                OnPropertyChanged();
                OnPropertyChanged("HasResults");
            }
        }

        /// <summary>
        /// True if tests are currently running
        /// </summary>
        public bool Running
        {
            get { return _running; }
            set
            {
                if (value.Equals(_running)) return;
                _running = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// True if we have test results to display
        /// </summary>
        public bool HasResults
        {
            get { return Results != null; }
        }

        public ICommand RunTestsCommand { set; get; }
        public ICommand ViewAllResultsCommand { set; get; }
        public ICommand ViewFailedResultsCommand { set; get; }

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
            Running = true;
            Results = null;

            // TODO: Wrap the runner in an async operation and await the result
            ITestResult result = _runner.Run(TestListener.NULL, TestFilter.Empty);
            Results = new ResultSummary(result);

            Running = false;
        }
    }
}
