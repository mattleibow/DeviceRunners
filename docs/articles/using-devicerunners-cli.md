# Using DeviceRunners CLI


The DeviceRunners CLI is a cross-platform .NET tool that provides comprehensive testing utilities for .NET MAUI applications across multiple platforms. It provides a consistent interface for installing, launching, and testing applications while handling platform-specific complexities automatically.

## Overview

The DeviceRunners CLI tool streamlines the testing process for .NET MAUI applications with:

- **Cross-Platform Support**: Works on Windows and macOS (Android and WASM commands also work on Linux)
- **Multiple Output Formats**: Human-readable, JSON, XML, and text formats
- **Automatic Resource Management**: Handles installation, cleanup, and certificates
- **TCP Result Streaming**: Real-time test result communication
- **Comprehensive Error Handling**: Clear error messages and validation
- **CI/CD Friendly**: Designed for automation and continuous integration

## Supported Platforms

| Platform | Commands Available | App Types Supported |
|----------|-------------------|-------------------|
| **Windows** | `install`, `uninstall`, `launch`, `test`, `cert` | MSIX packages, EXE files |
| **Android** | `install`, `uninstall`, `launch`, `test` | APK packages |
| **macOS** | `install`, `uninstall`, `launch`, `test` | .app bundles (Mac Catalyst) |
| **iOS** | `install`, `uninstall`, `launch`, `test` | .app bundles (Simulator) |
| **WASM** | `test`, `serve` | Published Blazor WebAssembly apps |

## Output Formats

All commands support global options for output formatting and automation:

### Available Formats

- **Default**: Rich, colored console output for human users
- **`--output json`**: Structured JSON for automation and scripting
- **`--output xml`**: XML format for CI/CD integration
- **`--output text`**: Simple key=value pairs for basic automation

### Examples

```bash
# Human-readable output with colors and progress
device-runners windows test --app app.msix

# JSON output for automation (suppresses verbose logs)
device-runners windows test --app app.msix --output json

# XML output for CI/CD systems
device-runners android test --app app.apk --output xml
```

## Platform-Specific Documentation

For detailed platform-specific instructions, see:

- **[Android - DeviceRunners CLI](cli-device-runner-for-android-using-devicerunners-cli.md)** - Android APK testing
- **[Windows - DeviceRunners CLI](cli-device-runner-for-windows-using-devicerunners-cli.md)** - Windows MSIX and EXE testing  
- **[macOS - DeviceRunners CLI](cli-device-runner-for-macos-using-devicerunners-cli.md)** - Mac Catalyst .app testing
- **[iOS - DeviceRunners CLI](cli-device-runner-for-ios-using-devicerunners-cli.md)** - iOS Simulator .app testing
- **[WASM - DeviceRunners CLI](cli-device-runner-for-wasm-using-devicerunners-cli.md)** - Blazor WebAssembly browser testing

## Common Command Patterns

### Testing Applications

The `test` command is the primary interface for running tests across all platforms:

```bash
# Windows (MSIX)
device-runners windows test --app path/to/app.msix --results-directory results

# Windows (EXE)  
device-runners windows test --app path/to/app.exe --results-directory results

# Android
device-runners android test --app path/to/app.apk --results-directory results

# macOS
device-runners macos test --app path/to/app.app --results-directory results

# iOS
device-runners ios test --app path/to/app.app --results-directory results

# WASM
device-runners wasm test --app path/to/wwwroot --logger "trx;LogFileName=test-results.trx" --results-directory results
```

### Filtering Tests

By default the `test` command runs every test in the app. You can run a subset in two
mutually-exclusive ways: a single `--filter` expression, or the Microsoft Testing Platform
"simple" filter switches. The chosen filter is forwarded to the device and evaluated
on-device, so it works the same across all frameworks (xUnit v2, xUnit v3 and NUnit) and
platforms.

> [!NOTE]
> Filtering is delivered to the app at launch time. On **Android** the filter is baked in
> at build time (via the `dotnet test` MSBuild integration), so the `device-runners android
> test` command launches an already-installed app and does not re-apply these options — use
> the `dotnet test --filter` path for Android filtering.

#### `--filter` (VSTest-style expression)

A single `dotnet test --filter` style expression:

```bash
# A single test
device-runners windows test --app app.msix \
  --filter "FullyQualifiedName=MyApp.Tests.CalculatorTests.Adds"

# Everything in a class, using a wildcard
device-runners windows test --app app.msix \
  --filter "ClassName=*CalculatorTests"
```

The supported properties are `FullyQualifiedName` (default), `DisplayName`, `Name`,
`ClassName`, `Namespace` and any trait name. Operators are `=`, `!=`, `~` (contains) and
`!~`, combined with `&`/`|` and parentheses. The `=`/`!=` operators support `*` wildcards.
See [Using `dotnet test`](using-dotnet-test.md#filtering-tests) for the full grammar.

#### Microsoft Testing Platform simple filters

These typed switches mirror xUnit v3's Microsoft Testing Platform filter options. Each is
**repeatable** and supports `*` wildcards:

| Option | Runs / excludes |
|--------|-----------------|
| `--filter-class <name>` | Tests in the given class |
| `--filter-not-class <name>` | Excludes tests in the given class |
| `--filter-method <name>` | The given method (fully qualified) |
| `--filter-not-method <name>` | Excludes the given method |
| `--filter-namespace <name>` | Tests in the given namespace |
| `--filter-not-namespace <name>` | Excludes tests in the given namespace |
| `--filter-trait <name=value>` | Tests with the given trait value |
| `--filter-not-trait <name=value>` | Excludes tests with the given trait value |

```bash
# Every test in two classes (repeat the switch)
device-runners windows test --app app.msix \
  --filter-class MyApp.Tests.CalculatorTests \
  --filter-class MyApp.Tests.ParserTests

# Every "Smoke" test, but not the slow ones, using a class wildcard
device-runners windows test --app app.msix \
  --filter-class Calc* \
  --filter-trait Category=Smoke \
  --filter-not-trait Category=Slow
```

Combining rules:

- **Same kind** → values OR together (`--filter-class A --filter-class B` runs class `A` **or** `B`).
- **Different kinds** → AND together (a class filter and a trait filter must both match).
- **`not-` variants** → exclude matching tests (AND-ed negations).
- `--filter` and the simple filters **cannot be combined** — the command fails with a clear
  error if you supply both.

### Application Management

Install, launch, and uninstall applications:

```bash
# Install applications
device-runners windows install --app app.msix
device-runners android install --app app.apk
device-runners macos install --app app.app
device-runners ios install --app app.app

# Launch applications
device-runners windows launch --identity "MyApp"
device-runners android launch --package com.example.app
device-runners macos launch --app app.app
device-runners ios launch --app app.app

# Uninstall applications
device-runners windows uninstall --identity "MyApp"
device-runners android uninstall --package com.example.app
device-runners macos uninstall --app app.app
device-runners ios uninstall --app app.app
```

> [!NOTE]
> WASM has no `install`, `launch`, or `uninstall` commands. Use `wasm test` to run tests and `wasm serve` for interactive browser testing.

## Network Configuration

All native platforms support TCP-based test result communication. WASM uses browser console output via Chrome DevTools Protocol instead — see [WASM CLI Testing](cli-device-runner-for-wasm-using-devicerunners-cli.md) for details.

### Default Configuration
- **Port**: 16384
- **Connection Timeout**: 120 seconds
- **Data Timeout**: 30 seconds

### Custom Configuration
```bash
device-runners [platform] test \
  --app path/to/app \
  --port 8080 \
  --connection-timeout 60 \
  --data-timeout 45
```

## TCP Port Listener

The CLI tool includes a standalone TCP port listener for receiving test results:

```bash
# Interactive mode (waits indefinitely)
device-runners listen --port 16384

# Non-interactive mode (with timeouts)
device-runners listen --port 16384 --non-interactive --connection-timeout 60 --data-timeout 30

# Save results to file
device-runners listen --port 16384 --results-file results.txt --non-interactive
```

The `test` command serves the published `wwwroot` directory, launches headless Chrome via CDP, navigates to `?device-runners-autorun=1`, captures console NDJSON events, and writes a TRX results file. Use `--headed` to see the browser window during test execution.

The `serve` command starts a local web server only, without launching Chrome. This is useful for opening the app manually in a browser to interact with the visual test runner UI.

For more details, see **[WASM CLI Testing](cli-device-runner-for-wasm-using-devicerunners-cli.md)**.

For more detailed information about the internal workflow and architecture:

- **[Technical Architecture Overview](technical-architecture-overview.md)** - Complete technical documentation
