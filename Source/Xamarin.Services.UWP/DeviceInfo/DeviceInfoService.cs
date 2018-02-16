using System;
using Windows.System.Profile;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.Foundation.Metadata;
using Windows.UI.ViewManagement;
using Windows.ApplicationModel;

namespace Xamarin.Services.DeviceInfo
{
	public partial class DeviceInfoService
	{
		private readonly EasClientDeviceInformation deviceInfo;
		private string id = null;

		public DeviceInfoService()
		{
			deviceInfo = new EasClientDeviceInformation();
		}

		public string Manufacturer => deviceInfo.SystemManufacturer;

		public string DeviceName => deviceInfo.FriendlyName;

		public string Id
		{
			get
			{

				if (id != null)
					return id;

				try
				{
					if (ApiInformation.IsTypePresent("Windows.System.Profile.SystemIdentification"))
					{
						var systemId = SystemIdentification.GetSystemIdForPublisher();

						// Make sure this device can generate the IDs
						if (systemId.Source != SystemIdentificationSource.None)
						{
							// The Id property has a buffer with the unique ID
							var hardwareId = systemId.Id;
							var dataReader = Windows.Storage.Streams.DataReader.FromBuffer(hardwareId);

							var bytes = new byte[hardwareId.Length];
							dataReader.ReadBytes(bytes);

							id = Convert.ToBase64String(bytes);
						}
					}
					else if (ApiInformation.IsTypePresent("Windows.System.Profile.HardwareIdentification"))
					{
						var token = HardwareIdentification.GetPackageSpecificToken(null);
						var hardwareId = token.Id;
						var dataReader = Windows.Storage.Streams.DataReader.FromBuffer(hardwareId);

						var bytes = new byte[hardwareId.Length];
						dataReader.ReadBytes(bytes);

						id = Convert.ToBase64String(bytes);
					}
					else
					{
						id = "unsupported";
					}

				}
				catch (Exception)
				{

				}

				return id;
			}
		}

		public string Model => deviceInfo.SystemProductName;

		public string Version
		{
			get
			{
				var sv = AnalyticsInfo.VersionInfo.DeviceFamilyVersion;
				try
				{

					var v = ulong.Parse(sv);
					var v1 = (v & 0xFFFF000000000000L) >> 48;
					var v2 = (v & 0x0000FFFF00000000L) >> 32;
					var v3 = (v & 0x00000000FFFF0000L) >> 16;
					var v4 = v & 0x000000000000FFFFL;
					return $"{v1}.{v2}.{v3}.{v4}";
				}
				catch { }

				return sv;
			}
		}

		public Platform Platform
		{
			get
			{
				switch (AnalyticsInfo.VersionInfo.DeviceFamily)
				{
					case "Windows.Mobile":
						return Platform.WindowsPhone;
					case "Windows.Desktop":
						return UIViewSettings.GetForCurrentView().UserInteractionMode == UserInteractionMode.Mouse
							? Platform.Windows
							: Platform.WindowsTablet;
					case "Windows.IoT":
						return Platform.IoT;
					case "Windows.Xbox":
						return Platform.Xbox;
					case "Windows.Team":
						return Platform.SurfaceHub;
					default:
						return Platform.Windows;
				}
			}
		}

		public Version VersionNumber
		{
			get
			{
				try
				{
					return new Version(Version);
				}
				catch
				{
					return new Version(10, 0);
				}
			}
		}

		public string AppVersion
		{
			get
			{
				var version = Package.Current.Id.Version;

				return string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
			}
		}

		public string AppBuild => Package.Current.Id.Version.Build.ToString();

		public Idiom Idiom
		{
			get
			{
				switch (Platform)
				{
					case Platform.Windows:
						return Idiom.Desktop;
					case Platform.WindowsPhone:
						return Idiom.Phone;
					case Platform.WindowsTablet:
						return Idiom.Tablet;
					default:
						return Idiom.Unknown;

				}
			}
		}

		// http://igrali.com/2014/07/17/get-device-information-windows-phone-8-1-winrt/
		public bool IsDevice => deviceInfo.SystemProductName == "Virtual";

		public string GenerateAppId(bool usingPhoneId = false, string prefix = null, string suffix = null)
		{
			var appId = "";

			if (!string.IsNullOrEmpty(prefix))
				appId += prefix;

			appId += Guid.NewGuid().ToString();

			if (usingPhoneId)
				appId += Id;

			if (!string.IsNullOrEmpty(suffix))
				appId += suffix;

			return appId;
		}
	}
}
