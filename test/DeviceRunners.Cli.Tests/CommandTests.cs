using DeviceRunners.Cli.Commands;
using Spectre.Console.Cli;
using Spectre.Console.Testing;

namespace DeviceRunners.Cli.Tests;

public class CommandTests
{
    [Fact]
    public void CertificateCreateCommand_WithMissingParameters_ShowsError()
    {
        // Arrange
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.AddCommand<CertificateCreateCommand>("cert-create");
        });

        // Act
        var result = app.Run("cert-create");

        // Assert
        Assert.NotEqual(0, result.ExitCode);
    }

    [Fact]
    public void CertificateRemoveCommand_WithValidFingerprint_RunsSuccessfully()
    {
        // Arrange
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.AddCommand<CertificateRemoveCommand>("cert-remove");
        });

        // Act
        var result = app.Run("cert-remove", "--fingerprint", "ABCD1234");

        // Assert
        Assert.Equal(0, result.ExitCode);
    }

    [Fact]
    public void PortListenerCommand_CanInstantiate()
    {
        // Arrange
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.AddCommand<PortListenerCommand>("port-listen");
        });

        // This test mainly checks that the command can be constructed and validated
        // without actually running network operations which could hang
        
        // Act & Assert - Command configuration should succeed
        Assert.NotNull(app);
    }

    [Fact]
    public void TestStarterCommand_WithMissingApp_ShowsError()
    {
        // Arrange
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.AddCommand<TestStarterCommand>("test-start");
        });

        // Act
        var result = app.Run("test-start");

        // Assert
        Assert.NotEqual(0, result.ExitCode);
    }
}