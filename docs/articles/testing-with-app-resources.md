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

1. Building your app as a **library** (via a Configuration-based trigger) so the test project can reference it
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
│  │ Your App (Library)     │  │  ← Referenced with Configuration=Library$(Configuration)
│  │  Pages, VMs, Controls  │  │
│  └───────────────────────┘  │
│  ┌───────────────────────┐  │
│  │ Test Classes (NUnit)   │  │  ← Your test code
│  └───────────────────────┘  │
└─────────────────────────────┘
```

## Step 1: Make your app buildable as a library

Add a `TestingWorkarounds.targets` file to your app project with the library-mode logic. This keeps the main `.csproj` clean:

**TestingWorkarounds.targets (in app project folder):**
```xml
<Project>
  <!-- When built in a "Library*" configuration (e.g. LibraryDebug), activate library mode.
       The test project uses AdditionalProperties=Configuration=Library$(Configuration) which
       triggers this automatically and provides separate intermediate/output paths. -->
  <PropertyGroup Condition="$(Configuration.StartsWith('Library'))">
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <PropertyGroup>
    <OutputType Condition="'$(IsTestProject)' == 'true'">Library</OutputType>
    <OutputType Condition="'$(IsTestProject)' != 'true'">Exe</OutputType>
  </PropertyGroup>

  <!-- Android: disable resource designer and clear RIDs -->
  <PropertyGroup Condition="'$(IsTestProject)' == 'true' and $([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'android'">
    <AndroidGenerateResourceDesigner>false</AndroidGenerateResourceDesigner>
    <RuntimeIdentifiers></RuntimeIdentifiers>
  </PropertyGroup>

  <!-- Android: remove entry point classes -->
  <ItemGroup Condition="'$(IsTestProject)' == 'true' and $([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'android'">
    <Compile Remove="Platforms\Android\MainApplication.cs" />
    <Compile Remove="Platforms\Android\MainActivity.cs" />
    <AndroidManifest Remove="Platforms\Android\AndroidManifest.xml" />
  </ItemGroup>

  <!-- iOS/MacCatalyst: remove entry point classes -->
  <ItemGroup Condition="'$(IsTestProject)' == 'true' and ($([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'ios' or $([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'maccatalyst')">
    <Compile Remove="Platforms\iOS\AppDelegate.cs" />
    <Compile Remove="Platforms\iOS\Program.cs" />
    <Compile Remove="Platforms\MacCatalyst\AppDelegate.cs" />
    <Compile Remove="Platforms\MacCatalyst\Program.cs" />
  </ItemGroup>

  <!-- Windows: remove all platform files and suppress packaging -->
  <PropertyGroup Condition="'$(IsTestProject)' == 'true' and $([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'windows'">
    <WindowsPackageType>None</WindowsPackageType>
    <GenerateLibraryLayout>false</GenerateLibraryLayout>
    <AppxGeneratePriEnabled>false</AppxGeneratePriEnabled>
    <IncludePriFilesOutputGroup>false</IncludePriFilesOutputGroup>
    <IncludeCopyLocalFilesOutputGroup>false</IncludeCopyLocalFilesOutputGroup>
  </PropertyGroup>

  <ItemGroup Condition="'$(IsTestProject)' == 'true' and $([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'windows'">
    <ApplicationDefinition Remove="Platforms\Windows\**" />
    <Page Remove="Platforms\Windows\**" />
    <Compile Remove="Platforms\Windows\**" />
    <Manifest Remove="Platforms\Windows\**" />
    <AppxManifest Remove="Platforms\Windows\**" />
  </ItemGroup>

  <!-- Remove app icons/splash that would conflict with test project's own -->
  <ItemGroup Condition="'$(IsTestProject)' == 'true'">
    <MauiIcon Remove="@(MauiIcon)" />
    <MauiSplashScreen Remove="@(MauiSplashScreen)" />
  </ItemGroup>
</Project>
```

Then import it from your app's `.csproj`:
```xml
<Import Project="TestingWorkarounds.targets" />
```

> **Note:** Images, fonts, and raw assets do *not* need stripping — they flow through fine as library content.

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

Create a new MAUI app project with a `TestingWorkarounds.targets` for the Resizetizer workaround:

**TestingWorkarounds.targets (in test project folder):**
```xml
<Project>
  <!--
    Resizetizer's GetMauiItems and Windows PRI's GetPriOutputs don't propagate
    AdditionalProperties from ProjectReference, so the app's MauiIcon/MauiSplashScreen
    leak into this project. These targets strip those imported items.
    See: https://github.com/dotnet/maui/issues/35574
  -->
  <Target Name="_RemoveImportedAppResizetizerItems" AfterTargets="ResizetizeCollectItems">
    <ItemGroup>
      <MauiIcon Remove="@(MauiIcon)"
        Condition="$([System.String]::new('%(Identity)').Contains('MyApp/Resources/AppIcon'))" />
      <MauiImage Remove="@(MauiImage)"
        Condition="$([System.String]::new('%(Identity)').Contains('MyApp/Resources/AppIcon'))" />
      <MauiSplashScreen Remove="@(MauiSplashScreen)"
        Condition="$([System.String]::new('%(Identity)').Contains('MyApp/Resources/Splash'))" />
    </ItemGroup>
  </Target>

  <!-- Windows: inject IsTestProject=true during PRI generation -->
  <Target Name="_FixWindowsPriForAppReference" AfterTargets="AssignProjectConfiguration"
    Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'windows'">
    <ItemGroup>
      <ProjectReferenceWithConfiguration Condition="'%(Filename)' == 'MyApp'">
        <SetConfiguration>%(ProjectReferenceWithConfiguration.SetConfiguration);IsTestProject=true;IncludePriFilesOutputGroup=false;IncludeCopyLocalFilesOutputGroup=false</SetConfiguration>
      </ProjectReferenceWithConfiguration>
    </ItemGroup>
  </Target>
</Project>
```

**Test project `.csproj`:**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <Import Project="path/to/DeviceRunners.Testing.Targets.props" />

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <UseMaui>true</UseMaui>
    <GenerateTestingPlatformEntryPoint>false</GenerateTestingPlatformEntryPoint>
  </PropertyGroup>

  <ItemGroup>
    <!-- Reference the app as a library via Configuration override -->
    <ProjectReference Include="..\MyApp\MyApp.csproj">
      <AdditionalProperties>Configuration=Library$(Configuration)</AdditionalProperties>
    </ProjectReference>
    <ProjectReference Include="path/to/DeviceRunners.VisualRunners.NUnit.csproj" />
    <ProjectReference Include="path/to/DeviceRunners.VisualRunners.Maui.csproj" />
  </ItemGroup>

  <!-- Prevent linker from stripping app assembly (tests use reflection) -->
  <ItemGroup>
    <TrimmerRootAssembly Include="MyApp" RootMode="all" />
  </ItemGroup>

  <Import Project="TestingWorkarounds.targets" />
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

### Configuration-based library mode

The test project references the app with `Configuration=Library$(Configuration)` (e.g., `LibraryDebug`). This:
- Triggers `IsTestProject=true` in the app's `TestingWorkarounds.targets`
- Automatically provides separate intermediate/output paths (MSBuild uses Configuration in the path)
- Strips platform entry points, app icons, and packaging that would conflict

### Known issue: Resizetizer doesn't propagate AdditionalProperties

Resizetizer's `GetMauiItems` target calls into referenced projects without passing `AdditionalProperties`. This means the app's conditional `MauiIcon Remove` doesn't fire during Resizetizer's collection phase. The test project's `TestingWorkarounds.targets` strips these leaked items after collection.

This is tracked at [dotnet/maui#35574](https://github.com/dotnet/maui/issues/35574).

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
