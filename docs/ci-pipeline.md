# CI Pipeline Configuration

This document describes the full CI matrix for DeviceRunners, including all supported platform/architecture/API level combinations and their current CI status. The active CI pipelines run a focused subset to keep CI fast; this document captures the full picture for future expansion.

## CI Services

| Service | Config File | Triggers |
|---|---|---|
| GitHub Actions | `.github/workflows/ci.yml` | PRs, pushes to `main`/`releases/**` |
| Azure Pipelines | `.azure/azure-pipelines.yml` | PRs, pushes to `main`/`releases/**` |

Both services run the same test matrix. They exclude each other's config files via path filters.

## Full Supported Platform Matrix

The "dream" matrix — every combination that _can_ work. Active CI runs a subset (see [Active CI Matrix](#active-ci-matrix) below).

### Android

| Host OS | Host Arch | API Level | Emulator Arch | Runner (GH) | Pool (Azure) | TCP | XHarness | Status |
|---|---|---|---|---|---|---|---|---|
| Linux | x64 | 36 | x86_64 | ubuntu-24.04 | ubuntu-24.04 | ✅ | ✅ | **Stable** — KVM hardware accel |
| Linux | x64 | 34 | x86_64 | ubuntu-24.04 | ubuntu-24.04 | ✅ | ✅ | **Stable** — KVM hardware accel |
| Linux | x64 | 24 | x86_64 | ubuntu-24.04 | ubuntu-24.04 | ✅ | ✅ | **Stable** — KVM hardware accel |
| macOS | x64 | 36 | x86_64 | macos-15-intel | macOS-15 | ✅ | ✅ | **Stable** — software emulation, slower boot (~24 min) |
| macOS | x64 | 34 | x86_64 | macos-15-intel | macOS-15 | ✅ | ✅ | **Stable** — software emulation (~20 min) |
| macOS | x64 | 24 | x86_64 | macos-15-intel | macOS-15 | ✅ | ✅ | **Stable** — software emulation (~12 min) |
| macOS | arm64 | 36 | arm64-v8a | macos-15 | macOS-15-arm64 | ❌ | ❌ | **Blocked** — HVF not available on CI runners |
| macOS | arm64 | 34 | arm64-v8a | macos-15 | macOS-15-arm64 | ❌ | ❌ | **Blocked** — HVF not available on CI runners |
| macOS | arm64 | 24 | arm64-v8a | macos-15 | macOS-15-arm64 | ❌ | ❌ | **Blocked** — HVF not available on CI runners |

### iOS (Simulator)

| Host Arch | RID | Runner (GH) | Pool (Azure) | TCP | XHarness | Status |
|---|---|---|---|---|---|---|
| x64 | iossimulator-x64 | macos-15-intel | macOS-15 | ✅ | ✅ | **Stable** — Rosetta translation, slower |
| arm64 | iossimulator-arm64 | macos-15 | macOS-15-arm64 | ✅ | ✅ | **Stable** — native speed |

### Mac Catalyst

| Host Arch | RID | Config | Runner (GH) | Pool (Azure) | TCP | XHarness | Status |
|---|---|---|---|---|---|---|---|
| x64 | maccatalyst-x64 | release | macos-15-intel | macOS-15 | ✅ | ✅ | **Stable** |
| arm64 | maccatalyst-arm64 | debug | macos-15 | macOS-15-arm64 | ✅ | ✅ | **Stable** — must use `debug` config (AOT crashes in .NET 10 preview) |

### Windows

| Packaging | RID | Runner (GH) | Pool (Azure) | TCP | XHarness | Status |
|---|---|---|---|---|---|---|
| MSIX (packaged) | win10-x64 | windows-2025 | windows-2025 | ✅ | ✅ | **Stable** |
| EXE (unpackaged) | win10-x64 | windows-2025 | windows-2025 | ✅ | N/A | **Stable** — TCP only |

## Active CI Matrix

The pipelines currently run x64-only with Android API 36 on Linux and macOS to keep CI fast (~15-25 min). To expand, uncomment or add matrix entries in the CI files.

### Build & Test

| Job | Runner (GH) | Pool (Azure) | Notes |
|---|---|---|---|
| Build Windows (debug) | windows-2025 | windows-2025 | |
| Build Windows (release) | windows-2025 | windows-2025 | |
| Build macOS (debug) | macos-15-intel | macOS-15 | |
| Unit Tests (Windows) | windows-2025 | windows-2025 | |
| Unit Tests (macOS) | macos-15-intel | macOS-15 | |
| Pack NuGets | windows-2025 | windows-2025 | |

### TCP Device Tests

| Job | Runner (GH) | Pool (Azure) | Key Config |
|---|---|---|---|
| TCP Android (Linux) | ubuntu-24.04 | ubuntu-24.04 | API 36, x86_64, KVM |
| TCP Android (macOS) | macos-15-intel | macOS-15 | API 36, x86_64, software emu |
| TCP iOS | macos-15-intel | macOS-15 | iossimulator-x64 |
| TCP macOS | macos-15-intel | macOS-15 | maccatalyst-x64, release |
| TCP Windows | windows-2025 | windows-2025 | MSIX packaged |
| TCP Windows (Unpackaged) | windows-2025 | windows-2025 | EXE unpackaged |

### XHarness Device Tests

| Job | Runner (GH) | Pool (Azure) | Key Config |
|---|---|---|---|
| XHarness Android (Linux) | ubuntu-24.04 | ubuntu-24.04 | API 36, x86_64, KVM |
| XHarness Android (macOS) | macos-15-intel | macOS-15 | API 36, x86_64, software emu |
| XHarness iOS | macos-15-intel | macOS-15 | iossimulator-x64 |
| XHarness Mac Catalyst | macos-15-intel | macOS-15 | maccatalyst-x64 |
| XHarness Windows | windows-2025 | windows-2025 | MSIX packaged |

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

The `validate-arch` composite action / template runs as the first step in every device test job to catch mismatches early.

## Caveats & Known Issues

### Android API 36 on macOS x64

API 36 boots and runs correctly on macOS x64 software emulation, but is **significantly slower** than lower API levels:

| API Level | Typical Job Duration (macOS x64) | Notes |
|---|---|---|
| 24 | ~12 min | Lightest image, fastest boot |
| 34 | ~20 min | Moderate |
| 36 | ~24 min | Heaviest image, most background services |

The system settling step (launcher wait + CPU load monitoring + 10s pause) is critical for API 36 reliability. Without it, `system_server` lock contention causes native crashes under instrumentation.

### ARM64 Android Emulation

ARM64 Android emulation requires hardware virtualization framework (HVF). Neither GitHub Actions nor Azure DevOps macOS ARM64 runners provide HVF access, so ARM64 Android emulation is **not possible on CI**. The error is:
```
qemu-system-aarch64-headless: failed to initialize HVF: Invalid argument
```

### Mac Catalyst ARM64 AOT

The .NET 10 preview AOT compiler crashes when building for `maccatalyst-arm64` in release mode:
```
Failed to AOT compile aot-instances.dll
```
**Workaround**: Use `debug` configuration for ARM64 Mac Catalyst. Alternatively, use `release` with `-p:RunAOTCompilation=false`.

### XHarness CLI Verbosity Flag

XHarness uses GNU-style optional value parsing. The `--verbosity` flag must use `=` syntax:
```bash
# ✅ Correct
dotnet xharness android test --verbosity=Debug

# ❌ Wrong — causes INVALID_ARGUMENTS (exit code 3)
dotnet xharness android test --verbosity Debug
```

### iOS Simulator SpringBoard Readiness

After `xcrun simctl bootstatus` reports the simulator as booted, SpringBoard may not be fully ready. XHarness can fail with `APP_LAUNCH_FAILURE` (exit code 83) if launched too early. The `wait-ios-simulator` template includes a SpringBoard readiness check with a 120s timeout and 10s settling delay.

### Azure MSBuild Variable Collision

Azure Pipelines matrix variable names become environment variables that MSBuild reads **case-insensitively**. Never name a matrix variable after an MSBuild property:
```yaml
# ❌ Wrong — MSBuild reads this as RuntimeIdentifier, breaking the build
runtimeIdentifier: android-x64

# ✅ Correct — use a different name
testRid: android-x64
```

### Emulator GPU Modes

| Host | GPU Mode | Notes |
|---|---|---|
| Linux (KVM) | `swiftshader_indirect` | Software rendering, works with headless KVM |
| macOS (x64) | `guest` | Guest-side rendering, required for software emulation |
| macOS (arm64) | `host` | Would use host GPU — but HVF is not available |

### Android AVD Path

On Ubuntu 24.04 CI runners, the AVD path follows XDG convention: `~/.config/.android/avd/` instead of `~/.android/avd/`. The emulator setup templates set `ANDROID_AVD_HOME=$HOME/.android/avd` explicitly to avoid this.

## Reusable Templates

Both CI services use the same set of reusable templates for device setup/teardown:

### GitHub Actions (Composite Actions)

| Action | Purpose |
|---|---|
| `.github/workflows/setup-tools/` | .NET 10, Xcode 26.2, MAUI workloads |
| `.github/workflows/validate-arch/` | CPU architecture validation |
| `.github/workflows/setup-android-emulator/` | SDK install, AVD create, emulator launch |
| `.github/workflows/wait-android-emulator/` | Boot wait, system settling, post-boot setup |
| `.github/workflows/teardown-android-emulator/` | Log collection, emulator shutdown |
| `.github/workflows/setup-ios-simulator/` | Runtime resolution, simulator create & boot |
| `.github/workflows/wait-ios-simulator/` | Boot wait, SpringBoard readiness check |
| `.github/workflows/teardown-ios-simulator/` | Log collection, simulator shutdown & delete |

### Azure Pipelines (Step Templates)

| Template | Purpose |
|---|---|
| `.azure/templates/setup-tools.yml` | .NET 10, Xcode 26.2, MAUI workloads |
| `.azure/templates/validate-arch.yml` | CPU architecture validation |
| `.azure/templates/setup-android-emulator.yml` | SDK install, AVD create, emulator launch |
| `.azure/templates/wait-android-emulator.yml` | Boot wait, system settling, post-boot setup |
| `.azure/templates/teardown-android-emulator.yml` | Log collection, emulator shutdown |
| `.azure/templates/setup-ios-simulator.yml` | Runtime resolution, simulator create & boot |
| `.azure/templates/wait-ios-simulator.yml` | Boot wait, SpringBoard readiness check |
| `.azure/templates/teardown-ios-simulator.yml` | Log collection, simulator shutdown & delete |

## How to Expand the Matrix

### Add an API level to Android

In the CI file, add another matrix entry to both `tcp-android` and `xharness-android` jobs:
```yaml
# GitHub Actions example
- name: Linux (API 34)
  os: ubuntu-24.04
  gpu: swiftshader_indirect
  emulator-image: system-images;android-34;google_apis;x86_64
  runtime-identifier: android-x64
  connection-timeout: 120
```

### Switch a job from x64 to arm64

Change the single matrix entry for that job:
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

### Enable ARM64 Android (when HVF becomes available)

Uncomment the ARM64 entries in the Android job matrices:
```yaml
- name: macOS (arm64, API 36)
  os: macos-15
  gpu: host
  emulator-image: system-images;android-36;google_apis;arm64-v8a
  runtime-identifier: android-arm64
```
