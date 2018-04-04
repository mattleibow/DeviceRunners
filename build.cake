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

var version = "3.10.1";
var packageModifier = configuration == "Debug" ? "-dbg" : "";

// Directories
var basePath = Context.Environment.WorkingDirectory.FullPath;
var outputDirectory = basePath + "/src/nunit.xamarin/bin/" + configuration;

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
    CleanDirectory(outputDirectory);
});

Task("Restore")
    .Does(() =>
{
    NuGetRestore("./nunit.xamarin.sln", new NuGetRestoreSettings {
        Source = new List<string> {
            "https://www.nuget.org/api/v2/",
            "https://www.myget.org/F/nunit/api/v2"
        },
        Verbosity = NuGetVerbosity.Quiet
    });
});

Task("Build")
    .IsDependentOn("Restore")
    .Does(() =>
{
    MSBuild("./nunit.xamarin.sln", new MSBuildSettings()
        .SetConfiguration(configuration)
        .SetPlatformTarget(PlatformTarget.MSIL)
        .WithProperty("TreatWarningsAsErrors", "true")
        .WithProperty("Version", version)
        .SetVerbosity(Verbosity.Minimal)
        .UseToolVersion(MSBuildToolVersion.VS2017)
    );

    MSBuild("./tests/nunit.runner.tests.uwp/nunit.runner.tests.uwp.csproj", new MSBuildSettings()
        .SetConfiguration(configuration)
        .SetPlatformTarget(PlatformTarget.x86)
        .WithProperty("TreatWarningsAsErrors", "true")
        .WithProperty("Version", version)
        .WithProperty("AppxPackageSigningEnabled", "false")
        .SetVerbosity(Verbosity.Minimal)
        .UseToolVersion(MSBuildToolVersion.VS2017)
    );
});

//////////////////////////////////////////////////////////////////////
// PACKAGE/PUBLISH
//////////////////////////////////////////////////////////////////////

Task("Package")
    .IsDependentOn("Build")
    .Does(() =>
{
    MSBuild("./src/nunit.xamarin/nunit.xamarin.csproj", new MSBuildSettings()
        .SetConfiguration(configuration)
        .SetPlatformTarget(PlatformTarget.MSIL)
        .WithProperty("TreatWarningsAsErrors", "true")
        .WithProperty("Version", version)
        .WithTarget("Pack")
        .SetVerbosity(Verbosity.Minimal)
        .UseToolVersion(MSBuildToolVersion.VS2017)
    );
});

Task("UploadArtifacts")
    .WithCriteria(() => isRunningOnAppVeyor)
    .IsDependentOn("Package")
    .Does(() =>
{
    foreach(var package in System.IO.Directory.GetFiles(outputDirectory, "*.nupkg"))
        AppVeyor.UploadArtifact(package);
});

Task("Default")
  .IsDependentOn("Build");

Task("Appveyor")
  .IsDependentOn("Set-Appveyor-Tag")
  .IsDependentOn("Package")
  .IsDependentOn("UploadArtifacts");

RunTarget(target);