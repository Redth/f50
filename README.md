# Xamarin.Services.Experimental



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
