using System.Reflection;

using Xunit.Sdk;
using Xunit.v3;

namespace DeviceRunners.VisualRunners.Xunit3;

/// <summary>
/// A WASM-safe subclass of <see cref="XunitTestAssembly"/> that provides a
/// logical assembly path when <see cref="Assembly.Location"/> is empty.
/// On WASM, <c>Assembly.Location</c> returns an empty string which causes
/// xunit's <c>TestAssemblyRunner.OnTestAssemblyStarting</c> to fail
/// (it calls <c>Path.GetFileNameWithoutExtension(AssemblyPath)</c>).
/// By re-implementing <see cref="IXunitTestAssembly"/> on this subclass,
/// the runtime re-maps the interface dispatch for <c>AssemblyPath</c>
/// to our override that returns the logical name.
/// </summary>
class WasmXunit3TestAssembly : XunitTestAssembly, IXunitTestAssembly
{
	readonly string _logicalAssemblyPath;

	public WasmXunit3TestAssembly(Assembly assembly, string? configFileName, Version? version, string logicalAssemblyPath)
		: base(assembly, configFileName, version)
	{
		_logicalAssemblyPath = logicalAssemblyPath;
	}

	/// <summary>
	/// Returns the logical assembly path instead of <see cref="Assembly.Location"/>
	/// which is empty on WASM.
	/// </summary>
	new public string AssemblyPath => _logicalAssemblyPath;
}
