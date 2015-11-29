using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.Extensions.UITest
{
    class UIFactDiscoverer : IXunitTestCaseDiscoverer
    {
        readonly FactDiscoverer factDiscoverer;
        public UIFactDiscoverer(IMessageSink diagnosticMessageSink)
        {
            factDiscoverer = new FactDiscoverer(diagnosticMessageSink);
        }
        public IEnumerable<IXunitTestCase> Discover(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo factAttribute)
        {
            return factDiscoverer.Discover(discoveryOptions, testMethod, factAttribute)
                                 .Select(testCase => new UITestCase(testCase));
        }
    }
}
