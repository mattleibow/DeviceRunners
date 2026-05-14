# CI Pipeline Configuration

This document describes the CI setup for DeviceRunners. There is a **single CI workflow** (GitHub Actions) and a **single pipeline** (Azure Pipelines), each using composite actions or templates for device test jobs. All device interaction uses the **dotnet CLI tools only** — no native `xcrun`, `simctl`, `adb`, or SDK tools are invoked directly.

## Tooling

All device and emulator management uses three dotnet CLI tools, configured in `.config/dotnet-tools.json`:

| Tool | NuGet Package | Command | Purpose |
|---|---|---|---|
| AndroidSdk.Tool | `androidsdk.tool` | `dotnet android` | Android SDK, AVD, emulator, device management |
| AppleDev.Tools | `appledev.tools` | `dotnet apple` | iOS/macOS simulator management |
| XHarness | `microsoft.dotnet.xharness.cli` | `dotnet xharness` | Cross-platform test execution |

Tools are restored via `dotnet tool restore` in the shared setup step. Versions are pinned in `.config/dotnet-tools.json`.

## CI Services

| Service | Entrypoint | Triggers |
|---|---|---|
| GitHub Actions | `.github/workflows/ci.yml` (single workflow) | PRs, pushes to `main`/`releases/**`, releases |
| Azure Pipelines | `.azure/azure-pipelines.yml` (single pipeline) | PRs, pushes to `main`/`releases/**` |

### Architecture

**Single workflow / pipeline** with multiple jobs organized as:
- **Build** — matrix of OS × configuration (Windows debug, Windows release, macOS debug)
- **Test** — matrix of OS (Windows, macOS) running unit tests
- **Device tests** — individual jobs using composite actions (GH) or templates (AzDO)

A **separate Release workflow** (GitHub Actions) handles pack and publish, triggered only on release events. This isolates secret access from the CI workflow.

**Azure Pipelines** has pack in its single pipeline (no publish — that's handled via GitHub releases).

Each device test is a **self-contained composite action or template** with its own parameters and defaults. Users can copy any single action/template to use as a reference or starting point.

**GitHub Actions**: The single `ci.yml` defines all jobs. Device test jobs call composite actions under `.github/workflows/test-*/action.yml`. Shared setup uses the composite action at `.github/workflows/setup-tools/action.yml`.

**Azure Pipelines**: The single `azure-pipelines.yml` defines build/test/pack stages inline and calls templates under `.azure/templates/test-*.yml` for device tests. Shared setup uses `.azure/templates/setup-tools.yml`.

## Shared Setup

Both CI services share equivalent setup logic:

| File | Purpose |
|---|---|
| `.github/workflows/setup-tools/action.yml` | GH Actions composite action |
| `.azure/templates/setup-tools.yml` | AzDO step template |

What it does:
1. Selects **Xcode 26.2** (macOS only)
2. Installs **.NET 9.0.x** and **10.0.x** SDKs (both needed for multi-targeting)
3. Installs **MAUI workloads** (`maui` on macOS/Windows, `maui-android` on Linux)
4. Runs `dotnet tool restore` (installs XHarness, AndroidSdk.Tool, AppleDev.Tools)

## File Reference

### GitHub Actions (`.github/workflows/`)

| File | Type | Description |
|---|---|---|
| `ci.yml` | **Workflow** | CI workflow with build, test, and all device test jobs |
| `release.yml` | **Workflow** | Release workflow with pack and publish (triggered on release only) |
| `setup-tools/action.yml` | Composite action | Shared setup (Xcode, .NET, MAUI, tools) |
| `validate-arch/action.yml` | Composite action | CPU architecture validation |
| `test-tcp-android-linux/action.yml` | Composite action | TCP Android tests on Linux |
| `test-tcp-ios/action.yml` | Composite action | TCP iOS simulator tests |
| `test-tcp-macos/action.yml` | Composite action | TCP Mac Catalyst tests |
| `test-tcp-windows/action.yml` | Composite action | TCP Windows (MSIX packaged) tests |
| `test-tcp-windows-loose/action.yml` | Composite action | TCP Windows (loose-file MSIX) tests |
| `test-tcp-windows-unpackaged/action.yml` | Composite action | TCP Windows (unpackaged EXE) tests |
| `test-xharness-android-linux/action.yml` | Composite action | XHarness Android tests on Linux |
| `test-xharness-ios/action.yml` | Composite action | XHarness iOS simulator tests |
| `test-xharness-maccatalyst/action.yml` | Composite action | XHarness Mac Catalyst tests |
| `test-xharness-windows/action.yml` | Composite action | XHarness Windows tests |
| `test-dotnet-test-android-linux/action.yml` | Composite action | `dotnet test` Android tests on Linux |
| `test-dotnet-test-ios/action.yml` | Composite action | `dotnet test` iOS simulator tests |
| `test-dotnet-test-macos/action.yml` | Composite action | `dotnet test` Mac Catalyst tests |
| `test-dotnet-test-windows/action.yml` | Composite action | `dotnet test` Windows (loose MSIX) tests |
| `test-dotnet-test-windows-exe/action.yml` | Composite action | `dotnet test` Windows (unpackaged EXE) tests |
| `test-wasm-browser/action.yml` | Composite action | WASM browser tests on Linux |

### Azure Pipelines (`.azure/`)

| File | Type | Description |
|---|---|---|
| `azure-pipelines.yml` | **Pipeline** | Single pipeline with build, test, pack (inline), and device test stages |
| `templates/setup-tools.yml` | Step template | Shared setup (Xcode, .NET, MAUI, tools) |
| `templates/validate-arch.yml` | Step template | CPU architecture validation |
| `templates/test-tcp-android.yml` | Job template | TCP Android tests (Linux) |
| `templates/test-tcp-ios.yml` | Job template | TCP iOS simulator tests |
| `templates/test-tcp-macos.yml` | Job template | TCP Mac Catalyst tests |
| `templates/test-tcp-windows.yml` | Job template | TCP Windows (MSIX packaged) tests |
| `templates/test-tcp-windows-loose.yml` | Job template | TCP Windows (loose-file MSIX) tests |
| `templates/test-tcp-windows-unpackaged.yml` | Job template | TCP Windows (unpackaged EXE) tests |
| `templates/test-xharness-android.yml` | Job template | XHarness Android tests (Linux) |
| `templates/test-xharness-ios.yml` | Job template | XHarness iOS simulator tests |
| `templates/test-xharness-maccatalyst.yml` | Job template | XHarness Mac Catalyst tests |
| `templates/test-xharness-windows.yml` | Job template | XHarness Windows tests |
| `templates/test-dotnet-test-android.yml` | Job template | `dotnet test` Android tests (Linux) |
| `templates/test-dotnet-test-ios.yml` | Job template | `dotnet test` iOS simulator tests |
| `templates/test-dotnet-test-macos.yml` | Job template | `dotnet test` Mac Catalyst tests |
| `templates/test-dotnet-test-windows.yml` | Job template | `dotnet test` Windows (loose MSIX) tests |
| `templates/test-dotnet-test-windows-exe.yml` | Job template | `dotnet test` Windows (unpackaged EXE) tests |
| `templates/test-wasm-browser.yml` | Job template | WASM browser tests on Linux |

## Supported Platform Matrix

### Android

| Host OS | Host Arch | API Level | Emulator Arch | Runner (GH) | Pool (Azure) | `dotnet test` | TCP (CLI) | XHarness | Status |
|---|---|---|---|---|---|---|---|---|---|
| Linux | x64 | 36 | x86_64 | ubuntu-24.04 | ubuntu-24.04 | ✅ | ✅ | ✅ | **Stable** — KVM hardware accel |
| macOS | arm64 | any | arm64-v8a | — | — | ❌ | ❌ | ❌ | **Blocked** — HVF not available on CI runners |

### iOS (Simulator)

| Host Arch | RID | Runner (GH) | Pool (Azure) | `dotnet test` | TCP (CLI) | XHarness | Status |
|---|---|---|---|---|---|---|---|
| x64 | iossimulator-x64 | macos-15-intel | macOS-15 | ✅ | ✅ | ✅ | **Stable** — Rosetta translation |

### Mac Catalyst

| Host Arch | RID | Config | Runner (GH) | Pool (Azure) | `dotnet test` | TCP (CLI) | XHarness | Status |
|---|---|---|---|---|---|---|---|---|
| x64 | maccatalyst-x64 | release | macos-15-intel | macOS-15 | ✅ | ✅ | ✅ | **Stable** |

### Windows

| Packaging | RID | Runner (GH) | Pool (Azure) | `dotnet test` | TCP (CLI) | XHarness | Status |
|---|---|---|---|---|---|---|---|
| MSIX (packaged) | win10-x64 | windows-2025 | windows-2025 | N/A | ✅ | ✅ | **Stable** |
| Loose MSIX (folder) | win10-x64 | windows-2025 | windows-2025 | ✅ | ✅ | N/A | **Stable** — requires Developer Mode |
| EXE (unpackaged) | win10-x64 | windows-2025 | windows-2025 | ✅ | ✅ | N/A | **Stable** — TCP only |

## Using `dotnet test` in CI

The `dotnet test` device test workflows (`test-dotnet-test-*`) use the `DeviceRunners.Testing.Targets` package to run tests via `dotnet test`. This is the recommended approach for new CI setups:

```yaml
# GitHub Actions example
- name: Run Device Tests
  run: |
    dotnet test sample/test/DeviceTestingKitApp.DeviceTests/DeviceTestingKitApp.DeviceTests.csproj \
      -f net10.0-maccatalyst \
      -c release
```

```yaml
# Azure Pipelines example
- script: |
    dotnet test sample/test/DeviceTestingKitApp.DeviceTests/DeviceTestingKitApp.DeviceTests.csproj \
      -f net10.0-android \
      -c release
  displayName: 'Run Android Device Tests'
```

The `DeviceRunners.Testing.Targets` package is included in the test project via `<PackageReference>`. When using in-repo development, the `.props` and `.targets` files are imported directly. For published packages, NuGet handles the import automatically.

> [!NOTE]
> The `dotnet test` workflows (`test-dotnet-test-*`) run tests via `dotnet test` and the `DeviceRunners.Testing.Targets` package. The TCP workflows (`test-tcp-*`) use the DeviceRunners CLI directly for scenarios requiring more control. The XHarness workflows remain as a legacy alternative.

## Device Management Patterns

### Android Emulator (via `dotnet android`)

```bash
# Install SDK packages
dotnet android sdk install --package 'platform-tools' --package 'emulator' \
  --package 'system-images;android-36;google_apis;x86_64'

# Create AVD
dotnet android avd create --name MyEmulator \
  --sdk 'system-images;android-36;google_apis;x86_64' --force

# Start emulator (--wait blocks until booted)
dotnet android avd start -p 5554 --name MyEmulator \
  --no-window --gpu swiftshader_indirect --no-snapshot --no-audio --no-boot-anim \
  --wait --no-animations \
  --cpu-threshold 3 --response-threshold 5

# List connected devices
dotnet android device list

# Capture logcat
dotnet android device logcat --output ./logcat.txt

# Delete AVD
dotnet android avd delete --name MyEmulator --force
```

Key flags for `avd start`:
- `--wait`: Block until the emulator is fully booted (default timeout is infinite)
- `--no-animations`: Disable window and transition animations (improves test reliability)
- `--cpu-threshold N`: Wait until CPU load drops below N% before returning
- `--response-threshold N`: Require N consecutive responsive checks under the CPU threshold

### iOS Simulator (via `dotnet apple`)

```bash
# Create simulator
RESULT=$(dotnet apple simulator create "MySimulator" --device-type "iPhone 16" --format json)
UDID=$(echo "$RESULT" | jq -r '.udid')

# Boot simulator (--wait blocks until ready, default timeout is sufficient)
dotnet apple simulator boot "$UDID" --wait

# Delete simulator
dotnet apple simulator delete --force "MySimulator"
```

### XHarness Test Execution

```bash
# Android
dotnet xharness android test \
  --timeout="00:10:00" \
  --launch-timeout=00:10:00 \
  --package-name com.example.myapp \
  --instrumentation myapp.XHarnessInstrumentation \
  --app path/to/app-Signed.apk \
  --output-directory artifacts/test-results \
  --verbosity=Debug

# iOS
dotnet xharness apple test \
  --target ios-simulator-64 \
  --device "$SIMULATOR_UDID" \
  --timeout="00:10:00" \
  --launch-timeout=00:10:00 \
  --app path/to/MyApp.app \
  --output-directory artifacts/test-results
```

> **Important**: XHarness `--verbosity` must use `=` syntax (`--verbosity=Debug`), not space-separated (`--verbosity Debug`), due to GNU-style optional value parsing.

## WASM Browser Tests

WASM browser tests run the Blazor WebAssembly test app in headless Chrome. Unlike other platforms, there is no device or emulator — only a published `wwwroot` directory and a browser.

### Flow

1. `dotnet pack` the DeviceRunners CLI and `dotnet tool install --global` it
2. `dotnet publish` the Blazor WASM test project
3. `device-runners wasm test --app <wwwroot>` — serves the app, launches headless Chrome, captures console NDJSON, writes TRX

### GitHub Actions

The `wasm-browser` job in `ci.yml` uses the `test-wasm-browser` composite action:

| File | Type | Description |
|---|---|---|
| `test-wasm-browser/action.yml` | Composite action | WASM browser tests on Linux |

```yaml
wasm-browser:
  name: WASM Browser (Linux)
  runs-on: ubuntu-24.04
  steps:
  - uses: actions/checkout@v4
  - uses: ./.github/workflows/test-wasm-browser
```

### Azure DevOps

The `WASM_Browser_Tests` stage uses the `test-wasm-browser.yml` template:

| File | Type | Description |
|---|---|---|
| `templates/test-wasm-browser.yml` | Job template | WASM browser tests on Linux |

```yaml
- stage: WASM_Browser_Tests
  displayName: 'WASM Browser Tests'
  dependsOn: []
  jobs:
    - template: templates/test-wasm-browser.yml
```

### Platform Matrix

| Host OS | Runner (GH) | Pool (Azure) | CLI | Status |
|---|---|---|---|---|
| Linux | ubuntu-24.04 | ubuntu-24.04 | ✅ | **Stable** — Chrome pre-installed |

## Runner Image Reference

**⚠️ GitHub Actions and Azure DevOps have OPPOSITE defaults for macOS!**

| Image Name | GitHub Actions | Azure DevOps |
|---|---|---|
| `macos-15` | **ARM64** (Apple Silicon) | **x64** (Intel) |
| `macos-15-intel` | x64 (Intel) | _(not available)_ |
| `macOS-15` | _(same as macos-15)_ | **x64** (Intel) |
| `macOS-15-arm64` | _(not available)_ | **ARM64** (Apple Silicon, preview) |
| `ubuntu-24.04` | x64 | x64 |
| `windows-2025` | x64 | x64 |

The `validate-arch` composite action / template runs as the first step in every device test job to catch architecture mismatches early.

## Caveats & Known Issues

### ARM64 Android Emulation

ARM64 Android emulation requires HVF. Neither GH Actions nor AzDO macOS ARM64 runners provide HVF access:
```
qemu-system-aarch64-headless: failed to initialize HVF: Invalid argument
```

### Android AVD Path on Ubuntu

Ubuntu 24.04 uses XDG convention (`~/.config/.android/avd/`) but the emulator expects `~/.android/avd/`. The workflows set `ANDROID_AVD_HOME=$HOME/.android/avd` explicitly.

### Azure MSBuild Variable Collision

Azure Pipelines matrix variable names become environment variables that MSBuild reads case-insensitively. Never name a matrix variable after an MSBuild property:
```yaml
# ❌ Wrong — MSBuild reads this as RuntimeIdentifier
runtimeIdentifier: android-x64

# ✅ Correct
testRid: android-x64
```

### Emulator GPU Modes

| Host | GPU Mode | Notes |
|---|---|---|
| Linux (KVM) | `swiftshader_indirect` | Software rendering, works with headless KVM |

## How to Expand the Matrix

### Add an API level to Android

Create a new composite action (GH) or add parameters in the template (AzDO):

```yaml
# GitHub Actions — new composite action or override inputs in ci.yml
# The composite actions accept inputs for emulator-image, gpu, cpu-threshold, etc.
# Example: create test-tcp-android-linux-api34/action.yml based on test-tcp-android-linux/

# Azure Pipelines — pass parameters to the template
- template: templates/test-tcp-android.yml
  parameters:
    emulatorImage: 'system-images;android-34;google_apis;x86_64'
    testRid: android-x64
    connectionTimeout: 120
    emulatorGpu: swiftshader_indirect
    cpuThreshold: 3
```

### Switch iOS from x64 to arm64

Update the runner and RID in `ci.yml` (GH) or `azure-pipelines.yml` (AzDO):

```yaml
# GitHub Actions — change the job's runs-on and composite action inputs
tcp-ios:
  name: TCP iOS (arm64)
  runs-on: macos-15          # ARM64
  steps:
  - uses: actions/checkout@v4
  - uses: ./.github/workflows/test-tcp-ios
    with:
      runtime-identifier: iossimulator-arm64

# Azure Pipelines — change pool and parameter
- template: templates/test-tcp-ios.yml
  parameters:
    poolImage: 'macOS-15-arm64'
    testRid: iossimulator-arm64
```
