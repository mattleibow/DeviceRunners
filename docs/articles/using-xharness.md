# Using XHarness

XHarness is primarily a command line tool that enables running tests on Android, iOS, tvOS, Mac Catalyst, WASI and desktop browsers (WASM). See https://github.com/dotnet/xharness

> [!NOTE]
> XHarness is not available on nuget.org at this time, so an additional feed is required:  
> https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-eng/nuget/v3/index.json

In order to test with XHarness, you will have to install the CLI tool first:

```bash
dotnet tool install Microsoft.DotNet.XHarness.CLI \
  --global \
  --version "8.0.0-prerelease*" \
  --add-source https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-eng/nuget/v3/index.json
```

> [!TIP]
> The snippets above install the XHarness tool globally. You can also remove the `--global` argument to install locally in the current working directory. If installed locally, prefix XHarness commands with `dotnet`. For example, `xharness apple test` becomes `dotnet xharness apple test`.

## Platform-Specific Guides

- **[iOS - XHarness](cli-device-runner-for-ios-using-xharness.md)**
- **[Android - XHarness](cli-device-runner-for-android-using-xharness.md)**
- **[Mac Catalyst - XHarness](cli-device-runner-for-mac-catalyst-using-xharness.md)**
- **[Windows - PowerShell](cli-device-runner-for-windows-using-powershell.md)**

## See Also

- **[Using DeviceRunners CLI](using-devicerunners-cli.md)** - The modern recommended alternative to XHarness
- **[Preview Packages](preview-packages.md)** - Feed configuration for XHarness packages
