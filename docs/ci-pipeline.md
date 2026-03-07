# CI Pipeline Configuration

This document describes the CI setup for DeviceRunners. Each test job has its own workflow file (GitHub Actions) or template file (Azure Pipelines). All device interaction uses the **dotnet CLI tools only** — no native `xcrun`, `simctl`, `adb`, or SDK tools are invoked directly.

## Tooling

All device and emulator management uses three dotnet CLI tools, configured in `.config/dotnet-tools.json`:

| Tool | NuGet Package | Command | Purpose |
|---|---|---|---|
| AndroidSdk.Tool | `androidsdk.tool` 0.34.0 | `dotnet android` | Android SDK, AVD, emulator, device management |
| AppleDev.Tools | `appledev.tools` 0.8.7 | `dotnet apple` | iOS/macOS simulator management |
| XHarness | `microsoft.dotnet.xharness.cli` 11.0.0-prerelease.26107.1 | `dotnet xharness` | Cross-platform test execution |

Tools are restored via `dotnet tool restore` in the shared setup step.

## CI Services

| Service | Entrypoint | Triggers |
|---|---|---|
| GitHub Actions | Individual `.github/workflows/*.yml` files | PRs, pushes to `main`/`releases/**` |
| Azure Pipelines | `.azure/azure-pipelines.yml` (orchestrator) | PRs, pushes to `main`/`releases/**` |

### Architecture

Each test job is a **self-contained file** with its own parameters and defaults. Users can copy any single file to use as a reference or starting point.

**GitHub Actions**: Each workflow file (e.g., `test-tcp-android.yml`) is completely standalone with its own `on:` triggers, `concurrency:` group, and job definition. Shared setup uses the composite action at `.github/workflows/setup-tools/`.

**Azure Pipelines**: Each template file (e.g., `.azure/templates/test-tcp-android.yml`) defines a reusable job with `parameters:` and defaults. The thin orchestrator at `.azure/azure-pipelines.yml` calls each template.

## Shared Setup

Both CI services share the same setup logic via a reusable action/template:

| File | Purpose |
|---|---|
| `.github/workflows/setup-tools/action.yml` | GH Actions composite action |
| `.azure/templates/setup-tools.yml` | AzDO step template |

What it does:
1. Selects **Xcode 26.2** (macOS only)
2. Installs **.NET 9.0.x** and **10.0.x** SDKs (both needed for multi-targeting)
3. Installs **MAUI workloads** (`maui` on macOS/Windows, `maui-android` on Linux)
4. Runs `dotnet tool restore` (installs XHarness, AndroidSdk.Tool, AppleDev.Tools)

## Workflow Files

### GitHub Actions (`.github/workflows/`)

| Workflow File | Description |
|---|---|
| `build.yml` | Build on Windows (debug + release) and macOS (debug) |
| `test.yml` | Unit tests on Windows and macOS |
| `pack.yml` | Pack NuGet packages |
| `test-tcp-android.yml` | TCP Android tests (Linux + macOS matrix) |
| `test-tcp-ios.yml` | TCP iOS simulator tests |
| `test-tcp-macos.yml` | TCP Mac Catalyst tests |
| `test-tcp-windows.yml` | TCP Windows (MSIX packaged) tests |
| `test-tcp-windows-unpackaged.yml` | TCP Windows (unpackaged EXE) tests |
| `test-xharness-android.yml` | XHarness Android tests (Linux + macOS matrix) |
| `test-xharness-ios.yml` | XHarness iOS simulator tests |
| `test-xharness-maccatalyst.yml` | XHarness Mac Catalyst tests |
| `test-xharness-windows.yml` | XHarness Windows tests |

### Azure Pipelines (`.azure/templates/`)

| Template File | Description |
|---|---|
| `build.yml` | Build on Windows (debug + release) and macOS (debug) |
| `test.yml` | Unit tests on Windows and macOS |
| `pack.yml` | Pack NuGet packages |
| `test-tcp-android.yml` | TCP Android tests (Linux + macOS matrix) |
| `test-tcp-ios.yml` | TCP iOS simulator tests |
| `test-tcp-macos.yml` | TCP Mac Catalyst tests |
| `test-tcp-windows.yml` | TCP Windows (MSIX packaged) tests |
| `test-tcp-windows-unpackaged.yml` | TCP Windows (unpackaged EXE) tests |
| `test-xharness-android.yml` | XHarness Android tests (Linux + macOS matrix) |
| `test-xharness-ios.yml` | XHarness iOS simulator tests |
| `test-xharness-maccatalyst.yml` | XHarness Mac Catalyst tests |
| `test-xharness-windows.yml` | XHarness Windows tests |
| `setup-tools.yml` | Shared setup (Xcode, .NET, MAUI, tools) |
| `validate-arch.yml` | CPU architecture validation |

## Supported Platform Matrix

### Android

| Host OS | Host Arch | API Level | Emulator Arch | Runner (GH) | Pool (Azure) | TCP | XHarness | Status |
|---|---|---|---|---|---|---|---|---|
| Linux | x64 | 36 | x86_64 | ubuntu-24.04 | ubuntu-24.04 | ✅ | ✅ | **Stable** — KVM hardware accel |
| macOS | x64 | 36 | x86_64 | macos-15-intel | macOS-15 | ✅ | ✅ | **Stable** — software emulation, slower |
| macOS | arm64 | any | arm64-v8a | macos-15 | macOS-15-arm64 | ❌ | ❌ | **Blocked** — HVF not available on CI runners |

### iOS (Simulator)

| Host Arch | RID | Runner (GH) | Pool (Azure) | TCP | XHarness | Status |
|---|---|---|---|---|---|---|
| x64 | iossimulator-x64 | macos-15-intel | macOS-15 | ✅ | ✅ | **Stable** — Rosetta translation |
| arm64 | iossimulator-arm64 | macos-15 | macOS-15-arm64 | ✅ | ✅ | **Stable** — native speed |

### Mac Catalyst

| Host Arch | RID | Config | Runner (GH) | Pool (Azure) | TCP | XHarness | Status |
|---|---|---|---|---|---|---|---|
| x64 | maccatalyst-x64 | release | macos-15-intel | macOS-15 | ✅ | ✅ | **Stable** |

### Windows

| Packaging | RID | Runner (GH) | Pool (Azure) | TCP | XHarness | Status |
|---|---|---|---|---|---|---|
| MSIX (packaged) | win10-x64 | windows-2025 | windows-2025 | ✅ | ✅ | **Stable** |
| EXE (unpackaged) | win10-x64 | windows-2025 | windows-2025 | ✅ | N/A | **Stable** — TCP only |

## Device Management Patterns

### Android Emulator (via `dotnet android`)

```bash
# Install SDK packages
dotnet android sdk install --package 'platform-tools' --package 'emulator' \
  --package 'system-images;android-36;google_apis;x86_64'

# Create AVD
dotnet android avd create --name MyEmulator \
  --sdk 'system-images;android-36;google_apis;x86_64' --force

# Start emulator with stability flags
dotnet android avd start -p 5554 --name MyEmulator \
  --no-window --gpu guest --no-snapshot --no-audio --no-boot-anim \
  --wait --no-animations \
  --cpu-threshold 25 --cores 2 --timeout 1800 --response-threshold 5

# List connected devices
dotnet android device list

# Capture logcat
dotnet android device logcat --output ./logcat.txt

# Delete AVD
dotnet android avd delete --name MyEmulator --force
```

Key flags for `avd start`:
- `--wait`: Block until the emulator is fully booted
- `--no-animations`: Disable window and transition animations (improves test reliability)
- `--cpu-threshold N`: Wait until CPU load drops below N% before returning
- `--response-threshold N`: Require N consecutive responsive checks under threshold
- `--timeout N`: Maximum seconds to wait for boot

### iOS Simulator (via `dotnet apple`)

```bash
# Create simulator
RESULT=$(dotnet apple simulator create "MySimulator" --device-type "iPhone 16" --format json)
UDID=$(echo "$RESULT" | jq -r '.udid')

# Boot simulator (blocks until ready)
dotnet apple simulator boot "$UDID" --wait --timeout 300

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

### Android macOS x64 Emulator Stability

The Android emulator on macOS x64 uses software emulation (no KVM/HVF), which is significantly slower. Mitigations:
- The `--response-threshold 5` and `--cpu-threshold` flags on `avd start` ensure the emulator is consistently responsive before proceeding
- XHarness tests use a 3-attempt retry loop with 60-second sleep between attempts
- TCP Android macOS uses a 900-second connection timeout (vs 120s on Linux)

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
| macOS (x64) | `guest` | Guest-side rendering, required for software emulation |

## How to Expand the Matrix

### Add an API level to Android

Add a matrix entry in `test-tcp-android.yml` or `test-xharness-android.yml`:

```yaml
# GitHub Actions
- name: Linux (API 34)
  os: ubuntu-24.04
  emulator-image: system-images;android-34;google_apis;x86_64
  runtime-identifier: android-x64
  connection-timeout: 120
  gpu: swiftshader_indirect
  cores: 2
  cpu-threshold: 3
  timeout-seconds: 1800
```

```yaml
# Azure Pipelines
- name: Linux_API34
  poolImage: 'ubuntu-24.04'
  emulatorImage: 'system-images;android-34;google_apis;x86_64'
  testRid: android-x64
  connectionTimeout: 120
  emulatorGpu: swiftshader_indirect
  emulatorCores: 2
  cpuThreshold: 3
  timeoutSeconds: 1800
```

### Switch iOS from x64 to arm64

```yaml
# Before (x64)
platform:
  - name: x64
    os: macos-15-intel
    runtime-identifier: iossimulator-x64

# After (arm64)
platform:
  - name: arm64
    os: macos-15
    runtime-identifier: iossimulator-arm64
```
