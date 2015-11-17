//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////
var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Get whether or not this is a local build.
var local = BuildSystem.IsLocalBuild;
var isRunningOnUnix = IsRunningOnUnix();
var isRunningOnWindows = IsRunningOnWindows();
var isRunningOnAppVeyor = AppVeyor.IsRunningOnAppVeyor;
var isPullRequest = AppVeyor.Environment.PullRequest.IsPullRequest;


// Versioning
var packageVersion = "3.0.0";
var packageModifier = "";
var displayVersion = "3.0.0";

var semVersion = packageVersion + packageModifier;

// Directories
var basePath = Directory(".");
var outputDirectory = basePath + Directory("bin") + Directory(configuration);
var androidDirectory = basePath + Directory("src/runner/nunit.runner.Droid/bin") + Directory(configuration);
var iosDirectory = basePath + Directory("src/runner/nunit.runner.iOS/bin/AnyCPU") + Directory(configuration);
var wp81Directory = basePath + Directory("src/runner/nunit.runner.wp81/bin") + Directory(configuration);

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(() =>
{
    Information("Building version {0} of Nunit.Xamarin.", semVersion);
});

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .WithCriteria(() => isRunningOnWindows)
    .Does(() =>
{
    CleanDirectories(new DirectoryPath[] {
        outputDirectory, androidDirectory, iosDirectory, wp81Directory});
});

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
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

Task("Create-NuGet-Packages")
    .IsDependentOn("Build")
    .Does(() =>
{
    NuGetPack("nuget/nunit.runners.xamarin.nuspec", new NuGetPackSettings
    {
        Version = semVersion,
        BasePath = basePath,
        OutputDirectory = outputDirectory,
    });        
});

Task("Publish-NuGet")
  .IsDependentOn("Create-NuGet-Packages")
  .WithCriteria(() => local)
  .Does(() =>
{
    // Resolve the API key.
    var apiKey = EnvironmentVariable("NUGET_API_KEY");
    if(string.IsNullOrEmpty(apiKey)) {
        throw new InvalidOperationException("Could not resolve NuGet API key.");
    }
    
    // Get the path to the package.
    var packagePath = outputDirectory + File(string.Concat("nunit.runner.xamarin.", semVersion, ".nupkg"));

    // Push the package.
    NuGetPush(packagePath, new NuGetPushSettings {
      ApiKey = apiKey
    });
});

Task("Default")
  .IsDependentOn("Build");  
  
Task("Package")
  .IsDependentOn("Create-NuGet-Packages");
  
Task("Publish")
  .IsDependentOn("Publish-NuGet");

RunTarget(target);