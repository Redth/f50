///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("Default")
.Does(() => {
    // build
    NuGetRestore("./Xamarin.Services.sln");
    MSBuild("./Xamarin.Services.sln", new MSBuildSettings {
        Configuration = configuration,
        PlatformTarget = PlatformTarget.MSIL,
        MSBuildPlatform = MSBuildPlatform.x86,
    });

    // copy output
    foreach (var platform in new [] { "Android", "iOS", "UWP" }) {
        EnsureDirectoryExists($"./Output/{platform}");
        CopyFileToDirectory($"./Source/Xamarin.Services.{platform}/bin/{configuration}/Xamarin.Services.dll", $"./Output/{platform}");
    }
    EnsureDirectoryExists($"./Output/NetStandard");
    CopyFileToDirectory($"./Source/Xamarin.Services.NetStandard/bin/{configuration}/netstandard2.0/Xamarin.Services.dll", $"./Output/NetStandard");

    // package
    NuGetPack("./NuGet/Xamarin.Services.nuspec", new NuGetPackSettings {
        BasePath = "./",
        OutputDirectory = "./Output"
    });
});

RunTarget(target);