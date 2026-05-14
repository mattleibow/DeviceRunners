# Browser (WASM) — `dotnet test`

> [!TIP]
> This guide covers WASM/Browser-specific details. See [Using dotnet test](using-dotnet-test.md) for general setup.

## Prerequisites

- **Chrome or Chromium** installed (pre-installed on most CI runners including `ubuntu-24.04`, `windows-2025`, and `macos-15`)

## Current Status

> [!NOTE]
> Full `dotnet test` integration for WASM is a stretch goal and is not yet supported. For now, use the DeviceRunners CLI directly. See [WASM CLI Testing](cli-device-runner-for-wasm-using-devicerunners-cli.md) for the recommended workflow.

## Running Tests via CLI

```bash
# Publish the Blazor WebAssembly app
dotnet publish MyApp.BrowserTests.csproj -c Release

# Locate the wwwroot output
wwwroot=$(find artifacts/publish -type d -name "wwwroot" | head -1)

# Run tests
device-runners wasm test --app "$wwwroot" --results-directory artifacts/test-results
```

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

## Troubleshooting

### Chrome not found

The CLI searches for Chrome/Chromium in standard installation paths. If your Chrome binary is in a non-standard location, ensure it is on your `PATH`. On CI runners (Ubuntu, Windows, macOS), Chrome is typically pre-installed.

### Tests hang or timeout

If the app fails to boot, check the Blazor WebAssembly publish output to ensure the `wwwroot` directory contains `_framework/blazor.webassembly.js` and the app's DLLs. The default timeout is 300 seconds — use `--timeout` to increase it for large test suites.

### Console output is empty

Ensure the test app calls `AddConsoleResultChannel()` or uses `AddCliConfiguration()` (which adds a console channel with `EventStreamFormatter` automatically when `device-runners-autorun=1` is detected).
