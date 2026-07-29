# Browser (WASM) — `dotnet test`

> [!TIP]
> This guide covers WASM/Browser-specific details. See [Using dotnet test](using-dotnet-test.md) for general setup.

## Prerequisites

- **Chrome or Chromium** installed (pre-installed on most CI runners including `ubuntu-24.04`, `windows-2025`, and `macos-15`)
- The project must reference `DeviceRunners.Testing.Targets` (or import the `.props`/`.targets` files directly for in-repo usage)

## Running Tests

```bash
dotnet test MyApp.BrowserTests.csproj
```

> [!NOTE]
> Unlike other platforms, Blazor WebAssembly projects target plain `net9.0`/`net10.0` (no `-browser` TFM), so no `-f` flag is needed.

This automatically publishes the Blazor WebAssembly app, starts a local web server, launches headless Chrome, captures test results via the Chrome DevTools Protocol, and reports them back to the `dotnet test` infrastructure.

## How It Works

### End-to-End Flow

1. The CLI starts a local static file server hosting the published `wwwroot` directory
2. It launches headless Chrome (or Chromium) using the Chrome DevTools Protocol (CDP)
3. The browser navigates to the app URL with `?device-runners-autorun=1` appended
4. The Blazor app boots, detects the query parameter, and auto-starts all tests
5. Test events are emitted as NDJSON lines via `console.log` using the `EventStreamFormatter`
6. The CLI captures console output through the CDP connection and parses the NDJSON events
7. A TRX results file is written to the output directory

### Query String Configuration

When the CLI launches the browser, it navigates to:

```
http://localhost:<port>/?device-runners-autorun=1
```

The `device-runners-autorun=1` query parameter triggers headless mode. The app's `UseVisualTestRunner` extension method calls `AddCliConfiguration()` which reads the current URL via a `JSImport` interop call to `window.location.href` and parses the query string:

| Parameter | Value | Purpose |
|-----------|-------|---------|
| `device-runners-autorun` | `1` | Enables auto-start with auto-terminate; adds `EventStreamFormatter` console output |

When the parameter is absent (e.g., manual browser navigation), the interactive visual runner UI is shown instead.

### Why Console Output Instead of TCP

On native platforms (Android, iOS, macOS, Windows), the CLI communicates with the test app over a TCP socket. In the browser, opening a TCP connection from WebAssembly is not possible. Instead, the WASM runner writes NDJSON events to `console.log` and the CLI captures them via the Chrome DevTools Protocol `Runtime.consoleAPICalled` event. This provides the same structured event stream without requiring network access from the browser sandbox.

### Reflection-Based Discovery

WASM apps use `AddXunit(useReflection: true)` which registers the `XunitReflectionTestDiscoverer` instead of the default `XunitFrontController`-based discoverer. The `XunitFrontController` requires filesystem access to locate test assemblies, which is not available in the browser. The reflection-based discoverer scans assemblies already loaded in memory.

### Cooperative Yielding

Blazor WebAssembly runs on a single thread. To keep the UI responsive during test execution, the xunit runners use cooperative yielding (`XunitYieldingAssemblyRunner`, `XunitYieldingCollectionRunner`, `XunitYieldingClassRunner`) which call `Task.Yield()` between test classes and collections to give the browser event loop a chance to process rendering updates.

## Configuration

To change the test execution timeout, set `DeviceRunnersWasmTimeout` (default: 300 seconds):

```bash
dotnet test MyApp.BrowserTests.csproj -p:DeviceRunnersWasmTimeout=600
```

### Passing switches to the browser

`DeviceRunnersWasmBrowserArgs` sets extra switches on the browser the CLI launches:

```bash
dotnet test MyApp.BrowserTests.csproj -p:DeviceRunnersWasmBrowserArgs=--enable-unsafe-webgpu
```

```xml
<PropertyGroup>
  <DeviceRunnersWasmBrowserArgs>--enable-unsafe-swiftshader --enable-unsafe-webgpu</DeviceRunnersWasmBrowserArgs>
</PropertyGroup>
```

The value is a command-line fragment, not an MSBuild list. Separate switches with spaces and
quote any value that contains one, exactly as you would in a shell:

```xml
<PropertyGroup>
  <DeviceRunnersWasmBrowserArgs>--enable-unsafe-webgpu "--host-resolver-rules=MAP a.test 127.0.0.1"</DeviceRunnersWasmBrowserArgs>
</PropertyGroup>
```

The switches are appended after everything the CLI sets itself, so they win for the switches
where Chrome keeps only the last occurrence. The full command line is printed as `Browser
args:` at the start of every run.

> [!NOTE]
> This configures the **browser**, not the app under test. The app is configured through the
> runner's own query-string parameters (`?device-runners-autorun=1`), which is the WASM
> equivalent of the `DEVICE_RUNNERS_*` environment variables used on the other platforms.

This is a passthrough — the CLI adds no graphics-related switches of its own. See
[WebGL and WebGPU](#webgl-and-webgpu) for the switches those APIs need on a CI agent.

## Troubleshooting

### Chrome not found

The CLI searches for Chrome/Chromium in standard installation paths. If your Chrome binary is in a non-standard location, ensure it is on your `PATH`. On CI runners (Ubuntu, Windows, macOS), Chrome is typically pre-installed.

### Tests hang or timeout

If the app fails to boot, check the Blazor WebAssembly publish output to ensure the `wwwroot` directory contains `_framework/blazor.webassembly.js` and the app's DLLs. The default timeout is 300 seconds — use `--timeout` to increase it for large test suites.

### WebGL and WebGPU

A browser on a machine with no usable GPU — which describes most CI agents — reports WebGL as
unavailable (`canvas.getContext("webgl")` returns `null`) and WebGPU as adapter-less
(`navigator.gpu.requestAdapter()` resolves to `null`). This is not caused by headless mode: a
visible browser behaves the same way. It is caused by there being no GPU.

Chrome can fall back to the SwiftShader software renderer, but only when asked. Measured with
Chrome 150 on GPU-less GitHub-hosted runners:

| Host | For WebGL | For WebGPU |
| --- | --- | --- |
| Linux | *(works already)* | `--enable-unsafe-webgpu` |
| macOS | `--enable-unsafe-swiftshader` | `--enable-unsafe-webgpu` |
| Windows | *(works already, via D3D11 WARP)* | `--enable-unsafe-webgpu --use-webgpu-adapter=swiftshader` |

For example, on a GPU-less Linux or macOS agent:

```bash
dotnet test MyApp.BrowserTests.csproj \
  -p:DeviceRunnersWasmBrowserArgs="--enable-unsafe-swiftshader --enable-unsafe-webgpu"
```

Notes:

- On **Windows**, `--enable-unsafe-swiftshader` moves ANGLE onto the SwiftShader Vulkan
  device, after which Dawn cannot enumerate any WebGPU adapter. Don't combine it with the
  WebGPU switches there.
- `--use-webgpu-adapter=swiftshader` forces software rendering even when a real GPU is
  present, so apply it on CI rather than in a checked-in property.
- Avoid `--disable-gpu`. On Windows it removes the fallback WebGPU needs, and
  `requestAdapter()` returns `null` even with `--use-webgpu-adapter=swiftshader`.
- `navigator.gpu` only exists in a [secure context]. The built-in server uses
  `http://127.0.0.1`, which qualifies; a plain `http://` LAN address does not, and WebGPU
  silently disappears there.

[secure context]: https://developer.mozilla.org/docs/Web/Security/Secure_Contexts

### Console output is empty

When using `UseVisualTestRunner` on `WebAssemblyHostBuilder`, CLI configuration is automatically injected — the `EventStreamFormatter` console output is enabled when `?device-runners-autorun=1` is in the URL. If you're building a custom setup, ensure `AddConsoleResultChannel()` is called.

## See Also

- [WASM CLI Testing](cli-device-runner-for-wasm-using-devicerunners-cli.md)
