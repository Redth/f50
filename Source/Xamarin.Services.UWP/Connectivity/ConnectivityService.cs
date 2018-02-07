using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Networking.Connectivity;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Networking;
using System.Diagnostics;
using Windows.ApplicationModel.Core;
using System.Threading;

namespace Xamarin.Services.Connectivity
{
	public class ConnectivityService
#if !EXCLUDE_INTERFACES
		: IConnectivityService
#endif
	{
		bool isConnected;

		public ConnectivityService()
		{
			isConnected = IsConnected;
			NetworkInformation.NetworkStatusChanged += NetworkStatusChanged;
		}

		async void NetworkStatusChanged(object sender)
		{
			var previous = isConnected;
			var newConnected = IsConnected;

			var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;

			if (dispatcher != null)
			{
				await dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
				{
					if (previous != newConnected)
						OnConnectivityChanged(new ConnectivityChangedEventArgs { IsConnected = newConnected });

					OnConnectivityTypeChanged(new ConnectivityTypeChangedEventArgs { IsConnected = newConnected, ConnectionTypes = this.ConnectionTypes });
				});
			}
			else
			{
				if (previous != newConnected)
					OnConnectivityChanged(new ConnectivityChangedEventArgs { IsConnected = newConnected });

				OnConnectivityTypeChanged(new ConnectivityTypeChangedEventArgs { IsConnected = newConnected, ConnectionTypes = this.ConnectionTypes });
			}
		}

		public bool IsConnected
		{
			get
			{
				var profile = NetworkInformation.GetInternetConnectionProfile();
				if (profile == null)
					isConnected = false;
				else
				{
					var level = profile.GetNetworkConnectivityLevel();
					isConnected = level != NetworkConnectivityLevel.None && level != NetworkConnectivityLevel.LocalAccess;
				}

				return isConnected;
			}
		}

		public async Task<bool> IsReachable(string host, int msTimeout = 5000)
		{
			if (string.IsNullOrEmpty(host))
				throw new ArgumentNullException("host");

			if (!IsConnected)
				return false;

			try
			{
				var serverHost = new HostName(host);
				using (var client = new StreamSocket())
				{
					var cancellationTokenSource = new CancellationTokenSource();
					cancellationTokenSource.CancelAfter(msTimeout);

					await client.ConnectAsync(serverHost, "http").AsTask(cancellationTokenSource.Token);
					return true;
				}


			}
			catch (Exception ex)
			{
				Debug.WriteLine("Unable to reach: " + host + " Error: " + ex);
				return false;
			}
		}

		public async Task<bool> IsRemoteReachable(string host, int port = 80, int msTimeout = 5000)
		{
			if (string.IsNullOrEmpty(host))
				throw new ArgumentNullException("host");

			if (!IsConnected)
				return false;

			host = host.Replace("http://www.", string.Empty).
			  Replace("http://", string.Empty).
			  Replace("https://www.", string.Empty).
			  Replace("https://", string.Empty).
			  TrimEnd('/');

			try
			{
				using (var tcpClient = new StreamSocket())
				{
					var cancellationTokenSource = new CancellationTokenSource();
					cancellationTokenSource.CancelAfter(msTimeout);

					await tcpClient.ConnectAsync(
						new Windows.Networking.HostName(host),
						port.ToString(),
						SocketProtectionLevel.PlainSocket).AsTask(cancellationTokenSource.Token);

					var localIp = tcpClient.Information.LocalAddress.DisplayName;
					var remoteIp = tcpClient.Information.RemoteAddress.DisplayName;

					tcpClient.Dispose();

					return true;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Unable to reach: " + host + " Error: " + ex);
				return false;
			}
		}

		public IEnumerable<ConnectionType> ConnectionTypes
		{
			get
			{
				var networkInterfaceList = NetworkInformation.GetConnectionProfiles();
				foreach (var networkInterfaceInfo in networkInterfaceList.Where(networkInterfaceInfo => networkInterfaceInfo.GetNetworkConnectivityLevel() != NetworkConnectivityLevel.None))
				{
					var type = ConnectionType.Other;

					if (networkInterfaceInfo.NetworkAdapter != null)
					{

						switch (networkInterfaceInfo.NetworkAdapter.IanaInterfaceType)
						{
							case 6:
								type = ConnectionType.Desktop;
								break;
							case 71:
								type = ConnectionType.WiFi;
								break;
							case 243:
							case 244:
								type = ConnectionType.Cellular;
								break;
						}
					}

					yield return type;
				}
			}
		}

		public IEnumerable<UInt64> Bandwidths
		{
			get
			{
				var networkInterfaceList = NetworkInformation.GetConnectionProfiles();
				foreach (var networkInterfaceInfo in networkInterfaceList.Where(networkInterfaceInfo => networkInterfaceInfo.GetNetworkConnectivityLevel() != NetworkConnectivityLevel.None))
				{
					UInt64 speed = 0;

					if (networkInterfaceInfo.NetworkAdapter != null)
					{
						speed = (UInt64)networkInterfaceInfo.NetworkAdapter.OutboundMaxBitsPerSecond;
					}

					yield return speed;
				}
			}
		}

		protected virtual void OnConnectivityChanged(ConnectivityChangedEventArgs e) =>
			ConnectivityChanged?.Invoke(this, e);

		protected virtual void OnConnectivityTypeChanged(ConnectivityTypeChangedEventArgs e) =>
			ConnectivityTypeChanged?.Invoke(this, e);

		public event ConnectivityChangedEventHandler ConnectivityChanged;

		public event ConnectivityTypeChangedEventHandler ConnectivityTypeChanged;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		~ConnectivityService()
		{
			Dispose(false);
		}

		private bool disposed = false;

		public virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					NetworkInformation.NetworkStatusChanged -= NetworkStatusChanged;
				}

				disposed = true;
			}
		}
	}
}
