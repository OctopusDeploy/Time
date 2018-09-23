//////////////////////////////////////////////////////////////////////
// TOOLS
//////////////////////////////////////////////////////////////////////
#tool "nuget:?package=GitVersion.CommandLine&prerelease"

using Path = System.IO.Path;
using IO = System.IO;
using Cake.Common.Tools;

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////
var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

///////////////////////////////////////////////////////////////////////////////
// GLOBAL VARIABLES
///////////////////////////////////////////////////////////////////////////////
var artifactsDir = "./artifacts";
var localPackagesDir = "../LocalPackages";

GitVersion gitVersionInfo;
string nugetVersion;

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////
Setup(context =>
{
    gitVersionInfo = GitVersion(new GitVersionSettings {
        OutputType = GitVersionOutput.Json
    });

    if(BuildSystem.IsRunningOnTeamCity)
        BuildSystem.TeamCity.SetBuildNumber(gitVersionInfo.NuGetVersion);

    nugetVersion = gitVersionInfo.NuGetVersion;

    Information("Building Octopus.Time v{0}", nugetVersion);
    Information("Informational Version {0}", gitVersionInfo.InformationalVersion);
});

Teardown(context =>
{
	Information("Finished running tasks.");
});

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectory(artifactsDir);
    CleanDirectories("./source/**/bin");
    CleanDirectories("./source/**/obj");
});

Task("Restore")
    .IsDependentOn("Clean")
    .Does(() => {
        DotNetCoreRestore("source");
    });

Task("Build")
    .IsDependentOn("Restore")
    .IsDependentOn("Clean")
    .Does(() =>
{
    DotNetCoreBuild("./source", new DotNetCoreBuildSettings
    {
        Configuration = configuration,
        ArgumentCustomization = args => args.Append($"/p:Version={nugetVersion}")
    });
});


Task("Pack")
    .IsDependentOn("Build")
    .Does(() =>
{
    DotNetCorePack("./source/Octopus.Time", new DotNetCorePackSettings
    {
        Configuration = configuration,
        OutputDirectory = artifactsDir,
        NoBuild = true,
        ArgumentCustomization = args => args.Append($"/p:Version={nugetVersion}")
    });
});

Task("CopyToLocalPackages")
    .IsDependentOn("Pack")
    .WithCriteria(BuildSystem.IsLocalBuild)
    .Does(() =>
{
    CreateDirectory(localPackagesDir);
    CopyFileToDirectory($"{artifactsDir}/Octopus.Time.{nugetVersion}.nupkg", localPackagesDir);
});

Task("Publish")
    .IsDependentOn("CopyToLocalPackages")
    .WithCriteria(BuildSystem.IsRunningOnTeamCity)
    .Does(() =>
{
	NuGetPush($"{artifactsDir}/Octopus.Time.{nugetVersion}.nupkg", new NuGetPushSettings {
		Source = "https://f.feedz.io/octopus-deploy/dependencies/nugetapi/v3/index.json",
		ApiKey = EnvironmentVariable("FeedzIoApiKey")
	});

    if (gitVersionInfo.PreReleaseTag == "")
    {
          NuGetPush($"{artifactsDir}/Octopus.Time.{nugetVersion}.nupkg", new NuGetPushSettings {
            Source = "https://www.nuget.org/api/v2/package",
            ApiKey = EnvironmentVariable("NuGetApiKey")
        });
    }
});

Task("Default")
    .IsDependentOn("Publish");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////
RunTarget(target);
