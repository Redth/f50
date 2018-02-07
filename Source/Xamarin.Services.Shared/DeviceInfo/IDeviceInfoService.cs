using System;

namespace Xamarin.Services.DeviceInfo
{
#if INCLUDE_INTERFACES
	public interface IDeviceInfoService
	{
		string GenerateAppId(bool usingPhoneId = false, string prefix = null, string suffix = null);

		string Id { get; }

		string Model { get; }

		string Manufacturer { get; }

		string DeviceName { get; }

		string Version { get; }

		Version VersionNumber { get; }

		string AppVersion { get; }

		string AppBuild { get; }

		Platform Platform { get; }

		Idiom Idiom { get; }

		bool IsDevice { get; }
	}
#endif
}
