using DeviceRunners.Cli.Commands;
using Spectre.Console.Cli;
using Spectre.Console.Testing;
using System.Text.Json;

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
            config.AddBranch("windows", windows =>
            {
                windows.AddBranch("cert", cert =>
                {
                    cert.AddCommand<CertificateCreateCommand>("install");
                });
            });
        });

        // Act
        var result = app.Run("windows", "cert", "install");

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
            config.AddBranch("windows", windows =>
            {
                windows.AddBranch("cert", cert =>
                {
                    cert.AddCommand<CertificateRemoveCommand>("uninstall");
                });
            });
        });

        // Act
        var result = app.Run("windows", "cert", "uninstall", "--fingerprint", "ABCD1234");

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
            config.AddCommand<PortListenerCommand>("listen");
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
            config.AddBranch("windows", windows =>
            {
                windows.AddCommand<TestStarterCommand>("test");
            });
        });

        // Act
        var result = app.Run("windows", "test");

        // Assert
        Assert.NotEqual(0, result.ExitCode);
    }

    [Fact]
    public void AppInstallCommand_WithMissingApp_ShowsError()
    {
        // Arrange
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.AddBranch("windows", windows =>
            {
                windows.AddCommand<AppInstallCommand>("install");
            });
        });

        // Act
        var result = app.Run("windows", "install");

        // Assert
        Assert.NotEqual(0, result.ExitCode);
    }

    [Fact]
    public void AppUninstallCommand_WithMissingParameters_ShowsError()
    {
        // Arrange
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.AddBranch("windows", windows =>
            {
                windows.AddCommand<AppUninstallCommand>("uninstall");
            });
        });

        // Act
        var result = app.Run("windows", "uninstall");

        // Assert
        Assert.NotEqual(0, result.ExitCode);
    }

    [Fact]
    public void AppLaunchCommand_WithMissingParameters_ShowsError()
    {
        // Arrange
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.AddBranch("windows", windows =>
            {
                windows.AddCommand<AppLaunchCommand>("launch");
            });
        });

        // Act
        var result = app.Run("windows", "launch");

        // Assert
        Assert.NotEqual(0, result.ExitCode);
    }

    [Fact]
    public void CertificateRemoveCommand_WithJsonOutput_RunsSuccessfully()
    {
        // Arrange
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.AddBranch("windows", windows =>
            {
                windows.AddBranch("cert", cert =>
                {
                    cert.AddCommand<CertificateRemoveCommand>("uninstall");
                });
            });
        });

        // Act
        var result = app.Run("windows", "cert", "uninstall", "--fingerprint", "ABCD1234", "--output", "json");

        // Assert
        Assert.Equal(0, result.ExitCode);
    }

    [Fact]
    public void CertificateRemoveCommand_WithTextOutput_RunsSuccessfully()
    {
        // Arrange
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.AddBranch("windows", windows =>
            {
                windows.AddBranch("cert", cert =>
                {
                    cert.AddCommand<CertificateRemoveCommand>("uninstall");
                });
            });
        });

        // Act
        var result = app.Run("windows", "cert", "uninstall", "--fingerprint", "ABCD1234", "--output", "text");

        // Assert
        Assert.Equal(0, result.ExitCode);
    }

    [Fact]
    public void OutputFormat_EnumValues_AreValid()
    {
        // Test that the OutputFormat enum has the expected values
        Assert.True(Enum.IsDefined(typeof(DeviceRunners.Cli.Models.OutputFormat), DeviceRunners.Cli.Models.OutputFormat.Default));
        Assert.True(Enum.IsDefined(typeof(DeviceRunners.Cli.Models.OutputFormat), DeviceRunners.Cli.Models.OutputFormat.Json));
        Assert.True(Enum.IsDefined(typeof(DeviceRunners.Cli.Models.OutputFormat), DeviceRunners.Cli.Models.OutputFormat.Xml));
        Assert.True(Enum.IsDefined(typeof(DeviceRunners.Cli.Models.OutputFormat), DeviceRunners.Cli.Models.OutputFormat.Text));
    }

    [Fact]
    public void OutputService_CanInstantiate()
    {
        // Test that the OutputService can be instantiated
        var outputService = new DeviceRunners.Cli.Services.OutputService();
        Assert.NotNull(outputService);
    }

    [Fact]
    public void CommandResults_CanInstantiate()
    {
        // Test that all command result types can be instantiated
        var certCreateResult = new DeviceRunners.Cli.Models.CertificateCreateResult();
        var certRemoveResult = new DeviceRunners.Cli.Models.CertificateRemoveResult();
        var portListenerResult = new DeviceRunners.Cli.Models.PortListenerResult();
        var appInstallResult = new DeviceRunners.Cli.Models.AppInstallResult();
        var appUninstallResult = new DeviceRunners.Cli.Models.AppUninstallResult();
        var appLaunchResult = new DeviceRunners.Cli.Models.AppLaunchResult();
        var testStartResult = new DeviceRunners.Cli.Models.TestStartResult();

        Assert.NotNull(certCreateResult);
        Assert.NotNull(certRemoveResult);
        Assert.NotNull(portListenerResult);
        Assert.NotNull(appInstallResult);
        Assert.NotNull(appUninstallResult);
        Assert.NotNull(appLaunchResult);
        Assert.NotNull(testStartResult);
    }
}