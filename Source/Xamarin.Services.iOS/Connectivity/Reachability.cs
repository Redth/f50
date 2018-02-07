using System;
using System.Net;
using CoreFoundation;
using System.Diagnostics;
using SystemConfiguration;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Xamarin.Services.Connectivity
{
	[Foundation.Preserve(AllMembers = true)]
	public enum NetworkStatus
	{
		NotReachable,
		ReachableViaCarrierDataNetwork,
		ReachableViaWiFiNetwork
	}

	[Foundation.Preserve(AllMembers = true)]
	public static class Reachability
	{
		public static string HostName = "www.google.com";

		public static bool IsReachableWithoutRequiringConnection(NetworkReachabilityFlags flags)
		{
			// Is it reachable with the current network configuration?
			bool isReachable = (flags & NetworkReachabilityFlags.Reachable) != 0;

			// Do we need a connection to reach it?
			bool noConnectionRequired = (flags & NetworkReachabilityFlags.ConnectionRequired) == 0;

#if __IOS__
			// Since the network stack will automatically try to get the WAN up,
			// probe that
			if ((flags & NetworkReachabilityFlags.IsWWAN) != 0)
				noConnectionRequired = true;
#endif
			return isReachable && noConnectionRequired;
		}

		public static bool IsHostReachable(string host, int port)
		{
			if (string.IsNullOrWhiteSpace(host))
				return false;

			IPAddress address;
			if (!IPAddress.TryParse(host + ":" + port, out address))
			{
				Debug.WriteLine(host + ":" + port + " is not valid");
				return false;
			}
			using (var r = new NetworkReachability(host))
			{

				NetworkReachabilityFlags flags;

				if (r.TryGetFlags(out flags))
				{
					return IsReachableWithoutRequiringConnection(flags);
				}
			}
			return false;
		}

		public static bool IsHostReachable(string host)
		{
			if (string.IsNullOrWhiteSpace(host))
				return false;

			using (var r = new NetworkReachability(host))
			{

				NetworkReachabilityFlags flags;

				if (r.TryGetFlags(out flags))
				{
					return IsReachableWithoutRequiringConnection(flags);
				}
			}
			return false;
		}

		public static event EventHandler ReachabilityChanged;

		static async void OnChange(NetworkReachabilityFlags flags)
		{
			await Task.Delay(100);
			ReachabilityChanged?.Invoke(null, EventArgs.Empty);

		}
		
		static NetworkReachability defaultRouteReachability;
		static bool IsNetworkAvailable(out NetworkReachabilityFlags flags)
		{

			if (defaultRouteReachability == null)
			{
				var ip = new IPAddress(0);
				defaultRouteReachability = new NetworkReachability(ip);
				defaultRouteReachability.SetNotification(OnChange);
				defaultRouteReachability.Schedule(CFRunLoop.Main, CFRunLoop.ModeDefault);
			}
			if (!defaultRouteReachability.TryGetFlags(out flags))
				return false;
			return IsReachableWithoutRequiringConnection(flags);
		}

		static NetworkReachability remoteHostReachability;

		public static NetworkStatus RemoteHostStatus()
		{
			NetworkReachabilityFlags flags;
			bool reachable;

			if (remoteHostReachability == null)
			{
				remoteHostReachability = new NetworkReachability(HostName);

				// Need to probe before we queue, or we wont get any meaningful values
				// this only happens when you create NetworkReachability from a hostname
				reachable = remoteHostReachability.TryGetFlags(out flags);

				remoteHostReachability.SetNotification(OnChange);
				remoteHostReachability.Schedule(CFRunLoop.Main, CFRunLoop.ModeDefault);
			}
			else
				reachable = remoteHostReachability.TryGetFlags(out flags);

			if (!reachable)
				return NetworkStatus.NotReachable;

			if (!IsReachableWithoutRequiringConnection(flags))
				return NetworkStatus.NotReachable;

#if __IOS__
			if ((flags & NetworkReachabilityFlags.IsWWAN) != 0)
				return NetworkStatus.ReachableViaCarrierDataNetwork;
#endif

			return NetworkStatus.ReachableViaWiFiNetwork;
		}

		public static IEnumerable<NetworkStatus> GetActiveConnectionType()
		{
			var status = new List<NetworkStatus>();

			NetworkReachabilityFlags flags;
			bool defaultNetworkAvailable = IsNetworkAvailable(out flags);

#if __IOS__
			// If it's a WWAN connection..
			if ((flags & NetworkReachabilityFlags.IsWWAN) != 0)
				status.Add(NetworkStatus.ReachableViaCarrierDataNetwork);
			else if (defaultNetworkAvailable)
#else
			// If the connection is reachable and no connection is required, then assume it's WiFi
			if (defaultNetworkAvailable)
#endif
			{
				status.Add(NetworkStatus.ReachableViaWiFiNetwork);
			}
			else if (((flags & NetworkReachabilityFlags.ConnectionOnDemand) != 0
				|| (flags & NetworkReachabilityFlags.ConnectionOnTraffic) != 0)
				&& (flags & NetworkReachabilityFlags.InterventionRequired) == 0)
			{
				// If the connection is on-demand or on-traffic and no user intervention
				// is required, then assume WiFi.
				status.Add(NetworkStatus.ReachableViaWiFiNetwork);
			}

			return status;
		}

		public static NetworkStatus InternetConnectionStatus()
		{
			NetworkStatus status = NetworkStatus.NotReachable;

			NetworkReachabilityFlags flags;
			bool defaultNetworkAvailable = IsNetworkAvailable(out flags);

#if __IOS__
			// If it's a WWAN connection..
			if ((flags & NetworkReachabilityFlags.IsWWAN) != 0)
				status = NetworkStatus.ReachableViaCarrierDataNetwork;
#endif

			// If the connection is reachable and no connection is required, then assume it's WiFi
			if (defaultNetworkAvailable)
			{
				status = NetworkStatus.ReachableViaWiFiNetwork;
			}

			// If the connection is on-demand or on-traffic and no user intervention
			// is required, then assume WiFi.
			if (((flags & NetworkReachabilityFlags.ConnectionOnDemand) != 0
				|| (flags & NetworkReachabilityFlags.ConnectionOnTraffic) != 0)
				&& (flags & NetworkReachabilityFlags.InterventionRequired) == 0)
			{
				status = NetworkStatus.ReachableViaWiFiNetwork;
			}


			return status;
		}

		public static void Dispose()
		{
			if (remoteHostReachability != null)
			{
				remoteHostReachability.Dispose();
				remoteHostReachability = null;
			}

			if (defaultRouteReachability != null)
			{
				defaultRouteReachability.Dispose();
				defaultRouteReachability = null;
			}
		}

	}
}
