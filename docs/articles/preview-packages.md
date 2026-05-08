Packages can be found on the preview feed:

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
</configuration>
```