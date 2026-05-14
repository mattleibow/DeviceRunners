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
   device-runners wasm test --app <path/to/wwwroot> --results-directory <path/to/output>
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
```
