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

    // package
    NuGetPack("./NuGet/Xamarin.Services.nuspec", new NuGetPackSettings {
        BasePath = "./",
        OutputDirectory = "./Output"
    });
});

RunTarget(target);