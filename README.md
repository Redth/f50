# Xamarin.Services.Experimental

This is a prototype to help explore various existing plugins, their code, and API's in an effort to better understand how we could combine various bits of cross platform API's/functionality into one single repository and binary effort.

Much of this code is ported from existing plugins, and has not been refactored to the extent it will need to be.

There are a number of discussion points and ideas that remain to be resolved and implemented:

 - We think that one assembly, one nuget package is ideal, but we need to discuss any outstanding issues with this approach:
   - We can't (easily) automatically and _conditionally_ define permissions for features like Location on Android
 - How do we ensure the managed linker strips out everything that is irrelevant to the app consuming the library?
 - We need to ensure that compiled apps don't link against frameworks they aren't actually using (for example, on iOS, apps may be rejected for linking against Location services, but not specifying the appropriate permissions in the Info.plist)
 - What's the impact to developers if we take dependencies on other libraries (eg: Google Play Services - Location) which are only used by small parts of the overall library
 - We should check to ensure developers have the appropriate permissions declared in their platform app configurations and throw a _useful_ exception when they do not:
   - iOS apps sometimes need entries in the Info.plist describing the intent behind using the functionality
   - Android apps need permissions defined in the AndroidManifest.xml

## Geolocation

```csharp
var geo = new GeolocationService {
    DesiredAccuracy = 50 // 50 meters
};

if (geo.IsSupported && geo.IsEnabled) {
    var pos = await geo.GetPositionAsync (TimeSpan.FromSeconds(60));
    Console.WriteLine($"{pos.Latitude}, {pos.Longitude} @ {pos.Timestamp}");
}
```


## Text-to-Speech

```csharp
var tts = new TextToSpeechService ();
await tts.SpeakAsync ("Hello, Monkey!");
```


## Permissions

```csharp
var ps = new PermissionsService ();

var status = await ps.CheckPermissionStatusAsync(Permission.Location);

if (status != PermissionStatus.Granted) {
    // See if we should request permissions
    if (await ps.ShouldShowRequestPermissionRationaleAsync (Permission.Location))
        // Request permissions
        var results = await ps.RequestPermissionsAsync (Permission.Location);
		// Check the request results
        if (results.ContainsKey (Permission.Location))
        	status = results[Permission.Location];
    }
}

if (status == PermissionStatus.Granted) {
    /* Do your thing! */
} else {
    Console.WriteLine ("Permission was denied :("));
}
```

