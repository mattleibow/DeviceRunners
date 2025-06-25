using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using DeviceRunners.SourceGenerators;
using System.Collections.Immutable;
using Xunit;

namespace DeviceRunners.SourceGenerators.Tests;

public class DeviceTestAppGeneratorTests
{
    [Fact]
    public void Generator_WithDeviceTestAppAttribute_GeneratesFiles()
    {
        // Arrange
        var source = @"
using DeviceRunners.SourceGenerators;

namespace TestApp
{
    [DeviceTestApp(AppTitle = ""My Test App"", AppId = ""com.test.myapp"")]
    public class Tests
    {
        [Fact]
        public void MyTest()
        {
            Assert.True(true);
        }
    }
}";

        // Act
        var (compilation, diagnostics) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);
        
        var generatedFiles = compilation.SyntaxTrees
            .Where(st => st.FilePath.Contains(".g.cs"))
            .ToList();

        Assert.NotEmpty(generatedFiles);
        
        // Should have MauiProgram.g.cs
        Assert.Contains(generatedFiles, f => f.FilePath.EndsWith("MauiProgram.g.cs"));
        
        // Should have platform files
        Assert.Contains(generatedFiles, f => f.FilePath.Contains("Android.MainActivity.g.cs"));
        Assert.Contains(generatedFiles, f => f.FilePath.Contains("iOS.Program.g.cs"));
    }

    [Fact]
    public void Generator_WithoutDeviceTestAppAttribute_DoesNotGenerate()
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

        // Act
        var (compilation, diagnostics) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);
        
        var generatedFiles = compilation.SyntaxTrees
            .Where(st => st.FilePath.Contains(".g.cs"))
            .ToList();

        Assert.Empty(generatedFiles);
    }

    private static (Compilation compilation, ImmutableArray<Diagnostic> diagnostics) GetGeneratedOutput(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => !assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
            .Concat(new[] { MetadataReference.CreateFromFile(typeof(DeviceTestAppAttribute).Assembly.Location) });

        var compilation = CSharpCompilation.Create("TestAssembly",
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new DeviceTestAppGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var newCompilation, out var diagnostics);

        return (newCompilation, diagnostics);
    }
}