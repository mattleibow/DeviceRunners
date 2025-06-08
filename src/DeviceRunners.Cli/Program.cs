using DeviceRunners.Cli.Commands;
using Spectre.Console.Cli;

var app = new CommandApp();

app.Configure(config =>
{
    config.AddCommand<CertificateCreateCommand>("cert-create")
        .WithDescription("Generate and install a self-signed certificate for MSIX packages")
        .WithExample(new[] { "cert-create", "--publisher", "CN=MyCompany" })
        .WithExample(new[] { "cert-create", "--manifest", "path/to/Package.appxmanifest" });

    config.AddCommand<CertificateRemoveCommand>("cert-remove")
        .WithDescription("Remove a certificate by fingerprint")
        .WithExample(new[] { "cert-remove", "--fingerprint", "ABCD1234..." });

    config.AddCommand<PortListenerCommand>("port-listen")
        .WithDescription("Start a TCP port listener")
        .WithExample(new[] { "port-listen", "--port", "16384" })
        .WithExample(new[] { "port-listen", "--port", "16384", "--output", "results.txt", "--non-interactive" });

    config.AddCommand<TestStarterCommand>("test-start")
        .WithDescription("Install and start a test application")
        .WithExample(new[] { "test-start", "--app", "path/to/app.msix" })
        .WithExample(new[] { "test-start", "--app", "path/to/app.msix", "--testing-mode", "XHarness" });
});

return await app.RunAsync(args);