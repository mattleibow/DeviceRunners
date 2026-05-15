using Xunit.Abstractions;

namespace DeviceRunners.VisualRunners.Xunit;

/// <summary>
/// Source information provider that returns empty source info.
/// Source file/line mapping is unavailable when running via reflection
/// (no PDBs in WASM, no source link in embedded scenarios).
/// </summary>
class EmptySourceInformationProvider : global::Xunit.Sdk.LongLivedMarshalByRefObject, ISourceInformationProvider
{
	public static readonly EmptySourceInformationProvider Instance = new();

	public ISourceInformation GetSourceInformation(ITestCase testCase) => new global::Xunit.SourceInformation();

	public void Dispose() { }
}
