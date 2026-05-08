
# Welcome to the DeviceRunners Documentation!

We are glad you are here and one step closer to learning more about this library and the ways you can contribute 🙌

Use the table of contents to navigate and find answers to your questions. You can submit an issue to suggest changes, introduce features, file bug reports, or leave a DeviceRunners related question. You can also submit a pull request and contribute to the project.

## Getting Started

DeviceRunners provides multiple ways to run tests for .NET MAUI applications:

### `dotnet test` (Recommended)
The simplest way to run device tests. Add a NuGet package, run `dotnet test`, and get TRX results. Works with standard CI/CD pipelines out of the box.

- **[Using dotnet test](using-dotnet-test.md)** - Setup, configuration, and usage
- Platform guides: [Android](dotnet-test-android.md) | [iOS](dotnet-test-ios.md) | [macOS Catalyst](dotnet-test-macos.md) | [Windows](dotnet-test-windows.md)

### Visual Test Runners
Interactive test execution within the IDE for development and debugging. Perfect for manual testing and development workflows.

- **[Visual Runner in the IDE](visual-runner-in-the-ide.md)** - Run tests interactively within your development environment

### CLI Test Runners
Automated command-line test execution for advanced scenarios requiring fine-grained control over deployment and execution.

#### DeviceRunners CLI (Advanced)
The CLI tool that `dotnet test` uses under the hood. Use it directly when you need more control:
- **[Using DeviceRunners CLI](using-devicerunners-cli.md)** - Overview and installation
- **[Android CLI Testing](cli-device-runner-for-android-using-devicerunners-cli.md)** - Android APK testing
- **[Windows CLI Testing](cli-device-runner-for-windows-using-devicerunners-cli.md)** - Windows MSIX and EXE testing
- **[macOS CLI Testing](cli-device-runner-for-macos-using-devicerunners-cli.md)** - Mac Catalyst testing
- **[iOS CLI Testing](cli-device-runner-for-ios-using-devicerunners-cli.md)** - iOS Simulator testing

#### XHarness (Legacy)
Platform-specific tools for specialized scenarios:
- **[Using XHarness](using-xharness.md)** - Legacy XHarness-based testing
- Platform-specific XHarness guides for iOS, Android, Mac Catalyst, and Windows

### Architecture and Development
- **[Technical Architecture Overview](technical-architecture-overview.md)** - Comprehensive technical documentation
- **[Types of Tests](types-of-tests.md)** - Understanding different testing approaches
- **[CLI Test Workflow](devicerunners-cli-test-workflow.md)** - Detailed workflow for CLI test execution

### Helper Guides
- **[Managing Android Emulators](managing-android-emulators.md)** - Set up and manage Android emulators
- **[Preview Packages](preview-packages.md)** - Using pre-release NuGet packages
- **[CI Pipeline Configuration](ci-pipeline.md)** - GitHub Actions and Azure Pipelines setup

## Feedback and Requests

Please use [GitHub Issues](https://github.com/mattleibow/DeviceRunners/issues) for bug reports and feature requests. You can also chat with community members about the project in our [Community Discord](https://aka.ms/dotnet-discord). There is also the [GitHub discussions](https://github.com/mattleibow/DeviceRunners/discussions) for topics relating to the use and development of this library.
