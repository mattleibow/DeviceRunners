//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

var isLocal = BuildSystem.IsLocalBuild;
var isRunningOnAppVeyor = AppVeyor.IsRunningOnAppVeyor;

var version = "3.6.1";
var packageModifier = configuration == "Debug" ? "-dbg" : "";

// Directories
var basePath = Context.Environment.WorkingDirectory.FullPath;
var outputDirectory = basePath + "/bin/" + configuration;
var androidDirectory = basePath + "/src/runner/nunit.runner.Droid/bin/" + configuration;
var iosDirectory = basePath + "/src/runner/nunit.runner.iOS/bin/AnyCPU/" + configuration;

//////////////////////////////////////////////////////////////////////
// SET VERSION
//////////////////////////////////////////////////////////////////////

Task("Set-Appveyor-Tag")
    .WithCriteria(() => isRunningOnAppVeyor)
    .Does(() =>
{
    var tag = AppVeyor.Environment.Repository.Tag;

    if (tag.IsTag)
    {
        version = tag.Name;
    }
    else
    {
        var buildNumber = AppVeyor.Environment.Build.Number.ToString("00000");
        var branch = AppVeyor.Environment.Repository.Branch;
        var isPullRequest = AppVeyor.Environment.PullRequest.IsPullRequest;

        if (branch == "master" && !isPullRequest)
        {
            version = version + "-dev-" + buildNumber + packageModifier;
        }
        else
        {
            var suffix = "-ci-" + buildNumber + packageModifier;

            if (isPullRequest)
                suffix += "-pr-" + AppVeyor.Environment.PullRequest.Number;
            else if (AppVeyor.Environment.Repository.Branch.StartsWith("release", StringComparison.OrdinalIgnoreCase))
                suffix += "-pre-" + buildNumber;
            else
                suffix += "-" + branch;

            // Nuget limits "special version part" to 20 chars. Add one for the hyphen.
            if (suffix.Length > 21)
                suffix = suffix.Substring(0, 21);

            version = version + suffix;
        }
    }

    AppVeyor.UpdateBuildVersion(version);
});

//////////////////////////////////////////////////////////////////////
// CLEAN/BUILD
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectories(new DirectoryPath[] {outputDirectory, androidDirectory, iosDirectory});
});

Task("Restore-NuGet-Packages")
    .Does(() =>
{
    NuGetRestore("./nunit.runner.sln", new NuGetRestoreSettings {
        Source = new List<string> {
            "https://www.nuget.org/api/v2/",
            "https://www.myget.org/F/nunit/api/v2"
        },
        Verbosity = NuGetVerbosity.Quiet 
    });
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    MSBuild("./nunit.runner.sln", new MSBuildSettings()
        .SetConfiguration(configuration)
        .SetPlatformTarget(PlatformTarget.MSIL)
        .WithProperty("TreatWarningsAsErrors", "true")
        .SetVerbosity(Verbosity.Minimal)
        .SetNodeReuse(false)
    );

    MSBuild("./src/tests/nunit.runner.tests.uwp/nunit.runner.tests.uwp.csproj", new MSBuildSettings()
        .SetConfiguration(configuration)
        .SetPlatformTarget(PlatformTarget.x86)
        .WithProperty("TreatWarningsAsErrors", "true")
        .WithProperty("AppxPackageSigningEnabled", "false")
        .SetVerbosity(Verbosity.Minimal)
        .SetNodeReuse(false)
    );
});

//////////////////////////////////////////////////////////////////////
// PACKAGE/PUBLISH
//////////////////////////////////////////////////////////////////////

Task("Create-NuGet-Packages")
    .IsDependentOn("Build")
    .Does(() =>
{
    CreateDirectory(outputDirectory);

    NuGetPack("nuget/nunit.runners.xamarin.nuspec", new NuGetPackSettings
    {
        Version = version,
        BasePath = basePath,
        OutputDirectory = outputDirectory,
    });        
});

Task("UploadArtifacts")
    .WithCriteria(() => isRunningOnAppVeyor)
    .IsDependentOn("Package")
    .Does(() =>
{
    foreach(var package in System.IO.Directory.GetFiles(outputDirectory, "*.nupkg"))
        AppVeyor.UploadArtifact(package);
});

Task("Publish-NuGet")
  .IsDependentOn("Create-NuGet-Packages")
  .WithCriteria(() => isLocal)
  .Does(() =>
{
    // Resolve the API key.
    var apiKey = EnvironmentVariable("NUGET_API_KEY");
    if(string.IsNullOrEmpty(apiKey)) {
        throw new InvalidOperationException("Could not resolve NuGet API key.");
    }
    
    // Get the path to the package.
    var packagePath = outputDirectory + File(string.Concat("nunit.runner.xamarin.", version, ".nupkg"));

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

Task("Appveyor")
  .IsDependentOn("Set-Appveyor-Tag")
  .IsDependentOn("Package")
  .IsDependentOn("UploadArtifacts");

RunTarget(target);