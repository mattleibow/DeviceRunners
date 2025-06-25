using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace DeviceRunners.SourceGenerators;

[Generator]
public class DeviceTestAppGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new DeviceTestAppSyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not DeviceTestAppSyntaxReceiver receiver)
            return;

        // Check if this project should generate device test app files
        var shouldGenerate = ShouldGenerateDeviceTestApp(context, receiver);
        if (!shouldGenerate)
            return;

        // Get configuration from attributes and MSBuild properties
        var config = GetConfiguration(context, receiver);

        // Generate all the required files
        GenerateFiles(context, config);
    }

    private bool ShouldGenerateDeviceTestApp(GeneratorExecutionContext context, DeviceTestAppSyntaxReceiver receiver)
    {
        // Check for DeviceTestApp attribute on any class
        if (receiver.DeviceTestAppClasses.Count > 0)
            return true;

        // Check for MSBuild property
        if (context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.GenerateDeviceTestApp", out var value))
            return bool.TrueString.Equals(value, StringComparison.OrdinalIgnoreCase);

        return false;
    }

    private DeviceTestAppConfiguration GetConfiguration(GeneratorExecutionContext context, DeviceTestAppSyntaxReceiver receiver)
    {
        var config = new DeviceTestAppConfiguration();

        // Get assembly name and namespace
        config.AssemblyName = context.Compilation.AssemblyName ?? "DeviceTestApp";
        config.RootNamespace = config.AssemblyName;

        // Get configuration from attribute if present
        if (receiver.DeviceTestAppClasses.Count > 0)
        {
            var firstClass = receiver.DeviceTestAppClasses.First();
            var semanticModel = context.Compilation.GetSemanticModel(firstClass.SyntaxTree);
            var classSymbol = semanticModel.GetDeclaredSymbol(firstClass);
            if (classSymbol != null)
            {
                var attribute = classSymbol.GetAttributes()
                    .FirstOrDefault(a => a.AttributeClass?.Name == "DeviceTestAppAttribute");
                
                if (attribute != null)
                {
                    // Extract configuration from attribute parameters
                    config = ExtractConfigurationFromAttribute(attribute, config);
                }
            }
        }

        // Override with MSBuild properties if present
        ApplyMSBuildProperties(context, config);

        return config;
    }

    private DeviceTestAppConfiguration ExtractConfigurationFromAttribute(AttributeData attribute, DeviceTestAppConfiguration config)
    {
        // Extract named arguments
        foreach (var namedArg in attribute.NamedArguments)
        {
            switch (namedArg.Key)
            {
                case "AppTitle":
                    config.AppTitle = namedArg.Value.Value?.ToString() ?? config.AppTitle;
                    break;
                case "AppId":
                    config.AppId = namedArg.Value.Value?.ToString() ?? config.AppId;
                    break;
                case "TestFrameworks":
                    if (namedArg.Value.Value is int frameworks)
                        config.TestFrameworks = (TestFrameworksEnum)frameworks;
                    break;
            }
        }

        return config;
    }

    private void ApplyMSBuildProperties(GeneratorExecutionContext context, DeviceTestAppConfiguration config)
    {
        var options = context.AnalyzerConfigOptions.GlobalOptions;

        if (options.TryGetValue("build_property.DeviceTestAppTitle", out var title))
            config.AppTitle = title;

        if (options.TryGetValue("build_property.DeviceTestAppId", out var appId))
            config.AppId = appId;

        if (options.TryGetValue("build_property.DeviceTestAppRootNamespace", out var rootNamespace))
            config.RootNamespace = rootNamespace;

        if (options.TryGetValue("build_property.DeviceTestFrameworks", out var frameworks))
        {
            if (Enum.TryParse<TestFrameworksEnum>(frameworks, true, out var parsed))
                config.TestFrameworks = parsed;
        }
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
        
        // Try different resource name formats
        var possibleNames = new[]
        {
            $"DeviceRunners.SourceGenerators.{resourceName}",
            resourceName,
            resourceName.Replace('.', '_')
        };

        foreach (var name in possibleNames)
        {
            using var stream = assembly.GetManifestResourceStream(name);
            if (stream != null)
            {
                using var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
        }
        
        // If not found, list all available resources for debugging
        var availableResources = string.Join(", ", assembly.GetManifestResourceNames());
        return $"// Resource not found: {resourceName}\n// Available resources: {availableResources}";
    }

    private string RemoveConditionalBlock(string source, string startTag, string endTag)
    {
        var startIndex = source.IndexOf(startTag);
        if (startIndex == -1) return source;

        var endIndex = source.IndexOf(endTag, startIndex);
        if (endIndex == -1) return source;

        return source.Remove(startIndex, endIndex + endTag.Length - startIndex);
    }
}

public class DeviceTestAppSyntaxReceiver : ISyntaxReceiver
{
    public List<ClassDeclarationSyntax> DeviceTestAppClasses { get; } = new();

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is ClassDeclarationSyntax classDeclaration)
        {
            // Look for classes with DeviceTestApp attribute
            if (classDeclaration.AttributeLists.Count > 0)
            {
                foreach (var attributeList in classDeclaration.AttributeLists)
                {
                    foreach (var attribute in attributeList.Attributes)
                    {
                        var attributeName = attribute.Name.ToString();
                        if (attributeName == "DeviceTestApp" || attributeName == "DeviceTestAppAttribute")
                        {
                            DeviceTestAppClasses.Add(classDeclaration);
                            return;
                        }
                    }
                }
            }
        }
    }
}

public class DeviceTestAppConfiguration
{
    public string AssemblyName { get; set; } = "DeviceTestApp";
    public string RootNamespace { get; set; } = "DeviceTestApp";
    public string AppTitle { get; set; } = "DeviceTestApp";
    public string AppId { get; set; } = "com.companyname.devicetestapp";
    public TestFrameworksEnum TestFrameworks { get; set; } = TestFrameworksEnum.Xunit;
}

[Flags]
public enum TestFrameworksEnum
{
    None = 0,
    Xunit = 1,
    NUnit = 2,
    Both = Xunit | NUnit
}