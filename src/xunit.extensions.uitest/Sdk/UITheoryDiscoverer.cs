using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.Extensions.UITest
{
    class UITheoryDiscoverer : IXunitTestCaseDiscoverer
    {
        readonly TheoryDiscoverer theoryDiscoverer;
        public UITheoryDiscoverer(IMessageSink diagnosticMessageSink)
        {
            theoryDiscoverer = new TheoryDiscoverer(diagnosticMessageSink);
        }
        public IEnumerable<IXunitTestCase> Discover(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo factAttribute)
        {
            return theoryDiscoverer.Discover(discoveryOptions, testMethod, factAttribute)
                                   .Select(testCase => new UITestCase(testCase));
        }
    }
}
