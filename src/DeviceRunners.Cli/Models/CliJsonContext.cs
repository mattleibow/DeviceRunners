using System.Text.Json.Serialization;

namespace DeviceRunners.Cli.Models;

[JsonSerializable(typeof(AppInstallResult))]
[JsonSerializable(typeof(AppLaunchResult))]
[JsonSerializable(typeof(AppUninstallResult))]
[JsonSerializable(typeof(CertificateCreateResult))]
[JsonSerializable(typeof(CertificateRemoveResult))]
[JsonSerializable(typeof(PortListenerResult))]
[JsonSerializable(typeof(TestStartResult))]
[JsonSerializable(typeof(Dictionary<string, object>))]
[JsonSourceGenerationOptions(
	WriteIndented = true,
	PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal partial class CliJsonContext : JsonSerializerContext;
