using System.Collections.Generic;
using System.Reflection;

namespace Xunit.Runner.Devices.VisualRunner
{
	public class RunnerOptions
	{
		/// <summary>
		/// The list of assemblies that contain tests.
		/// </summary>
		public List<Assembly> Assemblies { get; set; } = new List<Assembly>();

		/// <summary>
		/// The list of categories to skip in the form:
		///   [category-name]=[skip-when-value]
		/// </summary>
		public List<string> SkipCategories { get; set; } = new List<string>();

		public string TestResultsFilename { get; set; } = "TestResults.xml";
	}
}
