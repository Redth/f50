using System;
using Android.OS;
using static Android.Provider.Settings;
using Java.Interop;
using Android.Runtime;
using Android.Content.Res;
using Android.App;
using Android.Content;
using Android.Content.PM;

namespace Xamarin.Services.DeviceInfo
{
	public partial class DeviceInfoService
	{
		public string Manufacturer => global::Android.OS.Build.Manufacturer;

		public string DeviceName
		{
			get
			{
				var name = global::Android.Provider.Settings.System.GetString(Application.Context.ContentResolver, "device_name");
				if (string.IsNullOrWhiteSpace(name))
					name = Model;

				return name;
			}
		}

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

		static JniPeerMembers buildMembers = new XAPeerMembers("android/os/Build", typeof(Build));

		static string GetSerialField()
		{
			try
			{
				const string id = "SERIAL.Ljava/lang/String;";
				var value = buildMembers.StaticFields.GetObjectValue(id);
				return JNIEnv.GetString(value.Handle, JniHandleOwnership.TransferLocalRef);
			}
			catch
			{
				return string.Empty;

			}
		}

		string id = string.Empty;

		public string Id
		{
			get
			{
				if (!string.IsNullOrWhiteSpace(id))
					return id;


				id = GetSerialField();
				if (string.IsNullOrWhiteSpace(id) || id == Build.Unknown || id == "0")
				{
					try
					{
						var context = Xamarin.Services.Platform.CurrentContext;
						id = Secure.GetString(context.ContentResolver, Secure.AndroidId);
					}
					catch (Exception ex)
					{
						global::Android.Util.Log.Warn("DeviceInfo", "Unable to get id: " + ex.ToString());
					}
				}

				return id;
			}
		}

		public string Model => Build.Model;

		public string Version => Build.VERSION.Release;

		public Platform Platform => Platform.Android;

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
					return new Version();
				}
			}
		}

		public string AppVersion
		{
			get
			{
				using (var info = Application.Context.PackageManager.GetPackageInfo(Application.Context.PackageName, PackageInfoFlags.MetaData))
					return info.VersionName;
			}
		}

		public string AppBuild
		{
			get
			{
				using (var info = Application.Context.PackageManager.GetPackageInfo(Application.Context.PackageName, PackageInfoFlags.MetaData))
					return info.VersionCode.ToString();
			}
		}

		const int TabletCrossover = 600;

		public Idiom Idiom
		{
			get
			{
				try
				{
					var context = Xamarin.Services.Platform.CurrentContext;
					if (context == null)
						return Idiom.Unknown;


					//easiest way to get ui mode
					var uiModeManager = context.GetSystemService(Context.UiModeService) as UiModeManager;

					try
					{
						switch (uiModeManager?.CurrentModeType ?? UiMode.TypeUndefined)
						{
							case UiMode.TypeTelevision: return Idiom.TV;
							case UiMode.TypeCar: return Idiom.Car;
						}
					}
					finally
					{
						uiModeManager?.Dispose();
					}


					var config = context.Resources.Configuration;

					if (config == null)
						return Idiom.Unknown;


					var mode = config.UiMode;
					if ((int)Build.VERSION.SdkInt >= 20)
					{
						if (mode.HasFlag(UiMode.TypeWatch))
							return Idiom.Watch;
					}

					if (mode.HasFlag(UiMode.TypeTelevision))
						return Idiom.TV;
					if (mode.HasFlag(UiMode.TypeCar))
						return Idiom.Car;
					if (mode.HasFlag(UiMode.TypeDesk))
						return Idiom.Desktop;

					int minWidthDp = config.SmallestScreenWidthDp;

					return minWidthDp >= TabletCrossover ? Idiom.Tablet : Idiom.Phone;
				}
				catch (Exception ex)
				{
					System.Diagnostics.Debug.WriteLine($"Unable to get idiom: {ex}");
				}

				return Idiom.Unknown;
			}
		}

		public bool IsDevice => !(
			Build.Fingerprint.StartsWith("generic", StringComparison.InvariantCulture)
			|| Build.Fingerprint.StartsWith("unknown", StringComparison.InvariantCulture)
			|| Build.Model.Contains("google_sdk")
			|| Build.Model.Contains("Emulator")
			|| Build.Model.Contains("Android SDK built for x86")
			|| Build.Manufacturer.Contains("Genymotion")
			|| (Build.Brand.StartsWith("generic", StringComparison.InvariantCulture) && Build.Device.StartsWith("generic", StringComparison.InvariantCulture))
			|| Build.Product.Equals("google_sdk", StringComparison.InvariantCulture)
		);
	}
}
