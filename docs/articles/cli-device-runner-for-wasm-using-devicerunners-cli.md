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
| `--connection-timeout` | `120` | Seconds to wait for the first browser message (the app booting and reporting). |
| `--data-timeout` | `30` | Inactivity timeout: seconds without any browser output before the run is considered stalled. Resets on every message, so a long healthy run keeps going. |
| `--headed` | `false` | Run browser in visible mode (useful for debugging) |
| `--server-port` | `0` (auto) | HTTP port for the local web server |
| `--output` | `default` | CLI output format: `default`, `json`, `xml`, or `text` |

> A run that hits either of these timeouts before completing is treated as a **crash and fails** (exit code 2) — it is never reported as a passing run.

The test command also generates a `browser-console.log` file in the results directory containing all browser console output, similar to `logcat.txt` on Android or `ios-device-log.txt` on iOS.

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

Runs are governed by two timeouts: `--connection-timeout` (waiting for the app to boot and send its first browser message) and `--data-timeout` (an inactivity timeout that resets on every browser message). A large but healthy suite will keep running as long as it keeps producing output, so you rarely need to raise anything. If a run stalls, it **fails** rather than silently passing — check `browser-console.log` for the last output before the stall.

If the app fails to boot, check the Blazor WebAssembly publish output to ensure the `wwwroot` directory contains `_framework/blazor.webassembly.js` and the app's DLLs.

### Console output is empty

Ensure the test app calls `AddConsoleResultChannel()` in its `Program.cs`. When using `UseVisualTestRunner` on `WebAssemblyHostBuilder`, the CLI configuration is automatically injected — when the browser navigates to `?device-runners-autorun=1`, the `EventStreamFormatter` console output is enabled.

### Debugging with headed mode

Use `--headed` to launch a visible browser window. This lets you see the Blazor visual runner UI and inspect the browser console directly. Bumping `--data-timeout` gives you more time between output before the run is considered stalled, which is useful for interactive debugging.

### Reviewing browser logs

After a test run, check `browser-console.log` in the results directory for the full browser console output. This includes framework messages, app startup logs, and any JavaScript errors that may have occurred.
