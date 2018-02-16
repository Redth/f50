using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.Net;
using Android.Net.Wifi;
using Android.App;
using Java.Net;

namespace Xamarin.Services.Connectivity
{
	public partial class ConnectivityService
	{
		private ConnectivityChangeBroadcastReceiver receiver;
		private ConnectivityManager connectivityManager;
		private WifiManager wifiManager;

		private ConnectivityManager ConnectivityManager
		{
			get
			{
				if (connectivityManager == null || connectivityManager.Handle == IntPtr.Zero)
					connectivityManager = (ConnectivityManager)(Application.Context.GetSystemService(Context.ConnectivityService));

				return connectivityManager;
			}
		}

		private WifiManager WifiManager
		{
			get
			{
				if (wifiManager == null || wifiManager.Handle == IntPtr.Zero)
					wifiManager = (WifiManager)(Application.Context.GetSystemService(Context.WifiService));

				return wifiManager;
			}
		}

		public ConnectivityService()
		{
			ConnectivityChangeBroadcastReceiver.ConnectionChanged = OnConnectivityChanged;
			ConnectivityChangeBroadcastReceiver.ConnectionTypeChanged = OnConnectivityTypeChanged;
			receiver = new ConnectivityChangeBroadcastReceiver();
			Application.Context.RegisterReceiver(receiver, new IntentFilter(ConnectivityManager.ConnectivityAction));
		}

		public bool IsConnected => ConnectivityManager.GetIsConnected();

		public async Task<bool> IsReachable(string host, int msTimeout = 5000)
		{
			if (string.IsNullOrEmpty(host))
				throw new ArgumentNullException(nameof(host));

			if (!IsConnected)
				return false;

			return await Task.Run(() =>
			{
				bool reachable;
				try
				{
					reachable = InetAddress.GetByName(host).IsReachable(msTimeout);
				}
				catch (UnknownHostException ex)
				{
					Debug.WriteLine("Unable to reach: " + host + " Error: " + ex);
					reachable = false;
				}
				catch (Exception ex2)
				{
					Debug.WriteLine("Unable to reach: " + host + " Error: " + ex2);
					reachable = false;
				}
				return reachable;
			});
		}

		public async Task<bool> IsRemoteReachable(string host, int port = 80, int msTimeout = 5000)
		{

			if (string.IsNullOrEmpty(host))
				throw new ArgumentNullException(nameof(host));

			if (!IsConnected)
				return false;

			host = host.Replace("http://www.", string.Empty).
			  Replace("http://", string.Empty).
			  Replace("https://www.", string.Empty).
			  Replace("https://", string.Empty).
			  TrimEnd('/');

			return await Task.Run(async () =>
			{
				try
				{
					var tcs = new TaskCompletionSource<InetSocketAddress>();
					new System.Threading.Thread(() =>
					{
						/* this line can take minutes when on wifi with poor or none internet connectivity
                        and Task.Delay solves it only if this is running on new thread (Task.Run does not help) */
						InetSocketAddress result = new InetSocketAddress(host, port);

						if (!tcs.Task.IsCompleted)
							tcs.TrySetResult(result);

					}).Start();

					Task.Run(async () =>
					{
						await Task.Delay(msTimeout);

						if (!tcs.Task.IsCompleted)
							tcs.TrySetResult(null);
					});

					var sockaddr = await tcs.Task;

					if (sockaddr == null)
						return false;

					using (var sock = new Socket())
					{

						await sock.ConnectAsync(sockaddr, msTimeout);
						return true;

					}
				}
				catch (Exception ex)
				{
					Debug.WriteLine("Unable to reach: " + host + " Error: " + ex);
					return false;
				}
			});
		}

		public IEnumerable<ConnectionType> ConnectionTypes => ConnectivityManager.GetConnectionTypes();

		public IEnumerable<UInt64> Bandwidths
		{
			get
			{
				try
				{
					if (ConnectionTypes.Contains(ConnectionType.WiFi))
						return new[] { (UInt64)WifiManager.ConnectionInfo.LinkSpeed * 1000000 };
				}
				catch (Exception e)
				{
					Debug.WriteLine("Unable to get connected state - do you have ACCESS_WIFI_STATE permission? - error: {0}", e);
				}

				return new UInt64[] { };
			}
		}

		partial void OnDispose(bool disposing)
		{
			if (disposing)
			{
				if (receiver != null)
					Application.Context.UnregisterReceiver(receiver);

				ConnectivityChangeBroadcastReceiver.ConnectionChanged = null;
				if (wifiManager != null)
				{
					wifiManager.Dispose();
					wifiManager = null;
				}

				if (connectivityManager != null)
				{
					connectivityManager.Dispose();
					connectivityManager = null;
				}
			}
		}
	}
}
