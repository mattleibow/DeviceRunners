// ***********************************************************************
// Copyright (c) 2015 NUnit Project
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

using NUnit.Framework;

namespace NUnit.Runner.Tests
{
    [TestFixture]
    public class TestCaseSample
    {
        [TestCase(1, 1, ExpectedResult = 2)]
        [TestCase(10, 10, ExpectedResult = 20)]
        [TestCase(12, 13, ExpectedResult = 24)] // Deliberate failure
        public int TestAddWithResult(int x, int y)
        {
            return x + y;
        }

        [TestCase(1, 1, 2)]
        [TestCase(10, 10, 20)]
        [TestCase(12, 13, 24)] // Deliberate failure
        public void TestAddWithExpected(int x, int y, int expected)
        {
            Assert.That(x + y, Is.EqualTo(expected));
        }

        [Test]
        public void TestPassedInParameter()
        {
            var val = TestContext.Parameters.Get("Parameter");
            TestContext.WriteLine("The passed-in value associated with 'Parameter' is: {0}", val?? "null");
            Assert.True(val == null || val == "Value");
        }
}
}
