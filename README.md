# NUnit 3.0 Xamarin Runners

NUnit test runners for Xamarin and mobile devices

## How to Use ##

**Project templates will be coming soon**, in the meantime...

In your solution;

1. Add a new project to your solution
  - `Blank App (Android)` for Android,
  - `Blank App (iOS)` for iOS,
  - `Blank App (Windows Phone)` for Windows Phone 8.1
  - `Blank App (Universal Windows)` for Windows 10 Universal
2. Add the `nunit.xamarin` NuGet package to your projects
3. Text files will be added to your project, open them and copy over the coresponding file in each project as appropriate.
  - `MainActivity.cs.txt` for Android,
  - `AppDelegate.cs.txt` for iOS, or 
  - `MainPage.xaml.txt` and `MainPage.xaml.cs.txt` for WinPhone.
  - Windows 10 Universal doesn't currently add files, see below for what to change.
4. Once you are done with them, you can delete the text files that were added to your project.
5. On Windows Phone and Windows Universal, you will also need to add `Xamarin.Forms.Forms.Init(e);` to `App.OnLaunched()`.
6. Write your unit tests in this project, or in a shared project
7. Build and run the tests on your device or emulator

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