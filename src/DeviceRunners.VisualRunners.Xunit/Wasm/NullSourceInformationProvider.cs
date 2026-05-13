using Xunit.Abstractions;
using Xunit.Sdk;

namespace DeviceRunners.VisualRunners.Xunit;

class NullSourceInformationProvider : LongLivedMarshalByRefObject, ISourceInformationProvider
{
	public static readonly NullSourceInformationProvider Instance = new();

	public ISourceInformation GetSourceInformation(ITestCase testCase) => new SourceInformation();

	public void Dispose() { }
}
