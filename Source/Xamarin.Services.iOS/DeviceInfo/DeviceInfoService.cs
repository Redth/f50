using System;
using Foundation;

#if __MACOS__
using AppKit;
using System.Runtime.InteropServices;
using Foundation;
using ObjCRuntime;
using System.Diagnostics;
#elif __WATCHOS__
using WatchKit;
using ObjCRuntime;
#else
using UIKit;
using ObjCRuntime;
#endif

namespace Xamarin.Services.DeviceInfo
{
	public partial class DeviceInfoService
	{
#if __MACOS__

		NSProcessInfo info;
		string id, model = null;
#endif
		public DeviceInfoService()
		{
#if __MACOS__
			info = new NSProcessInfo();
#endif
		}
		public string Manufacturer => "Apple";

		public string DeviceName
		{
			get
			{
#if __IOS__ || __TVOS__
				return UIKit.UIDevice.CurrentDevice.Name;
#elif __WATCHOS__
				return WKInterfaceDevice.CurrentDevice.Name;
#elif __MACOS__
				using (var name = ObjCRuntime.Runtime.GetNSObject<NSString>(SCDynamicStoreCopyComputerName(IntPtr.Zero, IntPtr.Zero), true))
					return name;
#endif
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

#if __MACOS__

		[DllImport("/System/Library/Frameworks/SystemConfiguration.framework/SystemConfiguration")]
		static extern IntPtr SCDynamicStoreCopyComputerName(IntPtr store, IntPtr encoding);

		public string Id => id ?? (id = GetSerialNumber());


		[DllImport("/System/Library/Frameworks/IOKit.framework/IOKit")]
		static extern uint IOServiceGetMatchingService(uint masterPort, IntPtr matching);

		[DllImport("/System/Library/Frameworks/IOKit.framework/IOKit")]
		static extern IntPtr IOServiceMatching(string s);

		[DllImport("/System/Library/Frameworks/IOKit.framework/IOKit")]
		static extern IntPtr IORegistryEntryCreateCFProperty(uint entry, IntPtr key, IntPtr allocator, uint options);

		[DllImport("/System/Library/Frameworks/IOKit.framework/IOKit")]
		static extern int IOObjectRelease(uint o);

		string GetSerialNumber()
		{
			var serial = string.Empty;

			try
			{

				var platformExpert = IOServiceGetMatchingService(0, IOServiceMatching("IOPlatformExpertDevice"));
				if (platformExpert != 0)
				{
					var key = (NSString)"IOPlatformSerialNumber";
					var serialNumber = IORegistryEntryCreateCFProperty(platformExpert, key.Handle, IntPtr.Zero, 0);
					if (serialNumber != IntPtr.Zero)
					{
						serial = Runtime.GetNSObject<NSString>(serialNumber);
					}
					IOObjectRelease(platformExpert);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Unable to get serial number: " + ex.Message);
			}

			return serial;
		}
#elif __WATCHOS__
        public string Id => string.Empty;
#else
		public string Id => UIDevice.CurrentDevice.IdentifierForVendor.AsString();
#endif

#if __MACOS__
		public string Model => model ?? (model = GetModel());

		string GetModel()
		{
			var modelString = string.Empty;

			try
			{
				var platformExpert = IOServiceGetMatchingService(0, IOServiceMatching("IOPlatformExpertDevice"));
				if (platformExpert != 0)
				{
					var modelKey = (NSString)"model";
					var model = IORegistryEntryCreateCFProperty(platformExpert, modelKey.Handle, IntPtr.Zero, 0);
					if (model != IntPtr.Zero)
					{
						modelString = Runtime.GetNSObject<NSString>(model);
					}
					IOObjectRelease(platformExpert);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Unable to get model: " + ex.Message);
			}

			return modelString;
		}
#elif __WATCHOS__
        public string Model => WKInterfaceDevice.CurrentDevice.Model;
#else
		public string Model => UIDevice.CurrentDevice.Model;
#endif

#if __MACOS__
		public string Version => info.OperatingSystemVersionString;
#elif __WATCHOS__
        public string Version => WKInterfaceDevice.CurrentDevice.SystemVersion;
#else
		public string Version => UIDevice.CurrentDevice.SystemVersion;
#endif

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

		public string AppVersion => NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleShortVersionString").ToString();

		public string AppBuild => NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleVersion").ToString();

#if __IOS__
		public Platform Platform => Platform.iOS;
#elif __MACOS__
		public Abstractions.Platform Platform => Abstractions.Platform.macOS;
#elif __WATCHOS__
        public Platform Platform => Platform.watchOS;
#elif __TVOS__
        public Platform Platform => Platform.tvOS;
#endif

		public Idiom Idiom
		{
			get
			{
#if __MACOS__
				return Idiom.Desktop;
#elif __WATCHOS__
                return Idiom.Watch;
#elif __TVOS__
                return Idiom.TV;
#else

				switch (UIDevice.CurrentDevice.UserInterfaceIdiom)
				{
					case UIUserInterfaceIdiom.Pad:
						return Idiom.Tablet;
					case UIUserInterfaceIdiom.Phone:
						return Idiom.Phone;
					case UIUserInterfaceIdiom.TV:
						return Idiom.TV;
					case UIUserInterfaceIdiom.CarPlay:
						return Idiom.Car;
					default:
						return Idiom.Unknown;
				}
#endif
			}
		}

#if __MACOS__
		public bool IsDevice => true; // There is no simulator for mac OS
#else
		public bool IsDevice => Runtime.Arch == Arch.DEVICE;
#endif
	}
}
