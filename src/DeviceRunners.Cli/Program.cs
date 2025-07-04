using DeviceRunners.Cli.Commands;
using Spectre.Console.Cli;

var app = new CommandApp();

app.Configure(config =>
{
    // Set global information for the application
    config.SetApplicationName("device-runners");

    // These examples demonstrate the power of using DeviceRunners across different platforms
    config.AddExample(["windows", "test", "--app", "MyTests.msix", "--results-directory", "results"]);
    config.AddExample(["android", "test", "--app", "MyTests.apk", "--results-directory", "results"]);
    config.AddExample(["macos", "test", "--app", "MyTests.app", "--results-directory", "results"]);
    config.AddExample(["ios", "test", "--app", "MyTests.app", "--results-directory", "results"]);
    config.AddExample(["android", "install", "--app", "path/to/app.apk", "--device", "emulator-5554"]);
    config.AddExample(["listen", "--port", "16384", "--non-interactive"]);

    // TCP commands
    config.AddCommand<PortListenerCommand>("listen")
        .WithDescription("Start a TCP port listener")
        .WithExample(["listen", "--port", "16384"])
        .WithExample(["listen", "--port", "16384", "--non-interactive"])
        .WithExample(["listen", "--port", "16384", "--non-interactive", "--connection-timeout", "60", "--data-timeout", "45"])
        .WithExample(["listen", "--port", "16384", "--results-file", "results.txt", "--non-interactive"]);

    // Windows-specific commands
    config.AddBranch("windows", windows =>
    {
        windows.SetDescription("Windows-specific commands for certificate, app and test management");

        // Certificate management commands
        windows.AddBranch("cert", cert =>
        {
            cert.SetDescription("Certificate management commands");

            cert.AddCommand<WindowCertificateCreateCommand>("install")
                .WithDescription("Generate and install a self-signed certificate for MSIX packages")
                .WithExample(["windows", "cert", "install", "--publisher", "CN=MyCompany"])
                .WithExample(["windows", "cert", "install", "--manifest", "path/to/Package.appxmanifest"]);

            cert.AddCommand<WindowCertificateRemoveCommand>("uninstall")
                .WithDescription("Remove a certificate by fingerprint")
                .WithExample(["windows", "cert", "uninstall", "--fingerprint", "ABCD1234..."]);
        });

        // App management commands
        windows.AddCommand<WindowsAppInstallCommand>("install")
            .WithDescription("Install an MSIX application package")
            .WithExample(["windows", "install", "--app", "path/to/app.msix"]);

        windows.AddCommand<WindowsAppUninstallCommand>("uninstall")
            .WithDescription("Uninstall an application")
            .WithExample(["windows", "uninstall", "--app", "path/to/app.msix"])
            .WithExample(["windows", "uninstall", "--identity", "MyApp"]);

        windows.AddCommand<WindowsAppLaunchCommand>("launch")
            .WithDescription("Launch an installed application")
            .WithExample(["windows", "launch", "--identity", "MyApp"])
            .WithExample(["windows", "launch", "--identity", "MyApp", "--args", "test-arguments"]);

        // Test command
        windows.AddCommand<WindowsTestCommand>("test")
            .WithDescription("Install and start a test application (supports both .msix and .exe files)")
            .WithExample(["windows", "test", "--app", "path/to/app.msix"])
            .WithExample(["windows", "test", "--app", "path/to/app.exe"])
            .WithExample(["windows", "test", "--app", "path/to/app.msix", "--port", "8080"])
            .WithExample(["windows", "test", "--app", "path/to/app.exe", "--connection-timeout", "60", "--data-timeout", "45"]);
    });

    // Android-specific commands
    config.AddBranch("android", android =>
    {
        android.SetDescription("Android-specific commands for emulator, app and test management");

        // App management commands
        android.AddCommand<AndroidAppInstallCommand>("install")
            .WithDescription("Install an APK application package")
            .WithExample(["android", "install", "--app", "path/to/app.apk"])
            .WithExample(["android", "install", "--app", "path/to/app.apk", "--device", "emulator-5554"]);

        android.AddCommand<AndroidAppUninstallCommand>("uninstall")
            .WithDescription("Uninstall an application")
            .WithExample(["android", "uninstall", "--app", "path/to/app.apk"])
            .WithExample(["android", "uninstall", "--package", "com.example.app"])
            .WithExample(["android", "uninstall", "--package", "com.example.app", "--device", "emulator-5554"]);

        android.AddCommand<AndroidAppLaunchCommand>("launch")
            .WithDescription("Launch an installed application")
            .WithExample(["android", "launch", "--package", "com.example.app"])
            .WithExample(["android", "launch", "--app", "path/to/app.apk"])
            .WithExample(["android", "launch", "--package", "com.example.app", "--activity", "com.example.app.MainActivity"])
            .WithExample(["android", "launch", "--package", "com.example.app", "--device", "emulator-5554"]);

        // Test command
        android.AddCommand<AndroidTestCommand>("test")
            .WithDescription("Install and start an Android test application")
            .WithExample(["android", "test", "--app", "path/to/app.apk"])
            .WithExample(["android", "test", "--app", "path/to/app.apk", "--device", "emulator-5554"])
            .WithExample(["android", "test", "--app", "path/to/app.apk", "--activity", "com.mypackage.MainActivity"])
            .WithExample(["android", "test", "--app", "path/to/app.apk", "--port", "8080"])
            .WithExample(["android", "test", "--app", "path/to/app.apk", "--results-directory", "test-results"])
            .WithExample(["android", "test", "--app", "path/to/app.apk", "--connection-timeout", "60", "--data-timeout", "45"]);
    });

    // macOS-specific commands
    config.AddBranch("macos", macos =>
    {
        macos.SetDescription("macOS-specific commands for app and test management");

        // App management commands
        macos.AddCommand<MacOSAppInstallCommand>("install")
            .WithDescription("Install a .app application bundle")
            .WithExample(["macos", "install", "--app", "path/to/app.app"]);

        macos.AddCommand<MacOSAppUninstallCommand>("uninstall")
            .WithDescription("Uninstall an application")
            .WithExample(["macos", "uninstall", "--app", "path/to/app.app"]);

        macos.AddCommand<MacOSAppLaunchCommand>("launch")
            .WithDescription("Launch an installed application")
            .WithExample(["macos", "launch", "--app", "path/to/app.app"])
            .WithExample(["macos", "launch", "--app", "path/to/app.app", "--args", "test-arguments"]);

        // Test command
        macos.AddCommand<MacOSTestCommand>("test")
            .WithDescription("Install and start a macOS test application")
            .WithExample(["macos", "test", "--app", "path/to/app.app"])
            .WithExample(["macos", "test", "--app", "path/to/app.app", "--port", "8080"])
            .WithExample(["macos", "test", "--app", "path/to/app.app", "--results-directory", "test-results"])
            .WithExample(["macos", "test", "--app", "path/to/app.app", "--connection-timeout", "60", "--data-timeout", "45"]);
    });

    // iOS-specific commands
    config.AddBranch("ios", ios =>
    {
        ios.SetDescription("iOS-specific commands for simulator, app and test management");

        // App management commands
        ios.AddCommand<iOSAppInstallCommand>("install")
            .WithDescription("Install a .app application bundle on iOS simulator")
            .WithExample(["ios", "install", "--app", "path/to/app.app"])
            .WithExample(["ios", "install", "--app", "path/to/app.app", "--device", "simulator-id"]);

        ios.AddCommand<iOSAppUninstallCommand>("uninstall")
            .WithDescription("Uninstall an application from iOS simulator")
            .WithExample(["ios", "uninstall", "--app", "path/to/app.app"])
            .WithExample(["ios", "uninstall", "--bundle-id", "com.example.app"])
            .WithExample(["ios", "uninstall", "--bundle-id", "com.example.app", "--device", "simulator-id"]);

        ios.AddCommand<iOSAppLaunchCommand>("launch")
            .WithDescription("Launch an installed application on iOS simulator")
            .WithExample(["ios", "launch", "--bundle-id", "com.example.app"])
            .WithExample(["ios", "launch", "--app", "path/to/app.app"])
            .WithExample(["ios", "launch", "--bundle-id", "com.example.app", "--device", "simulator-id"]);

        // Test command
        ios.AddCommand<iOSTestCommand>("test")
            .WithDescription("Install and start an iOS test application on simulator")
            .WithExample(["ios", "test", "--app", "path/to/app.app"])
            .WithExample(["ios", "test", "--app", "path/to/app.app", "--device", "simulator-id"])
            .WithExample(["ios", "test", "--app", "path/to/app.app", "--port", "8080"])
            .WithExample(["ios", "test", "--app", "path/to/app.app", "--results-directory", "test-results"])
            .WithExample(["ios", "test", "--app", "path/to/app.app", "--connection-timeout", "60", "--data-timeout", "45"]);
    });
});

return await app.RunAsync(args);