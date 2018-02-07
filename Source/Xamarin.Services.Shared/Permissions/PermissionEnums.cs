namespace Xamarin.Services.Permissions
{
	public enum PermissionStatus
	{
		Denied,
		Disabled,
		Granted,
		Restricted,
		Unknown
	}

	public enum Permission
	{
		Unknown,
		Calendar,
		Camera,
		Contacts,
		Location,
		Microphone,
		Phone,
		Photos,
		Reminders,
		Sensors,
		Sms,
		Storage,
		Speech,
		LocationAlways,
		LocationWhenInUse,
		MediaLibrary
	}
}
