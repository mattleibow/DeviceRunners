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
using System.Threading.Tasks;

namespace NUnit.Runner.Tests
{
    [TestFixture]
    public class AsyncTests
    {
        [Test]
        public async Task TestAsyncMethodWithoutReturn()
        {
            await AsyncMethodWithoutReturn();
            Assert.Pass();
        }

        [Test]
        public async Task TestAsyncMethodWithReturnValueIndirectly()
        {
            bool actual = await AsyncMethodWithReturnValue();
            Assert.That(actual, Is.True);
        }

        [Test]
        public void TestAsyncMethodWithReturnValueDirectly()
        {
            Assert.That(async () => await AsyncMethodWithReturnValue(), Is.True);
        }

        public async Task AsyncMethodWithoutReturn()
        {
            await Task.FromResult<object>(null);
        }

        public async Task<bool> AsyncMethodWithReturnValue()
        {
            return await Task.FromResult(true);
        }
    }
}
