using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Xamarin.Services.Connectivity
{
#if !EXCLUDE_INTERFACES
	public interface IConnectivityService : IDisposable
	{
		bool IsConnected { get; }

		Task<bool> IsReachable(string host, int msTimeout = 5000);

		Task<bool> IsRemoteReachable(string host, int port = 80, int msTimeout = 5000);

		IEnumerable<ConnectionType> ConnectionTypes { get; }

		IEnumerable<UInt64> Bandwidths { get; }

		event ConnectivityChangedEventHandler ConnectivityChanged;

		event ConnectivityTypeChangedEventHandler ConnectivityTypeChanged;
	}
#endif

	public delegate void ConnectivityChangedEventHandler(object sender, ConnectivityChangedEventArgs e);

	public delegate void ConnectivityTypeChangedEventHandler(object sender, ConnectivityTypeChangedEventArgs e);
}
