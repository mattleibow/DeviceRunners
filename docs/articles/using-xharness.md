XHarness is primarily a command line tool that enables running tests on Android, iOS, tvOS, Mac Catalyst, WASI and desktop browsers (WASM). See https://github.com/dotnet/xharness

> XHarness is not available on nuget.org at this time, so an additional feed is required:  
> https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-eng/nuget/v3/index.json

In order to test with xharness, you will have to install the CLI tool first:

```
dotnet tool install Microsoft.DotNet.XHarness.CLI \
  --global \
  --version "8.0.0-prerelease*" \
  --add-source https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-eng/nuget/v3/index.json
```

> The snippets above are used to install the xharness tool globally, however you can also remove the `--global` argument to get the tool to install locally in the current working directory. If this is the case, then you will also need to prefix the xharness commands with `dotnet`. For example, if the sample commands below say `xharness apple test` you will need to do `dotnet xharness apple test`.
