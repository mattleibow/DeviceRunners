# Testing with App Resources

Test your custom controls, ViewModels, and services on a real device — with your app's actual styles, colors, and DI configuration. No mocking required.

## What you'll build

A separate test project that references your real app as a library. The DeviceRunners visual test runner hosts everything, and your app's `{StaticResource}` references resolve correctly because you import the resource dictionaries explicitly.

```
┌───────────────────────────────────┐
│  Test App (runs on device)        │
│                                   │
│  ┌─────────────────────────────┐  │
│  │ Visual Test Runner (host)   │  │
│  │  + Your Colors & Styles     │  │
│  │  + Your DI services         │  │
│  └─────────────────────────────┘  │
│  ┌─────────────────────────────┐  │
│  │ Your App (built as library) │  │
│  │  Pages, VMs, Controls       │  │
│  └─────────────────────────────┘  │
│  ┌─────────────────────────────┐  │
│  │ Test Classes                │  │
│  └─────────────────────────────┘  │
└───────────────────────────────────┘
```

## Prerequisites

- .NET 9+ with MAUI workload installed
- A MAUI app you want to test
- DeviceRunners NuGet packages (see [Preview Packages](preview-packages.md))

## Quick Start

### 1. Add library-mode support to your app

Create `TestingWorkarounds.targets` next to your app's `.csproj`:

```xml
<Project>

  <!-- When the test project references us with Configuration=LibraryDebug,
       this switches OutputType to Library and strips platform entry points. -->
  <PropertyGroup Condition="$(Configuration.StartsWith('Library'))">
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <PropertyGroup>
    <OutputType Condition="'$(IsTestProject)' == 'true'">Library</OutputType>
    <OutputType Condition="'$(IsTestProject)' != 'true'">Exe</OutputType>
  </PropertyGroup>

  <!-- Windows: suppress packaging -->
  <PropertyGroup Condition="'$(IsTestProject)' == 'true' and $([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'windows'">
    <WindowsPackageType>None</WindowsPackageType>
    <GenerateLibraryLayout>false</GenerateLibraryLayout>
    <AppxGeneratePriEnabled>false</AppxGeneratePriEnabled>
    <IncludePriFilesOutputGroup>false</IncludePriFilesOutputGroup>
    <IncludeCopyLocalFilesOutputGroup>false</IncludeCopyLocalFilesOutputGroup>
  </PropertyGroup>

  <!-- Android: disable resource designer and clear RIDs -->
  <PropertyGroup Condition="'$(IsTestProject)' == 'true' and $([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'android'">
    <AndroidGenerateResourceDesigner>false</AndroidGenerateResourceDesigner>
    <RuntimeIdentifiers></RuntimeIdentifiers>
  </PropertyGroup>

  <!-- Remove platform entry points that conflict with the test app -->
  <ItemGroup Condition="'$(IsTestProject)' == 'true' and $([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'android'">
    <Compile Remove="Platforms\Android\MainApplication.cs" />
    <Compile Remove="Platforms\Android\MainActivity.cs" />
    <AndroidManifest Remove="Platforms\Android\AndroidManifest.xml" />
  </ItemGroup>

  <ItemGroup Condition="'$(IsTestProject)' == 'true' and ($([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'ios' or $([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'maccatalyst')">
    <Compile Remove="Platforms\iOS\AppDelegate.cs" />
    <Compile Remove="Platforms\iOS\Program.cs" />
    <Compile Remove="Platforms\MacCatalyst\AppDelegate.cs" />
    <Compile Remove="Platforms\MacCatalyst\Program.cs" />
  </ItemGroup>

  <ItemGroup Condition="'$(IsTestProject)' == 'true' and $([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'windows'">
    <ApplicationDefinition Remove="Platforms\Windows\**" />
    <Page Remove="Platforms\Windows\**" />
    <Compile Remove="Platforms\Windows\**" />
    <Manifest Remove="Platforms\Windows\**" />
    <AppxManifest Remove="Platforms\Windows\**" />
  </ItemGroup>

  <!-- Remove icons/splash that would conflict with the test app's own -->
  <ItemGroup Condition="'$(IsTestProject)' == 'true'">
    <MauiIcon Remove="@(MauiIcon)" />
    <MauiSplashScreen Remove="@(MauiSplashScreen)" />
  </ItemGroup>

</Project>
```

Import it at the end of your app's `.csproj`:

```xml
<Import Project="TestingWorkarounds.targets" />
```

> Your app still builds normally as an Exe. The library mode only activates when the test project references it.

### 2. Make your resource dictionaries instantiable

Add `x:Class` to any XAML resource dictionaries you want to use in tests, plus a code-behind:

**Colors.xaml:**
```xml
<?xml version="1.0" encoding="utf-8" ?>
<ResourceDictionary xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
                    x:Class="MyApp.Resources.Styles.Colors">
    <!-- your colors -->
</ResourceDictionary>
```

**Colors.xaml.cs:**
```csharp
namespace MyApp.Resources.Styles;

public partial class Colors : ResourceDictionary
{
    public Colors() => InitializeComponent();
}
```

Repeat for `Styles.xaml` and any other resource dictionaries your controls depend on.

### 3. Extract shared service registration

Move your DI/font/page registration into a reusable extension method:

```csharp
// In your app project (e.g. MauiProgram.cs or a ServiceExtensions.cs)
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
```

Your app's `MauiProgram.cs` calls it:
```csharp
builder.UseMauiApp<App>().AddMyAppServices();
```

### 4. Create the test project

Create a new MAUI app project for your tests. The `.csproj` needs three things:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFrameworks>net10.0-android;net10.0-ios;net10.0-maccatalyst</TargetFrameworks>
    <TargetFrameworks Condition="$([MSBuild]::IsOSPlatform('windows'))">$(TargetFrameworks);net10.0-windows10.0.19041.0</TargetFrameworks>
    <OutputType>Exe</OutputType>
    <UseMaui>true</UseMaui>
    <SingleProject>true</SingleProject>
    <!-- ... standard MAUI project properties ... -->
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="nunit" />
    <PackageReference Include="NUnit3TestAdapter" />
    <PackageReference Include="DeviceRunners.VisualRunners.NUnit" />
    <PackageReference Include="DeviceRunners.VisualRunners.Maui" />
    <!-- Enables `dotnet test` to deploy and run on devices -->
    <PackageReference Include="DeviceRunners.Testing.Targets" />
  </ItemGroup>

  <!-- 1. Reference your app as a library -->
  <ItemGroup>
    <ProjectReference Include="..\MyApp\MyApp.csproj">
      <AdditionalProperties>Configuration=Library$(Configuration)</AdditionalProperties>
    </ProjectReference>
  </ItemGroup>

  <!-- 2. Prevent linker from stripping the app assembly -->
  <ItemGroup>
    <TrimmerRootAssembly Include="MyApp" RootMode="all" />
  </ItemGroup>

  <!-- 3. Import workarounds for Resizetizer/PRI bugs -->
  <Import Project="TestingWorkarounds.targets" />

</Project>
```

Create `TestingWorkarounds.targets` in the test project folder (replace `YOUR_APP_NAME` with your app's project folder name):

```xml
<Project>

  <!-- Strip leaked MauiIcon/MauiSplashScreen from the referenced app -->
  <Target Name="_RemoveImportedAppResizetizerItems" AfterTargets="ResizetizeCollectItems">
    <ItemGroup>
      <MauiIcon Remove="@(MauiIcon)"
        Condition="$([System.String]::new('%(Identity)').Replace('\','/').Contains('YOUR_APP_NAME/Resources/AppIcon'))" />
      <MauiImage Remove="@(MauiImage)"
        Condition="$([System.String]::new('%(Identity)').Replace('\','/').Contains('YOUR_APP_NAME/Resources/AppIcon'))" />
      <MauiSplashScreen Remove="@(MauiSplashScreen)"
        Condition="$([System.String]::new('%(Identity)').Replace('\','/').Contains('YOUR_APP_NAME/Resources/Splash'))" />
    </ItemGroup>
  </Target>

  <!-- Windows: fix PRI generation for app-as-library reference -->
  <Target Name="_FixWindowsPriForAppReference" AfterTargets="AssignProjectConfiguration"
    Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'windows'">
    <ItemGroup>
      <ProjectReferenceWithConfiguration Condition="'%(Filename)' == 'YOUR_APP_NAME'">
        <SetConfiguration>%(ProjectReferenceWithConfiguration.SetConfiguration);IsTestProject=true;IncludePriFilesOutputGroup=false;IncludeCopyLocalFilesOutputGroup=false</SetConfiguration>
      </ProjectReferenceWithConfiguration>
    </ItemGroup>
  </Target>

</Project>
```

> These workarounds are needed because of [dotnet/maui#35574](https://github.com/dotnet/maui/issues/35574). They'll be simplified once the upstream fix ships.

### 5. Wire up the test runner

Create `MauiProgram.cs` in the test project:

```csharp
using DeviceRunners.VisualRunners;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder.UseVisualTestRunner(conf => conf
            .AddCliConfiguration()
            .AddConsoleResultChannel()
            // Import your app's resource dictionaries (order matters!)
            .AddResourceDictionary<MyApp.Resources.Styles.Colors>()
            .AddResourceDictionary<MyApp.Resources.Styles.Styles>()
            .AddTestAssembly(typeof(MauiProgram).Assembly)
            .AddNUnit());

        // Register the same services as the real app
        builder.AddMyAppServices();

        return builder.Build();
    }
}
```

> **Order matters:** Add `Colors` before `Styles`. Styles typically reference colors via `{StaticResource}`, so they must already be merged when Styles is constructed.

### 6. Write tests

```csharp
using NUnit.Framework;

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
}
```

### 7. Run

```bash
# macOS
dotnet test MyApp.AppTests -f net10.0-maccatalyst

# iOS simulator
dotnet test MyApp.AppTests -f net10.0-ios

# Android emulator
dotnet test MyApp.AppTests -f net10.0-android

# Windows
dotnet test MyApp.AppTests -f net10.0-windows10.0.19041.0
```

## Testing patterns

### Simple tests (no live UI needed)

These work without attaching the page to a window:

- ✅ Page creation (verifies `{StaticResource}` resolves)
- ✅ BindingContext assignment
- ✅ Visual tree structure (finding elements by AutomationId)
- ✅ Command wiring (`button.Command` is the expected ICommand)

### Live UI tests (binding updates, rendering)

When you need the binding engine to process property changes, push the page as a modal:

```csharp
[Test]
public async Task Button_UpdatesText_AfterCommand()
{
    await MainThread.InvokeOnMainThreadAsync(async () =>
    {
        var vm = new MainViewModel();
        var page = new MainPage(vm);

        var nav = Application.Current!.Windows[0].Page!.Navigation;
        await nav.PushModalAsync(page);

        try
        {
            await Task.Delay(200); // let bindings settle

            var button = FindByAutomationId<Button>(page, "CounterButton");
            button!.Command.Execute(null);
            await Task.Delay(200);

            Assert.That(button.Text, Is.EqualTo("Clicked 1 time"));
        }
        finally
        {
            await nav.PopModalAsync();
        }
    });
}
```

> Always wrap live UI tests in `MainThread.InvokeOnMainThreadAsync` — platform UI requires the main thread.

### Helper: finding elements by AutomationId

```csharp
static T? FindByAutomationId<T>(Element root, string automationId) where T : Element
{
    if (root is T match && match.AutomationId == automationId)
        return match;

    IEnumerable<Element> children = root switch
    {
        IContentView { Content: Element content } => [content],
        Layout layout => layout.Children.OfType<Element>(),
        ContentPage { Content: View content } => [content],
        _ => []
    };

    foreach (var child in children)
    {
        var result = FindByAutomationId<T>(child, automationId);
        if (result is not null)
            return result;
    }

    return null;
}
```

## How resource scoping works

- **Your styles** are merged into `Application.Resources` via `AddResourceDictionary<T>()` — your pages find them via normal `{StaticResource}` lookup.
- **Runner styles** use page-level resources with explicit keys, so they don't override your named resources. Note that app-level implicit styles (e.g., `Style TargetType="Button"`) will still apply globally — this is by design so your controls render the same way they do in your real app.

## Complete sample

See the [`DeviceTestingKitApp.AppTests`](https://github.com/mattleibow/DeviceRunners/tree/main/sample/test/DeviceTestingKitApp.AppTests) project in this repository for a working example with NUnit tests covering page creation, style resolution, command binding, and live UI verification.
