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

        // Test command
        windows.AddCommand<TestStarterCommand>("test")
            .WithDescription("Install and start a test application")
            .WithExample(new[] { "windows", "test", "--app", "path/to/app.msix" })
            .WithExample(new[] { "windows", "test", "--app", "path/to/app.msix", "--testing-mode", "XHarness" });
    });

    // TCP commands
    config.AddBranch("tcp", tcp =>
    {
        tcp.SetDescription("TCP networking commands");
        
        tcp.AddBranch("listener", listener =>
        {
            listener.SetDescription("TCP listener commands");
            
            listener.AddCommand<PortListenerCommand>("start")
                .WithDescription("Start a TCP port listener")
                .WithExample(new[] { "tcp", "listener", "start", "--port", "16384" })
                .WithExample(new[] { "tcp", "listener", "start", "--port", "16384", "--output", "results.txt", "--non-interactive" });
        });
    });
});

return await app.RunAsync(args);