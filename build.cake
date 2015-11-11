//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////
var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Versioning
var packageVersion = "3.0.0";
var packageModifier = "-rc-2";
var displayVersion = "3.0.0";

// Get whether or not this is a local build.
var local = BuildSystem.IsLocalBuild;
var isRunningOnUnix = IsRunningOnUnix();
var isRunningOnWindows = IsRunningOnWindows();
var isRunningOnAppVeyor = AppVeyor.IsRunningOnAppVeyor;
var isPullRequest = AppVeyor.Environment.PullRequest.IsPullRequest;

var nugetVersion = packageVersion + packageModifier;

Task("Restore-NuGet-Packages")
    //.IsDependentOn("Clean")
    .Does(() =>
{
    NuGetRestore("./nunit.runner.sln", new NuGetRestoreSettings {
        Source = new List<string> {
            "https://www.nuget.org/api/v2/",
            "https://www.myget.org/F/nunit/api/v2"
        }
    });
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    if(isRunningOnUnix)
    {
        XBuild("./nunit.runner.sln", new XBuildSettings()
            .SetConfiguration("Debug")
            .WithTarget("AnyCPU")
            .WithProperty("TreatWarningsAsErrors", "true")
            .SetVerbosity(Verbosity.Minimal)
        );
    }
    else
    {
        MSBuild("./nunit.runner.sln", new MSBuildSettings()
            .SetConfiguration(configuration)
            .SetPlatformTarget(PlatformTarget.MSIL)
            .WithProperty("TreatWarningsAsErrors", "true")
            .SetVerbosity(Verbosity.Minimal)
            .SetNodeReuse(false)
        );
    }
});

Task("Package")
    .IsDependentOn("Build")
    .Does(() =>
{
    NuGetPack("nuget/nunit.runners.xamarin.nuspec", new NuGetPackSettings
    {
        Version = nugetVersion,
        BasePath = ".",
        OutputDirectory = "./bin/" + configuration,
    });        
});

Task("Default")
  .IsDependentOn("Build");

RunTarget(target);