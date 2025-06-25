using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using DeviceRunners.SourceGenerators;
using System.Collections.Immutable;
using Xunit;

namespace DeviceRunners.SourceGenerators.Tests;

public class DeviceTestAppGeneratorTests
{
    [Fact]
    public void Generator_WithMauiAndTestFramework_GeneratesFiles()
    {
        // Arrange
        var source = @"
namespace TestApp
{
    public class Tests
    {
        [Fact]
        public void MyTest()
        {
            Assert.True(true);
        }
    }
}";

        var globalOptions = new Dictionary<string, string>
        {
            ["build_property.UseMaui"] = "true",
            ["build_property.ApplicationTitle"] = "My Test App",
            ["build_property.ApplicationId"] = "com.test.myapp",
            ["build_property.RootNamespace"] = "TestApp",
            ["build_property.MSBuildProjectName"] = "TestApp.DeviceTests"
        };

        // Act
        var (compilation, diagnostics) = GetGeneratedOutput(source, globalOptions);

        // Assert
        Assert.Empty(diagnostics);
        
        var generatedFiles = compilation.SyntaxTrees
            .Where(st => st.FilePath.Contains(".g.cs"))
            .ToList();

        Assert.NotEmpty(generatedFiles);
        
        // Should have platform files
        Assert.Contains(generatedFiles, f => f.FilePath.Contains("Android.MainActivity.g.cs"));
        Assert.Contains(generatedFiles, f => f.FilePath.Contains("iOS.Program.g.cs"));
        Assert.Contains(generatedFiles, f => f.FilePath.Contains("Windows.App.xaml.g.cs"));
        Assert.Contains(generatedFiles, f => f.FilePath.Contains("MacCatalyst.Program.g.cs"));
    }

    [Fact]
    public void Generator_WithoutMaui_DoesNotGenerate()
    {
        // Arrange
        var source = @"
namespace TestApp
{
    public class Tests
    {
        [Fact]
        public void MyTest()
        {
            Assert.True(true);
        }
    }
}";

        var globalOptions = new Dictionary<string, string>
        {
            ["build_property.UseMaui"] = "false"
        };

        // Act
        var (compilation, diagnostics) = GetGeneratedOutput(source, globalOptions);

        // Assert
        Assert.Empty(diagnostics);
        
        var generatedFiles = compilation.SyntaxTrees
            .Where(st => st.FilePath.Contains(".g.cs") && !st.FilePath.Contains("TestGenerator"))
            .ToList();

        Assert.Empty(generatedFiles);
    }

    [Fact]
    public void Generator_WithoutTestFramework_DoesNotGenerate()
    {
        // Arrange
        var source = @"
namespace TestApp
{
    public class MyClass
    {
        public void MyMethod()
        {
        }
    }
}";

        var globalOptions = new Dictionary<string, string>
        {
            ["build_property.UseMaui"] = "true",
            ["build_property.MSBuildProjectName"] = "MyApp"
        };

        // Act
        var (compilation, diagnostics) = GetGeneratedOutput(source, globalOptions, includeXunit: false);

        // Assert
        Assert.Empty(diagnostics);
        
        var generatedFiles = compilation.SyntaxTrees
            .Where(st => st.FilePath.Contains(".g.cs") && !st.FilePath.Contains("TestGenerator"))
            .ToList();

        Assert.Empty(generatedFiles);
    }

    [Fact]
    public void Generator_WithExplicitProperty_GeneratesFiles()
    {
        // Arrange
        var source = @"
namespace TestApp
{
    public class Tests
    {
        [Fact]
        public void MyTest()
        {
            Assert.True(true);
        }
    }
}";

        var globalOptions = new Dictionary<string, string>
        {
            ["build_property.GenerateDeviceTestApp"] = "true",
            ["build_property.ApplicationTitle"] = "Explicit Test App",
            ["build_property.ApplicationId"] = "com.explicit.testapp",
            ["build_property.RootNamespace"] = "TestApp.Explicit"
        };

        // Act
        var (compilation, diagnostics) = GetGeneratedOutput(source, globalOptions);

        // Assert
        Assert.Empty(diagnostics);
        
        var generatedFiles = compilation.SyntaxTrees
            .Where(st => st.FilePath.Contains(".g.cs"))
            .ToList();

        Assert.NotEmpty(generatedFiles);
    }

    private static (Compilation compilation, ImmutableArray<Diagnostic> diagnostics) GetGeneratedOutput(
        string source, 
        Dictionary<string, string>? globalOptions = null,
        bool includeXunit = true)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        
        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => !assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
            .ToList();

        // Add xunit reference to simulate test project
        if (includeXunit)
        {
            try
            {
                var xunitAssembly = typeof(Xunit.FactAttribute).Assembly;
                references.Add(MetadataReference.CreateFromFile(xunitAssembly.Location));
            }
            catch
            {
                // If xunit is not available, create a mock reference
            }
        }

        var compilation = CSharpCompilation.Create("TestAssembly",
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new DeviceTestAppGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);

        // Set up analyzer config options
        var analyzerConfigOptions = globalOptions != null
            ? new TestAnalyzerConfigOptions(globalOptions)
            : new TestAnalyzerConfigOptions(new Dictionary<string, string>());

        var optionsProvider = new TestAnalyzerConfigOptionsProvider(analyzerConfigOptions);

        driver = (CSharpGeneratorDriver)driver.WithUpdatedAnalyzerConfigOptions(optionsProvider);
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var newCompilation, out var diagnostics);

        return (newCompilation, diagnostics);
    }
}

// Helper classes for testing
public class TestAnalyzerConfigOptions : AnalyzerConfigOptions
{
    private readonly Dictionary<string, string> _options;

    public TestAnalyzerConfigOptions(Dictionary<string, string> options)
    {
        _options = options;
    }

    public override bool TryGetValue(string key, out string value)
    {
        return _options.TryGetValue(key, out value!);
    }
}

public class TestAnalyzerConfigOptionsProvider : AnalyzerConfigOptionsProvider
{
    private readonly AnalyzerConfigOptions _globalOptions;

    public TestAnalyzerConfigOptionsProvider(AnalyzerConfigOptions globalOptions)
    {
        _globalOptions = globalOptions;
    }

    public override AnalyzerConfigOptions GlobalOptions => _globalOptions;

    public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => _globalOptions;

    public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => _globalOptions;
}