# NUnit 3.0 Xamarin Runners

NUnit test runners for Xamarin and mobile devices

## How to Use ##

The NuGet packages are nearly ready and we will likely create project templates, but until that is done,
you will need to build from source. For this, you will need a Xamarin trial or subscription.

1. Clone this repository
2. Open `nunit.runner.sln` in Visual Studio with Xamarin installed, or in Xamarin Studio.
3. Create a release build of the solution.

Then in your solution;

1. Add a new `Blank App (Android)` or `Blank App (iOS)` to your solution
2. Add NuGet packages to your project for `NUnit 3.0.0-beta-4` and `Xamarin.Forms 1.4.4.6392`
3. Browse and add a reference to the `nunit.runner.droid.dll` or `nunit.runner.ios.dll` that you built
4. Write your unit tests in this project, or in a shared project
5. Change the base class of `MainActivity` on Android to `global::Xamarin.Forms.Platform.Android.FormsApplicationActivity`
6. Change the base class of `AppDelegate` on iOS to `global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate`
7. Change MainActivity.OnCreate() on Android or AppDelegate.FinishedLaunching() on iOS
8. Build and run the tests on your device or emulator

### Android ###

```C#
protected override void OnCreate(Bundle savedInstanceState)
{
    base.OnCreate(savedInstanceState);

    global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

    // This will load all tests within the current project
    var nunit = new NUnit.Runner.App();

    // If you want to add tests in another assembly
    //nunit.AddTestAssembly(typof(MyTests).Assembly);

    // Do you want to automatically run tests when the app starts?
    nunit.AutoRun = true;

    LoadApplication(nunit);
}
```
### iOS ###

```C#
public override bool FinishedLaunching(UIApplication app, NSDictionary options)
{
    global::Xamarin.Forms.Forms.Init();

    // This will load all tests within the current project
    var nunit = new NUnit.Runner.App();

    // If you want to add tests in another assembly
    //nunit.AddTestAssembly(typof(MyTests).Assembly);

    // Do you want to automatically run tests when the app starts?
    nunit.AutoRun = true;

    LoadApplication(nunit);

    return base.FinishedLaunching(app, options);
}
```

