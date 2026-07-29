# WASM Browser Testing with DeviceRunners CLI


This guide covers testing Blazor WebAssembly applications in a headless browser using the DeviceRunners CLI tool.

## Running Tests

The DeviceRunners CLI tool provides a test workflow for WASM apps that publishes the app, serves it locally, launches headless Chrome via the Chrome DevTools Protocol (CDP), captures NDJSON console output, and produces a TRX results file.

1. Publish the Blazor WebAssembly test app:
   ```
   dotnet publish <path/to/app.csproj> -c Release
   ```

2. Run the tests:
   ```
   device-runners wasm test --app <path/to/wwwroot> --logger "trx;LogFileName=test-results.trx" --results-directory <path/to/output>
   ```

3. View test results in the output directory:
   ```
   <path/to/output>/test-results.trx
   ```

## Interactive Testing

For manual testing and debugging, use the `serve` command to host the app and open it in a browser yourself:

```bash
device-runners wasm serve --app <path/to/wwwroot> --port 5050
```

This starts a local web server without launching headless Chrome, so you can navigate to `http://localhost:5050` and interact with the visual test runner UI in a regular browser window.

## Complete Example

To build and test the app at the path `sample/test/DeviceTestingKitApp.BrowserTests/DeviceTestingKitApp.BrowserTests.csproj` and get the test output at the path `artifacts`:

```bash
# Publish the WASM test app
dotnet publish sample/test/DeviceTestingKitApp.BrowserTests/DeviceTestingKitApp.BrowserTests.csproj \
  -c Release

# Locate the published wwwroot
wwwroot=$(find artifacts/publish -type d -name "wwwroot" | head -1)

# Run tests in headless Chrome
device-runners wasm test \
  --app "$wwwroot" \
  --timeout 300 \
  --logger "trx;LogFileName=test-results.trx" \
  --results-directory artifacts/test-results

# Test result file will be: artifacts/test-results/test-results.trx
# Browser console log will be: artifacts/test-results/browser-console.log
```

## Command Options

### `wasm test`

| Option | Default | Description |
|--------|---------|-------------|
| `--app` | *(required)* | Path to the published WASM app directory (the `wwwroot` folder) |
| `--results-directory` | `artifacts` | Directory for test output files |
| `--logger` | *(none)* | Result format and file name (e.g. `trx;LogFileName=results.trx`) |
| `--timeout` | `300` | Test execution timeout in seconds |
| `--headed` | `false` | Run browser in visible mode (useful for debugging) |
| `--server-port` | `0` (auto) | HTTP port for the local web server |
| `--browser-args` | *(none)* | Extra switches for the browser, quoted as in a shell. Repeatable. Use the `=` form: `--browser-args=--enable-unsafe-webgpu` |
| `--output` | `default` | CLI output format: `default`, `json`, `xml`, or `text` |

The test command also generates a `browser-console.log` file in the results directory containing all browser console output, similar to `logcat.txt` on Android or `ios-device-log.txt` on iOS.

#### Passing switches to the browser

`--browser-args` takes a command-line fragment and splits it using shell quoting rules, so a
switch whose value contains a space just needs quotes:

```bash
device-runners wasm test --app "$wwwroot" \
  --browser-args='--enable-unsafe-webgpu "--host-resolver-rules=MAP a.test 127.0.0.1"'
```

It is repeatable, so this is equivalent:

```bash
device-runners wasm test --app "$wwwroot" \
  --browser-args=--enable-unsafe-swiftshader \
  --browser-args=--enable-unsafe-webgpu
```

Use the `=` form. Spectre.Console treats a following token that starts with `-` as another
option, so `--browser-args --enable-unsafe-webgpu` is rejected.

The switches are appended after everything the CLI sets itself, so they win for the switches
where Chrome keeps only the last occurrence. The full command line is printed as
`Browser args:` at the start of every run.

> [!NOTE]
> This configures the **browser**, not the app under test. The app is configured through the
> runner's own query-string parameters (`?device-runners-autorun=1`), the WASM equivalent of
> the `DEVICE_RUNNERS_*` environment variables used on the other platforms.

This is a passthrough — the CLI adds no graphics-related switches of its own. See
[WebGL and WebGPU](#webgl-and-webgpu) below for what those APIs need on a CI agent.

### `wasm serve`

| Option | Default | Description |
|--------|---------|-------------|
| `--app` | *(required)* | Path to the published WASM app directory |
| `--port` | `5000` | HTTP port for the web server |
| `--output` | `default` | CLI output format: `default`, `json`, `xml`, or `text` |

## Using `dotnet test`

If your project references `DeviceRunners.Testing.Targets`, you can run tests via `dotnet test`:

```bash
dotnet test sample/test/DeviceTestingKitApp.BrowserTests/DeviceTestingKitApp.BrowserTests.csproj
```

This automatically publishes the Blazor app, locates the `wwwroot`, launches headless Chrome, captures results, and reports them back to the `dotnet test` infrastructure. See [Browser (WASM) — dotnet test](dotnet-test-wasm.md) for details.

## Troubleshooting

### Chrome not found

The CLI searches for Chrome/Chromium in standard installation paths. If your Chrome binary is in a non-standard location, ensure it is on your `PATH`. On CI runners (Ubuntu, Windows, macOS), Chrome is typically pre-installed.

### Tests hang or timeout

If the app fails to boot, check the Blazor WebAssembly publish output to ensure the `wwwroot` directory contains `_framework/blazor.webassembly.js` and the app's DLLs. Use `--timeout` to increase the timeout for large test suites.

### Console output is empty

Ensure the test app calls `AddConsoleResultChannel()` in its `Program.cs`. When using `UseVisualTestRunner` on `WebAssemblyHostBuilder`, the CLI configuration is automatically injected — when the browser navigates to `?device-runners-autorun=1`, the `EventStreamFormatter` console output is enabled.

### WebGL and WebGPU

A browser on a machine with no usable GPU — most CI agents — reports WebGL as unavailable
(`canvas.getContext("webgl")` returns `null`) and WebGPU as adapter-less
(`navigator.gpu.requestAdapter()` resolves to `null`). Headless mode is not the cause: a
visible browser behaves the same. The cause is the missing GPU.

Chrome can fall back to the SwiftShader software renderer, but only when asked. Measured with
Chrome 150 on GPU-less GitHub-hosted runners:

| Host | For WebGL | For WebGPU |
| --- | --- | --- |
| Linux | *(works already)* | `--enable-unsafe-webgpu` |
| macOS | `--enable-unsafe-swiftshader` | `--enable-unsafe-webgpu` |
| Windows | *(works already, via D3D11 WARP)* | `--enable-unsafe-webgpu --use-webgpu-adapter=swiftshader` |

Notes:

- On **Windows**, `--enable-unsafe-swiftshader` moves ANGLE onto the SwiftShader Vulkan
  device, after which Dawn cannot enumerate any WebGPU adapter. Don't combine it with the
  WebGPU switches there.
- `--use-webgpu-adapter=swiftshader` forces software rendering even when a real GPU is
  present, so apply it on CI rather than everywhere.
- Avoid passing `--disable-gpu`. On Windows it removes the fallback WebGPU needs, and
  `requestAdapter()` returns `null` even with `--use-webgpu-adapter=swiftshader`.
- `navigator.gpu` only exists in a [secure context]. The built-in server uses
  `http://127.0.0.1`, which qualifies; a plain `http://` LAN address does not, and WebGPU
  silently disappears there.

[secure context]: https://developer.mozilla.org/docs/Web/Security/Secure_Contexts

### Debugging with headed mode

Use `--headed` to launch a visible browser window. This lets you see the Blazor visual runner UI and inspect the browser console directly. Combined with `--timeout 0` (infinite), this is useful for interactive debugging.

### Reviewing browser logs

After a test run, check `browser-console.log` in the results directory for the full browser console output. This includes framework messages, app startup logs, and any JavaScript errors that may have occurred.
