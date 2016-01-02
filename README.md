# NUnit 3.0 Xamarin Runners

NUnit test runners for Xamarin and mobile devices

## How to Use ##

The easiest way to get started is to install the [NUnit Templates extension for Visual Studio](https://visualstudiogallery.msdn.microsoft.com/6cd55f79-4936-49e7-b81d-c40fcd81abc7). It will add project templates for the various Xamarin platforms.

In your solution;

1. Add new test projects to your solution. These project types are included in the [NUnit Templates Extension](https://visualstudiogallery.msdn.microsoft.com/6cd55f79-4936-49e7-b81d-c40fcd81abc7)
  - NUnit 3 Test Project (Android)
  - NUnit 3 Test Project (iOS)
  - NUnit 3 Test Project (Windows Phone 8.1)
  - NUnit 3 Test Project (Universal Windows) 
2. Write your unit tests in this project, in a portable project, or in a shared project, referencing the project with the tests
3. Build and run the tests on your device or emulator

If your tests are in a portable project, you need to add that assembly to the `NUnit.Runner.App` in the startup code.

For more information, see [Testing Xamarin Projects using NUnit 3](http://www.alteridem.net/2015/12/21/testing-xamarin-projects-using-nunit-3/).

```C#
nunit.AddTestAssembly(typeof(MyTests).Assembly);
```

The startup code for each platform is as follows;

### Android ###

**MainActivity.cs**

```C#
[Activity(Label = "NUnit 3.0", MainLauncher = true, Theme = "@android:style/Theme.Holo.Light", ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsApplicationActivity
{
    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

        // This will load all tests within the current project
        var nunit = new NUnit.Runner.App();

        // If you want to add tests in another assembly
        //nunit.AddTestAssembly(typeof(MyTests).Assembly);

        // Do you want to automatically run tests when the app starts?
        nunit.AutoRun = true;

        LoadApplication(nunit);
    }
}
```
### iOS ###

**AppDelegate.cs**

```C#
[Register("AppDelegate")]
public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
{
    //
    // This method is invoked when the application has loaded and is ready to run. In this 
    // method you should instantiate the window, load the UI into it and then make the window
    // visible.
    //
    // You have 17 seconds to return from this method, or iOS will terminate your application.
    //
    public override bool FinishedLaunching(UIApplication app, NSDictionary options)
    {
        global::Xamarin.Forms.Forms.Init();

        // This will load all tests within the current project
        var nunit = new NUnit.Runner.App();

        // If you want to add tests in another assembly
        //nunit.AddTestAssembly(typeof(MyTests).Assembly);

        // Do you want to automatically run tests when the app starts?
        nunit.AutoRun = true;

        LoadApplication(nunit);

        return base.FinishedLaunching(app, options);
    }
}
```

### Windows Phone 8.1 ###

**MainPage.xaml**

```XML
<forms:WindowsPhonePage
    x:Class="Xamarin.Test.wp81.MainPage"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:forms="using:Xamarin.Forms.Platform.WinRT"
    mc:Ignorable="d"
    Background="{ThemeResource ApplicationPageBackgroundThemeBrush}">

    <Grid>

    </Grid>
</forms:WindowsPhonePage>
```

**MainPage.xaml.cs**

```C#
public sealed partial class MainPage : WindowsPhonePage
{
    public MainPage()
    {
        InitializeComponent();

        // Windows Phone will not load all tests within the current project,
        // you must do it explicitly below
        var nunit = new NUnit.Runner.App();

        // If you want to add tests in another assembly, add a reference and
        // duplicate the following line with a type from the referenced assembly
        nunit.AddTestAssembly(typeof(MainPage).GetTypeInfo().Assembly);

        // Do you want to automatically run tests when the app starts?
        nunit.AutoRun = true;

        LoadApplication(nunit);

        this.NavigationCacheMode = NavigationCacheMode.Required;
    }
}
```

**App.xaml.cs**

```C#
protected override void OnLaunched(LaunchActivatedEventArgs e)
{
    // <SNIP>

    Frame rootFrame = Window.Current.Content as Frame;

    // Do not repeat app initialization when the Window already has content,
    // just ensure that the window is active
    if (rootFrame == null)
    {
        // Create a Frame to act as the navigation context and navigate to the first page
        rootFrame = new Frame();

        // TODO: change this value to a cache size that is appropriate for your application
        rootFrame.CacheSize = 1;

        // Set the default language
        rootFrame.Language = Windows.Globalization.ApplicationLanguages.Languages[0];

        // ==> ADD THIS LINE <==
        Xamarin.Forms.Forms.Init(e);

        if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
        {
            // TODO: Load state from previously suspended application
        }

        // Place the frame in the current Window
        Window.Current.Content = rootFrame;
    }

    // <SNIP>
}
```



### Windows 10 Universal ###

**MainPage.xaml**

```XML
<forms:WindowsPage
    x:Class="NUnit.Runner.Tests.MainPage"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:local="using:NUnit.Runner.Tests"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:forms="using:Xamarin.Forms.Platform.WinRT"
    mc:Ignorable="d">

    <Grid Background="{ThemeResource ApplicationPageBackgroundThemeBrush}">

    </Grid>
</forms:WindowsPage>
```

**MainPage.xaml.cs**

```C#
public sealed partial class MainPage : WindowsPage
{
    public MainPage()
    {
        InitializeComponent();

        // Windows Universal will not load all tests within the current project,
        // you must do it explicitly below
        var nunit = new NUnit.Runner.App();

        // If you want to add tests in another assembly, add a reference and
        // duplicate the following line with a type from the referenced assembly
        nunit.AddTestAssembly(typeof(MainPage).GetTypeInfo().Assembly);

        // Do you want to automatically run tests when the app starts?
        nunit.AutoRun = true;

        LoadApplication(nunit);
    }
}
```

**App.xaml.cs**

```C#
protected override void OnLaunched(LaunchActivatedEventArgs e)
{
    // <SNIP>

    Frame rootFrame = Window.Current.Content as Frame;

    // Do not repeat app initialization when the Window already has content,
    // just ensure that the window is active
    if (rootFrame == null)
    {
        // Create a Frame to act as the navigation context and navigate to the first page
        rootFrame = new Frame();

        rootFrame.NavigationFailed += OnNavigationFailed;

        // ==> ADD THIS LINE <==
        Xamarin.Forms.Forms.Init(e);

        if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
        {
            // TODO: Load state from previously suspended application
        }

        // Place the frame in the current Window
        Window.Current.Content = rootFrame;
    }

    // <SNIP>
}
```