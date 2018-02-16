using System;
using System.Collections.Generic;
using System.Diagnostics;
using Android.Net;

namespace Xamarin.Services.Connectivity
{
	internal static class Extensions
	{
		public static IEnumerable<ConnectionType> GetConnectionTypes(this ConnectivityManager manager)
		{
			//When on API 21+ need to use getAllNetworks, else fall base to GetAllNetworkInfo
			//https://developer.android.com/reference/android/net/ConnectivityManager.html#getAllNetworks()
			if ((int)global::Android.OS.Build.VERSION.SdkInt >= 21)
			{
				foreach (var network in manager.GetAllNetworks())
				{
					NetworkInfo info = null;
					try
					{
						info = manager.GetNetworkInfo(network);
					}
					catch
					{
						//there is a possibility, but don't worry about it
					}

					if (info == null || !info.IsAvailable)
						continue;

					yield return GetConnectionType(info.Type, info.TypeName);
				}
			}
			else
			{
				foreach (var info in manager.GetAllNetworkInfo())
				{
					if (info == null || !info.IsAvailable)
						continue;

					yield return GetConnectionType(info.Type, info.TypeName);
				}
			}
		}

		public static ConnectionType GetConnectionType(this ConnectivityType connectivityType, string typeName)
		{
			switch (connectivityType)
			{
				case ConnectivityType.Ethernet:
					return ConnectionType.Desktop;
				case ConnectivityType.Wimax:
					return ConnectionType.Wimax;
				case ConnectivityType.Wifi:
					return ConnectionType.WiFi;
				case ConnectivityType.Bluetooth:
					return ConnectionType.Bluetooth;
				case ConnectivityType.Mobile:
				case ConnectivityType.MobileDun:
				case ConnectivityType.MobileHipri:
				case ConnectivityType.MobileMms:
					return ConnectionType.Cellular;
				case ConnectivityType.Dummy:
					return ConnectionType.Other;
				default:
					if (string.IsNullOrWhiteSpace(typeName))
						return ConnectionType.Other;

					var typeNameLower = typeName.ToLowerInvariant();
					if (typeNameLower.Contains("mobile"))
						return ConnectionType.Cellular;

					if (typeNameLower.Contains("wifi"))
						return ConnectionType.WiFi;


					if (typeNameLower.Contains("wimax"))
						return ConnectionType.Wimax;


					if (typeNameLower.Contains("ethernet"))
						return ConnectionType.Desktop;


					if (typeNameLower.Contains("bluetooth"))
						return ConnectionType.Bluetooth;

					return ConnectionType.Other;
			}
		}

		public static bool GetIsConnected(this ConnectivityManager manager)
		{
			try
			{
				//When on API 21+ need to use getAllNetworks, else fall base to GetAllNetworkInfo
				//https://developer.android.com/reference/android/net/ConnectivityManager.html#getAllNetworks()
				if ((int)global::Android.OS.Build.VERSION.SdkInt >= 21)
				{
					foreach (var network in manager.GetAllNetworks())
					{
						try
						{
							var capabilities = manager.GetNetworkCapabilities(network);

							if (capabilities == null)
								continue;

							//check to see if it has the internet capability
							if (!capabilities.HasCapability(NetCapability.Internet))
								continue;

							//if on 23+ then we can also check validated
							//Indicates that connectivity on this network was successfully validated.
							//this means that you can be connected to wifi and has internet
							if ((int)global::Android.OS.Build.VERSION.SdkInt >= 23 && !capabilities.HasCapability(NetCapability.Validated))
								continue;

							var info = manager.GetNetworkInfo(network);

							if (info == null || !info.IsAvailable)
								continue;

							if (info.IsConnected)
								return true;
						}
						catch
						{
							//there is a possibility, but don't worry
						}
					}
				}
				else
				{
#pragma warning disable CS0618 // Type or member is obsolete
					foreach (var info in manager.GetAllNetworkInfo())
#pragma warning restore CS0618 // Type or member is obsolete
					{
						if (info == null || !info.IsAvailable)
							continue;

						if (info.IsConnected)
							return true;
					}
				}

				return false;
			}
			catch (Exception e)
			{
				Debug.WriteLine("Unable to get connected state - do you have ACCESS_NETWORK_STATE permission? - error: {0}", e);
				return false;
			}
		}
	}
}
