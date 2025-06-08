using DeviceRunners.Cli.Commands;
using Spectre.Console.Cli;

var app = new CommandApp();

app.Configure(config =>
{
    // Windows-specific commands
    config.AddBranch("windows", windows =>
    {
        windows.SetDescription("Windows-specific commands for certificate and test management");
        
        // Certificate management commands
        windows.AddBranch("cert", cert =>
        {
            cert.SetDescription("Certificate management commands");
            
            cert.AddCommand<CertificateCreateCommand>("install")
                .WithDescription("Generate and install a self-signed certificate for MSIX packages")
                .WithExample(new[] { "windows", "cert", "install", "--publisher", "CN=MyCompany" })
                .WithExample(new[] { "windows", "cert", "install", "--manifest", "path/to/Package.appxmanifest" });

            cert.AddCommand<CertificateRemoveCommand>("uninstall")
                .WithDescription("Remove a certificate by fingerprint")
                .WithExample(new[] { "windows", "cert", "uninstall", "--fingerprint", "ABCD1234..." });
        });

        // App management commands
        windows.AddCommand<AppInstallCommand>("install")
            .WithDescription("Install an MSIX application package")
            .WithExample(new[] { "windows", "install", "--app", "path/to/app.msix" });

        windows.AddCommand<AppUninstallCommand>("uninstall")
            .WithDescription("Uninstall an application")
            .WithExample(new[] { "windows", "uninstall", "--app", "path/to/app.msix" })
            .WithExample(new[] { "windows", "uninstall", "--identity", "MyApp" });

        windows.AddCommand<AppLaunchCommand>("launch")
            .WithDescription("Launch an installed application")
            .WithExample(new[] { "windows", "launch", "--app", "path/to/app.msix" })
            .WithExample(new[] { "windows", "launch", "--identity", "MyApp" })
            .WithExample(new[] { "windows", "launch", "--identity", "MyApp", "--args", "test-arguments" });

        // Test command
        windows.AddCommand<TestStarterCommand>("test")
            .WithDescription("Install and start a test application")
            .WithExample(new[] { "windows", "test", "--app", "path/to/app.msix" });
    });

    // TCP commands
    config.AddCommand<PortListenerCommand>("listen")
        .WithDescription("Start a TCP port listener")
        .WithExample(new[] { "listen", "--port", "16384" })
        .WithExample(new[] { "listen", "--port", "16384", "--results-file", "results.txt", "--non-interactive" });
});

return await app.RunAsync(args);