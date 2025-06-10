using System.IO;
using Xunit;

namespace DeviceRunners.Cli.Tests.Infrastructure
{
    public class CentralPackageManagementTests
    {
        [Fact]
        public void Directory_Packages_Props_Should_Exist()
        {
            // Arrange
            var repoRoot = GetRepositoryRoot();
            var directoryPackagesProps = Path.Combine(repoRoot, "Directory.Packages.props");

            // Act & Assert
            Assert.True(File.Exists(directoryPackagesProps), 
                "Directory.Packages.props should exist at repository root for Central Package Management");
        }

        [Fact]
        public void Directory_Build_Props_Should_Enable_CPM()
        {
            // Arrange
            var repoRoot = GetRepositoryRoot();
            var directoryBuildProps = Path.Combine(repoRoot, "Directory.Build.props");

            // Act
            var content = File.ReadAllText(directoryBuildProps);

            // Assert
            Assert.Contains("ManagePackageVersionsCentrally", content);
            Assert.Contains("<ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>", content);
        }

        [Fact]
        public void Directory_Packages_Props_Should_Contain_PackageVersions()
        {
            // Arrange
            var repoRoot = GetRepositoryRoot();
            var directoryPackagesProps = Path.Combine(repoRoot, "Directory.Packages.props");

            // Act
            var content = File.ReadAllText(directoryPackagesProps);

            // Assert
            Assert.Contains("PackageVersion", content);
            Assert.Contains("Spectre.Console.Cli", content);
            Assert.Contains("xunit", content);
            Assert.Contains("Microsoft.Maui.Controls", content);
        }

        [Fact]
        public void Artifacts_Directory_Should_Be_Configured()
        {
            // Arrange
            var repoRoot = GetRepositoryRoot();
            var directoryBuildProps = Path.Combine(repoRoot, "Directory.Build.props");

            // Act
            var content = File.ReadAllText(directoryBuildProps);

            // Assert
            Assert.Contains("ArtifactsPath", content);
            Assert.Contains("BaseOutputPath", content);
            Assert.Contains("BaseIntermediateOutputPath", content);
            Assert.Contains("PackageOutputPath", content);
        }

        [Fact]
        public void Artifacts_Directory_Should_Exist_After_Build()
        {
            // Arrange
            var repoRoot = GetRepositoryRoot();
            var artifactsDir = Path.Combine(repoRoot, "artifacts");

            // Act & Assert
            Assert.True(Directory.Exists(artifactsDir), 
                "Artifacts directory should exist after build");

            var binDir = Path.Combine(artifactsDir, "bin");
            Assert.True(Directory.Exists(binDir), 
                "Artifacts bin directory should exist after build");
        }

        private static string GetRepositoryRoot()
        {
            var currentDir = Directory.GetCurrentDirectory();
            while (currentDir != null && !File.Exists(Path.Combine(currentDir, "DeviceRunners.sln")))
            {
                currentDir = Directory.GetParent(currentDir)?.FullName;
            }
            
            if (currentDir == null)
                throw new InvalidOperationException("Could not find repository root");
                
            return currentDir;
        }
    }
}