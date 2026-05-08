# Preview Packages

Preview packages can be found on the preview feed:

https://pkgs.dev.azure.com/mattleibow/OpenSource/_packaging/test-device-runners/nuget/v3/index.json

If you are also using XHarness, you will also need that feed:

https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-eng/nuget/v3/index.json

Your nuget.config should look something like this:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <!--To inherit the global NuGet package sources remove the <clear/> line below -->
    <clear />
    <add key="nuget" value="https://api.nuget.org/v3/index.json" />
    <add key="dotnet-eng" value="https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-eng/nuget/v3/index.json" />
    <add key="test-device-runners" value="https://pkgs.dev.azure.com/mattleibow/OpenSource/_packaging/test-device-runners/nuget/v3/index.json" />
  </packageSources>
  <packageSourceMapping>
    <packageSource key="dotnet-eng">
      <package pattern="Microsoft.DotNet.XHarness.*" />
    </packageSource>
    <packageSource key="test-device-runners">
      <package pattern="DeviceRunners.*" />
    </packageSource>
    <packageSource key="nuget">
      <package pattern="*" />
    </packageSource>
  </packageSourceMapping>
</configuration>
```

> [!NOTE]
> The DeviceRunners repository's own `nuget.config` does not include the preview feed because it builds the packages from source. You only need to add the preview feed when consuming the published packages in your own projects.