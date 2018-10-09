using System.Diagnostics;

#tool "nuget:?package=OctopusTools"

var target          = Argument("target", "Default");
var build           = Argument("build", "0");
var revision        = Argument("revision", string.Empty);
var octopusUrl      = Argument("octopusUrl", string.Empty);
var octopusApiKey   = Argument("octopusApiKey", string.Empty);

var configuration = "Release";
var dotnetcoreVersion = "2.1";

Task("Clean")
    .WithCriteria(!BuildSystem.IsRunningOnTeamCity)
    .Does(() =>
    {
        var dirsToClean = GetDirectories("./**/bin");
        dirsToClean.Add(GetDirectories("./**/obj"));

        foreach(var dir in dirsToClean) {
            Console.WriteLine(dir);
        }

        CleanDirectories(dirsToClean);
    });

Task("Test")
    .IsDependentOn("Clean")
    .Does(() =>
    {
        GetFiles("./tests/**/*.csproj")
            .ToList()
            .ForEach(file => DotNetCoreTest(file.FullPath));
    });

Task("Publish")
    .IsDependentOn("Test")
    .Does(() => 
    {
        DotNetCorePublish(".", new DotNetCorePublishSettings {
            Configuration = configuration,
            EnvironmentVariables = new Dictionary<string, string> {
                { "build", build },
                { "revision", revision }
            }
        });
    });

Task("Pack")
    .IsDependentOn("Publish")
    .Does(() => 
    {    
        var projectName = "Web";
        
        var basePath = $"./src/{projectName}/bin/{configuration}/netcoreapp{dotnetcoreVersion}/publish";
    
        var version = FileVersionInfo.GetVersionInfo($"{basePath}/{projectName}.dll").FileVersion;

        OctoPack($"CakeXunitDemo.{projectName}", new OctopusPackSettings {
            BasePath = basePath,
            Format = OctopusPackFormat.Zip,
            Version = version,
            OutFolder = new DirectoryPath(".")
        });
        
    });

Task("OctoPush")
    .IsDependentOn("Pack")
    .WithCriteria(BuildSystem.IsRunningOnTeamCity)
    .WithCriteria(!string.IsNullOrEmpty(octopusUrl))
    .WithCriteria(!string.IsNullOrEmpty(octopusApiKey))
    .Does(() =>
    {
        var packagePathCollection = new FilePathCollection(
            System.IO.Directory.GetFiles(".", "CakeXunitDemo.*.zip").Select(filePath => new FilePath(filePath)), 
            new PathComparer(false));

        OctoPush(
            octopusUrl, 
            octopusApiKey, 
            packagePathCollection, 
            new OctopusPushSettings { ReplaceExisting = true }
        );
    });

Task("Default")
  .IsDependentOn("OctoPush");

RunTarget(target);