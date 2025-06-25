using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using System.Linq;

namespace DeviceRunners.SourceGenerators;

[Generator]
public class DeviceTestAppGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        // No syntax receiver needed since we only use MSBuild properties
    }

    public void Execute(GeneratorExecutionContext context)
    {
        // Check if this project should generate device test app files
        var shouldGenerate = ShouldGenerateDeviceTestApp(context);
        if (!shouldGenerate)
        {
            return;
        }

        // Get configuration from MSBuild properties
        var config = GetConfiguration(context);

        // Generate all the required files
        GenerateFiles(context, config);
    }

    private bool ShouldGenerateDeviceTestApp(GeneratorExecutionContext context)
    {
        // Check for MSBuild property to explicitly enable generation
        if (context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.GenerateDeviceTestApp", out var value))
            return bool.TrueString.Equals(value, StringComparison.OrdinalIgnoreCase);

        // Check if this is a MAUI project
        var isMauiProject = context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.UseMaui", out var useMaui) &&
                           bool.TrueString.Equals(useMaui, StringComparison.OrdinalIgnoreCase);

        if (!isMauiProject)
            return false;

        // Check for test framework references (Xunit, NUnit, etc.)
        var hasTestFramework = context.Compilation.ReferencedAssemblyNames.Any(name => 
            name.Name.IndexOf("xunit", StringComparison.OrdinalIgnoreCase) >= 0 || 
            name.Name.IndexOf("nunit", StringComparison.OrdinalIgnoreCase) >= 0 ||
            name.Name.IndexOf("mstest", StringComparison.OrdinalIgnoreCase) >= 0);
        
        if (!hasTestFramework)
            return false;

        // Check for DeviceRunners references
        var hasDeviceRunners = context.Compilation.ReferencedAssemblyNames.Any(name => 
            name.Name.IndexOf("DeviceRunners", StringComparison.OrdinalIgnoreCase) >= 0);

        if (!hasDeviceRunners)
            return false;

        // Auto-detect based on project name patterns
        if (context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.MSBuildProjectName", out var projectName))
        {
            var lowerProjectName = projectName.ToLower();
            if (lowerProjectName.Contains("devicetest") || 
                lowerProjectName.Contains("device.test") ||
                lowerProjectName.EndsWith(".devicetests") ||
                lowerProjectName.EndsWith("tests") && lowerProjectName.Contains("device"))
                return true;
        }

        // Check for ApplicationId starting with common test patterns
        if (context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.ApplicationId", out var appId))
        {
            var lowerAppId = appId.ToLower();
            if (lowerAppId.Contains("test") || lowerAppId.Contains("devicetest"))
                return true;
        }

        return false;
    }

    private DeviceTestAppConfiguration GetConfiguration(GeneratorExecutionContext context)
    {
        var config = new DeviceTestAppConfiguration();
        var options = context.AnalyzerConfigOptions.GlobalOptions;

        // Get assembly name and root namespace
        config.AssemblyName = context.Compilation.AssemblyName ?? "DeviceTestApp";
        config.RootNamespace = config.AssemblyName;

        // Override with MSBuild properties
        if (options.TryGetValue("build_property.ApplicationTitle", out var title) && !string.IsNullOrEmpty(title))
            config.AppTitle = title;

        if (options.TryGetValue("build_property.ApplicationId", out var appId) && !string.IsNullOrEmpty(appId))
            config.AppId = appId;

        if (options.TryGetValue("build_property.RootNamespace", out var rootNamespace) && !string.IsNullOrEmpty(rootNamespace))
            config.RootNamespace = rootNamespace;

        return config;
    }

    private void GenerateFiles(GeneratorExecutionContext context, DeviceTestAppConfiguration config)
    {
        // Generate platform-specific files only - MauiProgram.cs and Usings.cs should be in the app
        GenerateAndroidFiles(context, config);
        GenerateiOSFiles(context, config);
        GenerateWindowsFiles(context, config);
        GenerateMacCatalystFiles(context, config);
    }

    private void GenerateAndroidFiles(GeneratorExecutionContext context, DeviceTestAppConfiguration config)
    {
        // MainActivity.cs
        var mainActivity = GetEmbeddedResource("Templates.Platforms.Android.MainActivity.cs.template")
            .Replace("{{RootNamespace}}", config.RootNamespace)
            .Replace("{{AppId}}", config.AppId);
        context.AddSource("Platforms.Android.MainActivity.g.cs", SourceText.From(mainActivity, Encoding.UTF8));

        // MainApplication.cs
        var mainApplication = GetEmbeddedResource("Templates.Platforms.Android.MainApplication.cs.template")
            .Replace("{{RootNamespace}}", config.RootNamespace);
        context.AddSource("Platforms.Android.MainApplication.g.cs", SourceText.From(mainApplication, Encoding.UTF8));
    }

    private void GenerateiOSFiles(GeneratorExecutionContext context, DeviceTestAppConfiguration config)
    {
        // Program.cs
        var program = GetEmbeddedResource("Templates.Platforms.iOS.Program.cs.template")
            .Replace("{{RootNamespace}}", config.RootNamespace);
        context.AddSource("Platforms.iOS.Program.g.cs", SourceText.From(program, Encoding.UTF8));

        // AppDelegate.cs
        var appDelegate = GetEmbeddedResource("Templates.Platforms.iOS.AppDelegate.cs.template")
            .Replace("{{RootNamespace}}", config.RootNamespace);
        context.AddSource("Platforms.iOS.AppDelegate.g.cs", SourceText.From(appDelegate, Encoding.UTF8));
    }

    private void GenerateWindowsFiles(GeneratorExecutionContext context, DeviceTestAppConfiguration config)
    {
        // App.xaml.cs
        var appXamlCs = GetEmbeddedResource("Templates.Platforms.Windows.App.xaml.cs.template")
            .Replace("{{RootNamespace}}", config.RootNamespace);
        context.AddSource("Platforms.Windows.App.xaml.g.cs", SourceText.From(appXamlCs, Encoding.UTF8));
    }

    private void GenerateMacCatalystFiles(GeneratorExecutionContext context, DeviceTestAppConfiguration config)
    {
        // Program.cs
        var program = GetEmbeddedResource("Templates.Platforms.MacCatalyst.Program.cs.template")
            .Replace("{{RootNamespace}}", config.RootNamespace);
        context.AddSource("Platforms.MacCatalyst.Program.g.cs", SourceText.From(program, Encoding.UTF8));

        // AppDelegate.cs
        var appDelegate = GetEmbeddedResource("Templates.Platforms.MacCatalyst.AppDelegate.cs.template")
            .Replace("{{RootNamespace}}", config.RootNamespace);
        context.AddSource("Platforms.MacCatalyst.AppDelegate.g.cs", SourceText.From(appDelegate, Encoding.UTF8));
    }

    private string GetEmbeddedResource(string resourceName)
    {
        var assembly = typeof(DeviceTestAppGenerator).Assembly;
        
        // Convert the resource name to the actual embedded resource name
        var actualResourceName = $"DeviceRunners.SourceGenerators.{resourceName}";
        
        using var stream = assembly.GetManifestResourceStream(actualResourceName);
        if (stream != null)
        {
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
        
        // If not found, list all available resources for debugging
        var availableResources = string.Join(", ", assembly.GetManifestResourceNames());
        return $"// Resource not found: {resourceName}\n// Available resources: {availableResources}";
    }
}

public class DeviceTestAppConfiguration
{
    public string AssemblyName { get; set; } = "DeviceTestApp";
    public string RootNamespace { get; set; } = "DeviceTestApp";
    public string AppTitle { get; set; } = "DeviceTestApp";
    public string AppId { get; set; } = "com.companyname.devicetestapp";
}