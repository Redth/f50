using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace Xamarin.Services.Connectivity
{
	public partial class ConnectivityService
	{
		private Task initialTask;
		private bool isConnected;
		private NetworkStatus previousInternetStatus = NetworkStatus.NotReachable;

		public ConnectivityService()
		{
			//start an update on the background.
			initialTask = Task.Run(() => { UpdateConnected(false); });
			Reachability.ReachabilityChanged += ReachabilityChanged;
		}

		public virtual bool IsConnected
		{
			get
			{
				if (initialTask?.IsCompleted ?? true)
				{
					UpdateConnected(false);
					return isConnected;
				}

				//await for the initial run to complete
				initialTask.Wait();
				return isConnected;
			}
		}

		public virtual async Task<bool> IsReachable(string host, int msTimeout = 5000)
		{
			if (string.IsNullOrEmpty(host))
				throw new ArgumentNullException(nameof(host));

			if (!IsConnected)
				return false;

			return await IsRemoteReachable(host, 80, msTimeout);
		}

		public virtual async Task<bool> IsRemoteReachable(string host, int port = 80, int msTimeout = 5000)
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

			return await Task.Run(() =>
			{
				try
				{
					var clientDone = new ManualResetEvent(false);
					var reachable = false;

					var hostEntry = new DnsEndPoint(host, port);

					using (var socket = new Socket(SocketType.Stream, ProtocolType.Tcp))
					{
						var socketEventArg = new SocketAsyncEventArgs { RemoteEndPoint = hostEntry };
						socketEventArg.Completed += (s, e) =>
						{
							reachable = e.SocketError == SocketError.Success;

							clientDone.Set();
						};

						clientDone.Reset();

						socket.ConnectAsync(socketEventArg);

						clientDone.WaitOne(msTimeout);

						return reachable;
					}
				}
				catch (Exception ex)
				{
					Debug.WriteLine("Unable to reach: " + host + " Error: " + ex);
					return false;
				}
			});
		}

		public virtual IEnumerable<ConnectionType> ConnectionTypes
		{
			get
			{
				var statuses = Reachability.GetActiveConnectionType();
				foreach (var status in statuses)
				{
					switch (status)
					{
						case NetworkStatus.ReachableViaCarrierDataNetwork:
							yield return ConnectionType.Cellular;
							break;
						case NetworkStatus.ReachableViaWiFiNetwork:
							yield return ConnectionType.WiFi;
							break;
						default:
							yield return ConnectionType.Other;
							break;
					}
				}
			}
		}

		public virtual IEnumerable<UInt64> Bandwidths => new UInt64[] { };

		private async void ReachabilityChanged(object sender, EventArgs e)
		{
			//Add in artifical delay so the connection status has time to change
			//else it will return true no matter what.
			await Task.Delay(100);
			UpdateConnected();
		}

		private void UpdateConnected(bool triggerChange = true)
		{
			var remoteHostStatus = Reachability.RemoteHostStatus();
			var internetStatus = Reachability.InternetConnectionStatus();

			var previouslyConnected = isConnected;
			isConnected = (internetStatus == NetworkStatus.ReachableViaCarrierDataNetwork ||
							internetStatus == NetworkStatus.ReachableViaWiFiNetwork) ||
						  (remoteHostStatus == NetworkStatus.ReachableViaCarrierDataNetwork ||
							remoteHostStatus == NetworkStatus.ReachableViaWiFiNetwork);

			if (triggerChange)
			{
				if (previouslyConnected != isConnected || previousInternetStatus != internetStatus)
					OnConnectivityChanged(new ConnectivityChangedEventArgs { IsConnected = isConnected });

				var connectionTypes = this.ConnectionTypes.ToArray();
				OnConnectivityTypeChanged(new ConnectivityTypeChangedEventArgs { IsConnected = isConnected, ConnectionTypes = connectionTypes });
			}
			previousInternetStatus = internetStatus;
		}

		private void OnDispose(bool disposing)
		{
			if (disposing)
			{
				Reachability.ReachabilityChanged -= ReachabilityChanged;
				Reachability.Dispose();
			}
		}
	}
}
