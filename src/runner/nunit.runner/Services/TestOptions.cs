using System;
using System.Collections.Generic;
using System.Text;

namespace NUnit.Runner.Services
{
    /// <summary>
    /// Options for the device test suite.
    /// </summary>
    public class TestOptions
    {
        /// <summary>
        /// If True, the tests will run automatically when the app starts
        /// otherwise you must run them manually.
        /// </summary>
        public bool AutoRun { get; set; }

        /// <summary>
        /// Information about the tcp listener host and port.
        /// For now, send result as XML to the listening server.
        /// </summary>
        public TcpWriterInfo TcpWriterParamaters { get; set; }

        /// <summary>
        /// Creates a NUnit Xml result file on the host file system using PCLStorage library.
        /// </summary>
        public bool CreateXmlResultFile { get; set; }
    }
}
