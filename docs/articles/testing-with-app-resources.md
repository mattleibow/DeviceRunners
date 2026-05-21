# Testing with App Resources (Library Mode)

This guide explains how to create a **separate test app** that references your real application as a library. This lets you test custom controls, ViewModels, and services that depend on your app's styles, colors, and DI configuration — without mocking anything code-related.

## When to use this pattern

Use this approach when:

- Your custom controls use `{StaticResource}` references to app-level styles/colors
- You want to test ViewModels with the same DI services your app registers
- You need to instantiate pages and verify bindings work with real styles
- You want to avoid duplicating service registration logic between app and tests

## Overview

The pattern works by:

1. Building your app as a **library** (via an MSBuild property) so the test project can reference it
2. Using the visual test runner as the host application
3. Importing your app's `Colors.xaml` and `Styles.xaml` via `AddResourceDictionary<T>()`
4. Calling the same service registration your app uses

```
┌─────────────────────────────┐
│    Test App (Exe)           │
│  ┌───────────────────────┐  │
│  │ Visual Test Runner     │  │  ← Hosts the app, runs tests
│  │  + AddResourceDictionary│  │  ← Your styles/colors
│  │  + Your services       │  │  ← Same DI as your app
│  └───────────────────────┘  │
│  ┌───────────────────────┐  │
│  │ Your App (Library)     │  │  ← Referenced with IsTestLibrary=true
│  │  Pages, VMs, Controls  │  │
│  └───────────────────────┘  │
│  ┌───────────────────────┐  │
│  │ Test Classes (NUnit)   │  │  ← Your test code
│  └───────────────────────┘  │
└─────────────────────────────┘
```

## Step 1: Make your app buildable as a library

Add the following to your app's `.csproj`:

```xml
<PropertyGroup>
  <!-- When IsTestLibrary is set, build as a class library instead of an app -->
  <OutputType Condition="'$(IsTestLibrary)' != 'true'">Exe</OutputType>
  <OutputType Condition="'$(IsTestLibrary)' == 'true'">Library</OutputType>
</PropertyGroup>

<ItemGroup>
  <!-- Strip app resources that conflict with the test project's icons -->
  <MauiIcon Include="..." Condition="'$(IsTestLibrary)' != 'true'" />
  <MauiSplashScreen Include="..." Condition="'$(IsTestLibrary)' != 'true'" />
</ItemGroup>

<!-- Strip platform entry points when building as a library -->
<ItemGroup Condition="'$(IsTestLibrary)' == 'true'">
  <Compile Remove="Platforms\Android\MainApplication.cs" />
  <Compile Remove="Platforms\Android\MainActivity.cs" />
  <Compile Remove="Platforms\iOS\AppDelegate.cs" />
  <Compile Remove="Platforms\iOS\Program.cs" />
  <Compile Remove="Platforms\MacCatalyst\AppDelegate.cs" />
  <Compile Remove="Platforms\MacCatalyst\Program.cs" />
</ItemGroup>
```

> **Note:** Images, fonts, and raw assets do *not* need the condition — they flow through fine as library content.

## Step 2: Add `x:Class` to your resource dictionaries

Your `Colors.xaml` and `Styles.xaml` need to be instantiable as C# types. Add an `x:Class` attribute and a codebehind:

**Colors.xaml:**
```xml
<?xml version="1.0" encoding="utf-8" ?>
<ResourceDictionary xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
                    x:Class="MyApp.Resources.Styles.Colors">
    <!-- your colors here -->
</ResourceDictionary>
```

**Colors.xaml.cs:**
```csharp
namespace MyApp.Resources.Styles;

public partial class Colors : ResourceDictionary
{
    public Colors()
    {
        InitializeComponent();
    }
}
```

Do the same for `Styles.xaml`.

## Step 3: Create a shared service registration method

In your app project, extract service/VM/page registration into an extension method so both the app and test project can call it:

```csharp
// In your app project
public static class ServiceCollectionExtensions
{
    public static MauiAppBuilder AddMyAppServices(this MauiAppBuilder builder)
    {
        builder.ConfigureFonts(fonts =>
        {
            fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
        });

        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<MainViewModel>();
        // ... all your services
        
        return builder;
    }
}
```

Then your app's `MauiProgram.cs` becomes:
```csharp
builder.UseMauiApp<App>().AddMyAppServices();
```

## Step 4: Create the test project

Create a new MAUI app project that references:
- Your app (with `IsTestLibrary=true`)
- `DeviceRunners.VisualRunners.NUnit` (or `.Xunit`)
- `DeviceRunners.VisualRunners.Maui`

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <Import Project="path/to/DeviceRunners.Testing.Targets.props" />
  <!-- Or: <PackageReference Include="DeviceRunners.Testing.Targets" /> -->

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <UseMaui>true</UseMaui>
    <GenerateTestingPlatformEntryPoint>false</GenerateTestingPlatformEntryPoint>
  </PropertyGroup>

  <ItemGroup>
    <!-- Reference the app as a library -->
    <ProjectReference Include="..\MyApp\MyApp.csproj">
      <AdditionalProperties>IsTestLibrary=true</AdditionalProperties>
    </ProjectReference>
    <ProjectReference Include="path/to/DeviceRunners.VisualRunners.NUnit.csproj" />
    <ProjectReference Include="path/to/DeviceRunners.VisualRunners.Maui.csproj" />
  </ItemGroup>

  <!-- Prevent linker from stripping app assembly (tests use reflection) -->
  <ItemGroup>
    <TrimmerRootAssembly Include="MyApp" RootMode="all" />
  </ItemGroup>

  <Import Project="path/to/DeviceRunners.Testing.Targets.targets" />
</Project>
```

## Step 5: Configure the test runner

Your test project's `MauiProgram.cs`:

```csharp
using DeviceRunners.VisualRunners;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseVisualTestRunner(conf => conf
                .AddCliConfiguration()
                .AddConsoleResultChannel()
                // Import app styles — order matters! Colors before Styles.
                .AddResourceDictionary<MyApp.Resources.Styles.Colors>()
                .AddResourceDictionary<MyApp.Resources.Styles.Styles>()
                .AddTestAssembly(typeof(MauiProgram).Assembly)
                .AddNUnit())
            // Register the same services as the real app
            .AddMyAppServices();

        return builder.Build();
    }
}
```

> **Important:** Add `Colors` before `Styles` — Styles.xaml typically uses `{StaticResource}` references to keys defined in Colors.xaml. Dictionaries are merged in order, so earlier ones must be available when later ones are constructed.

## Step 6: Write tests

```csharp
[TestFixture]
public class MainPageTests
{
    [Test]
    public void MainPage_CanBeCreated()
    {
        var vm = new MainViewModel();
        var page = new MainPage(vm);
        Assert.That(page, Is.Not.Null);
    }

    [Test]
    public void AppStyles_AreAvailable()
    {
        var app = Application.Current!;
        var found = app.Resources.TryGetValue("Primary", out var color);
        Assert.That(found, Is.True);
        Assert.That(color, Is.InstanceOf<Color>());
    }

    [Test]
    public async Task Page_WithLiveBindings()
    {
        // For tests that need the binding engine to process updates,
        // push the page as a modal on the UI thread:
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            var vm = new MainViewModel();
            var page = new MainPage(vm);

            var nav = Application.Current!.Windows[0].Page!.Navigation;
            await nav.PushModalAsync(page);

            try
            {
                await Task.Delay(200); // let bindings settle
                // ... assert binding-driven UI state
            }
            finally
            {
                await nav.PopModalAsync();
            }
        });
    }
}
```

## How it works

### Resource scoping

The test runner uses **page-level resources** for its own styles (colors, button styles, etc.). This means:

- **Your styles don't affect runner pages** — the runner's UI looks correct regardless of what you register
- **Runner styles don't leak into your pages** — your controls render with your styles only
- **App-level resources are yours** — `AddResourceDictionary<T>()` merges into `Application.Resources`, which your pages can access via `{StaticResource}`

### Live binding tests

Tests that instantiate pages without attaching them to a window work for verifying:
- Page creation doesn't throw (StaticResource resolution)
- BindingContext is set correctly  
- Visual tree structure (finding elements by AutomationId)
- Command wiring (`button.Command` is the expected ICommand)

For tests that need binding *updates* (e.g., verifying Button.Text changes after a command), push the page as a modal via `PushModalAsync`. This attaches it to a live handler where the binding engine processes property changes. Always use `MainThread.InvokeOnMainThreadAsync` for these — UIKit requires UI operations on the main thread.

## Running tests

```bash
# Run on macOS
dotnet test MyApp.AppTests -f net10.0-maccatalyst

# Run on iOS simulator
dotnet test MyApp.AppTests -f net10.0-ios

# Run on Android emulator
dotnet test MyApp.AppTests -f net10.0-android
```

The `AddCliConfiguration()` call enables the DeviceRunners CLI to auto-start tests and stream results via TCP when invoked through `dotnet test`.
